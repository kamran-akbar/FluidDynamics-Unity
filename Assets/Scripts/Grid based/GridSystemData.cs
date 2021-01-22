using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GridDataStructure;
using GridSystemBuilder;

public class GridSystemData
{
    private Vector3 resoloution;
    private Vector3 gridSpacing;
    private Vector3 origin;
    private BoxCollider boundingBox;
    private FaceCenteredGrid velocity;
    private List<ScalarGrid> scalarDataList;
    private List<VectorGrid> vectorDataList;

    public GridSystemData() { }

    public GridSystemData(Vector3 gridSpacing, Vector3 resoloution, Vector3 origin, BoxCollider boundingBox)
    {
        this.gridSpacing = gridSpacing;
        this.resoloution = resoloution;
        this.origin = origin;
        this.boundingBox = boundingBox;
        scalarDataList = new List<ScalarGrid>();
        vectorDataList = new List<VectorGrid>();
        velocity = new FaceCenteredGrid(gridSpacing, resoloution, origin, boundingBox);
    }

    public Vector3 Resoloution()
    {
        return resoloution;
    }

    public Vector3 GridSpacing()
    {
        return gridSpacing;
    }

    public Vector3 Origin()
    {
        return origin;
    }

    public BoxCollider BoundingBox()
    {
        return boundingBox;
    }

    public FaceCenteredGrid Velocity()
    {
        return velocity;
    }

    public int ScalarDataNumber()
    {
        return scalarDataList.Count;
    }

    public int VectorDataNumber()
    {
        return vectorDataList.Count;
    }

    public ScalarGrid ScalarDataAt(int index)
    {
        return scalarDataList[index];
    }

    public VectorGrid VectorDataAt(int index)
    {
        return vectorDataList[index];
    }

    public int AddScalarData(ScalarGridBuilder builder)
    {
        scalarDataList.Add(builder.build(gridSpacing, resoloution, origin, boundingBox));
        return scalarDataList.Count - 1;
    }

    public int AddVectorData(VectorGridBuilder builder)
    {
        vectorDataList.Add(builder.build(gridSpacing, resoloution, origin, boundingBox));
        return vectorDataList.Count - 1;
    }
}
