using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleTime : MonoBehaviour
{

    [Range(0, 1)]
    public float scale = 0.01f;

    void Update()
    {
        Time.timeScale = scale; ;
    }
}
