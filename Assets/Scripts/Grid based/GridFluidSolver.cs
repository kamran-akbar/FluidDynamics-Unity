using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using GridDataStructure;

public class GridFluidSolver : MonoBehaviour
{
    private const float epsilon = 0.001f;
    private const int THREAD_NUM = 8;

    public Vector3 gridSpacing;
    public Vector3 resoloution;
    public Vector3 origin;
    public ComputeShader gridSolver;

    private GridSystemData gridSystemData;
    private Vector3 gravity;

    public void Start()
    {
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.size = Vector3.Scale(resoloution, gridSpacing);
        collider.center = origin + 0.5f * collider.size;
        gridSystemData = new GridSystemData(gridSpacing, resoloution, origin, collider);
        gravity = new Vector3(0, -9.8f, 0);
    }


    private void Update()
    {
        OnBeginTimeAdvance(Time.deltaTime);
        ComputeGravity(Time.deltaTime);
        ComputePressure(Time.deltaTime);
        ComputeViscosity(Time.deltaTime);
        ComputeAdvection(Time.deltaTime);
        OnEndTimeAdvance(Time.deltaTime);
    }

    protected virtual void OnBeginTimeAdvance(float deltaTime)
    {

    }

    protected virtual void ComputeGravity(float deltaTime)
    {
        var vel = gridSystemData.Velocity();
        var u = vel.U_Accessor();
        var v = vel.V_Accessor();
        var w = vel.W_Accessor();

        if (gravity.sqrMagnitude > epsilon)
        {
            if (Mathf.Abs(gravity.x) > epsilon)
            {
                ApplyVelocityAtAxis(u, vel.DataSize_U(), deltaTime, gravity.x);
            }
            if (Mathf.Abs(gravity.y) > epsilon)
            {
                ApplyVelocityAtAxis(v, vel.DataSize_V(), deltaTime, gravity.y);
            }
            if (Mathf.Abs(gravity.z) > epsilon)
            {
                ApplyVelocityAtAxis(w, vel.DataSize_W(), deltaTime, gravity.z);
            }
        }
    }

    void ApplyVelocityAtAxis<T>(T[] vel, Vector3 ds, float dt, float g)
    {
        ComputeBuffer velBuffer = new ComputeBuffer(vel.Length, Marshal.SizeOf(typeof(T)));
        velBuffer.SetData(vel);
        int kernelId = gridSolver.FindKernel("ComputeGravity");
        gridSolver.SetBuffer(kernelId, "scalarData", velBuffer);
        gridSolver.SetFloats("gridSpacing", new float[3] { gridSpacing.x, gridSpacing.y, gridSpacing.z });
        gridSolver.SetFloats("dataSize", new float[3] { ds.x, ds.y, ds.z });
        gridSolver.SetFloats("origin", new float[3] { origin.x, origin.y, origin.z });
        gridSolver.SetFloat("deltaTime", dt);
        gridSolver.SetFloat("gravity", g);
        gridSolver.Dispatch(kernelId, (int)ds.x / THREAD_NUM + 1, (int)ds.y / THREAD_NUM + 1, (int)ds.z / THREAD_NUM + 1);
        velBuffer.GetData(vel);
        velBuffer.Release();
    }

    protected virtual void ComputePressure(float deltaTime)
    {

    }

    protected virtual void ComputeViscosity(float deltaTime)
    {

    }

    protected virtual void ComputeAdvection(float deltaTime)
    {

    }

    void OnEndTimeAdvance(float deltaTime)
    {

    }
}
