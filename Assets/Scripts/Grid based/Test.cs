using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridSystemBuilder;
using GridDataStructure;

public class Test : MonoBehaviour
{
    public ComputeShader gridSolver;

    void Start()
    {
        //TestGradient();
        //TestDivergenceDataPoint();
        //TestDivergenceFaceCentered();
        //TestLaplacian();
        //GridSystemData gridData= new GridSystemData(Vector3.one, Vector3.one, Vector3.zero, new BoxCollider());
        //CellCenteredScalarGridBuilder builder = new CellCenteredScalarGridBuilder();
        //int idx = gridData.AddScalarData(builder);
        //Debug.Log(gridData.ScalarDataAt(idx).GetType());
        //TestInterpolation();
        //TestSampler1();
        //TestSampler2();
    }

    void TestGradient()
    {
        Vector3 ds = new Vector3(4, 4, 4);
        float[] input = new float[(int)(ds.x * ds.y * ds.z)];
        Vector3[] output = new Vector3[input.Length];
        float[] t = new float[1] { 0 };
        InitializeInputScalar(input, ds);
        Debug.Log("--------------------------------------Input Data----------------------------------------------");
        DisplayArray(input, ds);
        ComputeBuffer bufferIn = new ComputeBuffer((int)(ds.x * ds.y * ds.z), sizeof(float));
        ComputeBuffer bufferOut = new ComputeBuffer((int)(ds.x * ds.y * ds.z), sizeof(float) * 3);
        ComputeBuffer b = new ComputeBuffer(1, sizeof(float));
        bufferIn.SetData(input);
        bufferOut.SetData(output);
        b.SetData(t);
        int kernelId = gridSolver.FindKernel("TestGradient");
        gridSolver.SetBuffer(kernelId, "scalarData", bufferIn);
        gridSolver.SetBuffer(kernelId, "vectorData", bufferOut);
        gridSolver.SetBuffer(kernelId, "t", b);
        gridSolver.SetFloats("dataSize", new float[3] { ds.x, ds.y, ds.z });
        gridSolver.SetFloats("gridSpacing", new float[3] { 1, 1, 1 });
        gridSolver.Dispatch(kernelId, (int)ds.x / 2, (int)ds.y / 2, (int)ds.z / 2);
        Debug.Log("--------------------------------------Output Data----------------------------------------------");
        bufferOut.GetData(output);
        b.GetData(t);
        Debug.Log("t " + t[0]);
        bufferOut.Release();
        bufferIn.Release();
        b.Release();
        DisplayVectorArray(output, ds);
    }

    void TestDivergenceDataPoint()
    {
        Vector3 ds = new Vector3(4, 4, 4);
        Vector3[] input = new Vector3[(int)(ds.x * ds.y * ds.z)];
        float[] output = new float[input.Length];
        float[] t = new float[1] { 0 };
        InitializeInputVector(input, ds);
        Debug.Log("--------------------------------------Input Data----------------------------------------------");
        DisplayVectorArray(input, ds);
        ComputeBuffer bufferIn = new ComputeBuffer((int)(ds.x * ds.y * ds.z), sizeof(float) * 3);
        ComputeBuffer bufferOut = new ComputeBuffer((int)(ds.x * ds.y * ds.z), sizeof(float));
        ComputeBuffer b = new ComputeBuffer(1, sizeof(float));
        bufferIn.SetData(input);
        bufferOut.SetData(output);
        b.SetData(t);
        int kernelId = gridSolver.FindKernel("TestDivergence");
        gridSolver.SetBuffer(kernelId, "vectorData", bufferIn);
        gridSolver.SetBuffer(kernelId, "scalarData", bufferOut);
        gridSolver.SetBuffer(kernelId, "t", b);
        gridSolver.SetFloats("dataSize", new float[3] { ds.x, ds.y, ds.z });
        gridSolver.SetFloats("gridSpacing", new float[3] { 1, 1, 1 });
        gridSolver.Dispatch(kernelId, (int)ds.x / 2, (int)ds.y / 2, (int)ds.z / 2);
        Debug.Log("--------------------------------------Output Data----------------------------------------------");
        bufferOut.GetData(output);
        b.GetData(t);
        Debug.Log("t " + t[0]);
        bufferOut.Release();
        bufferIn.Release();
        b.Release();
        DisplayArray(output, ds);
    }

    void TestDivergenceFaceCentered()
    {
        Vector3 ds = new Vector3(4, 4, 4);
        float[] u = new float[(int)(ds.x * ds.y * ds.z)];
        float[] v = new float[(int)(ds.x * ds.y * ds.z)];
        float[] w = new float[(int)(ds.x * ds.y * ds.z)];
        float[] output = new float[u.Length];
        float[] t = new float[1] { 0 };
        InitializeInputScalar(u, ds);
        InitializeInputScalar(v, ds);
        InitializeInputScalar(w, ds);
        Debug.Log("--------------------------------------Input Data----------------------------------------------");
        DisplayUVW(u, v, w, ds);
        ComputeBuffer bufferU = new ComputeBuffer((int)(ds.x * ds.y * ds.z), sizeof(float));
        ComputeBuffer bufferV = new ComputeBuffer((int)(ds.x * ds.y * ds.z), sizeof(float));
        ComputeBuffer bufferW = new ComputeBuffer((int)(ds.x * ds.y * ds.z), sizeof(float));
        ComputeBuffer bufferOut = new ComputeBuffer((int)(ds.x * ds.y * ds.z), sizeof(float));
        ComputeBuffer b = new ComputeBuffer(1, sizeof(float));
        bufferU.SetData(u);
        bufferV.SetData(v);
        bufferW.SetData(w);
        bufferOut.SetData(output);
        b.SetData(t);
        int kernelId = gridSolver.FindKernel("TestDivergenceFaceCentered");
        gridSolver.SetBuffer(kernelId, "u", bufferU);
        gridSolver.SetBuffer(kernelId, "v", bufferV);
        gridSolver.SetBuffer(kernelId, "w", bufferW);
        gridSolver.SetBuffer(kernelId, "scalarData", bufferOut);
        gridSolver.SetBuffer(kernelId, "t", b);
        gridSolver.SetFloats("dataSize", new float[3] { ds.x, ds.y, ds.z });
        gridSolver.SetFloats("gridSpacing", new float[3] { 1, 1, 1 });
        gridSolver.Dispatch(kernelId, (int)ds.x / 2, (int)ds.y / 2, (int)ds.z / 2);
        Debug.Log("--------------------------------------Output Data----------------------------------------------");
        bufferOut.GetData(output);
        b.GetData(t);
        Debug.Log("t " + t[0]);
        bufferOut.Release();
        bufferU.Release();
        bufferV.Release();
        bufferW.Release();
        b.Release();
        DisplayArray(output, ds);
    }

    void TestLaplacian()
    {
        Vector3 ds = new Vector3(4, 4, 4);
        float[] input = new float[(int)(ds.x * ds.y * ds.z)];
        float[] output = new float[input.Length];
        float[] t = new float[1] { 0 };
        InitializeInputScalar(input, ds);
        Debug.Log("--------------------------------------Input Data----------------------------------------------");
        DisplayArray(input, ds);
        ComputeBuffer bufferIn = new ComputeBuffer((int)(ds.x * ds.y * ds.z), sizeof(float));
        ComputeBuffer bufferOut = new ComputeBuffer((int)(ds.x * ds.y * ds.z), sizeof(float));
        ComputeBuffer b = new ComputeBuffer(1, sizeof(float));
        bufferIn.SetData(input);
        bufferOut.SetData(output);
        b.SetData(t);
        int kernelId = gridSolver.FindKernel("TestLaplacian");
        gridSolver.SetBuffer(kernelId, "scalarData", bufferIn);
        gridSolver.SetBuffer(kernelId, "u", bufferOut);
        gridSolver.SetBuffer(kernelId, "t", b);
        gridSolver.SetFloats("dataSize", new float[3] { ds.x, ds.y, ds.z });
        gridSolver.SetFloats("gridSpacing", new float[3] { 1, 1, 1 });
        gridSolver.Dispatch(kernelId, (int)ds.x / 2, (int)ds.y / 2, (int)ds.z / 2);
        Debug.Log("--------------------------------------Output Data----------------------------------------------");
        bufferOut.GetData(output);
        b.GetData(t);
        Debug.Log("t " + t[0]);
        bufferOut.Release();
        bufferIn.Release();
        b.Release();
        DisplayArray(output, ds);
    }

    void TestInterpolation()
    {
        float[] data = new float[1] { 0 };
        ComputeBuffer buffer = new ComputeBuffer(1, sizeof(float));
        buffer.SetData(data);
        int kernelId = gridSolver.FindKernel("TestInterpolation");
        gridSolver.SetBuffer(kernelId, "t", buffer);
        gridSolver.Dispatch(kernelId, 1, 1, 1);
        buffer.GetData(data);
        buffer.Release();
        Debug.Log(data[0]);
    }

    void TestSampler1()
    {
        float[] t = new float[8];
        Vector3[] v = new Vector3[8];
        ComputeBuffer bufferT = new ComputeBuffer(8, sizeof(float));
        ComputeBuffer bufferV = new ComputeBuffer(8, sizeof(float) * 3);
        bufferT.SetData(t);
        bufferV.SetData(v);
        int kernelId = gridSolver.FindKernel("TestSampler1");
        gridSolver.SetBuffer(kernelId, "t", bufferT);
        gridSolver.SetBuffer(kernelId, "vectorData", bufferV);
        gridSolver.SetFloats("origin", new float[3] { 0, 0, 0 });
        gridSolver.SetFloats("dataSize", new float[3] { 2, 2, 2 });
        gridSolver.SetFloats("gridSpacing", new float[3] { 1, 1, 1 });
        gridSolver.Dispatch(kernelId, 1, 1, 1);
        bufferT.GetData(t);
        bufferV.GetData(v);
        bufferT.Release();
        bufferV.Release();
        DisplayArray(t, new Vector3(2, 2, 2));
        DisplayVectorArray(v, new Vector3(2, 2, 2));
    }

    void TestSampler2()
    {
        float[] t = new float[1];
        float[] scalar = new float[8];
        InitializeInputScalar(scalar, new Vector3(2, 2, 2));
        DisplayArray(scalar, new Vector3(2, 2, 2));
        ComputeBuffer bufferT = new ComputeBuffer(1, sizeof(float));
        ComputeBuffer bufferScalar = new ComputeBuffer(8, sizeof(float));
        bufferT.SetData(t);
        bufferScalar.SetData(scalar);
        int kernelId = gridSolver.FindKernel("TestSampler2");
        gridSolver.SetBuffer(kernelId, "t", bufferT);
        gridSolver.SetBuffer(kernelId, "scalarData", bufferScalar);
        gridSolver.SetFloats("origin", new float[3] { 0, 0, 0 });
        gridSolver.SetFloats("dataSize", new float[3] { 2, 2, 2 });
        gridSolver.SetFloats("gridSpacing", new float[3] { 1, 1, 1 });
        gridSolver.Dispatch(kernelId, 1, 1, 1);
        bufferT.GetData(t);
        bufferScalar.GetData(scalar);
        bufferT.Release();
        bufferScalar.Release();
        Debug.Log(t[0]);
    }

    void InitializeInputScalar(float[] input, Vector3 ds)
    {
        for (int i = 0; i < ds.x * ds.y * ds.z; i++)
        {
            input[i] = Random.Range(0.5f, 5f);
        }
    }

    void InitializeInputVector(Vector3[] input, Vector3 ds)
    {
        for (int i = 0; i < ds.x * ds.y * ds.z; i++)
        {
            input[i] = new Vector3(Random.Range(0.5f, 5f), Random.Range(0.5f, 5f), Random.Range(0.5f, 5f));
        }
    }

    void DisplayArray(float[] input, Vector3 ds)
    {
        for (int i = 0; i < input.Length; i++)
        {
            Debug.Log(Transform1DTo3D(i, ds) + " " + input[i]);
        }
    }

    void DisplayVectorArray(Vector3[] input, Vector3 ds)
    {
        for (int i = 0; i < input.Length; i++)
        {
            Debug.Log(Transform1DTo3D(i, ds) + " " + input[i]);
        }
    }

    void DisplayUVW(float[] u, float[] v, float[] w,  Vector3 ds)
    {
        for (int i = 0; i < u.Length; i++)
        {
            Debug.Log(Transform1DTo3D(i, ds) + " " + u[i] + " " + v[i] + " " + w[i]);
        }
    }

    Vector3 Transform1DTo3D(float idx, Vector3 ds)
    {
        Vector3 idx3 = new Vector3((int)idx % ds.x, (int)(idx / ds.y % ds.z), (int)(idx / (ds.y * ds.x)));
        return idx3;
    }


}
