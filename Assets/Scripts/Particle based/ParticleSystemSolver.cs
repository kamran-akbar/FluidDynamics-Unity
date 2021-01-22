using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemSolver : MonoBehaviour
{
    protected const int THREAD_NUM = 128;

    public int particleCount;
    public Mesh instanceMesh;
    
    public float mass;
    public float dragCoeff;
    public float gravity;
    public float restutionCoefficient;
    public float frictionScale;
    public ComputeShader computeShader;
    public Material material;
    [HideInInspector]
    public float gridSpacing;
    [HideInInspector]
    public Vector3 resoloution;
    [HideInInspector]
    public float raduis;
    
    protected ParticleSystemData particleSystemData;

    protected int computeShaderKernelID;
    protected Dictionary<string, ComputeBuffer> computeBuffers;
    protected const int SIZE_OF_VECTOR3 = 12;
    protected Vector3[] _newPositions;
    protected Vector3[] _newVelocities;
    protected Vector3[] _wind;
    protected float iteration;
    protected ComputeBuffer argsBuffer;
    protected uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    protected int subMeshIndex;

    private void Start()
    {
        subMeshIndex = 0;
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        particleSystemData = new ParticleSystemData(particleCount, gridSpacing, resoloution, raduis);
        computeBuffers = new Dictionary<string, ComputeBuffer>();
        SampleWind();
        InitializeComputeBuffer();
    }

    protected virtual void SampleWind()
    {
        _wind = new Vector3[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            _wind[i] = new Vector3(Random.Range(-1, 0f), 0, Random.Range(-1, 1f));
        }
    }

    protected virtual void InitializeComputeBuffer()
    {
        computeBuffers.Add("Velocity", new ComputeBuffer(particleCount, SIZE_OF_VECTOR3));
        computeBuffers["Velocity"].SetData(particleSystemData.Velocities());
        computeBuffers.Add("Position", new ComputeBuffer(particleCount, SIZE_OF_VECTOR3));
        computeBuffers["Position"].SetData(particleSystemData.Positions());
        computeBuffers.Add("Force", new ComputeBuffer(particleCount, SIZE_OF_VECTOR3));
        computeBuffers["Force"].SetData(particleSystemData.Forces());
        computeBuffers.Add("Wind", new ComputeBuffer(particleCount, SIZE_OF_VECTOR3));

        computeShaderKernelID = computeShader.FindKernel("Initialize");
        computeShader.SetBuffer(computeShaderKernelID, "_velocities", computeBuffers["Velocity"]);
        computeShader.SetBuffer(computeShaderKernelID, "_positions", computeBuffers["Position"]);
        computeShader.SetBuffer(computeShaderKernelID, "_forces", computeBuffers["Force"]);
        computeShader.SetInt("_numParticles", particleCount);
        computeShader.SetFloat("_mass", mass);
        computeShader.SetFloat("_gravity", gravity);
        computeShader.SetFloat("_dragCoef", dragCoeff);
        computeShader.SetFloat("_frictionScale", frictionScale);
        computeShader.SetFloat("_restutionCoeff", restutionCoefficient);
        computeShader.Dispatch(computeShaderKernelID, Mathf.CeilToInt(particleCount / THREAD_NUM), 1, 1);
        computeBuffers["Position"].GetData(particleSystemData.Positions());
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

    private void Update()
    {
        PreprocessSystem();
        AccumulateForces(Time.deltaTime);
        TimeIntegration(Time.deltaTime);
        CollisionResoloution();
        PostProcessSystem();
    }

    protected virtual void AccumulateForces(float deltaTime)
    {
        AccumulateExternalForces(deltaTime);
    }

    protected virtual void AccumulateExternalForces(float deltaTime)
    {
        computeShaderKernelID = computeShader.FindKernel("ComputeExternalForce");
        computeBuffers["Force"].SetData(particleSystemData.Forces());
        computeBuffers["Velocity"].SetData(particleSystemData.Velocities());
        computeBuffers["Wind"].SetData(_wind);
        computeShader.SetBuffer(computeShaderKernelID, "_velocities", computeBuffers["Velocity"]);
        computeShader.SetBuffer(computeShaderKernelID, "_winds", computeBuffers["Wind"]);
        computeShader.SetBuffer(computeShaderKernelID, "_forces", computeBuffers["Force"]);
        computeShader.Dispatch(computeShaderKernelID, Mathf.CeilToInt(particleCount / THREAD_NUM), 1, 1);
        computeBuffers["Force"].GetData(particleSystemData.Forces());
        computeBuffers["Wind"].GetData(_wind);
    }

    protected virtual void TimeIntegration(float deltaTime)
    {
        computeShaderKernelID = computeShader.FindKernel("ComputeTimeIntegration");
        computeShader.SetFloat("_deltaTime", deltaTime);
        computeBuffers["Velocity"].SetData(particleSystemData.Velocities());
        computeBuffers["Force"].SetData(particleSystemData.Forces());
        computeBuffers["Position"].SetData(particleSystemData.Positions());
        computeShader.SetBuffer(computeShaderKernelID, "_velocities", computeBuffers["Velocity"]);
        computeShader.SetBuffer(computeShaderKernelID, "_forces", computeBuffers["Force"]);
        computeShader.SetBuffer(computeShaderKernelID, "_positions", computeBuffers["Position"]);
        computeShader.Dispatch(computeShaderKernelID, Mathf.CeilToInt(particleCount / THREAD_NUM), 1, 1);
        computeBuffers["Velocity"].GetData(particleSystemData.Velocities());
        computeBuffers["Position"].GetData(particleSystemData.Positions());
    }

    protected virtual void CollisionResoloution()
    {
        float[] test = new float[1] { 1 };
        Vector3[] testVec = new Vector3[1] { Vector3.one };
        ComputeBuffer testBuffer = new ComputeBuffer(1, sizeof(float));
        ComputeBuffer testVecBuffer = new ComputeBuffer(1, sizeof(float) * 3);
        testBuffer.SetData(test);

        computeShaderKernelID = computeShader.FindKernel("CollisionResoloution");
        computeBuffers["Velocity"].SetData(particleSystemData.Velocities());
        computeBuffers["Position"].SetData(particleSystemData.Positions());
        computeShader.SetBuffer(computeShaderKernelID, "_velocities", computeBuffers["Velocity"]);
        computeShader.SetBuffer(computeShaderKernelID, "_positions", computeBuffers["Position"]);
        computeShader.SetBuffer(computeShaderKernelID, "test", testBuffer);
        computeShader.SetBuffer(computeShaderKernelID, "test_vec", testVecBuffer);
        computeShader.Dispatch(computeShaderKernelID, Mathf.CeilToInt(particleCount / THREAD_NUM), 1, 1);
        testBuffer.GetData(test);
        testVecBuffer.GetData(testVec);
        computeBuffers["Velocity"].GetData(particleSystemData.Velocities());
        computeBuffers["Position"].GetData(particleSystemData.Positions());
    }

    protected virtual void PreprocessSystem()
    {
        particleSystemData.SetForces(new Vector3[particleCount]);
    }

    protected virtual void PostProcessSystem()
    {
        computeBuffers["Velocity"].GetData(particleSystemData.Velocities());
        computeBuffers["Position"].GetData(particleSystemData.Positions());
    }

    private void OnRenderObject()
    {
        material.SetPass(0);
      //   Graphics.DrawProcedural(MeshTopology.Points, 1, particleCount);
        Graphics.DrawMeshInstancedIndirect(instanceMesh, subMeshIndex, material, new Bounds(Vector3.zero,
            Vector3.one * 100), argsBuffer);

    }

    private void OnDestroy()
    {
        foreach (ComputeBuffer buffer in computeBuffers.Values)
        {
            buffer.Release();
        }
        argsBuffer.Release();
    }
}
