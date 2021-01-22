using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fdm;

public class FdmTester : MonoBehaviour
{
    public ComputeShader computeShader;
    FdmSolver fdmSolver;
    Vector3 ds;

    void Start()
    {
        ds = new Vector3(2, 2, 2);
        fdmSolver = new FdmSolver(ds, computeShader);
        TestSystemSolve();
    }
    
    void TestL2Norm()
    {
        float[] vector = new float[11];
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = i;
        }
        Debug.Log(fdmSolver.L2Norm(vector));
    }

    void TestMVM()
    {
        MatrixRow[] A = new MatrixRow[(int)(ds.x * ds.y * ds.z)];
        A[0] = SetMatrixRow(10, 1, 1, 0);
        A[1] = SetMatrixRow(10, 0, 2, 0);
        A[2] = SetMatrixRow(10, 1, 0, 5);
        A[3] = SetMatrixRow(10, 0, 0, 5);
        A[4] = SetMatrixRow(10, 0, 0, 0);
        A[5] = SetMatrixRow(10, 0, 0, 0);
        A[6] = SetMatrixRow(10, 0, 0, 0);
        A[7] = SetMatrixRow(10, 0, 0, 0);
        float[] vector = new float[(int)(ds.x * ds.y * ds.z)];
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = i;
        }
        vector = fdmSolver.MVM(vector, A);
        for (int i = 0; i < vector.Length; i++)
        {
            Debug.Log(vector[i]);
        }
    }

    void TestSystemSolve()
    {
        MatrixRow[] A = new MatrixRow[(int)(ds.x * ds.y * ds.z)];
        A[0] = SetMatrixRow(10, 1, 1, 0);
        A[1] = SetMatrixRow(10, 0, 2, 0);
        A[2] = SetMatrixRow(10, 1, 0, 5);
        A[3] = SetMatrixRow(10, 0, 0, 5);
        A[4] = SetMatrixRow(10, 0, 0, 0);
        A[5] = SetMatrixRow(10, 0, 0, 0);
        A[6] = SetMatrixRow(10, 0, 0, 0);
        A[7] = SetMatrixRow(10, 0, 0, 0);
        float[] b = new float[(int)(ds.x * ds.y * ds.z)];
        float[] x = new float[(int)(ds.x * ds.y * ds.z)];
        for (int i = 0; i < b.Length; i++)
        {
            x[i] = 0;
        }
        b[0] = 3;
        b[1] = 16;
        b[2] = 53;
        b[3] = 69;
        b[4] = 40;
        b[5] = 50;
        b[6] = 70;
        b[7] = 85;
        FdmSystem sys = new FdmSystem();
        sys.x = x;
        sys.A = A;
        sys.b = b;
        Debug.Log(fdmSolver.Solve(sys));
    }

    void TestResidual()
    {
        MatrixRow[] A = new MatrixRow[(int)(ds.x * ds.y * ds.z)];
        A[0] = SetMatrixRow(10, 1, 1, 0);
        A[1] = SetMatrixRow(10, 0, 2, 0);
        A[2] = SetMatrixRow(10, 1, 0, 5);
        A[3] = SetMatrixRow(10, 0, 0, 5);
        A[4] = SetMatrixRow(10, 0, 0, 0);
        A[5] = SetMatrixRow(10, 0, 0, 0);
        A[6] = SetMatrixRow(10, 0, 0, 0);
        A[7] = SetMatrixRow(10, 0, 0, 0);
        float[] b = new float[(int)(ds.x * ds.y * ds.z)];
        float[] x = new float[(int)(ds.x * ds.y * ds.z)];
        for (int i = 0; i < b.Length; i++)
        {
            x[i] = i;
        }
        b[0] = 3;
        b[1] = 16;
        b[2] = 53;
        b[3] = 69;
        b[4] = 40;
        b[5] = 50;
        b[6] = 70;
        b[7] = 85;
        float[] residual = fdmSolver.Residual(b, x, A);
        Debug.Log(fdmSolver.L2Norm(residual));
    }

    MatrixRow SetMatrixRow(float c, float r, float u, float f)
    {
        MatrixRow row = new MatrixRow();
        row.center = c;
        row.right = r;
        row.up = u;
        row.forward = f;
        return row;
    }

    
    
}
