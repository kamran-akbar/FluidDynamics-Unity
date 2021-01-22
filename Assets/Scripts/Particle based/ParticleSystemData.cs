using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemData
{
    public int particleCount;
    public float maxRadius;
    public List<float>[] neighboursList;
    public float[] neighbour1D;
    public Interval[] neighbourIndices;

    HashGridSearch hashGridSearch;
    Vector3[] positions;
    Vector3[] velocities;
    Vector3[] forces;

    
    public ParticleSystemData(int particleCount, float gridSpacing, Vector3 resoloution, float raduis)
    {
        this.particleCount = particleCount;
        maxRadius = raduis;
        hashGridSearch = new HashGridSearch(gridSpacing, resoloution);
        positions = new Vector3[particleCount];
        velocities = new Vector3[particleCount];
        forces = new Vector3[particleCount];
        neighboursList = new List<float>[particleCount];
    }

    public Vector3[] Velocities()
    {
        return velocities;
    }

    public Vector3[] Forces()
    {
        return forces;
    }

    public Vector3[] Positions()
    {
        return positions;
    }

    public void SetForces(Vector3[] forces)
    {
        this.forces = forces;
    }

    public void Build()
    {
        hashGridSearch.BuildHashTable(positions);
    }

    public void CreateNeighbourList()
    {
        int count = 0;
        for (int i = 0; i < positions.Length; i++)
        {
            neighboursList[i] = new List<float>();
            hashGridSearch.FindNearPoints(positions[i], maxRadius, (int j) =>
            {
                if (i != j)
                {
                    count++;
                    neighboursList[i].Add(j);
                }
            });
        }
        Build1DNeighbourArray(count);

    }

    private void Build1DNeighbourArray(int count)
    {
        neighbour1D = new float[count];
        neighbourIndices = new Interval[positions.Length];
        int lastIndex = 0;
        for (int i = 0; i < positions.Length; i++)
        {
            List<float> arr = neighboursList[i];
            arr.CopyTo(neighbour1D, lastIndex);
            neighbourIndices[i] = new Interval(lastIndex, lastIndex + arr.Count);
            lastIndex += arr.Count;
        }
    }

    public struct Interval
    {
        int first;
        int end;

        public Interval(int first, int end)
        {
            this.first = first;
            this.end = end;
        }
    }

}



