using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class SphSolver : ParticleSystemSolver
{
    
    public float kernelRadius;
    public float viscosityCoefficient;
    public float eosExponent;
    public float soundSpeed;
    public float targetDensity;
    protected SphSystemData sphSystemData;
    protected float[] test = new float[1] { 1 };
    protected Vector3[] test_vec = new Vector3[1] { new Vector3(1, 1, 1) };
    protected float deltaTime;
    protected float landa = 0.4f;
    void Start()
    {
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        sphSystemData = new SphSystemData(particleCount, gridSpacing, resoloution, raduis);
        particleSystemData = sphSystemData;
        computeBuffers = new Dictionary<string, ComputeBuffer>();
        SampleWind();
        deltaTime = kernelRadius / soundSpeed;
        viscosityCoefficient = Mathf.Clamp01(viscosityCoefficient);
        InitializeComputeBuffer();
    }

    protected override void InitializeComputeBuffer()
    {
        computeBuffers.Add("Velocity", new ComputeBuffer(particleCount, SIZE_OF_VECTOR3));
        computeBuffers["Velocity"].SetData(sphSystemData.Velocities());
        computeBuffers.Add("Position", new ComputeBuffer(particleCount, SIZE_OF_VECTOR3));
        computeBuffers["Position"].SetData(sphSystemData.Positions());
        computeBuffers.Add("Force", new ComputeBuffer(particleCount, SIZE_OF_VECTOR3));
        computeBuffers["Force"].SetData(sphSystemData.Forces());
        computeBuffers.Add("Density", new ComputeBuffer(particleCount, sizeof(float)));
        computeBuffers["Density"].SetData(sphSystemData.Densities());
        computeBuffers.Add("Pressure", new ComputeBuffer(particleCount, sizeof(float)));
        computeBuffers["Pressure"].SetData(sphSystemData.Pressure());
        computeBuffers.Add("Wind", new ComputeBuffer(particleCount, SIZE_OF_VECTOR3));

        computeShaderKernelID = computeShader.FindKernel("Initialize");
        computeShader.SetBuffer(computeShaderKernelID, "_velocities", computeBuffers["Velocity"]);
        computeShader.SetBuffer(computeShaderKernelID, "_positions", computeBuffers["Position"]);
        computeShader.SetBuffer(computeShaderKernelID, "_forces", computeBuffers["Force"]);
        computeShader.SetBuffer(computeShaderKernelID, "_densities", computeBuffers["Density"]);
        computeShader.SetBuffer(computeShaderKernelID, "_pressures", computeBuffers["Pressure"]);

        computeShader.SetInt("_numParticles", particleCount);
        computeShader.SetFloat("_mass", mass);
        computeShader.SetFloat("_gravity", gravity);
        computeShader.SetFloat("_dragCoef", dragCoeff);
        computeShader.SetFloat("_frictionScale", frictionScale);
        computeShader.SetFloat("_targetDensity", targetDensity);
        computeShader.SetFloat("_eosExponent", eosExponent);
        computeShader.SetFloat("_soundSpeed", soundSpeed);
        computeShader.SetFloat("_restutionCoeff", restutionCoefficient);
        computeShader.SetFloat("_kernelRadius", kernelRadius);
        computeShader.SetFloat("_viscosityCoeff", viscosityCoefficient);

        computeShader.Dispatch(computeShaderKernelID, Mathf.CeilToInt(particleCount / THREAD_NUM), 1, 1);
        computeBuffers["Position"].GetData(sphSystemData.Positions());
        if (instanceMesh != null)
        {
            args[0] = (uint)instanceMesh.GetIndexCount(subMeshIndex);
            args[1] = (uint)particleCount;
            args[2] = (uint)instanceMesh.GetIndexStart(subMeshIndex);
            args[3] = (uint)instanceMesh.GetBaseVertex(subMeshIndex);
        }
        argsBuffer.SetData(args);
        material.SetBuffer("positions", computeBuffers["Position"]);
    }

    protected override void SampleWind()
    {
        base.SampleWind();
    }

    void Update()
    {
        PreprocessSystem();
        AccumulateForces(deltaTime);
        TimeIntegration(deltaTime);
        CollisionResoloution();
        PostProcessSystem();
    }

    protected override void PreprocessSystem()
    {
       base.PreprocessSystem();
        UpdateDensity();
        ComputePressureFromEOS();
    }

    protected void UpdateDensity()
    {
        computeBuffers["Density"].SetData(sphSystemData.Densities());
        computeBuffers["Position"].SetData(sphSystemData.Positions());

        computeShaderKernelID = computeShader.FindKernel("UpdateDensity");
        computeShader.SetBuffer(computeShaderKernelID, "_densities", computeBuffers["Density"]);
        computeShader.SetBuffer(computeShaderKernelID, "_positions", computeBuffers["Position"]);
        computeShader.Dispatch(computeShaderKernelID, sphSystemData.particleCount / THREAD_NUM, 1, 1);
        computeBuffers["Density"].GetData(sphSystemData.Densities());
       // Debug.Log(sphSystemData.Densities()[100]);

    }

    protected void ComputePressureFromEOS()
    {
        computeBuffers["Density"].SetData(sphSystemData.Densities());
        computeBuffers["Pressure"].SetData(sphSystemData.Pressure());
        ComputeBuffer b = new ComputeBuffer(1, sizeof(float));
        b.SetData(test);

        computeShaderKernelID = computeShader.FindKernel("ComputePressureFromEOS");
        computeShader.SetBuffer(computeShaderKernelID, "_densities", computeBuffers["Density"]);
        computeShader.SetBuffer(computeShaderKernelID, "_pressures", computeBuffers["Pressure"]);

        computeShader.Dispatch(computeShaderKernelID, sphSystemData.particleCount / THREAD_NUM, 1, 1);
        computeBuffers["Pressure"].GetData(sphSystemData.Pressure());
    }

    protected override void AccumulateForces(float deltaTime)
    {
        AccumulatePressureForces(deltaTime);
        AccumulateNonPressureForces(deltaTime); 
    }

    protected void AccumulatePressureForces(float deltaTime)
    {
        ComputeGradientPressureForce();
    }

    protected void AccumulateNonPressureForces(float deltaTime)
    {
        AccumulateViscosityForces();
        AccumulateExternalForces(deltaTime);
    }

    protected void ComputeGradientPressureForce()
    {
        computeBuffers["Density"].SetData(sphSystemData.Densities());
        computeBuffers["Pressure"].SetData(sphSystemData.Pressure());
        computeBuffers["Position"].SetData(sphSystemData.Positions());
        computeBuffers["Force"].SetData(sphSystemData.Forces());

        computeShaderKernelID = computeShader.FindKernel("ComputeGradientForcePressure");
        computeShader.SetBuffer(computeShaderKernelID, "_densities", computeBuffers["Density"]);
        computeShader.SetBuffer(computeShaderKernelID, "_pressures", computeBuffers["Pressure"]);
        computeShader.SetBuffer(computeShaderKernelID, "_positions", computeBuffers["Position"]);
        computeShader.SetBuffer(computeShaderKernelID, "_forces", computeBuffers["Force"]);
        computeShader.Dispatch(computeShaderKernelID, sphSystemData.particleCount / THREAD_NUM, 1, 1);
      //  Vector3 f1 = sphSystemData.Forces()[100];
        computeBuffers["Force"].GetData(sphSystemData.Forces());
        //Vector3 f2 = sphSystemData.Forces()[100];
        //Debug.Log(f2 - f1);
    }

    protected override void AccumulateExternalForces(float deltaTime)
    {
        computeShaderKernelID = computeShader.FindKernel("ComputeExternalForce");
        computeBuffers["Force"].SetData(sphSystemData.Forces());
        computeBuffers["Velocity"].SetData(sphSystemData.Velocities());
        computeBuffers["Density"].SetData(sphSystemData.Densities());
        computeBuffers["Wind"].SetData(_wind);
        computeShader.SetBuffer(computeShaderKernelID, "_densities", computeBuffers["Density"]);
        computeShader.SetBuffer(computeShaderKernelID, "_velocities", computeBuffers["Velocity"]);
        computeShader.SetBuffer(computeShaderKernelID, "_winds", computeBuffers["Wind"]);
        computeShader.SetBuffer(computeShaderKernelID, "_forces", computeBuffers["Force"]);
        computeShader.Dispatch(computeShaderKernelID, Mathf.CeilToInt(particleCount / THREAD_NUM), 1, 1);
        computeBuffers["Force"].GetData(sphSystemData.Forces());
        computeBuffers["Wind"].GetData(_wind);
    }

    protected void AccumulateViscosityForces()
    {
        computeBuffers["Density"].SetData(sphSystemData.Densities());
        computeBuffers["Position"].SetData(sphSystemData.Positions());
        computeBuffers["Velocity"].SetData(sphSystemData.Velocities());
        computeBuffers["Force"].SetData(sphSystemData.Forces());

        computeShaderKernelID = computeShader.FindKernel("ViscosityForce");
        computeShader.SetBuffer(computeShaderKernelID, "_densities", computeBuffers["Density"]);
        computeShader.SetBuffer(computeShaderKernelID, "_positions", computeBuffers["Position"]);
        computeShader.SetBuffer(computeShaderKernelID, "_forces", computeBuffers["Force"]);
        computeShader.SetBuffer(computeShaderKernelID, "_velocities", computeBuffers["Velocity"]);
        computeShader.Dispatch(computeShaderKernelID, sphSystemData.particleCount / THREAD_NUM, 1, 1);
        computeBuffers["Force"].GetData(sphSystemData.Forces());
    }
    
    protected override void TimeIntegration(float deltaTime)
    {
        computeShaderKernelID = computeShader.FindKernel("ComputeTimeIntegration");
        computeShader.SetFloat("_deltaTime", deltaTime);
        computeBuffers["Density"].SetData(sphSystemData.Densities());
        computeBuffers["Velocity"].SetData(sphSystemData.Velocities());
        computeBuffers["Force"].SetData(sphSystemData.Forces());
        computeBuffers["Position"].SetData(sphSystemData.Positions());
        computeShader.SetBuffer(computeShaderKernelID, "_velocities", computeBuffers["Velocity"]);
        computeShader.SetBuffer(computeShaderKernelID, "_densities", computeBuffers["Density"]);
        computeShader.SetBuffer(computeShaderKernelID, "_forces", computeBuffers["Force"]);
        computeShader.SetBuffer(computeShaderKernelID, "_positions", computeBuffers["Position"]);
        computeShader.Dispatch(computeShaderKernelID, Mathf.CeilToInt(particleCount / THREAD_NUM), 1, 1);
        computeBuffers["Velocity"].GetData(sphSystemData.Velocities());
        computeBuffers["Position"].GetData(sphSystemData.Positions());
    }

    protected override void CollisionResoloution()
    {
        base.CollisionResoloution();
    }

    protected override void PostProcessSystem()
    {
        ComputePsudoViscosityVelocity();
        base.PostProcessSystem();
    }

    void ComputePsudoViscosityVelocity()
    {
        ComputeBuffer smoothVelBuffer = new ComputeBuffer(sphSystemData.particleCount, sizeof(float) * 3);
        computeBuffers["Density"].SetData(sphSystemData.Densities());
        computeBuffers["Position"].SetData(sphSystemData.Positions());
        computeBuffers["Velocity"].SetData(sphSystemData.Velocities());
        smoothVelBuffer.SetData(sphSystemData.SmoothVelocities());

        computeShaderKernelID = computeShader.FindKernel("PsuedoViscosityVelocity");
        computeShader.SetBuffer(computeShaderKernelID, "_densities", computeBuffers["Density"]);
        computeShader.SetBuffer(computeShaderKernelID, "_positions", computeBuffers["Position"]);
        computeShader.SetBuffer(computeShaderKernelID, "_velocities", computeBuffers["Velocity"]);
        computeShader.SetBuffer(computeShaderKernelID, "_smoothVelocities", smoothVelBuffer);
        computeShader.Dispatch(computeShaderKernelID, sphSystemData.particleCount / THREAD_NUM, 1, 1);
        smoothVelBuffer.GetData(sphSystemData.SmoothVelocities());

        smoothVelBuffer.SetData(sphSystemData.SmoothVelocities());
        computeBuffers["Velocity"].SetData(sphSystemData.Velocities());
        computeShaderKernelID = computeShader.FindKernel("CalcFinalVel");
        computeShader.SetBuffer(computeShaderKernelID, "_velocities", computeBuffers["Velocity"]);
        computeShader.SetBuffer(computeShaderKernelID, "_smoothVelocities", smoothVelBuffer);
        computeShader.Dispatch(computeShaderKernelID, sphSystemData.particleCount / THREAD_NUM, 1, 1);
        computeBuffers["Velocity"].GetData(sphSystemData.Velocities());

        smoothVelBuffer.Release();

    }
    

}

