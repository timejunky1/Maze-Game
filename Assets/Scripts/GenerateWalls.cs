using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class GenerateWalls
{
    
}
public class MeshData
{
    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;

    List<Vector3> verticesL;
    List<Vector2> uvsL;
    List<int> trianglesL;

    Vector3 newTopLeft;
    Vector3 newTopRight;
    Vector3 newBotRight;
    Vector3 newBotLeft;

    int topLeft;
    int topRight;
    int botRight;
    int botLeft;

    Square curSquare;
    int height;
    int width;

    Material mat;
    Mesh mesh;
    GameObject squareObject;
    public MeshData()
    {
        verticesL = new List<Vector3>();
        uvsL = new List<Vector2>();
        trianglesL = new List<int>();
        squareObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        mesh = new Mesh();
    }
    void GetNewValues()
    {
        if (curSquare.sides[0])//top
        {
            newTopLeft += new Vector3(0, 0, -width);
            newTopRight += new Vector3(0, 0, -width);
        }
        if (curSquare.sides[1])//right
        {
            newTopRight += new Vector3(-width, 0, 0);
            newBotRight += new Vector3(-width, 0, 0);
        }
        if (curSquare.sides[2])
        {
            newBotRight += new Vector3(0, 0, +width);
            newBotLeft += new Vector3(0, 0, +width);
        }
        if (curSquare.sides[3])
        {
            newBotLeft += new Vector3(width, 0, 0);
            newTopLeft += new Vector3(width, 0, 0);
        }
    }
    void AddVertices()
    {
        verticesL.Clear();
        Vector3 verticy = newTopLeft;
        if (curSquare.extends[0])
        {
            verticy += new Vector3(-width, 0, width);
            AddVertLine(verticy);
            verticy += new Vector3(0, 0, -width);
            AddVertLine(verticy);
        }
        topLeft = verticesL.Count;
        AddVertLine(newTopLeft);
        verticy = newTopRight;
        if (curSquare.extends[2])
        {
            verticy += new Vector3(width, 0, width);
            AddVertLine(verticy);
            verticy += new Vector3(-width, 0, 0);
            AddVertLine(verticy);
        }
        topRight = verticesL.Count;
        AddVertLine(newTopRight);
        if (curSquare.extends[1])
        {
            verticy += new Vector3(width, 0, 0);
            AddVertLine(verticy);
            verticy += new Vector3(0, 0, width);
            AddVertLine(verticy);
        }
        verticy = newBotRight;
        if (curSquare.extends[4])
        {
            verticy += new Vector3(width, 0, -width);
            AddVertLine(verticy);
            verticy += new Vector3(0, 0, width);
            AddVertLine(verticy);
        }
        botRight = verticesL.Count;
        AddVertLine(newBotRight);
        if (curSquare.extends[3])
        {
            verticy += new Vector3(0, 0, -width);
            AddVertLine(verticy);
            verticy += new Vector3(width, 0, 0);
            AddVertLine(verticy);
        }
        verticy = newBotLeft;
        if (curSquare.extends[6])
        {
            verticy += new Vector3(-width, 0, -width);
            AddVertLine(verticy);
            verticy += new Vector3(width, 0, 0);
            AddVertLine(verticy);
        }
        botLeft = verticesL.Count;
        AddVertLine(newBotLeft);
        if (curSquare.extends[5])
        {
            verticy += new Vector3(-width, 0, 0);
            AddVertLine(verticy);
            verticy += new Vector3(0, 0, -width);
            AddVertLine(verticy);
        }
        verticy = newTopLeft;
        if (curSquare.extends[7])
        {
            verticy += new Vector3(0, 0, width);
            AddVertLine(verticy);
            verticy += new Vector3(-width, 0, 0);
            AddVertLine(verticy);
        }
    }
    void AddVertLine(Vector3 verticy)
    {
        verticesL.Add(verticy);
        verticesL.Add(verticy + height * Vector3.up);
    }
    void AddUvs()
    {
        foreach(Vector3 vert in verticesL)
        {
            uvsL.Add(vert);
        }
    }
    void CreateFloor()
    {
        trianglesL.Add(topLeft);
        trianglesL.Add(topRight);
        trianglesL.Add(botLeft);
        trianglesL.Add(botLeft);
        trianglesL.Add(topRight);
        trianglesL.Add(botRight);
    }

    void CreateWalls()
    {
        trianglesL.Clear();
        Debug.Log(topLeft + ", " + topRight + ", " + botRight + ", " + botLeft);
        int count = 0;
        int oldCount = 0;
        while(count < verticesL.Count/2 - 1)
        {
            Debug.Log(count);
            if (count == topLeft/2 && curSquare.sides[0] == false)
            {
                count = 1;
            }
            if (count >= topRight/2  && count < botRight/2)
            {
                count = EvaluateSide(curSquare.sides[1], curSquare.extends[1], curSquare.extends[4], topRight, botRight, count);
            }
            //if (count >= botRight / 2 && count < botLeft / 2)
            //{
            //    count = EvaluateSide(curSquare.sides[2], curSquare.extends[3], curSquare.extends[6], botRight, botLeft, count);
            //}
            if (count < oldCount)
            {
                break;
            }
            trianglesL.Add(count * 2);
            trianglesL.Add(count * 2 + 1);
            trianglesL.Add(count * 2 + 2);
            trianglesL.Add(count * 2 + 2);
            trianglesL.Add(count * 2 + 1);
            trianglesL.Add(count * 2 + 3);
            oldCount = count;
            count += 1;
        }
        if (curSquare.sides[3] && curSquare.extends[5] == false)
        {
            trianglesL.Add(count * 2);
            trianglesL.Add(count * 2 + 1);
            trianglesL.Add(0);
            trianglesL.Add(0);
            trianglesL.Add(count * 2 + 1);
            trianglesL.Add(1);
        }
    }
    int EvaluateSide(bool eSide, bool eExtention1, bool eExtention2, int pointValue1, int pointValue2, int count)
    {
        if (count == pointValue1/2 && eExtention1 == false && eSide == false)
        {
            Debug.Log("A");
            return NextVert(eExtention2, pointValue2) / 2;
        }
        if (count - 2 == pointValue1/2 && eSide == false)
        {
            Debug.Log("B");
            return NextVert(eExtention2, pointValue2) / 2;
        }
        return count;
    }
    int NextVert(bool evaluation, int value)
    {
        if (evaluation)
        {
            return value - 4;
        }
        else
        {
            return value;
        }
    }
    void ConvertLists()
    {
        int count = 0;
        vertices = new Vector3[verticesL.Count];
        foreach(Vector3 point in verticesL)
        {
            vertices[count] = point;
            count++;
        }
        count = 0;
        uvs= new Vector2[uvsL.Count];
        foreach(Vector2 point in uvsL)
        {
            uvs[count] = point;
            count++;
        }
        count = 0;
        triangles = new int[trianglesL.Count];
        foreach(int i in trianglesL)
        {
            triangles[count] = i;
            count++;
        }
    }

    public void CreateMesh(Square square, Material material, int wallHeight, int wallWidth)
    {
        curSquare = square;
        height = wallHeight;
        width = wallWidth;
        mat = material;

        newTopLeft = curSquare.corners[0];
        newTopRight = curSquare.corners[1];
        newBotRight = curSquare.corners[2];
        newBotLeft = curSquare.corners[3];

        GetNewValues();
        AddVertices();
        AddUvs();
        CreateWalls();
        CreateFloor();
        ConvertLists();

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public void RenderMesh()
    {
        squareObject.GetComponent<MeshFilter>().mesh = mesh;
        squareObject.GetComponent<MeshRenderer>().material = mat;
        squareObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void UpdateMesh(Square square, Material material, int wallHeight, int wallWidth)
    {
        CreateMesh(square, material, wallHeight, wallWidth);

        squareObject.GetComponent<MeshFilter>().mesh = mesh;
        squareObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        squareObject.GetComponent<MeshRenderer>().material = mat;
    }
}
