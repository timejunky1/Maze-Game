using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class EditorCubeSettings: UpdatableData
{
    public int cubeSize;
    [Range(0,3)]
    public int region;
    public bool canSpawn;
    public int regionValue = 0;
    public bool Top;
    public bool Right;
    public bool Bot;
    public bool Left;
    public bool IsBoss;
    public Vector3Int pointA;
    public Vector3Int pointB;
}
