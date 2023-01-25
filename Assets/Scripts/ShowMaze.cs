using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    List<Square> placesOfIntrest;
    List<Square> bossLocations;
    Square[,] grid;
    List<Square> places;
    Mazegeneration generation = new Mazegeneration();
    // Start is called before the first frame update
    void Start()
    {
        string[] regs = new string[regions.Length];
        for (int i = 0;i < regions.Length; i++)
        {
            regs[i] = regions[i].regionName;
            materials.Add(regions[i].regionName, regions[i].Material);
        }
        grid = generation.GenerateMaze(mazeSize, cubeSize, backTrackDst, maxPathLength, bossDst, amountOfBosses, baseSize, regionSpread, bossChance, regs);
        places = generation.visitedSquares;
        placesOfIntrest = generation.PlacesOfIntrest;
        bossLocations = generation.bossLocations;
        int count = 0;
        foreach(Square square in places)
        {
            count++;
            square.calculateExtends(grid);
            if (materials.ContainsKey(square.region))
            {
                square.GetMeshData(materials[square.region], wallHeight, wallWidth);
            }
            else
            {
                square.GetMeshData(defaultmaterial, wallHeight, wallWidth);
            }
        }
        foreach(Square s in places)
        {
            s.RenderMesh();
        }
    }
    
    // Update is called once per frame
    private void OnDrawGizmos()
    {
        if (grid != null)
        {
            //for (int x = 0; x < grid.GetLength(0); x++)
            //{
            //    for (int y = 0; y < grid.GetLength(1); y++)
            //    {
            //        Gizmos.color = Color.gray;
            //        if (grid[x, y].left == true)
            //        {
            //            Gizmos.DrawLine(grid[x, y].topLeft, grid[x, y].bottomLeft);
            //        }
            //        if (grid[x, y].right == true)
            //        {
            //            Gizmos.DrawLine(grid[x, y].topRight, grid[x, y].bottomRight);
            //        }
            //        if (grid[x, y].top == true)
            //        {
            //            Gizmos.DrawLine(grid[x, y].topLeft, grid[x, y].topRight);
            //        }
            //        if (grid[x, y].bottom == true)
            //        {
            //            Gizmos.DrawLine(grid[x, y].bottomLeft, grid[x, y].bottomRight);
            //        }
            //        if (grid[x,y].region == "boss")
            //        {
            //            Gizmos.color = Color.red;
            //            Gizmos.DrawSphere(grid[x, y].location, 0.5f / grid[x,y].regionValue);
            //        }
            //    }
            //}
            foreach (Square square in places)
            {
                Gizmos.color = Color.gray;
                if (square.left == true)
                {
                    Gizmos.DrawLine(square.topLeft, square.bottomLeft);
                }
                if (square.right == true)
                {
                    Gizmos.DrawLine(square.topRight, square.bottomRight);
                }
                if (square.top == true)
                {
                    Gizmos.DrawLine(square.topLeft, square.topRight);
                }
                if (square.bottom == true)
                {
                    Gizmos.DrawLine(square.bottomLeft, square.bottomRight);
                }
                if (square.region == "boss")
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawSphere(square.location, 0.5f / square.regionValue);
                }
            }
            Gizmos.color = Color.green;
            foreach(Square square in placesOfIntrest)
            {
                Gizmos.DrawSphere(square.location, cubeSize/2);
            }
            Gizmos.color = Color.blue;
            foreach (Square square in bossLocations)
            {
                Gizmos.DrawSphere(square.location, cubeSize/2);
            }

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
