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
    [SerializeField] int mazeSize;
    [SerializeField] int cubeSize;
    [SerializeField] int wallHeight;
    [SerializeField] int wallWidth;
    [SerializeField] int backTrackDst;
    [SerializeField] int maxPathLength;
    [Range(0, 40)]
    [SerializeField] int bossDst;
    [SerializeField] int amountOfBosses;
    [SerializeField] int baseSize;
    [SerializeField] int regionSpread;
    [SerializeField] int bossChance;
    [SerializeField] Material defaultmaterial;
    [SerializeField]
    Region[] regions;
    Dictionary<string, Material> materials = new Dictionary<string, Material>();
    List<Square> placesOfIntrest = new List<Square>();
    List<Square> bossLocations = new List<Square>();
    Square[,] grid;
    List<Square> places = new List<Square>();
    Mazegeneration maze = new Mazegeneration();
    // Start is called before the first frame update
    void Start()
    {
        string[] regs = new string[regions.Length];
        for (int i = 0;i < regions.Length; i++)
        {
            regs[i] = regions[i].regionName;
            materials.Add(regions[i].regionName, regions[i].Material);
        }
        grid = maze.GenerateMaze(mazeSize, cubeSize, backTrackDst, maxPathLength, bossDst, amountOfBosses, baseSize, regionSpread, bossChance, regs);
        places = maze.visitedSquares;
        Debug.Log(places.Count);
        placesOfIntrest = maze.PlacesOfIntrest;
        bossLocations = maze.bossLocations;
        Square[,] sampleSquares = new Square[3, 3];
        foreach (Square square in places)
        {
            for (int y = -1; y < sampleSquares.GetLength(0) - 1; y++)
            {
                for (int x = -1; x < sampleSquares.GetLength(1) - 1; x++)
                {
                    if (square.xIndex >= 1 && square.yIndex >= 1 && square.xIndex < grid.GetLength(0) - 1 && square.yIndex < grid.GetLength(1) - 1)
                    {
                        sampleSquares[x + 1, y + 1] = grid[square.xIndex + x, square.yIndex + y];
                    }
                    else
                    {
                        sampleSquares[x + 1, y + 1] = null;
                    }
                }
            }
            square.calculateExtends(sampleSquares);
            if (materials.ContainsKey(square.region))
            {
                square.GetMeshData(materials[square.region], wallHeight, wallWidth);
            }
            else
            {
                square.GetMeshData(defaultmaterial, wallHeight, wallWidth);
            }
            Debug.Log("SQUARE");
            for (int i = 0; i < square.sides.Length; i++)
            {
                Debug.Log(square.sides[i]);
            }
            for (int i = 0; i < square.extends.Length; i++)
            {
                Debug.Log(square.extends[i]);
            }
        }
        foreach (Square s in places)
        {
            s.RenderMesh();
        }
    }

    // Update is called once per frame
    private void OnDrawGizmos()
    {
        foreach (Square square in places)
        {
            Gizmos.color = Color.gray;
            for (int i = 0; i < 4; i++)
            {
                if (square.sides[i] == true)
                {
                    Gizmos.DrawLine(square.corners[i], square.corners[(i + 1) % 4]);
                }
            }
        }
        Gizmos.color = Color.green;
        foreach (Square square in placesOfIntrest)
        {
            Gizmos.DrawSphere(square.location, cubeSize / 2);
        }
        Gizmos.color = Color.blue;
        foreach (Square square in bossLocations)
        {
            Gizmos.DrawSphere(square.location, cubeSize / 2);
        }
    }
    [Serializable]
    struct Region
    {
        [SerializeField]
        public string regionName;
        [SerializeField]
        public Material Material;
    }
}
