using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class ShowMaze: MonoBehaviour
{
    public MazeWallsSettings wallsSettings;
    public MazeSettings mazeSettings;
    public MazeSettings smallMazeSettings;
    public TextUreSettings textUreSettings;

    List<Vector2Int> placesOfIntrest = new List<Vector2Int>();
    Vector2Int[] bossLocations;
    List<Vector2Int> places = new List<Vector2Int>();
    Mazegeneration maze = new Mazegeneration();
    bool drawMap = false;
    // Start is called before the first frame update
    void Start()
    {
        textUreSettings.ReloadDicts();
        LoadMaze();
    }

    //private void OnValidate()
    //{
    //    if (smallMazeSettings.autoUpdate)
    //    {
    //        DrawMazeInEditor();
    //    }
    //}

    void RenderMaze()
    {
        foreach (Vector2Int point in places)
        {
            string region = maze.matrix[point.x, point.y].region;

            if (textUreSettings.regionNames.Contains(region))
            {
                maze.matrix[point.x, point.y].GetMeshData(wallsSettings.wallHeight, wallsSettings.wallWidth, textUreSettings.materials[region], textUreSettings.colors[region]);
            }
            else
            {
                maze.matrix[point.x, point.y].GetMeshData(wallsSettings.wallHeight, wallsSettings.wallWidth, textUreSettings.defaultmaterial, Color.green);
            }
        }
        foreach (Vector2Int point in places)
        {
            maze.matrix[point.x, point.y].RenderMesh(true);
        }
    }
    // Update is called once per frame
    void LoadMaze()
    {
        maze.GenerateMaze(mazeSettings);
        maze.CreateOutsideWall(mazeSettings, wallsSettings, textUreSettings.defaultmaterial);
        placesOfIntrest = maze.placesOfIntrest;
        maze.CalculatePlacesOfIntrest(mazeSettings, placesOfIntrest, textUreSettings.regionNames);
        places = maze.visitedSquares;
        bossLocations = maze.bossLocations;
        RenderMaze();
    }
    void DrawMazeInEditor()
    {
        GameObject mazeParent = GameObject.Find("MazeWalls");
        for (int i = 0; i < mazeParent.transform.childCount; i++)
        {
            Destroy(mazeParent.transform.GetChild(0));
        }
        textUreSettings.ReloadDicts();
        maze.GenerateMaze(smallMazeSettings);
        maze.CreateOutsideWall(smallMazeSettings, wallsSettings, textUreSettings.defaultmaterial);
        placesOfIntrest = maze.placesOfIntrest;
        maze.CalculatePlacesOfIntrest(smallMazeSettings, placesOfIntrest, textUreSettings.regionNames);
        places = maze.visitedSquares;
        bossLocations = maze.bossLocations;
        RenderMaze();
        Debug.Log("mazeCreated");
    }
    private void OnDrawGizmos()
    {
        if (places.Count > 0)
        {
            foreach (Vector2Int point in places)
            {
                Gizmos.color = Color.gray;
                for (int i = 0; i < 4; i++)
                {
                    if (maze.matrix[point.x, point.y].sides[i] == true)
                    {
                        Gizmos.DrawLine(maze.matrix[point.x, point.y].corners[i], maze.matrix[point.x, point.y].corners[(i + 1) % 4]);
                    }
                }
                if (maze.matrix[point.x, point.y].region == textUreSettings.regionNames[0])
                {
                    Gizmos.color = Color.blue;
                }
                else if (maze.matrix[point.x, point.y].region == textUreSettings.regionNames[1])
                {
                    Gizmos.color = Color.green;
                }
                else if (maze.matrix[point.x, point.y].region == textUreSettings.regionNames[2])
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    continue;
                }
                Debug.Log(maze.matrix[point.x, point.y].regionValue);
                Gizmos.DrawSphere(maze.matrix[point.x, point.y].location, (mazeSettings.cubeSize / (2 * (maze.matrix[point.x,point.y].regionValue + 1))));
                
            }

        }
        if (placesOfIntrest.Count > 0 && textUreSettings.regions.Length > 0)
        {
            try
            {
                Gizmos.color = Color.white;
                foreach (Vector2Int point in placesOfIntrest)
                {
                    Gizmos.DrawSphere(maze.matrix[point.x, point.y].location, mazeSettings.cubeSize / (2 * 4));
                }
                Gizmos.color = Color.black;
                foreach (Vector2Int point in bossLocations)
                {
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
