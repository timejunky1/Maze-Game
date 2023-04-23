using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MeshData
{
    public Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;
    string region;

    public Vector3[] newCorners;
    int[] cornerIndexes;

    bool[] sides;
    bool[] extends;
    int height;
    int width;
    int triangleIndex;
    int vertIndex;

    Material mat;
    Mesh mesh;
    GameObject squareObject;
    public MeshData()
    {
        newCorners = new Vector3[4];
        cornerIndexes = new int[4];
        triangleIndex = 0;
        vertIndex= 0;
    }
    void GetNewValues()
    {
        if (sides[0])//top
        {
            newCorners[0] += new Vector3(0, 0, -width);
            newCorners[1] += new Vector3(0, 0, -width);
        }
        if (sides[1])//right
        {
            newCorners[1] += new Vector3(-width, 0, 0);
            newCorners[2] += new Vector3(-width, 0, 0);
        }
        if (sides[2])//bot
        {
            newCorners[2] += new Vector3(0, 0, +width);
            newCorners[3] += new Vector3(0, 0, +width);
        }
        if (sides[3])//left
        {
            newCorners[3] += new Vector3(width, 0, 0);
            newCorners[0] += new Vector3(width, 0, 0);
        }
    }
    void AddVertices()
    {
        Vector3 verticy = newCorners[0];
        if (extends[0] && width>0)
        {
            verticy += new Vector3(-width, 0, width);
            AddVertLine(verticy);
            verticy += new Vector3(0, 0, -width);
            AddVertLine(verticy);
        }
        cornerIndexes[0] = vertIndex;
        AddVertLine(newCorners[0]);
        verticy = newCorners[1];
        if (extends[2] && width > 0)
        {
            verticy += new Vector3(width, 0, width);
            AddVertLine(verticy);
            verticy += new Vector3(-width, 0, 0);
            AddVertLine(verticy);
        }
        cornerIndexes[1] = vertIndex;
        AddVertLine(newCorners[1]);
        if (extends[1] && width > 0)
        {
            verticy += new Vector3(width, 0, 0);
            AddVertLine(verticy);
            verticy += new Vector3(0, 0, width);
            AddVertLine(verticy);
        }
        verticy = newCorners[2];
        if (extends[4] && width > 0)
        {
            verticy += new Vector3(width, 0, -width);
            AddVertLine(verticy);
            verticy += new Vector3(0, 0, width);
            AddVertLine(verticy);
        }
        cornerIndexes[2] = vertIndex;
        AddVertLine(newCorners[2]);
        if (extends[3] && width > 0)
        {
            verticy += new Vector3(0, 0, -width);
            AddVertLine(verticy);
            verticy += new Vector3(width, 0, 0);
            AddVertLine(verticy);
        }
        verticy = newCorners[3];
        if (extends[6] && width > 0)
        {
            verticy += new Vector3(-width, 0, -width);
            AddVertLine(verticy);
            verticy += new Vector3(width, 0, 0);
            AddVertLine(verticy);
        }
        cornerIndexes[3] = vertIndex;
        AddVertLine(newCorners[3]);
        if (extends[5] && width > 0)
        {
            verticy += new Vector3(-width, 0, 0);
            AddVertLine(verticy);
            verticy += new Vector3(0, 0, -width);
            AddVertLine(verticy);
        }
        verticy = newCorners[0];
        if (extends[7] && width > 0)
        {
            verticy += new Vector3(0, 0, width);
            AddVertLine(verticy);
            verticy += new Vector3(-width, 0, 0);
            AddVertLine(verticy);
        }
    }
    void AddVertLine(Vector3 verticy)
    {
        vertices[vertIndex] = verticy;
        vertices[vertIndex + 1] = verticy + height * Vector3.up;
        vertIndex = vertIndex + 2;
    }
    void AddUvs()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = vertices[i];
        }
    }

    void CreateFloor()
    {
        triangles[triangleIndex] = cornerIndexes[0];
        triangles[triangleIndex + 1] = cornerIndexes[1];
        triangles[triangleIndex + 2] = cornerIndexes[3];
        triangles[triangleIndex + 3] = cornerIndexes[3];
        triangles[triangleIndex + 4] = cornerIndexes[1];
        triangles[triangleIndex + 5] = cornerIndexes[2];
        triangleIndex = triangleIndex + 6;
    }
    void CreateWalls()
    {
        int count = vertices.Length;
        for (int i = 0; i < count/2 - 1; i++)
        {
                if(i == 0 && !sides[0])
                {
                    continue;
                }
                else if (EvaluateSide(sides[1], extends[1], cornerIndexes[1], i))
                {
                    continue;
                }
                else if(EvaluateSide(sides[2], extends[3], cornerIndexes[2], i))
                {
                    continue;
                }
                triangles[triangleIndex] = i * 2;
                triangles[triangleIndex + 1] = i * 2 + 1;
                triangles[triangleIndex + 2] = i * 2 + 2;
                triangles[triangleIndex + 3] = i * 2 + 2;
                triangles[triangleIndex + 4] = i * 2 + 1;
                triangles[triangleIndex + 5] = i * 2 + 3;
                triangleIndex = triangleIndex + 6;
        }
        if (sides[3] && extends[7] == false)
        {
            triangles[triangleIndex] = (count - 2);
            triangles[triangleIndex + 1] = (count - 1);
            triangles[triangleIndex + 2] = (0);
            triangles[triangleIndex + 3] = (0);
            triangles[triangleIndex + 4] = (count - 1);
            triangles[triangleIndex + 5] = (1);
            triangleIndex= triangleIndex + 6;
        }
    }
    bool EvaluateSide(bool eSide, bool eExtention,int pointValue,int count)
    {
        if (count == pointValue/2 && !eExtention && !eSide)
        {
            return true;
        }
        if (count - 2 == pointValue/2 && !eSide && eExtention)
        {
            return true;
        }
        return false;
    }

    public void CreateMeshData(bool[] sides, bool[] extends, Vector3[] corners ,string region ,MazeWallsSettings wallSettings)
    {
        vertIndex= 0;
        triangleIndex= 0;
        this.sides = sides;
        this.extends = extends;
        height = wallSettings.wallHeight;
        width = wallSettings.wallWidth;
        this.region = region;

        newCorners[0] = corners[0];
        newCorners[1] = corners[1];
        newCorners[2] = corners[2];
        newCorners[3] = corners[3];

        InitializeArays();
        GetNewValues();
        AddVertices();
        AddUvs();
        CreateWalls();
        CreateFloor();
    }

    public void InitializeArays()
    {
        int count = 8;
        if (width > 0)
        {
            for (int i = 0; i < extends.Length; i++)
            {
                if (extends[i])
                {
                    count = count + 4;
                }
            }
        }
        vertices = new Vector3[count];
        uvs = new Vector2[count];
        count = (count / 2) + 1;
        for (int i = 0; i < sides.Length; i++)
        {
            if (!sides[i])
            {
                count--;
            }
        }
        if (sides[3] && extends[7] == false)
        {
            count++;
        }
        triangles = new int[count * 6];
    }

    void calculateNormals()
    {

    }
    public void CreateMesh(GameObject parent)
    {
        //string str = "triangles...";
        //for (int i = 0; i < triangles.Length; i++)
        //{
        //    str += triangles[i].ToString() + ",";
        //}
        //Debug.Log(str);
        //str = "Verts...";
        //for (int i = 0; i < vertices.Length; i++)
        //{
        //    str += vertices[i].ToString() + ",";
        //}
        //Debug.Log(str);
        if (triangles.Length > 0)
        {
            squareObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            squareObject.transform.parent = parent.transform;
            mesh = new Mesh();
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            squareObject.GetComponent<MeshFilter>().mesh = mesh;
            squareObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
    public bool HasMesh()
    {
        return (squareObject != null);
    }
    public void RenderMesh(bool b)
    {
        squareObject.SetActive(b);
    }
    public void LoadTextures(TextureSettings textureSettings)//somehow this loads with a seperate texturesettings instance throughout the rendering proces
    {
        Debug.Log(region);
        if (textureSettings.regionNames.Contains(region))
        {
            try
            {
                mat = textureSettings.materials[region];
            }
            catch (Exception ex)
            {
                Debug.Log("Texture Problem: " + ex.Message);
                mat = textureSettings.defaultmaterial;
            }
        }
        else
        {
            mat = textureSettings.defaultmaterial;
        }
        
        squareObject.GetComponent<MeshRenderer>().material = mat;
    }
    public void UpdateMesh()
    {
        Debug.Log("Update Mesh");
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        squareObject.GetComponent<MeshFilter>().mesh = mesh;
        squareObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        squareObject.GetComponent<MeshRenderer>().material = mat;

    }
}
