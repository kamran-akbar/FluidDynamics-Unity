using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System;
namespace Fdm
{
    public class FdmSolver
    {
        public ComputeShader fdmSolver;
        public int maxIterationNumber = 500;
        public int checkInterval = 1;
        public float thereshold= 0.5f;
        Vector3 dataSize;

        public FdmSolver(Vector3 dataSize, ComputeShader computeShader)
        {
            this.dataSize = dataSize;
            fdmSolver = computeShader;
        }

        public float[] MVM(float[] vector, MatrixRow[] matrix)
        {
            int count = vector.Length;
            float[] result = new float[count];
            ComputeBuffer vBuffer = new ComputeBuffer(count, sizeof(float));
            ComputeBuffer matBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(MatrixRow)));
            ComputeBuffer rBuffer = new ComputeBuffer(count, sizeof(float));
            vBuffer.SetData(vector);
            matBuffer.SetData(matrix);
            rBuffer.SetData(result);
            int kernelID = fdmSolver.FindKernel("MvmKernel");
            fdmSolver.SetBuffer(kernelID, "x", vBuffer);
            fdmSolver.SetBuffer(kernelID, "A", matBuffer);
            fdmSolver.SetBuffer(kernelID, "result", rBuffer);
            fdmSolver.SetFloats("ds", new float[3] { dataSize.x, dataSize.y, dataSize.z });
            fdmSolver.Dispatch(kernelID, (int)dataSize.x / 8, (int)dataSize.y / 8, (int)dataSize.z / 8);
            rBuffer.GetData(result);
            vBuffer.Release();
            matBuffer.Release();
            rBuffer.Release();
            return result;
        }

        public float[] Residual(float[] b, float[] x, MatrixRow[] A)
        {
            int count = b.Length;
            float[] result = new float[count];
            ComputeBuffer bBuffer = new ComputeBuffer(count, sizeof(float));
            ComputeBuffer xBuffer = new ComputeBuffer(count, sizeof(float));
            ComputeBuffer aBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(MatrixRow)));
            ComputeBuffer rBuffer = new ComputeBuffer(count, sizeof(float));
            bBuffer.SetData(b);
            xBuffer.SetData(x);
            aBuffer.SetData(A);
            rBuffer.SetData(result);
            int kernelID = fdmSolver.FindKernel("ResidualKernel");
            fdmSolver.SetBuffer(kernelID, "b", bBuffer);
            fdmSolver.SetBuffer(kernelID, "x", xBuffer);
            fdmSolver.SetBuffer(kernelID, "A", aBuffer);
            fdmSolver.SetBuffer(kernelID, "result", rBuffer);
            fdmSolver.SetFloats("ds", new float[3] { dataSize.x, dataSize.y, dataSize.z });
            fdmSolver.Dispatch(kernelID, (int)dataSize.x / 8, (int)dataSize.y / 8, (int)dataSize.z / 8);
            rBuffer.GetData(result);
            aBuffer.Release();
            bBuffer.Release();
            xBuffer.Release();
            rBuffer.Release();
            return result;
        }

        public float[] Relax(FdmSystem system)
        {
            int count = system.x.Length;
            float[] result = new float[count];
            ComputeBuffer bBuffer = new ComputeBuffer(count, sizeof(float));
            ComputeBuffer xBuffer = new ComputeBuffer(count, sizeof(float));
            ComputeBuffer aBuffer = new ComputeBuffer(count, Marshal.SizeOf(typeof(MatrixRow)));
            ComputeBuffer rBuffer = new ComputeBuffer(count, sizeof(float));
            bBuffer.SetData(system.b);
            xBuffer.SetData(system.x);
            aBuffer.SetData(system.A);
            rBuffer.SetData(result);
            int kernelID = fdmSolver.FindKernel("RelaxKernel");
            fdmSolver.SetBuffer(kernelID, "b", bBuffer);
            fdmSolver.SetBuffer(kernelID, "x", xBuffer);
            fdmSolver.SetBuffer(kernelID, "A", aBuffer);
            fdmSolver.SetBuffer(kernelID, "result", rBuffer);
            fdmSolver.SetFloats("ds", new float[3] { dataSize.x, dataSize.y, dataSize.z });
            fdmSolver.Dispatch(kernelID, (int)dataSize.x / 8, (int)dataSize.y / 8, (int)dataSize.z / 8);
            rBuffer.GetData(result);
            aBuffer.Release();
            bBuffer.Release();
            xBuffer.Release();
            rBuffer.Release();
            return result;
        }

        public bool Solve(FdmSystem system)
        {
            float[] residual;
            for (int i = 0; i < maxIterationNumber; i++)
            {
                float[] xTemp = Relax(system);
                
                for (int j = 0; j < system.x.Length; j++)
                {
                    system.x[j] = xTemp[j];
                   // Debug.Log(xTemp[j]);
                }

                if (i != 0 && i % checkInterval == 0)
                {
                    residual = Residual(system.b, system.x, system.A);
                    if (L2Norm(residual) < thereshold)
                        break;
                }
            }
            residual = Residual(system.b, system.x, system.A);
            return L2Norm(residual) < thereshold;
        }

        float[] SelfPairWiseMultiply(float[] vector)
        {
            int count = vector.Length;
            float[] result = new float[count];
            ComputeBuffer vBuffer = new ComputeBuffer(count, sizeof(float));
            ComputeBuffer rBuffer = new ComputeBuffer(count, sizeof(float));
            vBuffer.SetData(vector);
            rBuffer.SetData(result);
            int kernelID = fdmSolver.FindKernel("PairwiseMultKernel");
            fdmSolver.SetBuffer(kernelID, "x", vBuffer);
            fdmSolver.SetBuffer(kernelID, "result", rBuffer);
            fdmSolver.SetFloats("ds", new float[3] { dataSize.x, dataSize.y, dataSize.z });
            fdmSolver.Dispatch(kernelID, (int)dataSize.x / 8, (int)dataSize.y / 8, (int)dataSize.z / 8);
            rBuffer.GetData(result);
            vBuffer.Release();
            rBuffer.Release();
            return result;
        }

        float Sum(float[] vector)
        {
            float sum = 0;
            int length = vector.Length;
            while (length > 1)
            {
                int prevLength = length;
                if (length % 2 == 0)
                {
                    length = length / 2;
                }
                else
                {
                    length = (length / 2) + 1;
                }
                ComputeBuffer rBuffer = new ComputeBuffer(vector.Length, sizeof(float));
                rBuffer.SetData(vector);
                int kernelID = fdmSolver.FindKernel("SumKernel");
                fdmSolver.SetBuffer(kernelID, "result", rBuffer);
                fdmSolver.SetFloat("length", prevLength);
                fdmSolver.SetFloat("halfLength", length);
                fdmSolver.SetFloats("ds", new float[3] { dataSize.x, dataSize.y, dataSize.z });
                fdmSolver.Dispatch(kernelID, (int)dataSize.x / 8, (int)dataSize.y / 8, (int)dataSize.z / 8);
                rBuffer.GetData(vector);
                rBuffer.Release();
            }
            sum = vector[0];
            return sum;
        }

        public float L2Norm(float[] vector)
        {
            float[] result = SelfPairWiseMultiply(vector);
            float sum = Sum(result);
            return sum;
        }

    }

    public struct MatrixRow
    {
        public float right;
        public float up;
        public float forward;
        public float center;
    }

    public struct FdmSystem
    {
        public MatrixRow[] A;
        public float[] x;
        public float[] b;
    }
}
