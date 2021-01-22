using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class ShaderRunner : MonoBehaviour
{
    public int particleCount = 1000;
    public Material material;
    public ComputeShader computeShader;

    private const int SIZE_OF_PARTICLE = 24;
    private int computeShaderKernelID;
    private const int NUMBER_OF_PARTICLE_IN_EACH_THREAD = 256;
    private ComputeBuffer computeBuffer;
    private int threadNumber;

    private AudioSource audioData;
    private struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
    }
    // Use this for initialization
    void Start()
    {
        RenderSettings.ambientLight = Color.black;
        audioData = GetComponent<AudioSource>();
        //audioData.Play(0);
        if (particleCount <= 0)
            particleCount = 1;
        threadNumber = Mathf.CeilToInt((float)particleCount / NUMBER_OF_PARTICLE_IN_EACH_THREAD);

        Particle[] particles = new Particle[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            if (i % 4 == 0)
            {
                particles[i].position.x = Random.value * 4 - 4.0f;
                particles[i].position.y = Random.value * 4 - 4.0f;
                particles[i].position.z = Random.value * 4 - 4.0f;
                particles[i].velocity.x = 0;
                particles[i].velocity.y = 0;
                particles[i].velocity.z = 0;
            }
            else if (i % 4 == 1)
            {
                particles[i].position.x = Random.value * 4 + 50.0f;
                particles[i].position.y = Random.value * 4 + 10.0f;
                particles[i].position.z = Random.value * 4 + 10.0f;
                particles[i].velocity.x = 0;
                particles[i].velocity.y = 0;
                particles[i].velocity.z = 0;
            }
            else if (i % 4 == 2)
            {
                particles[i].position.x = Random.value * 4 - 250.0f;
                particles[i].position.y = Random.value * 4 - 4.0f;
                particles[i].position.z = Random.value * 4 - 4.0f;
                particles[i].velocity.x = 0;
                particles[i].velocity.y = 0;
                particles[i].velocity.z = 0;
            }
            else
            {
                particles[i].position.x = Random.value * 4 + 0.0f;
                particles[i].position.y = Random.value * 4 + 150.0f;
                particles[i].position.z = Random.value * 4 + 150.0f;
                particles[i].velocity.x = 0;
                particles[i].velocity.y = 0;
                particles[i].velocity.z = 0;
            }


        }
        computeBuffer = new ComputeBuffer(particleCount, Marshal.SizeOf(typeof(Particle)));
        computeBuffer.SetData(particles);

        computeShaderKernelID = computeShader.FindKernel("CSMain");

        computeShader.SetBuffer(computeShaderKernelID, "particleBuffers", computeBuffer);
        material.SetBuffer("particleBuffer", computeBuffer);
        computeShader.SetInt("_numofParticles", particleCount);

    }
    void OnDestroy()
    {
        if (computeBuffer != null)
            computeBuffer.Release();
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = GetMousePosition();
        float[] mousePos2D = { mousePos.x, mousePos.y };
        float[] mousePos3D = { mousePos.x, mousePos.y, mousePos.z };


        computeShader.SetFloat("_deltaTime", Time.deltaTime);
        computeShader.SetFloats("_mousePos", mousePos2D);

        computeShader.Dispatch(computeShaderKernelID, threadNumber, 1, 1);

    }

    private void OnRenderObject()
    {
        material.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, 1, particleCount);

    }

    private Vector3 GetMousePosition()
    {
        Ray ray = Camera.allCameras[0].ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit) && Input.GetMouseButton(0))
            return hit.point;
        return new Vector3(0, 0, 0);
    }
}
