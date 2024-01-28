using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ShowEditorsmaze : MonoBehaviour
{
    public EditorCubeSettings cubeSettings;
    public MazeWallsSettings wallsSettings;
    public MazeSettings mazeSettings;
    public TextureSettings textureSettings;
    public TextureSettings.Region[] regions;
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
        textureSettings = new TextureSettings();
        if(textureSettings.regions == null)
        {
            textureSettings.regions = regions;
        }
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
            maze = new Mazegeneration(mazeSettings, wallsSettings);
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
        Debug.Log(textureSettings.regionNames[cubeSettings.region]);
        square = new SquareData(cubeSettings.cubeSize, new Vector3(0,0,0));
        square.region = textureSettings.regionNames[cubeSettings.region];
        square.regionValue = cubeSettings.regionValue;
        for (int i = 0; i < square.extends.Length; i++)
        {
            square.extends[i] = false;
        }
        if (!cubeSettings.Top)
        {
            square.sides[0] = false;
        }
        if (!cubeSettings.Right)
        {
            square.sides[1] = false;
        }
        if (!cubeSettings.Bot)
        {
            square.sides[2] = false;
        }
        if (!cubeSettings.Left)
        {
            square.sides[3] = false;
        }
        if (!cubeSettings.Top)
        {
            if (square.sides[3]) { square.extends[7] = true; }
            if (square.sides[1]) { square.extends[2] = true; }
        }
        if (!cubeSettings.Right)
        {
            if (square.sides[0]) { square.extends[1] = true; }
            if (square.sides[2]) { square.extends[4] = true; }
        }
        if (!cubeSettings.Bot)
        {
            if (square.sides[1]) { square.extends[3] = true; }
            if (square.sides[3]) { square.extends[6] = true; }
        }
        if (!cubeSettings.Left)
        {
            if (square.sides[2]) { square.extends[5] = true; }
            if (square.sides[0]) { square.extends[0] = true; }
        }
        square.GetMeshData(wallsSettings);
        square.RenderMesh(textureSettings, CubeParent);
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
