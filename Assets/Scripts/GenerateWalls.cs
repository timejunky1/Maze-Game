using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public class MeshData
{
    int cubeWidth;
    Vector3[,,] landscapeVertMatrix;
    public List<Vector3> landscapeVerts;
    List<Vector2> landscapeUvs;
    public List<int> landscapeTriangles;
    public int[,,] spaces;
    ItemLocation[,,] items;
    public Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;
    string region;

    public Vector3[] newCorners;
    public Vector3[] cubeCorners;
    int[] cornerIndexes;

    bool[] sides;
    bool[] pillars;
    int height;
    int width;
    public Vector3Int centre;
    int triangleIndex;
    int vertIndex;

    Material mat;
    Mesh mesh;
    GameObject squareObject;
    GameObject floor;
    GameObject walls;
    GameObject landscape;

    bool hasLandscape;
    bool hasFloor;
    bool hasmesh;
    public MeshData()
    {
        landscapeVerts = new List<Vector3>();
        landscapeTriangles = new List<int>();
        landscapeUvs = new List<Vector2>();
        newCorners = new Vector3[4];
        cubeCorners = new Vector3[4];
        cornerIndexes = new int[4];
        hasFloor = false;
        hasmesh = false;
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
        if (pillars[0] && width>0)
        {
            AddVertLine(verticy + new Vector3(0, 0, -width));
            AddVertLine(verticy + new Vector3(width, 0, -width));
            cornerIndexes[0] = vertIndex;
            AddVertLine(verticy + new Vector3(width, 0, 0));
        }
        else
        {
            cornerIndexes[0] = vertIndex;
            AddVertLine(verticy);
        }
        
        verticy = newCorners[1];
        if (pillars[1] && width > 0)
        {
            AddVertLine(verticy + new Vector3(-width, 0, 0));
            AddVertLine(verticy + new Vector3(-width, 0, -width));
            cornerIndexes[1] = vertIndex;
            AddVertLine(verticy + new Vector3(0, 0, -width));
        }
        else
        {
            cornerIndexes[1] = vertIndex;
            AddVertLine(verticy);
        }
        
        verticy = newCorners[2];
        if (pillars[2] && width > 0)
        {
            AddVertLine(verticy + new Vector3(0, 0, width));
            AddVertLine(verticy + new Vector3(-width, 0, width));
            cornerIndexes[2] = vertIndex;
            AddVertLine(verticy + new Vector3(-width, 0, 0));
        }
        else
        {
            cornerIndexes[2] = vertIndex;
            AddVertLine(verticy);
        }

        verticy = newCorners[3];
        if (pillars[3] && width > 0)
        {
            AddVertLine(verticy + new Vector3(width, 0, 0));
            AddVertLine(verticy + new Vector3(width, 0, width));
            cornerIndexes[3] = vertIndex;
            AddVertLine(verticy + new Vector3(0, 0, width));
        }
        else
        {
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
    void CalculateLandscape()
    {
        int count = 0;
        for (int x = 0; x < spaces.GetLength(0); x++)
        {
            //int r = UnityEngine.Random.Range(0, 20);
            for (int z = 0; z < spaces.GetLength(2); z++)
            {
                if((x>cubeWidth-width || x < width || z>cubeWidth-width || z < width))
                {
                    Debug.Log("Wall");
                    continue;
                }
                int r = 2;
                for (int y = 0; y < spaces.GetLength(1); y++)
                {
                    if(x>5 && x < 15 && z > 5 && z < 15)
                    {
                        r = 6;
                    }
                    if (y == r)
                    {
                        float xo = (centre.x - cubeWidth / 2) + x;
                        float zo = (centre.z - cubeWidth / 2) + z;
                        float yo = centre.y + y;
                        spaces[x, y, z] = 2;

                        landscapeVerts.Add(new Vector3(xo - 0.5f, yo - 0.5f, zo + 0.5f));
                        landscapeVerts.Add(new Vector3(xo + 0.5f, yo - 0.5f, zo + 0.5f));
                        landscapeVerts.Add(new Vector3(xo + 0.5f, yo - 0.5f, zo - 0.5f));
                        landscapeVerts.Add(new Vector3(xo - 0.5f, yo - 0.5f, zo - 0.5f));
                        int vIndex = landscapeVerts.Count-1;
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 3].x, landscapeVerts[vIndex - 3].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 2].x, landscapeVerts[vIndex - 2].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 1].x, landscapeVerts[vIndex - 1].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex].x, landscapeVerts[vIndex].y));
                        int tIndex = ((landscapeVerts.Count/2)*3)-1;
                        landscapeTriangles.Add(vIndex - 3);
                        landscapeTriangles.Add(vIndex - 2);
                        landscapeTriangles.Add(vIndex);
                        landscapeTriangles.Add(vIndex);
                        landscapeTriangles.Add(vIndex - 2);
                        landscapeTriangles.Add(vIndex - 1);
                    }
                    if (y < r)
                    {
                        spaces[x, y, z] = 1;
                    }
                }
            }
        }
        
        //Z wise sweep
        int previous = 10;
        for (int y = 0; y < spaces.GetLength(1); y++)
        {
            for (int x = 0; x < spaces.GetLength(0); x++)
            {
                for (int z = 0; z < spaces.GetLength(2); z++)
                {
                    if ((previous == 0 || previous == 2) && (spaces[x, y, z] == 1 || spaces[x, y, z] == 3))
                    {
                        previous = spaces[x, y, z];
                        spaces[x, y, z] = 3;

                        float xo = (centre.x - cubeWidth / 2) + x;
                        float zo = (centre.z - cubeWidth / 2) + z;
                        float yo = centre.y + y;
                        //spaces[x, y, z] = 2;

                        landscapeVerts.Add(new Vector3(xo - 0.5f, yo + 0.5f, zo - 0.5f));
                        landscapeVerts.Add(new Vector3(xo + 0.5f, yo + 0.5f, zo - 0.5f));
                        landscapeVerts.Add(new Vector3(xo + 0.5f, yo - 0.5f, zo - 0.5f));
                        landscapeVerts.Add(new Vector3(xo - 0.5f, yo - 0.5f, zo - 0.5f));
                        int vIndex = landscapeVerts.Count - 1;
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 3].x, landscapeVerts[vIndex - 3].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 2].x, landscapeVerts[vIndex - 2].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 1].x, landscapeVerts[vIndex - 1].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex].x, landscapeVerts[vIndex].y));
                        int tIndex = ((landscapeVerts.Count / 2) * 3) - 1;
                        landscapeTriangles.Add(vIndex - 3);
                        landscapeTriangles.Add(vIndex - 2);
                        landscapeTriangles.Add(vIndex);
                        landscapeTriangles.Add(vIndex);
                        landscapeTriangles.Add(vIndex - 2);
                        landscapeTriangles.Add(vIndex - 1);
                        continue;
                    }
                    previous = spaces[x, y, z];
                }
            }
        }
        //Z wise sweep backwards
        for (int y = 0; y < spaces.GetLength(1); y++)
        {
            for (int x = spaces.GetLength(0) - 1; x >= 0; x--)
            {
                for (int z = spaces.GetLength(2) - 1; z >= 0; z--)
                {
                    if ((previous == 0 || previous == 2) && (spaces[x, y, z] == 1 || spaces[x, y, z] == 3))
                    {
                        Debug.Log("Rock");
                        previous = spaces[x, y, z];
                        spaces[x, y, z] = 3;

                        float xo = (centre.x - cubeWidth / 2) + x;
                        float zo = (centre.z - cubeWidth / 2) + z;
                        float yo = centre.y + y;
                        spaces[x, y, z] = 2;

                        landscapeVerts.Add(new Vector3(xo - 0.5f, yo + 0.5f, zo + 0.5f));
                        landscapeVerts.Add(new Vector3(xo + 0.5f, yo + 0.5f, zo + 0.5f));
                        landscapeVerts.Add(new Vector3(xo + 0.5f, yo - 0.5f, zo + 0.5f));
                        landscapeVerts.Add(new Vector3(xo - 0.5f, yo - 0.5f, zo + 0.5f));
                        int vIndex = landscapeVerts.Count - 1;
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 3].x, landscapeVerts[vIndex - 3].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 2].x, landscapeVerts[vIndex - 2].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 1].x, landscapeVerts[vIndex - 1].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex].x, landscapeVerts[vIndex].y));
                        int tIndex = ((landscapeVerts.Count / 2) * 3) - 1;
                        landscapeTriangles.Add(vIndex);//0
                        landscapeTriangles.Add(vIndex - 2);//1
                        landscapeTriangles.Add(vIndex -3);//3
                        landscapeTriangles.Add(vIndex -1);//3
                        landscapeTriangles.Add(vIndex - 2);//1
                        landscapeTriangles.Add(vIndex);//0
                        continue;
                    }
                    previous = spaces[x, y, z];
                }
            }
        }

        //X wise sweep
        for (int y = 0; y < spaces.GetLength(1); y++)
        {
            for (int z = 0; z < spaces.GetLength(2); z++)
            {
                for (int x = 0; x < spaces.GetLength(0); x++)
                {
                    if ((previous == 0 || previous == 2) && (spaces[x, y, z] == 1 || spaces[x,y,z] == 3))
                    {
                        previous = spaces[x, y, z];
                        spaces[x, y, z] = 3;

                        float xo = (centre.x - cubeWidth / 2) + x;
                        float zo = (centre.z - cubeWidth / 2) + z;
                        float yo = centre.y + y;
                        spaces[x, y, z] = 2;

                        landscapeVerts.Add(new Vector3(xo - 0.5f, yo + 0.5f, zo + 0.5f));
                        landscapeVerts.Add(new Vector3(xo - 0.5f, yo + 0.5f, zo - 0.5f));
                        landscapeVerts.Add(new Vector3(xo - 0.5f, yo - 0.5f, zo - 0.5f));
                        landscapeVerts.Add(new Vector3(xo - 0.5f, yo - 0.5f, zo + 0.5f));
                        int vIndex = landscapeVerts.Count - 1;
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 3].x, landscapeVerts[vIndex - 3].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 2].x, landscapeVerts[vIndex - 2].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 1].x, landscapeVerts[vIndex - 1].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex].x, landscapeVerts[vIndex].y));
                        int tIndex = ((landscapeVerts.Count / 2) * 3) - 1;
                        landscapeTriangles.Add(vIndex - 3);
                        landscapeTriangles.Add(vIndex - 2);
                        landscapeTriangles.Add(vIndex);
                        landscapeTriangles.Add(vIndex);
                        landscapeTriangles.Add(vIndex - 2);
                        landscapeTriangles.Add(vIndex - 1);
                    }
                    else
                    {
                        previous = spaces[x, y, z];
                    }
                }
            }
        }
        //X wise sweep backwards
        for (int y = 0; y < spaces.GetLength(1); y++)
        {
            for (int z = spaces.GetLength(2) - 1; z >= 0; z--)
            {
                for (int x = spaces.GetLength(0) - 1; x >= 0; x--)
                {
                    if ((previous == 0 || previous == 2) && (spaces[x, y, z] == 1 || spaces[x, y, z] == 3))
                    {
                        Debug.Log("Rock");
                        previous = spaces[x, y, z];
                        spaces[x, y, z] = 3;

                        float xo = (centre.x - cubeWidth / 2) + x;
                        float zo = (centre.z - cubeWidth / 2) + z;
                        float yo = centre.y + y;
                        spaces[x, y, z] = 2;

                        landscapeVerts.Add(new Vector3(xo + 0.5f, yo + 0.5f, zo + 0.5f));
                        landscapeVerts.Add(new Vector3(xo + 0.5f, yo + 0.5f, zo - 0.5f));
                        landscapeVerts.Add(new Vector3(xo + 0.5f, yo - 0.5f, zo - 0.5f));
                        landscapeVerts.Add(new Vector3(xo + 0.5f, yo - 0.5f, zo + 0.5f));
                        int vIndex = landscapeVerts.Count - 1;
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 3].x, landscapeVerts[vIndex - 3].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 2].x, landscapeVerts[vIndex - 2].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex - 1].x, landscapeVerts[vIndex - 1].y));
                        landscapeUvs.Add(new Vector3(landscapeVerts[vIndex].x, landscapeVerts[vIndex].y));
                        int tIndex = ((landscapeVerts.Count / 2) * 3) - 1;
                        landscapeTriangles.Add(vIndex);//0
                        landscapeTriangles.Add(vIndex - 2);//1
                        landscapeTriangles.Add(vIndex - 3);//3
                        landscapeTriangles.Add(vIndex - 1);//3
                        landscapeTriangles.Add(vIndex - 2);//1
                        landscapeTriangles.Add(vIndex);//0
                        continue;
                    }
                    previous = spaces[x, y, z];
                }
            }
        }
    }

    void CreateLandscape()
    {
        Debug.Log("Verts: "+landscapeVerts.Count);
        Debug.Log("Uvs: " + landscapeUvs.Count);
        Debug.Log("Triangles: " + landscapeTriangles.Count);

        landscape = new GameObject("Landscape", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        landscape.transform.parent = squareObject.transform;
        Mesh l = new Mesh();
        l.vertices = landscapeVerts.ToArray();
        l.uv = landscapeUvs.ToArray();
        l.triangles = landscapeTriangles.ToArray();
        l.RecalculateNormals();
        landscape.GetComponent<MeshFilter>().mesh = l;
        landscape.GetComponent<MeshCollider>().sharedMesh = l;
        hasLandscape = true;
    }
    void CreateFloor()
    {
        int[] floorT = new int[6];
        Vector3[] floorV = cubeCorners;
        Vector2[] floorUV = new Vector2[cubeCorners.Length];
        for(int i = 0; i < cubeCorners.Length; i++)
        {
            floorUV[i] = new Vector2(cubeCorners[i].x, cubeCorners[i].y);
        }

        floorT[0] = 0;
        floorT[1] = 1;
        floorT[2] = 3;
        floorT[3] = 3;
        floorT[4] = 1;
        floorT[5] = 2;

        floor = new GameObject("Floor", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        floor.transform.parent = squareObject.transform;
        Mesh f = new Mesh();
        f.Clear();
        f.vertices = floorV;
        f.uv = floorUV;
        f.triangles = floorT;
        f.RecalculateNormals();
        floor.GetComponent<MeshFilter>().mesh = f;
        floor.GetComponent<MeshCollider>().sharedMesh = f;
        hasFloor = true;

    }
    void CreateWalls()
    {
        int side = 0;
        int count = vertices.Length;
        for (int i = 0; i < count-2; i+=2)
        {
            try
            {
                if (i == 0 && cornerIndexes[side] == i)
                {
                    bool s = sides[side];
                    side++;
                    if (!s)
                    {
                        continue;
                    }
                }
                if (cornerIndexes[side] == i)
                {
                    bool s = sides[side];
                    side++;
                    if (!s)
                    {
                        continue;
                    }
                }
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
        centre = square.location;
        vertIndex = 0;
        triangleIndex= 0;
        this.sides = square.sides;
        this.pillars = square.pillars;
        height = wallSettings.wallHeight;
        width = wallSettings.wallWidth;
        cubeWidth = (int)(square.corners[1].x - square.corners[0].x);
        this.region = square.region;
        items = new ItemLocation[cubeWidth,height,cubeWidth];
        spaces = new int[cubeWidth, height, cubeWidth];


        for (int i = 0; i < square.corners.Length; i++)
        {
            newCorners[i] = square.corners[i];
            cubeCorners[i] = square.corners[i];
        }

        if (square.isBoss)
        {
            AddAddition("BossStatue", new Vector3(0,0,0), new Quaternion(0, 0, 0, 0));
        }

        //CreateChain(new Vector3(1, 1, 1), new Vector3(10, 10, 10));

        InitializeArays();
        GetNewValues();
        AddVertices();
        AddUvs();
        CalculateLandscape();
        CreateWalls();
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
            if (sides[i])
            {
                count += 6;
            }
            if (pillars[i])
            {
                count += 12;
            }
        }
        triangles = new int[count];
    }

    public void AddAddition(string name, Vector3 position, Quaternion rotation)
    {
        Vector3Int location = new Vector3Int((int)(position.x + cubeWidth/2), (int)(position.y), (int)(position.z + cubeWidth / 2));
        if (items[location.x, location.y, location.z].exists) return;
        if (spaces[location.x, location.y, location.z] == 0)
        {
            items[location.x, location.y, location.z] = new ItemLocation(name, position, rotation);
            spaces[location.x, location.y, location.z] = 1;
        }
        else
        {
            Debug.Log("Space is already Aucupied");
        }
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
                            else if(items[x, y, z].name == "BossStatue")
                            {
                                items[x, y, z].o = UnityEngine.Object.Instantiate(textureSettings.bossStatue, items[x, y, z].position, items[x, y, z].rotation, squareObject.transform);
                            }
                            else if (items[x, y, z].name == "Chain")
                            {
                                items[x, y, z].o = UnityEngine.Object.Instantiate(textureSettings.chain, items[x, y, z].position, items[x, y, z].rotation, squareObject.transform);
                            }
                            items[x, y, z].o.SetActive(true);
                            //GameObject prefab = Resources.Load<GameObject>($"{mainFile}{items[i].name}");
                        }
                    }
                }
            }
            
        }
    }
    void calculateNormals()
    {

    }

    public void CreateChain(Vector3 pointA, Vector3 pointB)
    {
        int xstart = (int)Math.Min(pointA.x, pointB.x);
        int ystart = (int)Math.Min(pointA.y, pointB.y);
        int zstart = (int)Math.Min(pointA.z, pointB.z);
        int lengthx = (int)Math.Abs(pointA.y - pointB.y);
        int lengthy = (int)Math.Abs(pointA.y - pointB.y);
        int lengthz = (int)Math.Abs(pointA.y - pointB.y);
        Vector3Int point = new Vector3Int(xstart, ystart, zstart);
        AddAddition("Chain", point, Quaternion.identity);
        for (int x = xstart; x < lengthx; x++)
        {
            point += new Vector3Int(1, 0, 0);
            AddAddition("Chain", point, Quaternion.identity);
        }
        for (int y = xstart; y < lengthx; y++)
        {
            point += new Vector3Int(0, 1, 0);
            AddAddition("Chain", point, Quaternion.identity);
        }
        for (int z = xstart; z < lengthx; z++)
        {
            point += new Vector3Int(0, 0, 1);
            AddAddition("Chain", point, Quaternion.identity);
        }
    }

    public void CreateMesh(GameObject parent)
    {
        Debug.Log("Create Mesh");
        Debug.Log($"{vertices.Length}");
        if(squareObject == null) { 
            squareObject = new GameObject();
            squareObject.transform.parent = parent.transform;
        }
        if (triangles.Length > 0)
        {
            walls = new GameObject("Walls", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
            walls.transform.parent = squareObject.transform;
            mesh = new Mesh();
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            walls.GetComponent<MeshFilter>().mesh = mesh;
            walls.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
        if(!hasFloor)
        {
            CreateFloor();
        }
        if(!hasLandscape)
        {
            CreateLandscape();
        }
        hasmesh = true;
        
    }
    public bool HasMesh()
    {
        return (hasmesh);
    }
    public void RenderMesh(bool b)
    {
        squareObject.SetActive(b);
        floor.SetActive(b);
        landscape.SetActive(b);
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

        if(walls != null)walls.GetComponent<MeshRenderer>().material = mat;
        if(floor != null) floor.GetComponent<MeshRenderer>().material = mat;
    }
    public void UpdateMesh()
    {
        Debug.Log("Update Mesh");
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        walls.GetComponent<MeshFilter>().mesh = mesh;
        walls.GetComponent<MeshCollider>().sharedMesh = mesh;
        walls.GetComponent<MeshRenderer>().material = mat;

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
