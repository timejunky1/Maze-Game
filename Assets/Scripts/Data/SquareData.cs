using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareData
{
    public bool isBoss = false;
    public Dictionary<String, int> regions;
    public string region;
    public int regionIndex;
    public bool canSpawn;
    public int regionValue = 0;
    public Vector3 location;
    public bool visited;
    public bool[] sides;
    public bool[] pillars;
    public Vector3[] corners;

    public bool hasMesh;
    bool hasChanged;
    public bool isRendered;
    public bool isLocked;
    public MeshData meshData;
    public Color color;
    int meshCount = 0;

    public SquareData(int cubeSize, Vector3 location)
    {
        isLocked = false;
        isRendered = false;
        hasMesh = false;
        hasChanged = true;
        regions = new Dictionary<String, int>();
        meshData = new MeshData();
        corners = new Vector3[4];//starting from topLeft
        region = "Maze";
        canSpawn = true;
        visited = false;
        sides = new bool[4]{ true, true, true, true }; //sides sterting with top left
        pillars = new bool[4]{ false, false, false, false};//The values of the extends starting with top left 
        this.location = location;
        CalculateCorners(cubeSize);
    }
    public void StripWalls()
    {
        for (int i = 0; i < sides.Length; i++)
        {
            sides[i] = false;
        }
        for (int i = 0; i < pillars.Length; i++)
        {
            pillars[i] = false;
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
        meshData.CreateMeshData(this, wallSettings);
        hasChanged = true;
        meshCount++;
    }

    public void RenderAdditions(TextureSettings textureSettings)
    {
        if(isRendered)
        {
            meshData.RenderAdditions(textureSettings, "Assets/Prefabs/");
        }
    }

    public void RenderMesh(TextureSettings textureSettings, GameObject parent)
    {
        if(isRendered && !hasChanged)
        {
            Debug.Log("load textures");
            meshData.LoadTextures(textureSettings);
            return;
        }
        hasMesh = meshData.HasMesh();
        if (!hasMesh)
        {
            Debug.Log("create mesh");
            meshData.CreateMesh(parent);
            meshData.LoadTextures(textureSettings);
            meshData.RenderMesh(true);
            hasChanged = false;
        }
        else if (hasMesh && hasChanged)
        {
            Debug.Log("update mesh");
            meshData.UpdateMesh();
            meshData.LoadTextures(textureSettings);
            meshData.RenderMesh(true);
            hasChanged = false;
        }
        else if (hasMesh)
        {
            meshData.LoadTextures(textureSettings);
            meshData.RenderMesh(true);
        }
        isRendered= true;
        //EndRender();
    }
    public void HideMesh()
    {
        isRendered=false;
        meshData.RenderMesh(false);
    }

    public void ReloadTextures(TextureSettings textureSettings)
    {
        meshData.LoadTextures(textureSettings);
    }
}
