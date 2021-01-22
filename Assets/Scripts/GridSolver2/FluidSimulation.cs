using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidSimulation : MonoBehaviour
{
    public static int THREAD_NUM = 32;

    public ComputeShader renderer;
    public float additiveDensity;
    public float diffusion;
    public float viscosity;
    public float deltaTime;
    RenderTexture result;
    Camera camera;
    GridFluid2 fluid;
    Vector2Int ds;
    Vector3 currMousePos;
    Vector3 prevMousePos;

    void Start()
    {
        camera = Camera.main;
        ds = new Vector2Int(camera.pixelWidth, camera.pixelHeight);
        fluid = new GridFluid2(diffusion, viscosity, ds, deltaTime);
        fluid.gridSolver2 = renderer;
        renderer.SetFloats("resoloution", new float[2] { ds.x, ds.y });
        result = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        result.enableRandomWrite = true;
        result.Create();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Vector3 res = new Vector3(ds.x, ds.y, 0);

        int cx = (int)(0.5 * ds.x);
        int cy = (int)(0.5 * ds.y);
        if (Input.GetMouseButton(0))
        {
            fluid.AddDensityAt((int)currMousePos.x, (int)currMousePos.y, additiveDensity);
            Vector2 vel = new Vector2(currMousePos.x - prevMousePos.x, currMousePos.y - prevMousePos.y);
            fluid.AddVelocityAt((int)currMousePos.x, (int)currMousePos.y, vel.x, vel.y);
        }
        fluid.FluidSimStep();
        RenderD(result);
        Graphics.Blit(result, destination);
    }

    private void Update()
    {
        prevMousePos = currMousePos;
        currMousePos = Input.mousePosition;
    }

    public void RenderD(RenderTexture rt)
    {
        ComputeBuffer densityBuffer = new ComputeBuffer(fluid.density.Length, sizeof(float));
        densityBuffer.SetData(fluid.density);
        int kernelId = renderer.FindKernel("Render");
        renderer.SetTexture(kernelId, "finalTex", result);
        renderer.SetBuffer(kernelId, "density", densityBuffer);
        renderer.SetFloats("resoloution", new float[2] { camera.pixelWidth, camera.pixelHeight });
        renderer.Dispatch(kernelId, camera.pixelWidth / THREAD_NUM, camera.pixelHeight / THREAD_NUM, 1);
        densityBuffer.Release();
    }

    public void FadeD()
    {
        for (int i = 0; i < fluid.density.Length; i++)
        {
            fluid.density[i] *= 0.1f;
        }
    }
}
