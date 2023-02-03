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
                Square square = new Square(xpos - Mathf.FloorToInt(baseSize/2) + x, ypos - Mathf.FloorToInt(baseSize/2) + y, cubeSize, mazeSize);
                if (x == 0) { square.sides[1] = false; } else { square.sides[3] = false; }
                if (x == baseSize - 1) { square.sides[3] = false; } else { square.sides[1] = false; }
                if (y == 0) { square.sides[0] = false; } else { square.sides[2] = false; }
                if (y == baseSize - 1) { square.sides[2] = false; } else { square.sides[0] = false; }
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
            matrix[sqr.xIndex, sqr.yIndex] = sqr;
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
                    matrix[xpos, ypos].sides[1] = false;
                    matrix[newXPos, newYPos].sides[3] = false;
                }
                if (xpos > newXPos)
                {
                    matrix[xpos, ypos].sides[3] = false;
                    matrix[newXPos, newYPos].sides[1] = false;
                }
                if (ypos < newYPos)
                {
                    matrix[xpos, ypos].sides[0] = false;
                    matrix[newXPos, newYPos].sides[2] = false;
                }
                if (ypos > newYPos)
                {
                    matrix[xpos, ypos].sides[2] = false;
                    matrix[newXPos, newYPos].sides[0] = false;
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
            if (matrix[xPos, yPos].sides[1] == false && matrix[xPos + 1, yPos].sides[3] == false && matrix[xPos + 1, yPos].regions.Contains(region) == false)//right
            {
                result = new Vector2(xPos + 1, yPos);
                return result;
            }
        }
        if(yPos != 0)
        {
            if (matrix[xPos, yPos].sides[2] == false && matrix[xPos, yPos - 1].sides[0] == false && matrix[xPos, yPos - 1].regions.Contains(region) == false)//down
            {
                result = new Vector2(xPos, yPos - 1);
                return result;
            }
        }
        if(xPos != 0)
        {
            if (matrix[xPos, yPos].sides[3] == false && matrix[xPos - 1, yPos].sides[1] == false && matrix[xPos - 1, yPos].regions.Contains(region) == false)//left
            {
                result = new Vector2(xPos - 1, yPos);
                return result;
            }
        }
        if(yPos != matrix.GetLength(1) - 1)
        {
            if (matrix[xPos, yPos].sides[0] == false && matrix[xPos, yPos + 1].sides[2] == false && matrix[xPos,yPos + 1].regions.Contains(region) == false)//up
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
    public Vector3 location;
    public bool visited = false;
    public int xIndex;
    public int yIndex;
    public bool[] sides = {true, true, true, true}; //sides sterting with top left
    public bool[] extends = { false, false, false, false, false, false, false, false }; //The values of the extends starting with top left 
    public Vector3[] corners; //starting from topLeft

    bool hasMesh = false;
    MeshData meshData;

    public Square(int xIndex,int yIndex, int cubeSize, int mazeSize)
    {
        corners = new Vector3[4];
        this.xIndex= xIndex;
        this.yIndex= yIndex;
        this.location = new Vector3((xIndex + 1) * cubeSize - (mazeSize * cubeSize / 2) - cubeSize / 2, 0, (yIndex + 1) * cubeSize - (mazeSize * cubeSize / 2) - cubeSize / 2);
        CalculateCorners(cubeSize);
        
    }
    void CalculateCorners(int cubeSize)
    {

        corners[0] = new Vector3(location.x - cubeSize / 2,0, location.z + cubeSize / 2);
        corners[1] = new Vector3(location.x + cubeSize / 2,0, location.z + cubeSize / 2);
        corners[2] = new Vector3(location.x + cubeSize / 2,0, location.z - cubeSize / 2);
        corners[3] = new Vector3(location.x - cubeSize / 2,0, location.z - cubeSize / 2);
    }

    public void calculateExtends(Square[,] grid)
    {
            int x = 1; int y = 1;int signX = -1; int signY = 1;
        //for( int i = 0; i < 4; i++ )
        //{
        //    if(i == 1 || i == 3)
        //    {
        //        signX = -signX;
        //    }
        //    if (grid[x + -(i % 2 - 1), y - i % 2] != null && sides[i] && sides[(i+1)%4] == false && grid[x + -(i%2-1), y - i%2].sides[i] == false && grid[x -signX, y + signY].extends[(i*2+6)%8] == false)//bot
        //    {
        //        extends[(i*2+1)%8] = true;
        //    }
        //    if(i == 1 || i == 3)
        //    {
        //        signY = -signY;
        //        signX = -signX;
        //    }
        //    if (grid[x + (i % 2 - 1), y + i % 2] != null && sides[i] && sides[(i+3)%4] == false && grid[x + (i % 2 - 1), y + i%2].sides[i] == false && grid[x + signX, y + signY].extends[(i*2+3)%8] == false)
        //    {
        //        extends[(i * 2)%8] = true;
        //    }
        //    if(i == 1 || i == 3)
        //    {
        //        signX = -signX;
        //    }

        //}
        if (grid[x + 1, y] != null)
        {
            if (sides[0] && sides[1] == false && grid[x + 1, y].sides[0] == false && grid[x + 1, y + 1].extends[6] == false)//top
            {
                extends[1] = true;
            }
            if (sides[2] && sides[1] == false && grid[x + 1, y].sides[2] == false && grid[x + 1, y - 1].extends[7] == false)
            {
                extends[4] = true;
            }
        }
        if(grid[x - 1, y] != null)
        {
            if (sides[0] && sides[3] == false && grid[x - 1, y].sides[0] == false && grid[x - 1, y + 1].extends[3] == false)
            {
                extends[0] = true;
            }
            if (sides[2] && sides[3] == false && grid[x - 1, y].sides[2] == false && grid[x - 1, y - 1].extends[2] == false)//bot
            {
                extends[5] = true;
            }
        }
        if (grid[x, y - 1] != null)
        {
            if (sides[3] && sides[2] == false && grid[x, y - 1].sides[3] == false && grid[x - 1, y - 1].extends[1] == false)
            {
                extends[6] = true;
            }
            if (sides[1] && sides[2] == false && grid[x, y - 1].sides[1] == false && grid[x + 1, y - 1].extends[0] == false)//right
            {
                extends[3] = true;
            }
        }
        if(grid[x, y + 1] != null)
        {
            if (sides[1] && sides[0] == false && grid[x, y + 1].sides[1] == false && grid[x + 1, y + 1].extends[5] == false)
            {
                extends[2] = true;
            }     
            if (sides[3] && sides[0] == false && grid[x, y + 1].sides[3] == false && grid[x - 1, y + 1].extends[4] == false)//left
            {
                extends[7] = true;
            }
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
