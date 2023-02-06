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
    public int xIndex;
    public int yIndex;
    public bool[] sides;
    public bool[] extends;
    public Vector3[] corners;

    bool hasMesh;
    bool hasChanged;
    MeshData meshData;
    public Color color;

    public SquareData(int xIndex, int yIndex, int cubeSize, int mazeSize)
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
        this.xIndex = xIndex;
        this.yIndex = yIndex;
        this.location = new Vector3((xIndex + 1) * cubeSize - (mazeSize * cubeSize / 2) - cubeSize / 2, 0, (yIndex + 1) * cubeSize - (mazeSize * cubeSize / 2) - cubeSize / 2);
        CalculateCorners(cubeSize);

    }
    void CalculateCorners(int cubeSize)
    {

        corners[0] = new Vector3(location.x - cubeSize / 2, 0, location.z + cubeSize / 2);
        corners[1] = new Vector3(location.x + cubeSize / 2, 0, location.z + cubeSize / 2);
        corners[2] = new Vector3(location.x + cubeSize / 2, 0, location.z - cubeSize / 2);
        corners[3] = new Vector3(location.x - cubeSize / 2, 0, location.z - cubeSize / 2);
    }
    public void GetMeshData(int wallHeight, int wallWidth, Material material, Color colour)
    {
        color = colour;
        meshData.CreateMeshData(sides, extends, corners, material, wallHeight, wallWidth);
        hasChanged = true;
    }

    public void RenderMesh(bool b)
    {
        if(b && !hasMesh)
        {
            hasMesh = meshData.CreateMesh();
            meshData.RenderMesh(b);
            hasChanged = false;
        }
        else if (hasMesh && b && hasChanged)
        {
            meshData.UpdateMesh();
            meshData.RenderMesh(b);
            hasChanged = false;
        }
        else if(hasMesh)
        {
            meshData.RenderMesh(b);
        }
    }
}
