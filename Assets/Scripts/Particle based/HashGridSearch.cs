using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HashGridSearch
{
    public delegate void CallBack(int index);

    public float _gridSpacing;
    public Vector3 _resoloution;
    public Vector3[] _points;
    public List<int>[] _buckets;

    public HashGridSearch(float gridSpacing, Vector3 resoloution)
    {
        _gridSpacing = gridSpacing;
        _resoloution = resoloution;
        _buckets = new List<int>[(int)(_resoloution.x * _resoloution.y * _resoloution.z)];
    }

    public void BuildHashTable(Vector3[] points)
    {
        _buckets = new List<int>[(int)(_resoloution.x * _resoloution.y * _resoloution.z)];
        _points = new Vector3[points.Length];
        int max = 0;
        for (int i = 0; i < points.Length; i++)
        {
            _points[i] = points[i];
            int key = GetHashKeyFromPosition(_points[i]);
            if(_buckets[key] == null)
            {
                _buckets[key] = new List<int>();
            }
            _buckets[key].Add(i);
            max = Mathf.Max(max, _buckets[key].Count);
        }

    }

    private int GetHashKeyFromPosition(Vector3 p)
    {
        Vector3 bucketIndex = FindBucketIndex(p);
        return GetKeyFromBucketIndex(bucketIndex);
    }

    private Vector3 FindBucketIndex(Vector3 p)
    {
        Vector3 bucketIndex = new Vector3(
            Mathf.FloorToInt(p.x / _gridSpacing),
            Mathf.FloorToInt(p.y / _gridSpacing),
            Mathf.FloorToInt(p.z / _gridSpacing));
        return bucketIndex;
    }

    private int GetKeyFromBucketIndex(Vector3 bucketIndex)
    {
        Vector3 wrapperIndex = new Vector3(
            bucketIndex.x % _resoloution.x,
            bucketIndex.y % _resoloution.y,
            bucketIndex.z % _resoloution.z);
        if (wrapperIndex.x < 0)
        {
            wrapperIndex.x += _resoloution.x;
        }
        if (wrapperIndex.y < 0)
        {
            wrapperIndex.y += _resoloution.y;
        }
        if (wrapperIndex.z < 0)
        {
            wrapperIndex.z += _resoloution.z;
        }
        return (int)(wrapperIndex.z * _resoloution.y * _resoloution.x + wrapperIndex.y * _resoloution.x + wrapperIndex.x);
    }

    public void FindNearPoints(Vector3 point, float raduis, CallBack callBack)
    {
        Vector3 bucketIndex = FindBucketIndex(point);
        int[] nearKeys = FindNearkey(point, bucketIndex);
        for (int i = 0; i < nearKeys.Length; i++)
        {
            if (_buckets[nearKeys[i]] != null)
            {
                for (int j = 0; j < _buckets[nearKeys[i]].Count; j++)
                {
                    if ((point - _points[_buckets[nearKeys[i]][j]]).magnitude < raduis)
                    {
                        callBack(_buckets[nearKeys[i]][j]);
                    }
                }
            }
        }
    }

    private int[] FindNearkey(Vector3 origin, Vector3 bucketIndex)
    {
        Vector3[] nearBucketIndices = new Vector3[8];
        for (int i = 0; i < 8; i++)
        {
            nearBucketIndices[i] = bucketIndex;
        }

        if ((bucketIndex.x + 0.5f) * _gridSpacing <= origin.x)
        {
            nearBucketIndices[1].x += 1; nearBucketIndices[2].x += 1;
            nearBucketIndices[5].x += 1; nearBucketIndices[6].x += 1;
        }
        else
        {
            nearBucketIndices[1].x -= 1; nearBucketIndices[2].x -= 1;
            nearBucketIndices[5].x -= 1; nearBucketIndices[6].x -= 1;
        }
        if ((bucketIndex.y + 0.5f) * _gridSpacing <= origin.y)
        {
            nearBucketIndices[3].y += 1; nearBucketIndices[2].y += 1;
            nearBucketIndices[7].y += 1; nearBucketIndices[6].y += 1;
        }
        else
        {
            nearBucketIndices[3].y -= 1; nearBucketIndices[2].y -= 1;
            nearBucketIndices[7].y -= 1; nearBucketIndices[6].y -= 1;
        }
        if ((bucketIndex.z + 0.5f) * _gridSpacing <= origin.z)
        {
            nearBucketIndices[7].z += 1; nearBucketIndices[6].z += 1;
            nearBucketIndices[4].z += 1; nearBucketIndices[5].z += 1;
        }
        else
        {
            nearBucketIndices[1].z -= 1; nearBucketIndices[3].z -= 1;
            nearBucketIndices[4].z -= 1; nearBucketIndices[6].z -= 1;
        }

        int[] nearKeys = new int[8];
        for (int i = 0; i < 8; i++)
        {
            nearKeys[i] = GetKeyFromBucketIndex(nearBucketIndices[i]);
        }
        return nearKeys;
    }
}
