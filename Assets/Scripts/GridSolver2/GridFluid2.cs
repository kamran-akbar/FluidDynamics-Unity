using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class GridFluid2
{
    private const int iteration = 10;
    private const float threshold = 0.1f;
    public Vector2Int dataSize;
    public ComputeShader gridSolver2;

    float diffusion;
    float viscosity;
    float deltaTime;

    public float[] Vx;
    public float[] Vy;
    public float[] pVx;
    public float[] pVy;

    public float[] density;
    public float[] pDensity;

    private int threadNum;

    public GridFluid2(float diffusion, float viscosity, Vector2Int dataSize, float deltaTime)
    {
        this.diffusion = diffusion;
        this.viscosity = viscosity;
        this.dataSize = dataSize;
        this.deltaTime = deltaTime;
        Vx = new float[dataSize.x * dataSize.y];
        Vy = new float[dataSize.x * dataSize.y];
        pVx = new float[dataSize.x * dataSize.y];
        pVy = new float[dataSize.x * dataSize.y];
        density = new float[dataSize.x * dataSize.y];
        pDensity = new float[dataSize.x * dataSize.y];
        threadNum = FluidSimulation.THREAD_NUM;
    }

    float Lerp(float x0, float x1, float f)
    {
        return (1 - f) * x0 + f * x1;
    }

    float BiLerp(float x00, float x10, float x01, float x11, float fx, float fy)
    {
        return Lerp(Lerp(x00, x10, fx), Lerp(x01, x11, fx), fy);
    }

    public void AddDensityAt(int x, int y, float amount)
    {
        density[IX(x, y)] += amount;
    }
    
    public void AddVelocityAt(int x, int y, float amountX, float amountY)
    {
        Vx[IX(x, y)] += amountX;
        Vy[IX(x, y)] += amountY;
    }

    public void FluidSimStep()
    {
        pVx = Diffuse(1, pVx, Vx, iteration, viscosity);
        pVy = Diffuse(2, pVy, Vy, iteration, viscosity);
        Project(pVx, pVy, iteration);
        Advect(1, Vx, pVx, pVx, pVy);
        Advect(2, Vy, pVy, pVx, pVy);
        Project(Vx, Vy, iteration);
        pDensity = Diffuse(0, pDensity, density, iteration, diffusion);
        Advect(0, density, pDensity, Vx, Vy);
    }

    void SetBoundary(int type, float[] value)
    {
        gridSolver2.SetFloat("type", type);
        //Set boundary at X 0 and resoloution.x - 1 Y 0 and resoloution.y - 1
        ComputeBuffer valueBuffer = new ComputeBuffer(value.Length, sizeof(float));
        valueBuffer.SetData(value);
        int kernelId = gridSolver2.FindKernel("SetBoundry");
        gridSolver2.SetBuffer(kernelId, "value", valueBuffer);
        gridSolver2.Dispatch(kernelId, dataSize.x / threadNum, dataSize.y / threadNum, 1);
        valueBuffer.GetData(value);
        valueBuffer.Release();
        // Set Corners
        value[IX(0, 0)] = 0.5f * (value[IX(0, 1)] + value[IX(1, 0)]);
        value[IX(dataSize.x - 1, dataSize.y - 1)] = 0.5f * (value[IX(dataSize.x - 2, dataSize.y - 1)] +
            value[IX(dataSize.x - 1, dataSize.y - 2)]);
        value[IX(dataSize.x - 1, 0)] = 0.5f * (value[IX(dataSize.x - 2, 0)] +
            value[IX(dataSize.x - 1, 1)]);
        value[IX(0, dataSize.y - 1)] = 0.5f * (value[IX(1, dataSize.y - 1)] +
            value[IX(0, dataSize.y - 2)]);
    }

    void SetCorners(float[] value)
    {
        value[IX(0, 0)] = 0.5f * (value[IX(0, 1)] + value[IX(1, 0)]);
        value[IX(dataSize.x - 1, dataSize.y - 1)] = 0.5f * (value[IX(dataSize.x - 2, dataSize.y - 1)] +
            value[IX(dataSize.x - 1, dataSize.y - 2)]);
        value[IX(dataSize.x - 1, 0)] = 0.5f * (value[IX(dataSize.x - 2, 0)] +
            value[IX(dataSize.x - 1, 1)]);
        value[IX(0, dataSize.y - 1)] = 0.5f * (value[IX(1, dataSize.y - 1)] +
            value[IX(0, dataSize.y - 2)]);
    }
    
    float[] LinearSolve(int type, float[] x, float[] b, float diam, float coeff, int iter)
    {
        float inv_diam = 1 / diam;
        float[] result = new float[x.Length];
        gridSolver2.SetFloat("coeff", coeff);
        gridSolver2.SetFloat("inv_diam", inv_diam);
        gridSolver2.SetFloat("type", type);

        ComputeBuffer valueBuffer = new ComputeBuffer(x.Length, sizeof(float));
        ComputeBuffer bBuffer = new ComputeBuffer(b.Length, sizeof(float));
        ComputeBuffer resultBuffer = new ComputeBuffer(result.Length, sizeof(float));
        bBuffer.SetData(b);
        resultBuffer.SetData(result);
        int kernelID = gridSolver2.FindKernel("LinearSolve");
        gridSolver2.SetBuffer(kernelID, "b", bBuffer);
        gridSolver2.SetBuffer(kernelID, "result", resultBuffer);
        for (int i = 0; i < iter; i++)
        {
            ApproximateJacobian(valueBuffer, bBuffer, resultBuffer, result, x, kernelID);
            x = result;
            result = new float[x.Length];
            SetCorners(x);
        }
        valueBuffer.Release();
        resultBuffer.Release();
        bBuffer.Release();

        return x;
    }

    void ApproximateJacobian(ComputeBuffer valueBuffer, ComputeBuffer bBuffer, ComputeBuffer resultBuffer, float[] result, float[] x, int kernelID)
    {
        valueBuffer.SetData(x);
        
        gridSolver2.SetBuffer(kernelID, "value", valueBuffer);
        gridSolver2.Dispatch(kernelID, dataSize.x / threadNum, dataSize.y / threadNum, 1);
        resultBuffer.GetData(result);

    }


    float[] Diffuse(int type, float[] nextData, float[] currData, int iter, float diff)
    {
        float coeff = deltaTime * diff ;
        float diam = 4 * coeff + 1;
        nextData = LinearSolve(type, nextData, currData, diam, coeff, iter);
        return nextData;
    }

    void Project(float[] Velx, float[] Vely, int iter)
    {
        float[] pressure = new float[Velx.Length];
        float[] div = new float[Velx.Length];
        CalcDivergenceAndPressure(Velx, Vely, pressure, div);
        SetCorners(div);
        pressure = LinearSolve(0, pressure, div, 4f, 1f, iter);
        CalcGradientPressure(Velx, Vely, pressure);
        SetCorners(Velx);
        SetCorners(Vely);
    }

    void CalcDivergenceAndPressure(float[] Velx, float[] Vely, float[] pressure, float[] div)
    {
        ComputeBuffer VelxBuffer = new ComputeBuffer(Velx.Length, sizeof(float));
        ComputeBuffer VelyBuffer = new ComputeBuffer(Vely.Length, sizeof(float));
        ComputeBuffer divBuffer = new ComputeBuffer(Velx.Length, sizeof(float));
        ComputeBuffer pressureBuffer = new ComputeBuffer(Velx.Length, sizeof(float));
        VelxBuffer.SetData(Velx);
        VelyBuffer.SetData(Vely);
        divBuffer.SetData(div);
        pressureBuffer.SetData(pressure);

        int kernelID = gridSolver2.FindKernel("DivergencePressure");
        gridSolver2.SetBuffer(kernelID, "Velx", VelxBuffer);
        gridSolver2.SetBuffer(kernelID, "Vely", VelyBuffer);
        gridSolver2.SetBuffer(kernelID, "divergence", divBuffer);
        gridSolver2.SetBuffer(kernelID, "pressure", pressureBuffer);
        gridSolver2.Dispatch(kernelID, dataSize.x / threadNum, dataSize.y / threadNum, 1);
        divBuffer.GetData(div);
        pressureBuffer.GetData(pressure);

        VelxBuffer.Release();
        VelyBuffer.Release();
        divBuffer.Release();
        pressureBuffer.Release();
    }

    void CalcGradientPressure(float[] Velx, float[] Vely, float[] pressure)
    {
        ComputeBuffer VelxBuffer = new ComputeBuffer(Velx.Length, sizeof(float));
        ComputeBuffer VelyBuffer = new ComputeBuffer(Vely.Length, sizeof(float));
        ComputeBuffer pressureBuffer = new ComputeBuffer(Velx.Length, sizeof(float));
        VelxBuffer.SetData(Velx);
        VelyBuffer.SetData(Vely);
        pressureBuffer.SetData(pressure);

        int kernelID = gridSolver2.FindKernel("GradientPressure");
        gridSolver2.SetBuffer(kernelID, "Velx", VelxBuffer);
        gridSolver2.SetBuffer(kernelID, "Vely", VelyBuffer);
        gridSolver2.SetBuffer(kernelID, "pressure", pressureBuffer);
        gridSolver2.Dispatch(kernelID, dataSize.x / threadNum, dataSize.y / threadNum, 1);
        VelxBuffer.GetData(Velx);
        VelyBuffer.GetData(Vely);

        VelxBuffer.Release();
        VelyBuffer.Release();
        pressureBuffer.Release();
    }

    void Advect(int type, float[] data, float[] pData, float[] Vx, float[] Vy)
    {
        float dtx = deltaTime * (dataSize.x - 2);
        float dty = deltaTime * (dataSize.y - 2);

        BackwardEuler(type, data, pData, Vx, Vy, dtx, dty);
        SetCorners(data);
        //SetBoundary(type, data);
    }

    void BackwardEuler(int type, float[] data, float[] pData, float[] Vx, float[] Vy, float dtx, float dty)
    {
        gridSolver2.SetFloat("type", type);
        gridSolver2.SetFloat("dtx", dtx);
        gridSolver2.SetFloat("dty", dty);

        ComputeBuffer VxBuffer = new ComputeBuffer(Vx.Length, sizeof(float));
        ComputeBuffer VyBuffer = new ComputeBuffer(Vy.Length, sizeof(float));
        ComputeBuffer dataBuffer = new ComputeBuffer(data.Length, sizeof(float));
        ComputeBuffer pdataBuffer = new ComputeBuffer(pData.Length, sizeof(float));
        VxBuffer.SetData(Vx);
        VyBuffer.SetData(Vy);
        dataBuffer.SetData(data);
        pdataBuffer.SetData(pData);

        int kernelID = gridSolver2.FindKernel("Advect");
        gridSolver2.SetBuffer(kernelID, "Velx", VxBuffer);
        gridSolver2.SetBuffer(kernelID, "Vely", VyBuffer);
        gridSolver2.SetBuffer(kernelID, "value", dataBuffer);
        gridSolver2.SetBuffer(kernelID, "pValue", pdataBuffer);
        gridSolver2.Dispatch(kernelID, dataSize.x / threadNum, dataSize.y / threadNum, 1);
        dataBuffer.GetData(data);

        VxBuffer.Release();
        VyBuffer.Release();
        dataBuffer.Release();
        pdataBuffer.Release();
    }

    public int IX(int x, int y)
    {
        x = x % dataSize.x;
        y = y % dataSize.y;
        return y * dataSize.x + x;
    }

}
