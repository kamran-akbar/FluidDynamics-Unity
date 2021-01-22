using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawBoxOutline : MonoBehaviour
{
    public float forwardFacePos;
    public float backwardFacePos;
    public float upFacePos;
    public float downFacePos;
    public float rightFacePos;
    public float leftFacePos;
    public Material material;

    private GameObject[] boxOutlines = new GameObject[12];

    private void Awake()
    {
        for (int i = 0; i < boxOutlines.Length; i++)
        {
            boxOutlines[i] = new GameObject();
            boxOutlines[i].transform.parent = transform;
            boxOutlines[i].transform.name = "Outline " + i;
            boxOutlines[i].AddComponent<LineRenderer>();
        }
        SetLineRendererVertexPositions(0, new Vector3(rightFacePos, upFacePos, forwardFacePos + 0.5f), new Vector3(rightFacePos, upFacePos, backwardFacePos - 0.5f));
        SetLineRendererVertexPositions(1, new Vector3(rightFacePos, downFacePos, forwardFacePos + 0.5f), new Vector3(rightFacePos, downFacePos, backwardFacePos - 0.5f));
        SetLineRendererVertexPositions(2, new Vector3(rightFacePos, upFacePos + 0.5f, forwardFacePos), new Vector3(rightFacePos, downFacePos - 0.5f, forwardFacePos));
        SetLineRendererVertexPositions(3, new Vector3(rightFacePos, upFacePos + 0.5f, backwardFacePos), new Vector3(rightFacePos, downFacePos - 0.5f, backwardFacePos));
        SetLineRendererVertexPositions(4, new Vector3(leftFacePos, upFacePos, forwardFacePos + 0.5f), new Vector3(leftFacePos, upFacePos, backwardFacePos - 0.5f));
        SetLineRendererVertexPositions(5, new Vector3(leftFacePos, downFacePos, forwardFacePos + 0.5f), new Vector3(leftFacePos, downFacePos, backwardFacePos - 0.5f));
        SetLineRendererVertexPositions(6, new Vector3(leftFacePos, upFacePos + 0.5f, backwardFacePos), new Vector3(leftFacePos, downFacePos - 0.5f, backwardFacePos));
        SetLineRendererVertexPositions(7, new Vector3(leftFacePos, upFacePos + 0.5f, forwardFacePos), new Vector3(leftFacePos, downFacePos - 0.5f, forwardFacePos));
        SetLineRendererVertexPositions(8, new Vector3(rightFacePos + 0.5f, upFacePos, forwardFacePos), new Vector3(leftFacePos - 0.5f, upFacePos, forwardFacePos));
        SetLineRendererVertexPositions(9, new Vector3(rightFacePos + 0.5f, downFacePos, forwardFacePos), new Vector3(leftFacePos - 0.5f, downFacePos, forwardFacePos));
        SetLineRendererVertexPositions(10, new Vector3(rightFacePos + 0.5f, upFacePos, backwardFacePos), new Vector3(leftFacePos - 0.5f, upFacePos, backwardFacePos));
        SetLineRendererVertexPositions(11, new Vector3(rightFacePos + 0.5f, downFacePos, backwardFacePos), new Vector3(leftFacePos - 0.5f, downFacePos, backwardFacePos));
        
    }

    void SetLineRendererVertexPositions(int index, Vector3 pos1, Vector3 pos2)
    {
        boxOutlines[index].GetComponent<LineRenderer>().SetPosition(0, pos1);
        boxOutlines[index].GetComponent<LineRenderer>().SetPosition(1, pos2);
        boxOutlines[index].GetComponent<LineRenderer>().material = material;
    }
}
