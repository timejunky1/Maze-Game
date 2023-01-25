using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class Mazegeneration
{
    public Square[,] matrix; 
    public List<Square> visitedSquares = new List<Square>();
    public List<Square> PlacesOfIntrest = new List<Square>();
    public List<Square> bossLocations = new List<Square>();
    public Square[,] GenerateMaze(int mazeSize, int cubeSize, int backTrackDst, int maxPathLength, int bossDist, int amountOfBosses, int baseSize, int regionSpread, int bossChance, string[] regions)
    {
        PlacesOfIntrest.Clear();
        matrix = new Square[mazeSize, mazeSize];
        List<Square> lockedSquares = new List<Square>();
        int xpos = Mathf.CeilToInt(mazeSize / 2);
        int ypos = Mathf.CeilToInt(mazeSize / 2);
        //Open path gen
        //for (int i = (int)(mazeSize / 2); i < mazeSize; i++)
        //{
        //    Square square = new Square(Mathf.CeilToInt(mazeSize / 2), i, cubeSize, mazeSize);
        //    square.top = false; square.bottom = false;
        //    lockedSquares.Add(square);
        //}
        for (int x = 0; x < baseSize; x++)
        {
            for (int y = 0; y < baseSize; y++)
            {
                Square square = new Square(xpos - Mathf.CeilToInt(baseSize/2) + x, ypos - Mathf.CeilToInt(baseSize/2) + y, cubeSize, mazeSize);
                if (x == 0) { square.right = false; } else { square.left = false; }
                if (x == baseSize - 1) { square.left = false; } else { square.right = false; }
                if (y == 0) { square.top = false; } else { square.bottom = false; }
                if (y == baseSize - 1) { square.bottom = false; } else { square.top = false; }
                lockedSquares.Add(square);
            }

        }

        for (int x = 0;x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                matrix[x,y] = new Square(x, y, cubeSize, mazeSize);
            }
        }
        foreach (Square sqr in lockedSquares)
        {
            matrix[sqr.xIndex, sqr.yIndex].left = sqr.left;
            matrix[sqr.xIndex, sqr.yIndex].right = sqr.right;
            matrix[sqr.xIndex, sqr.yIndex].bottom = sqr.bottom;
            matrix[sqr.xIndex, sqr.yIndex].top = sqr.top;
        }
        Stack<Vector2> visited = new Stack<Vector2>();
        List<Vector2> options = new List<Vector2>();
        bool mazeComplete = false;
        int visitCount = 1;
        int count = 0;
        int bCount = 0;
        int sightingCount = 0;
        bool end = true;
        visitedSquares.Add(matrix[xpos, ypos]);
        while (mazeComplete == false)
        {
            count++;
            matrix[xpos, ypos].visited = true;
            options = AnalizeOptions(options, xpos, ypos);
            if (visited.Count == 0 && options.Count == 0)
            {
                mazeComplete= true;
            }
            if(options.Count > 0 && visited.Count < maxPathLength)
            {
                end = true;
                visitCount++;
                visited.Push(new Vector2(xpos, ypos));
                Vector2 option = options[UnityEngine.Random.Range(0, options.Count - 1)];
                int newXPos = (int)option.x;
                int newYPos = (int)option.y;
                if (xpos < newXPos)
                {
                    matrix[xpos, ypos].right = false;
                    matrix[newXPos, newYPos].left = false;
                }
                if (xpos > newXPos)
                {
                    matrix[xpos, ypos].left = false;
                    matrix[newXPos, newYPos].right = false;
                }
                if (ypos < newYPos)
                {
                    matrix[xpos, ypos].top = false;
                    matrix[newXPos, newYPos].bottom = false;
                }
                if (ypos > newYPos)
                {
                    matrix[xpos, ypos].bottom = false;
                    matrix[newXPos, newYPos].top = false;
                }
                xpos = newXPos;
                ypos = newYPos;
                visitedSquares.Add(matrix[xpos, ypos]);
            }
            else if(visited.Count > 0)
            {
                if (end == true)
                {
                    int rnd = UnityEngine.Random.Range(0, 10);
                    sightingCount++;
                    if (rnd == 0 && visited.Count > bossDist && bCount < amountOfBosses && sightingCount > bossChance)
                    {
                        bossLocations.Add(matrix[xpos, ypos]);
                        bCount++;
                        sightingCount = 0;
                    }
                    else if(rnd%2 == 0)
                    {
                        PlacesOfIntrest.Add(matrix[xpos, ypos]);
                    }
                }
                Vector2 oldVector = visited.Pop();
                xpos = (int)oldVector.x;
                ypos = (int)oldVector.y;
                end = false;
            }
            else { mazeComplete = true; }
        }
        int regionCount = 0;
        string region = "";
        foreach(Square square in bossLocations)
        {
            region = regions[UnityEngine.Random.Range(0,regions.Length)];
            Debug.Log(region);
            LoadRegions(region, square.xIndex, square.yIndex, regionSpread);
            regionCount++;
        }
        return matrix;
    }

    List<Vector2> AnalizeOptions(List<Vector2> options,int xpos, int ypos)
    {
        options.Clear();
        if (matrix.GetLength(0) - 1 >= xpos + 1 && matrix[xpos + 1, ypos].visited == false)//To the right
        {
            options.Add(new Vector2(xpos + 1,ypos));
        }
        if (ypos - 1 >= 0 && matrix[xpos, ypos - 1].visited == false)//Down
        {
            options.Add(new Vector2(xpos, ypos - 1));
            options.Add(new Vector2(xpos, ypos - 1));
        }
        if (xpos - 1 >= 0 && matrix[xpos - 1, ypos].visited == false)//Left
        {
            options.Add(new Vector2(xpos - 1, ypos));
        }
        if (matrix.GetLength(1) - 1 >= ypos + 1 && matrix[xpos, ypos + 1].visited == false)//Up
        {
            options.Add(new Vector2(xpos, ypos + 1));
            options.Add(new Vector2(xpos, ypos + 1));
        }
        return options;
    }

    void LoadRegions(string regionName,int xPos,int yPos, int regionSpread)
    {
        Stack<Vector2> visited = new Stack<Vector2>();
        bool mazeComplete = false;
        int visitCount = 0;
        int count = 0;
        bool end = true;
        while (mazeComplete == false)
        {
            count++;
            matrix[xPos, yPos].regions.Add(regionName);
            float regionvalue = -(visited.Count + 1);
            matrix[xPos, yPos].regionvalues.Add(regionvalue);
            if(matrix[xPos, yPos].regionValue == 0 || matrix[xPos, yPos].regionValue < regionvalue)
            {
                matrix[xPos, yPos].region = regionName;
                matrix[xPos, yPos].regionValue = regionvalue;
            }
            Vector2 newVector = RegionOptions(xPos, yPos, regionName);
            bool hasOptions = (newVector.x != xPos || newVector.y != yPos);
            if (visited.Count == 0 && hasOptions == false)
            {
                mazeComplete = true;

            }
            if (visited.Count < regionSpread && hasOptions)
            {
                end = true;
                visitCount++;
                visited.Push(new Vector2(xPos, yPos));
                xPos = (int)newVector.x;
                yPos = (int)newVector.y;
            }
            else if (visited.Count > 0)
            {
                if (end == true)
                {

                }
                Vector2 oldVector = visited.Pop();
                xPos = (int)oldVector.x;
                yPos = (int)oldVector.y;
                end = false;
            }
            else { mazeComplete = true; }
        }
    }

    Vector2 RegionOptions(int xPos, int yPos, string region)
    {
        Vector2 result = new Vector2(xPos, yPos); ;
        if(xPos != matrix.GetLength(0) - 1)
        {
            if (matrix[xPos, yPos].right == false && matrix[xPos + 1, yPos].left == false && matrix[xPos + 1, yPos].regions.Contains(region) == false)//right
            {
                result = new Vector2(xPos + 1, yPos);
                return result;
            }
        }
        if(yPos != 0)
        {
            if (matrix[xPos, yPos].bottom == false && matrix[xPos, yPos - 1].top == false && matrix[xPos, yPos - 1].regions.Contains(region) == false)//down
            {
                result = new Vector2(xPos, yPos - 1);
                return result;
            }
        }
        if(xPos != 0)
        {
            if (matrix[xPos, yPos].left == false && matrix[xPos - 1, yPos].right == false && matrix[xPos - 1, yPos].regions.Contains(region) == false)//left
            {
                result = new Vector2(xPos - 1, yPos);
                return result;
            }
        }
        if(yPos != matrix.GetLength(1) - 1)
        {
            if (matrix[xPos, yPos].top == false && matrix[xPos, yPos + 1].bottom == false && matrix[xPos,yPos + 1].regions.Contains(region) == false)//up
            {
                result = new Vector2(xPos, yPos + 1);
                return result;
            }
        }
        return result;
    }
}
public class Square
{
    public SortedSet<String> regions = new SortedSet<String>();
    public SortedSet<float> regionvalues = new SortedSet<float>();
    public string region = "";
    public float regionValue = 0;
    public bool visited;
    public bool left;
    public bool right;
    public bool top;
    public bool bottom;
    public Vector3 location;
    public Vector3 topLeft;
    public Vector3 topRight;
    public Vector3 bottomLeft;
    public Vector3 bottomRight;
    public int xIndex;
    public int yIndex;

    public bool topExtend2 = false;
    public bool topExtend1 = false;
    public bool rightExtend2 = false ;
    public bool rightExtend1 = false ;
    public bool bottomExtend2 = false;
    public bool bottomExtend1 = false;
    public bool leftExtend2 = false;
    public bool leftExtend1 = false;

    bool hasMesh = false;
    MeshData meshData;

    public Square(int xIndex,int yIndex, int cubeSize, int mazeSize)
    {
        this.visited = false;
        this.left = true;
        this.right = true;
        this.top = true;
        this.bottom = true;
        this.xIndex= xIndex;
        this.yIndex= yIndex;
        this.location = new Vector3((xIndex + 1) * cubeSize - (mazeSize * cubeSize / 2) - cubeSize / 2, 0, (yIndex + 1) * cubeSize - (mazeSize * cubeSize / 2) - cubeSize / 2);
        CalculateCorners(cubeSize);
        
    }
        void CalculateCorners(int cubeSize)
        {
            this.topLeft = new Vector3(location.x - cubeSize / 2,0, location.z + cubeSize / 2);
            this.topRight = new Vector3(location.x + cubeSize / 2,0, location.z + cubeSize / 2);
            this.bottomLeft = new Vector3(location.x - cubeSize / 2,0, location.z - cubeSize / 2);
            this.bottomRight = new Vector3(location.x + cubeSize / 2,0, location.z - cubeSize / 2);
        }

        public void calculateExtends(Square[,] grid)
        {
            int x = xIndex; int y = yIndex;
            if (top && right == false && grid[x + 1, y].top == false && grid[x + 1, y + 1].leftExtend1 == false)//top
            {
                topExtend2 = true;
            }
            if (top && left == false && grid[x - 1, y].top == false && grid[x - 1, y + 1].rightExtend2 == false)
            {
                topExtend1 = true;
            }
            if (right && bottom == false && grid[x, y - 1].right == false && grid[x + 1, y - 1].topExtend1 == false)//right
            {
                rightExtend2 = true;
            }
            if (right && top == false && grid[x, y + 1].right == false && grid[x + 1, y + 1].bottomExtend2 == false)
            {
                rightExtend1 = true;
            }
            if (bottom && left == false && grid[x - 1, y].bottom == false && grid[x - 1, y - 1].rightExtend1 == false)//bot
            {
                bottomExtend2 = true;
            }
            if (bottom && right == false && grid[x + 1, y].bottom == false && grid[x + 1, y - 1].leftExtend2 == false)
            {
                bottomExtend1 = true;
            }
            if (left && top == false && grid[x, y + 1].left == false && grid[x - 1, y + 1].bottomExtend1 == false)//left
            {
                rightExtend2 = true;
            }
            if (left && bottom == false && grid[x, y - 1].left == false && grid[x - 1, y - 1].topExtend2 == false)
            {
                rightExtend1 = true;
            }
        }
    public void GetMeshData(Material material, int wallHeight, int wallWidth)
    {
        if (hasMesh)
        {
            meshData.UpdateMesh(this, material, wallHeight, wallWidth);
        }
        else
        {
            meshData = new MeshData();
            meshData.CreateMesh(this, material, wallHeight, wallWidth);
            hasMesh = true;
        }
    }

    public void RenderMesh()
    {
        if (hasMesh)
        {
            meshData.RenderMesh();
        }
    }
}
