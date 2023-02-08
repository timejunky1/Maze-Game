using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowEditorsmaze : MonoBehaviour
{
    public EditorCubeSettings cubeSettings;
    public MazeWallsSettings wallsSettings;
    public MazeSettings mazeSettings;
    public TextUreSettings textUreSettings;
    public GameObject CubeParent;
    public GameObject MazeParent;
    public bool GenerateCube;

    List<Vector2Int> placesOfIntrest = new List<Vector2Int>();
    Vector3Int[] bossLocations;
    List<Vector2Int> places = new List<Vector2Int>();
    Mazegeneration maze = new Mazegeneration();
    SquareData square;

    void OnValuesUpdated()//Does not want to destroy the walls
    {
        if (!GenerateCube)
        {
            MazeParent.SetActive(true);
            if (mazeSettings.newMaze || !HasMesh(MazeParent))
            {
                for (int i = 0; i < MazeParent.transform.childCount; i++)
                {
                    DestroyImmediate(MazeParent.transform.GetChild(i));
                }
                Debug.Log(MazeParent.transform.childCount);
                maze.SetLockedSquares();
                LoadMaze();
            }
            maze.CalculatePlacesOfIntrest(mazeSettings, placesOfIntrest, textUreSettings.regionNames);
            places = maze.visitedSquares;
            bossLocations = maze.bossLocations;
            RenderMaze();
            CubeParent.SetActive(false);
        }
        else if (GenerateCube)
        {
            CubeParent.SetActive(true);
            RenderCube();
            MazeParent.SetActive(false);
        }
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
        if (!GenerateCube)
        {
            if (HasMesh(MazeParent))
            {
                LoadMazeTextures();
            }
        }
        else if (GenerateCube)
        {
            LoadCubeTextures();
        }
    }

    private void OnValidate()
    {
        if (cubeSettings != null)
        {
            cubeSettings.OnValuesUpdated -= OnValuesUpdated;
            cubeSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (wallsSettings != null)
        {
            wallsSettings.OnValuesUpdated -= OnValuesUpdated;
            wallsSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (mazeSettings != null)
        {
            mazeSettings.OnValuesUpdated -= OnValuesUpdated;
            mazeSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (textUreSettings != null)
        {
            textUreSettings.OnValuesUpdated -= OnTextureValuesUpdated;
            textUreSettings.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    void LoadMazeTextures()
    {
        foreach(Vector2Int point in places)
        {
            maze.matrix[point.x, point.y].ReloadTextures(textUreSettings);
        }
    }

    void LoadCubeTextures()
    {
        square.ReloadTextures(textUreSettings);
    }
    void RenderMaze()
    {
        foreach (Vector2Int point in places)
        {
            maze.matrix[point.x, point.y].GetMeshData(wallsSettings);
        }
        foreach (Vector2Int point in places)
        {
            maze.matrix[point.x, point.y].RenderMesh(true, textUreSettings, MazeParent);
        }
    }

    void RenderCube()//destroy the initial cube
    {
        DestroyImmediate(CubeParent.transform.GetChild(0));
        square = new SquareData(mazeSettings.cubeSize, new Vector3(0,0,0));
        square.region = textUreSettings.regionNames[cubeSettings.region];
        square.canSpawn = cubeSettings.canSpawn;
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
        square.RenderMesh(true, textUreSettings, CubeParent);
    }
    void LoadMaze()
    {
        maze.GenerateMaze(mazeSettings);
        placesOfIntrest = maze.placesOfIntrest;
    }
    private void OnDrawGizmos()
    {
        if (placesOfIntrest.Count > 0 && textUreSettings.regions.Length > 0)
        {
            try
            {
                Gizmos.color = Color.white;
                foreach (Vector2Int point in placesOfIntrest)
                {
                    Gizmos.DrawSphere(maze.matrix[point.x, point.y].location, mazeSettings.cubeSize / (2 * 4));
                }
                foreach (Vector3Int point in bossLocations)
                {
                    if (point.z == 0)
                    {
                        Gizmos.color = Color.blue;
                    }
                    if (point.z == 1)
                    {
                        Gizmos.color = Color.green;
                    }
                    if (point.z == 2)
                    {
                        Gizmos.color = Color.red;
                    }
                    if (point.x == 0 || point.y == 0) { continue; }
                    Gizmos.DrawSphere(maze.matrix[point.x, point.y].location, mazeSettings.cubeSize / 2);
                }
            }
            catch
            {
                Debug.Log("Some Gismos Were not Drawn");
            }
        }
    }
}
