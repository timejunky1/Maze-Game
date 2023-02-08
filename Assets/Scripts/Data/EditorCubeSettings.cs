using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu()]
public class EditorCubeSettings: UpdatableData
{
    [Range(0,3)]
    public int region;
    public bool canSpawn;
    public int regionValue = 0;
    public bool Top;
    public bool Right;
    public bool Bot;
    public bool Left;
}
