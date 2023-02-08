using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareData
{
    public Dictionary<String, int> regions;
    public string region;
    public int regionIndex;
    public bool canSpawn;
    public int regionValue = 0;
    public Vector3 location;
    public bool visited;
    public bool[] sides;
    public bool[] extends;
    public Vector3[] corners;

    bool hasMesh;
    bool hasChanged;
    public bool isLocked = false;
    MeshData meshData;
    public Color color;

    public SquareData(int cubeSize, Vector3 location)
    {
        hasMesh = false;
        hasChanged = false;
        regions = new Dictionary<String, int>();
        meshData = new MeshData();
        corners = new Vector3[4];//starting from topLeft
        region = "Maze";
        canSpawn = true;
        visited = false;
        sides = new bool[4]{ true, true, true, true }; //sides sterting with top left
        extends = new bool[8]{ false, false, false, false, false, false, false, false };//The values of the extends starting with top left 
        this.location = location;
        CalculateCorners(cubeSize);
    }
    public void StripWalls()
    {
        for (int i = 0; i < sides.Length; i++)
        {
            sides[i] = false;
        }
    }
    void CalculateCorners(int cubeSize)
    {

        corners[0] = new Vector3(location.x - cubeSize / 2, 0, location.z + cubeSize / 2);
        corners[1] = new Vector3(location.x + cubeSize / 2, 0, location.z + cubeSize / 2);
        corners[2] = new Vector3(location.x + cubeSize / 2, 0, location.z - cubeSize / 2);
        corners[3] = new Vector3(location.x - cubeSize / 2, 0, location.z - cubeSize / 2);
    }
    public void GetMeshData(MazeWallsSettings wallSettings)
    {
        meshData.CreateMeshData(sides, extends, corners, region, wallSettings);
        hasChanged = true;
    }

    public void RenderMesh(bool b, TextUreSettings textureSettings, GameObject parent)
    {
        hasMesh = meshData.HasMesh();
        if(b && !hasMesh)
        {
            meshData.CreateMesh(parent);
            meshData.RenderMesh(b);
            meshData.LoadTextures(textureSettings);
            hasChanged = false;
        }
        else if (hasMesh && b && hasChanged)
        {
            meshData.UpdateMesh();
            meshData.RenderMesh(b);
            meshData.LoadTextures(textureSettings);
            hasChanged = false;
        }
        else if(hasMesh)
        {
            meshData.RenderMesh(b);
        }
    }

    public void ReloadTextures(TextUreSettings textureSettings)
    {
        meshData.LoadTextures(textureSettings);
    }
}
