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
    public TextUreSettings textUreSettings;
    public GameObject Parent;
    public Transform viewer;
    public int renderDistance;
    public int sqrviewerMoveThresholdForUpdaate;
    Vector2 viewerPosition;
    Vector2 viewerPositionOld;

    List<Vector2Int> placesOfIntrest = new List<Vector2Int>();
    Vector3Int[] bossLocations;
    List<Vector2Int> places = new List<Vector2Int>();
    List<Vector3Int> renderedPlaces = new List<Vector3Int>();
    Mazegeneration maze = new Mazegeneration();
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            GameObject.Find("EditorsMaze").SetActive(false);
            GameObject.Find("EditorsCube").SetActive(false);
        }
        catch { }
        maze = new Mazegeneration();
        textUreSettings.ReloadDicts();
        maze.SetLockedSquares();
        LoadMaze();
    }

    private void Update()
    {
        Debug.Log(1);
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        Debug.Log(2);
        if (viewerPosition != viewerPositionOld)
        {
            Debug.Log(3);
            ProcessRendering();
        }
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrviewerMoveThresholdForUpdaate)
        {
            Debug.Log(4);
            viewerPositionOld = viewerPosition;
        }
    }

    void ProcessRendering()//seperate Thread Pls
    {
        renderedPlaces = maze.LoadRegion(Mathf.FloorToInt(viewer.transform.position.x), Mathf.FloorToInt(viewer.transform.position.z), renderDistance);
        Debug.Log(renderedPlaces.Count);
        RenderMaze(renderedPlaces);
    }
    void RenderMaze(List<Vector3Int> places)// make this rendered based on the viewers possition
    {
        foreach (Vector3Int point in places)
        {
            maze.matrix[point.x, point.y].RenderMesh(true, textUreSettings, Parent);
        }
    }
    void LoadMaze()
    {
        maze.GenerateMaze(mazeSettings);
        placesOfIntrest = maze.placesOfIntrest;
        maze.CalculatePlacesOfIntrest(mazeSettings, placesOfIntrest, textUreSettings.regionNames);
        places = maze.visitedSquares;
        Debug.Log(places.Count);
        bossLocations = maze.bossLocations;
        foreach(Vector2Int point in places)
        {
            maze.matrix[point.x, point.y].GetMeshData(wallsSettings);   
        }
        //RenderMaze();
    }
    private void OnDrawGizmos()
    {
        foreach(Vector2Int point in maze.lockedSquares)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(maze.matrix[point.x, point.y].location, Vector3.one * (mazeSettings.cubeSize/2));
        }
        foreach(Vector3 point in maze.wallSquares)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(point, Vector3.one * (mazeSettings.cubeSize / 2));
        }
        for (int x = 0; x < maze.matrix.GetLength(0); x++)
        {
            for (int y = 0; y < maze.matrix.GetLength(1); y++)
            {
                if(x == 0 || y == 0 || x == maze.matrix.GetLength(0)-1 || y == maze.matrix.GetLength(1) - 1)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(maze.matrix[x, y].location, Vector3.one * mazeSettings.cubeSize / 2);
                }
            }
        }
    }
}
