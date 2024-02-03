using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class MeshData
{
    int cubeWidth;
    ItemLocation[,,] items;
    public Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;
    string region;

    public Vector3[] newCorners;
    int[] cornerIndexes;

    bool[] sides;
    bool[] pillars;
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
        int counter = 0;
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
        if (pillars[0] && width>0)
        {
            Debug.Log("Pillar 0");
            AddVertLine(verticy + new Vector3(0, 0, -width));
            AddVertLine(verticy + new Vector3(width, 0, -width));
            cornerIndexes[0] = vertIndex;
            AddVertLine(verticy + new Vector3(width, 0, 0));
        }
        else
        {
            Debug.Log("Side 0");
            cornerIndexes[0] = vertIndex;
            AddVertLine(verticy);
        }
        
        verticy = newCorners[1];
        if (pillars[1] && width > 0)
        {
            Debug.Log("Pillar 1");
            AddVertLine(verticy + new Vector3(-width, 0, 0));
            AddVertLine(verticy + new Vector3(-width, 0, -width));
            cornerIndexes[1] = vertIndex;
            AddVertLine(verticy + new Vector3(0, 0, -width));
        }
        else
        {
            Debug.Log("Side 1");
            cornerIndexes[1] = vertIndex;
            AddVertLine(verticy);
        }
        
        verticy = newCorners[2];
        if (pillars[2] && width > 0)
        {
            Debug.Log("Pillar 2");
            AddVertLine(verticy + new Vector3(0, 0, width));
            AddVertLine(verticy + new Vector3(-width, 0, width));
            cornerIndexes[2] = vertIndex;
            AddVertLine(verticy + new Vector3(-width, 0, 0));
        }
        else
        {
            Debug.Log("Side 2");
            cornerIndexes[2] = vertIndex;
            AddVertLine(verticy);
        }

        verticy = newCorners[3];
        if (pillars[3] && width > 0)
        {
            Debug.Log("Pillar 3");
            AddVertLine(verticy + new Vector3(width, 0, 0));
            AddVertLine(verticy + new Vector3(width, 0, width));
            cornerIndexes[3] = vertIndex;
            AddVertLine(verticy + new Vector3(0, 0, width));
        }
        else
        {
            Debug.Log("Side 3");
            cornerIndexes[3] = vertIndex;
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
        int side = 0;
        int count = vertices.Length;
        Debug.Log($"Vertcies: {vertices.Length}");
        Debug.Log($"{cornerIndexes[0]}, {cornerIndexes[1]}, {cornerIndexes[2]}, {cornerIndexes[3]}");
        Debug.Log($"{pillars[0]}, {pillars[1]}, {pillars[2]}, {pillars[3]}");
        for (int i = 0; i < cornerIndexes.Length-1; i++)
        {
            GameObject o = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            o.GetComponent<Transform>().position = vertices[cornerIndexes[i]];
        }
        for (int i = 0; i < count-2; i+=2)
        {
            Debug.Log(i);
            try
            {
                if (i == 0 && cornerIndexes[side] == i)
                {
                    Debug.Log($"{side}: {sides[side]}: {i}");
                    bool s = sides[side];
                    side++;
                    if (!s)
                    {
                        Debug.Log("Continue");
                        continue;
                    }
                }
                if (cornerIndexes[side] == i)
                {
                    Debug.Log($"{side}: {sides[side]}: {i}");
                    bool s = sides[side];
                    side++;
                    if (!s)
                    {
                        Debug.Log("Continue");
                        continue;
                    }
                }
                Debug.Log($"index: {i}");
                triangles[triangleIndex] = i;
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = i + 2;
                triangles[triangleIndex + 3] = i + 2;
                triangles[triangleIndex + 4] = i + 1;
                triangles[triangleIndex + 5] = i + 3;
                triangleIndex = triangleIndex + 6;

            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

        }
        if (sides[3])
        {
            triangles[triangleIndex] = (count - 2);
            triangles[triangleIndex + 1] = (count - 1);
            triangles[triangleIndex + 2] = (0);
            triangles[triangleIndex + 3] = (0);
            triangles[triangleIndex + 4] = (count - 1);
            triangles[triangleIndex + 5] = (1);
            triangleIndex = triangleIndex + 6;
        }
    }
    public void CreateMeshData(SquareData square,MazeWallsSettings wallSettings)
    {
        vertIndex = 0;
        triangleIndex= 0;
        this.sides = square.sides;
        this.pillars = square.pillars;
        height = wallSettings.wallHeight;
        width = wallSettings.wallWidth;
        cubeWidth = (int)(square.corners[1].x - square.corners[0].x);
        this.region = square.region;
        items = new ItemLocation[cubeWidth,height,cubeWidth];

        newCorners[0] = square.corners[0];
        newCorners[1] = square.corners[1];
        newCorners[2] = square.corners[2];
        newCorners[3] = square.corners[3];
        if (square.isBoss)
        {
            Debug.Log("Add Boss Statue");
            AddAddition("BossStatue", new Vector3(0,0,0), new Quaternion(0, 0, 0, 0));
        }


        InitializeArays();
        GetNewValues();
        AddVertices();
        AddUvs();
        CreateWalls();
        //CreateFloor();
    }

    public void InitializeArays()
    {
        int count = 8;
        if (width > 0)
        {
            for (int i = 0; i < pillars.Length; i++)
            {
                if (pillars[i])
                {
                    count = count + 4;
                }
            }
        }
        vertices = new Vector3[count];
        uvs = new Vector2[count];
        count = 0;
        for (int i = 0; i < sides.Length; i++)
        {
            count += 18;
            //if (sides[i])
            //{
            //    count+=6;
            //}
            //if (pillars[i])
            //{
            //    count += 12;
            //}
        }
        triangles = new int[count];
    }

    public void AddAddition(string name, Vector3 position, Quaternion rotation)
    {
        Debug.Log($"position x: {position.x} y: {position.y} z: {position.z}");
        Vector3Int location = new Vector3Int((int)(position.x + cubeWidth/2), (int)(position.y), (int)(position.z + cubeWidth / 2));
        Debug.Log($"location x: {location.x} y: {location.y} z: {location.z}");
        if (items[location.x, location.y, location.z].exists) return;
        items[location.x, location.y, location.z] = new ItemLocation(name, position, rotation);
    }
    public void RenderAdditions(TextureSettings textureSettings, string mainFile)
    {
        Debug.Log("Render Additions");
        for(int x = 0;x < items.GetLength(0);x++)
        {
            for(int y = 0;y < items.GetLength(1); y++)
            {
                for (int z = 0; z < items.GetLength(2); z++)
                {
                    if (items[x, y, z].exists)
                    {
                        if(items[x, y, z].exists)
                        {
                            if (items[x,y,z].o != null)
                            {
                                Debug.Log("Set Active");
                                items[x, y, z].o.SetActive(true);
                                continue;
                            }
                            //GameObject prefab = Resources.Load<GameObject>($"{mainFile}{items[i].name}");
                            items[x, y, z].o = UnityEngine.Object.Instantiate(textureSettings.bossStatue, items[x, y, z].position, items[x, y, z].rotation, squareObject.transform);
                            items[x, y, z].o.SetActive(true);
                        }
                    }
                }
            }
            
        }
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
        //Debug.Log(str
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
        try
        {
            mat = textureSettings.materials[region];
        }
        catch (Exception ex)
        {
            Debug.Log(region);
            Debug.Log("Texture Problem: " + ex.Message);
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
    struct ItemLocation
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public GameObject o;
        public bool exists;

        public ItemLocation(string name, Vector3 position, Quaternion rotation)
        {
            o = null;
            this.name = name;
            this.position = position;
            this.rotation = rotation;
            exists = true;
        }
    }
}
