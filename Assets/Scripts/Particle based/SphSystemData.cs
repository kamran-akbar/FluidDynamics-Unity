using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphSystemData : ParticleSystemData
{

    float[] densities;
    float[] pressures;
    Vector3[] smoothVelocities;
    
    public SphSystemData(int particleCount, float gridSpacing, Vector3 resoloution, float raduis) 
        : base (particleCount, gridSpacing, resoloution, raduis)
    {
        densities = new float[particleCount];
        pressures = new float[particleCount];
        smoothVelocities = new Vector3[particleCount];
    }

    public float[] Densities()
    {
        return densities;
    }

    public float[] Pressure()
    {
        return pressures;
    }

    public Vector3[] SmoothVelocities()
    {
        return smoothVelocities;
    }


   
}
