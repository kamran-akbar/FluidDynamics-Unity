using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CameraPostEffect : MonoBehaviour
{
    public ComputeShader rayMarcher;
    public RenderTexture result;
    public RayMarcherMode mode;
    RenderTexture test;
    public Light light;
    Camera camera;
    float[] densities;
    BoundingBox[] boxes;
    int count;

    private void Start()
    {
        camera = GetComponent<Camera>();
        boxes = new BoundingBox[] { GetCollider() };
        int dataSize = (int)(boxes[0].dataSize.x * boxes[0].dataSize.y * boxes[0].dataSize.z);
        densities = new float[dataSize];
        InitializeDensity(boxes[0].dataSize);
        if (result == null)
        {
            result = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            test = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 1, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            result.enableRandomWrite = true;
            test.enableRandomWrite = true;
            result.Create();
            test.Create();
        }
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        switch (mode)
        {
            case RayMarcherMode.Primitive:
                RayMarch(source, destination);
                break;
            case RayMarcherMode.VolumetricRendering:
                VolumetricRayCast(source, destination);
                break;
            case RayMarcherMode.Gyroid:
                Gyroids(source, destination);
                break;
        }
    }

    void VolumetricRayCast(RenderTexture source, RenderTexture destination)
    {
        
        Vector3 cameraPos = camera.transform.position;
        Vector3 lightPos = light.transform.position;
        Vector3 euler = camera.transform.rotation.eulerAngles;
        ComputeBuffer buffer = new ComputeBuffer(1, Marshal.SizeOf(typeof(BoundingBox)));
        ComputeBuffer densityBuffer = new ComputeBuffer(densities.Length, sizeof(float));
        buffer.SetData(boxes);
        densityBuffer.SetData(densities);
        int kernelId = rayMarcher.FindKernel("VolumeRayCasting");
        rayMarcher.SetTexture(kernelId, "Result", result);
        rayMarcher.SetTexture(kernelId, "Source", source);
        rayMarcher.SetTexture(kernelId, "Test", test);
        rayMarcher.SetBuffer(kernelId, "boundingBox", buffer);
        rayMarcher.SetBuffer(kernelId, "density", densityBuffer);
        rayMarcher.SetFloats("resoloution", new float[2] { camera.pixelWidth, camera.pixelHeight });
        rayMarcher.SetFloats("lightColor", new float[4] { light.color.r, light.color.g, light.color.b, light.color.a });
        rayMarcher.SetFloats("cameraPos", new float[3] { cameraPos.x, cameraPos.y, cameraPos.z });
        rayMarcher.SetFloats("cameraEuler", new float[3] { euler.x, euler.y, euler.z });
        rayMarcher.SetFloats("lightPos", new float[3] { lightPos.x, lightPos.y, lightPos.z });
        rayMarcher.Dispatch(kernelId, camera.pixelWidth / 8, camera.pixelHeight / 8, 1);
        buffer.Release();
        densityBuffer.Release();
        Graphics.Blit(result, destination);
    }

    void RayMarch(RenderTexture source, RenderTexture destination)
    {
        Vector3 cameraPos = camera.transform.position;
        Vector3 lightPos = light.transform.position;
        Vector3 euler = camera.transform.rotation.eulerAngles;
        int kernelId = rayMarcher.FindKernel("RayMarching");
        rayMarcher.SetTexture(kernelId, "Result", result);
        rayMarcher.SetTexture(kernelId, "Source", source);
        rayMarcher.SetFloats("resoloution", new float[2] { camera.pixelWidth, camera.pixelHeight });
        rayMarcher.SetFloats("lightColor", new float[4] { light.color.r, light.color.g, light.color.b, light.color.a });
        rayMarcher.SetFloats("cameraPos", new float[3] { cameraPos.x, cameraPos.y, cameraPos.z });
        rayMarcher.SetFloats("cameraEuler", new float[3] { euler.x, euler.y, euler.z });
        rayMarcher.SetFloats("lightPos", new float[3] { lightPos.x, lightPos.y, lightPos.z });
        rayMarcher.Dispatch(kernelId, camera.pixelWidth / 8, camera.pixelHeight / 8, 1);
        Graphics.Blit(result, destination);
    }

    void Gyroids(RenderTexture source, RenderTexture destination)
    {
        Vector3 cameraPos = camera.transform.position;
        Vector3 lightPos = light.transform.position;
        Vector3 euler = camera.transform.rotation.eulerAngles;
        int kernelId = rayMarcher.FindKernel("Gyroid");
        rayMarcher.SetTexture(kernelId, "Result", result);
        rayMarcher.SetTexture(kernelId, "Source", source);
        rayMarcher.SetFloat("time", Time.time);
        rayMarcher.SetFloats("resoloution", new float[2] { camera.pixelWidth, camera.pixelHeight });
        rayMarcher.SetFloats("lightColor", new float[4] { light.color.r, light.color.g, light.color.b, light.color.a });
        rayMarcher.SetFloats("cameraPos", new float[3] { cameraPos.x, cameraPos.y, cameraPos.z });
        rayMarcher.SetFloats("cameraEuler", new float[3] { euler.x, euler.y, euler.z });
        rayMarcher.SetFloats("lightPos", new float[3] { lightPos.x, lightPos.y, lightPos.z });
        rayMarcher.Dispatch(kernelId, camera.pixelWidth / 8, camera.pixelHeight / 8, 1);
        Graphics.Blit(result, destination);
    }

    BoundingBox GetCollider()
    {
        BoundingBox bounding = new BoundingBox();
        bounding.origin = Vector3.zero;
        bounding.gridSpacing = Vector3.one * 0.15f;
        bounding.resoloution = Vector3.one * 128;
        bounding.dataSize = bounding.resoloution;
        bounding.size = new Vector3(
            bounding.gridSpacing.x * bounding.resoloution.x,
            bounding.gridSpacing.y * bounding.resoloution.y,
            bounding.gridSpacing.z * bounding.resoloution.z);
        bounding.center = bounding.origin + 0.5f * bounding.size;
        return bounding;
    }

    void InitializeDensity(Vector3 ds)
    {
        float r = 5;
        float max = 0;
        float threshold = 0.1f;
        for (int i = 0; i < densities.Length; i++)
        {
            Vector3 index = Transform1DTo3D(i, ds);
            Vector3 pos = boxes[0].origin + Vector3.Scale(index, boxes[0].gridSpacing);
            float distance = Vector3.Distance(boxes[0].center, pos);
            if (distance < r)
            {
                count++;
                if (distance >= threshold)
                {
                    densities[i] = r / distance;
                }
                else
                {
                    densities[i] =  r / threshold;
                }
                if ( densities[i] > max)
                {
                    max = densities[i];
                }
            }
        }

        for (int i = 0; i < densities.Length; i++)
        {
            densities[i] = Mathf.Max(0, densities[i] / max);
        }
    }

    void DisplayArray(float[] input, Vector3 ds)
    {
        for (int i = 0; i < input.Length; i++)
        {
            Debug.Log(Transform1DTo3D(i, ds) + " " + input[i]);
        }
    }

    Vector3 Transform1DTo3D(float idx, Vector3 ds)
    {
        Vector3 idx3 = new Vector3((int)idx % ds.x, (int)(idx / ds.y % ds.z), (int)(idx / (ds.y * ds.x)));
        return idx3;
    }

    private void OnDestroy()
    {
        result.Release();
        test.Release();
    }
    
    struct BoundingBox
    {
        public Vector3 center;
        public Vector3 origin;
        public Vector3 size;
        public Vector3 dataSize;
        public Vector3 resoloution;
        public Vector3 gridSpacing;
    }

    public enum RayMarcherMode
    {
        Primitive, VolumetricRendering, Gyroid
    }
}
