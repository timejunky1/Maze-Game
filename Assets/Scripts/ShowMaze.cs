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
    public TextureSettings textureSettings;
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
    Mazegeneration maze;
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            GameObject.Find("EditorsMaze").SetActive(false);
            GameObject.Find("EditorsCube").SetActive(false);
        }
        catch { }
        maze = new Mazegeneration(mazeSettings);
        textureSettings.ReloadDicts();
        maze.SetLockedSquares();
        maze.LoadMaze();
        LoadMaze();
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        ProcessRendering(new Vector2Int(Mathf.FloorToInt((viewerPosition.x /mazeSettings.mazeSize) + (mazeSettings.mazeSize / 2)), Mathf.FloorToInt((viewerPosition.y / mazeSettings.mazeSize) + (mazeSettings.mazeSize / 2))));
        viewerPositionOld = viewerPosition;
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if (viewerPosition != viewerPositionOld)
        {
            ProcessRendering(new Vector2Int(Mathf.FloorToInt((viewerPosition.x / mazeSettings.mazeSize) + (mazeSettings.mazeSize / 2)), Mathf.FloorToInt((viewerPosition.y / mazeSettings.mazeSize) + (mazeSettings.mazeSize / 2))));
        }
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrviewerMoveThresholdForUpdaate)
        {
            viewerPositionOld = viewerPosition;
        }
    }

    void ProcessRendering(Vector2Int pos)//seperate Thread Pls
    {
        Debug.Log(pos.x + ", " + pos.y);
        renderedPlaces = maze.LoadRegion(pos.x, pos.y, renderDistance, true);
        Debug.Log(renderedPlaces.Count);
        foreach(Vector2Int renderedPlace in renderedPlaces)
        {
            Debug.Log(renderedPlace);
            maze.matrix[renderedPlace.x, renderedPlace.y].RenderMesh(textureSettings, Parent);
        }
    }
    void RenderMaze(List<Vector3Int> places)// make this rendered based on the viewers possition
    {
        foreach (Vector3Int point in places)
        {
            maze.matrix[point.x, point.y].RenderMesh(textureSettings, Parent);
        }
    }
    void LoadMaze()
    {
        maze.GenerateMaze();
        placesOfIntrest = maze.placesOfIntrest;
        maze.CalculatePlacesOfIntrest(placesOfIntrest, textureSettings.regionNames);
        places = maze.visitedSquares;
        Debug.Log(places.Count);
        foreach (Vector3Int point in places)
        {
            Debug.Log(maze.matrix[point.x,point.y].region);
        }
        int oldPointY = -1;
        int oldPointX = -1;
        foreach (Vector3Int point in places)
        {
            if(point.x == oldPointX && point.y == oldPointY)
            {
                Debug.Log("Hass Doubles");
                break;
            }
            oldPointY = point.y;
            oldPointX = point.x;
        }
        bossLocations = maze.bossLocations;
        foreach(Vector2Int point in places)
        {
            maze.matrix[point.x, point.y].GetMeshData(wallsSettings);   
        }
    }
    private void OnDrawGizmos()
    {
        int count = 0;
        foreach (Vector2Int point in maze.lockedSquares)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(maze.matrix[point.x, point.y].location, Vector3.one * (mazeSettings.cubeSize / 2));
        }
        foreach (Vector3 point in maze.wallSquares)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawCube(point, Vector3.one * (mazeSettings.cubeSize / 2));
        }
        for (int x = 0; x < maze.matrix.GetLength(0); x++)
        {
            for (int y = 0; y < maze.matrix.GetLength(1); y++)
            {
                if (x == 0 || y == 0 || x == maze.matrix.GetLength(0) - 1 || y == maze.matrix.GetLength(1) - 1)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(maze.matrix[x, y].location, Vector3.one * mazeSettings.cubeSize / 2);
                }
            }
        }
        Gizmos.color = Color.white;
        foreach(Vector2Int point in renderedPlaces)
        {
            Gizmos.DrawCube(maze.matrix[point.x, point.y].location, new Vector3(mazeSettings.cubeSize/2,mazeSettings.cubeSize/2,mazeSettings.cubeSize / 2));
        }
        foreach(Vector2Int point in places)
        {
            Gizmos.color = Color.white;
            count++;
            SquareData square = maze.matrix[point.x, point.y];
            if (square.sides[0] == true)
            {
                Gizmos.DrawLine(square.corners[0], square.corners[1]);
            }
            if (square.sides[1] == true)
            {
                Gizmos.DrawLine(square.corners[1], square.corners[2]);
            }
            if (square.sides[2] == true)
            {
                Gizmos.DrawLine(square.corners[2], square.corners[3]);
            }
            if (square.sides[3] == true)
            {
                Gizmos.DrawLine(square.corners[3], square.corners[0]);
            }
            if(square.region == "Desert")
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(square.location, mazeSettings.cubeSize / 3);
            }
            if (square.region == "Jungle")
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(square.location, mazeSettings.cubeSize / 3);
            }
            if (square.region == "Fire")
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(square.location, mazeSettings.cubeSize / 3);
            }
            if (square.region == "Ice")
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(square.location, mazeSettings.cubeSize / 3);
            }
            //if (square.region == "Maze")
            //{
            //    Gizmos.color = Color.white;
            //    Gizmos.DrawSphere(square.location, mazeSettings.cubeSize/3);
            //}
        }
    }
}
