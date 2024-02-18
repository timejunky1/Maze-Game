using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;

public class ShowEditorsmaze : MonoBehaviour
{
    public EditorCubeSettings cubeSettings;
    public MazeWallsSettings wallsSettings;
    public MazeSettings mazeSettings;
    public TextureSettings textureSettings;
    public GameObject CubeParent;
    public GameObject MazeParent;
    bool cube = false;
    bool newMaze = true;
    bool update;

    List<Vector2Int> placesOfIntrest = new List<Vector2Int>();
    Vector3Int[] bossLocations;
    List<Vector2Int> places;
    Mazegeneration maze;
    SquareData square;
    private void Awake()
    {
        places = new List<Vector2Int>();
        update= false;
        textureSettings.ReloadDicts();
        OnValuesUpdated();
    }
    void OnValuesUpdated()//Does not want to destroy the walls
    {
        if (!update)
        {
            newMaze = mazeSettings.newMaze;
        }
        cube = false;
        CubeParent.SetActive(false);
        MazeParent.SetActive(true);
        if (!HasMesh(MazeParent) || newMaze)
        {
            maze = new Mazegeneration(mazeSettings, wallsSettings, textureSettings);
            maze.LoadMaze();
            int count = MazeParent.transform.childCount;
            for (int i = 0; i < count; i++)
            {
                DestroyImmediate(MazeParent.transform.GetChild(0).gameObject);
            }
            maze.SetLockedSquares();
            LoadMaze();
            places = maze.visitedSquares;
            maze.CalculatePlacesOfIntrest(maze.placesOfIntrest, textureSettings);
            bossLocations = maze.bossLocations;
        }
        if(maze!= null)
        {
            RenderMaze();
        }
    }
    void OnCubesValuesUpdated()
    {
        cube = true;
        CubeParent.SetActive(true);
        int childCount = CubeParent.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            DestroyImmediate(CubeParent.transform.GetChild(0).gameObject);
        }
        RenderCube(textureSettings);
        MazeParent.SetActive(false);
    }
    bool HasMesh(GameObject parent)
    {
        if(parent.transform.childCount > 0)
        {
            return true;
        }
        return false;
    }
    void OnTextureValuesUpdated()
    {
        textureSettings.ReloadDicts();
        if (!cube)
        {
            if (HasMesh(MazeParent))
            {
                RenderMaze();
            }
        }
        else if (cube)
        {
            LoadCubeTextures();
        }
    }
    void OnWallSettingsUpdated()
    {
        update = true;
        newMaze = false;
        if(cube)
        {
            OnCubesValuesUpdated();
            return;
        }
        foreach(Vector2Int point in places)
        {
            Mazegeneration.matrix[point.y, point.x].GetMeshData(wallsSettings);
        }
        OnValuesUpdated();
        update= false;
    }

    private void OnValidate()
    {
        update= false;
        if (cubeSettings != null)
        {
            cubeSettings.OnValuesUpdated -= OnCubesValuesUpdated;
            cubeSettings.OnValuesUpdated += OnCubesValuesUpdated;
        }
        if (wallsSettings != null)
        {
            wallsSettings.OnValuesUpdated -= OnWallSettingsUpdated;
            wallsSettings.OnValuesUpdated += OnWallSettingsUpdated;
        }
        if (mazeSettings != null)
        {
            mazeSettings.OnValuesUpdated -= OnValuesUpdated;
            mazeSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureSettings != null)
        {
            textureSettings.OnValuesUpdated -= OnTextureValuesUpdated;
            textureSettings.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    void LoadCubeTextures()
    {
        square.RenderMesh(textureSettings, CubeParent);
    }
    void RenderMaze()
    {
        foreach (Vector2Int point in places)
        {
            Mazegeneration.matrix[point.x, point.y].RenderMesh(textureSettings, MazeParent);
        }
    }

    void RenderCube(TextureSettings textureSettings)//destroy the initial cube
    {
        Debug.Log("RenderCube");
        square = new SquareData(cubeSettings.cubeSize, new Vector3Int(0,0,0));
        square.region = textureSettings.regionNames[cubeSettings.region];
        square.regionValue = cubeSettings.regionValue;
        for (int i = 0; i < square.pillars.Length; i++)
        {
            square.sides[i] = false;
            square.pillars[i] = true;
        }
        if (cubeSettings.Top)
        {
            square.sides[0] = true;
            square.pillars[0] = false;
            square.pillars[1] = false;
        }
        if (cubeSettings.Right)
        {
            square.sides[1] = true;
            square.pillars[1] = false;
            square.pillars[2] = false;
        }
        if (cubeSettings.Bot)
        {
            square.sides[2] = true;
            square.pillars[2] = false;
            square.pillars[3] = false;
        }
        if (cubeSettings.Left)
        {
            square.sides[3] = true;
            square.pillars[3] = false;
            square.pillars[0] = false;
        }
        square.isBoss = cubeSettings.IsBoss;
        square.GetMeshData(wallsSettings);
        square.RenderMesh(textureSettings, CubeParent);
        square.RenderAdditions(textureSettings);
    }
    void LoadMaze()
    {
        maze.GenerateMaze();
        placesOfIntrest = maze.placesOfIntrest;
    }

    private void OnDrawGizmos()
    {
        if (maze != null)
        {
            UnityEngine.Color[] colors = { UnityEngine.Color.blue, UnityEngine.Color.yellow, UnityEngine.Color.red, UnityEngine.Color.green, UnityEngine.Color.magenta };
            int count = 1;
            if(mazeSettings.showVertacies < places.Count)
            {
                Vector2Int point = places[mazeSettings.showVertacies];
                if (Mazegeneration.matrix[point.x, point.y].meshData.HasMesh())
                {
                    Vector3[] vectors = Mazegeneration.matrix[point.x, point.y].meshData.newCorners;
                    for (int i = 0; i < vectors.Length - 1; i++)
                    {
                        Gizmos.color = colors[(count - 1) % 5];
                        Gizmos.DrawSphere(vectors[i], 1);
                        //Gizmos.DrawLine(vectors[i], vectors[i + 1]);
                        count++;
                    }
                    count++;
                }
                else
                {
                    Gizmos.DrawSphere(new Vector3(point.x, point.y, 0), 2f);
                }
            }
        }
        if(square != null)
        {
            int[,,] spaces = square.meshData.spaces;
            List<Vector3> verts = square.meshData.landscapeVerts;
            //foreach(int i in square.meshData.landscapeTriangles)
            //{
            //    Gizmos.DrawSphere(verts[i], 0.2f);
            //}
            foreach (Vector3 v in verts)
            {
                Gizmos.DrawSphere(v, 0.2f);
            }
            //Vector3 pos = square.meshData.centre;
            //for (int x = 0; x < spaces.GetLength(0); x++)
            //{
            //    for (int y = 0; y < spaces.GetLength(1); y++)
            //    {
            //        for (int z = 0; z < spaces.GetLength(2); z++)
            //        {
            //            if (spaces[x, y, z] == 1)
            //            {
            //                Gizmos.color = Color.gray;
            //                Gizmos.DrawCube(new Vector3(x - cubeSettings.cubeSize / 2, y, z - cubeSettings.cubeSize / 2), new Vector3(1, 1, 1));
            //            }
            //            if (spaces[x, y, z] == 2)
            //            {
            //                Gizmos.color = Color.green;
            //                Gizmos.DrawCube(new Vector3(x - cubeSettings.cubeSize / 2, y, z - cubeSettings.cubeSize / 2), new Vector3(1, 1, 1));
            //            }
            //            if (spaces[x, y, z] == 3)
            //            {
            //                Gizmos.color = Color.black;
            //                Gizmos.DrawCube(new Vector3(x - cubeSettings.cubeSize / 2, y, z - cubeSettings.cubeSize / 2), new Vector3(1, 1, 1));
            //            }
            //        }
            //    }

            //}
        }
        

    }
    //private void OnDrawGizmos()
    //{
    //    if (placesOfIntrest.Count > 0 && textureSettings.regions.Length > 0)
    //    {
    //        try
    //        {
    //            Gizmos.color = Color.white;
    //            foreach (Vector2Int point in placesOfIntrest)
    //            {
    //                Gizmos.DrawSphere(maze.matrix[point.x, point.y].location, mazeSettings.cubeSize / (2 * 4));
    //            }
    //            foreach (Vector3Int point in bossLocations)
    //            {
    //                if (point.z == 0)
    //                {
    //                    Gizmos.color = Color.blue;
    //                }
    //                if (point.z == 1)
    //                {
    //                    Gizmos.color = Color.green;
    //                }
    //                if (point.z == 2)
    //                {
    //                    Gizmos.color = Color.red;
    //                }
    //                if (point.x == 0 || point.y == 0) { continue; }
    //                Gizmos.DrawSphere(maze.matrix[point.x, point.y].location, mazeSettings.cubeSize / 2);
    //            }
    //        }
    //        catch
    //        {
    //            Debug.Log("Some Gismos Were not Drawn");
    //        }
    //    }
    //}
}
