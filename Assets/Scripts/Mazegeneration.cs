using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class Mazegeneration
{
    public SquareData[,] matrix; 
    public List<Vector2Int> visitedSquares = new List<Vector2Int>();
    public List<Vector2Int> placesOfIntrest = new List<Vector2Int>();
    public Vector2Int[] bossLocations;
    public List<Vector2Int> entranceCubes = new List<Vector2Int>();
    bool isFirstMaze = true;
    int regionLoadIndex;

    public void GenerateMaze(MazeSettings maze)
    {
        regionLoadIndex= 0;
        placesOfIntrest.Clear();
        matrix = new SquareData[maze.mazeSize, maze.mazeSize];
        List<Vector2Int> lockedSquares = new List<Vector2Int>();
        int xpos = Mathf.CeilToInt(maze.mazeSize / 2);
        int ypos = Mathf.CeilToInt(maze.mazeSize / 2);


        for (int x = 0;x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                if(lockedSquares.Contains(new Vector2Int(x, y)))
                {
                    continue;
                }
                matrix[x,y] = new SquareData(x, y, maze.cubeSize, maze.mazeSize);
            }
        }
        if (isFirstMaze && maze.baseSize>0)
        {
            CreateBase(maze, xpos, ypos);
        }
        if (maze.createEntrance)
        {
            CreateEntrance(maze);
        }
        Stack<Vector2Int> visited = new Stack<Vector2Int>();
        List<Vector2Int> options = new List<Vector2Int>();
        bool mazeComplete = false;
        int visitCount = 1;
        int count = 0;
        bool end = true;
        visitedSquares.Add(new Vector2Int(xpos,ypos));
        while (mazeComplete == false)
        {
            count++;
            matrix[xpos, ypos].visited = true;
            options = AnalizeOptions(xpos, ypos);
            if (visited.Count == 0 && options.Count == 0)
            {
                mazeComplete= true;
            }
            if (options.Count > 0 && visited.Count < maze.maxPathLength)
            {
                end = true;
                visitCount++;
                visited.Push(new Vector2Int(xpos, ypos));
                Vector2Int option = options[UnityEngine.Random.Range(0, options.Count - 1)];
                int newXPos = option.x;
                int newYPos = option.y;
                if (visited.Count == 0 && visitCount>10)//change so that it forces the last direction option when back at start
                {
                    visited.Push(new Vector2Int(xpos, ypos));
                    option = options[options.Count - 1];
                    newXPos = option.x;
                    newYPos = option.y;
                }
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
                visitedSquares.Add(new Vector2Int(xpos,ypos));
            }
            else if(visited.Count > 0)
            {
                if (end == true)
                {                   
                    placesOfIntrest.Add(new Vector2Int(xpos, ypos));
                }
                Vector2Int oldVector = visited.Pop();
                xpos = oldVector.x;
                ypos = oldVector.y;
                end = false;
            }
            else { mazeComplete = true; }
            
        }
        foreach(Vector2Int point in entranceCubes)
        {
            if (!matrix[point.x, point.y].visited)
            {
                visitedSquares.Add(new Vector2Int(point.x, point.y));
            }
        }
        foreach (Vector2Int pos in visitedSquares)
        {
            matrix[pos.x,pos.y].extends = CalcualateExtends(matrix[pos.x, pos.y].sides, pos.x,pos.y);
        }
    }
    public void CreateOutsideWall(MazeSettings maze, MazeWallsSettings walls, Material material)
    {
        Debug.Log("CreateOutsideWall");
        GameObject outsideWall = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
        outsideWall.transform.parent = GameObject.Find("MazeWalls").transform;
        outsideWall.name = "OutsideWall";
        Vector3 vector = matrix[Mathf.FloorToInt(maze.mazeSize / 2), Mathf.FloorToInt(maze.mazeSize / 2)].corners[1];
        int halfMaze = Mathf.FloorToInt(maze.mazeSize / 2);
        Mesh mesh = new Mesh();
        Vector3[] verticies = new Vector3[]{
            vector += new Vector3(maze.cubeSize - walls.wallWidth - maze.cubeSize, 0, halfMaze * maze.cubeSize + ((maze.mazeSize%2 - 1) * maze.cubeSize)),
            vector + Vector3.up * walls.wallHeight,
            vector += new Vector3(halfMaze * maze.cubeSize + ((maze.mazeSize%2 - 1) * maze.cubeSize) ,0,0),
            vector + Vector3.up * walls.wallHeight,
            vector += new Vector3(0,0,-maze.mazeSize * maze.cubeSize),
            vector + Vector3.up * walls.wallHeight,
            vector += new Vector3(-maze.mazeSize*maze.cubeSize,0,0),
            vector + Vector3.up * walls.wallHeight,
            vector += new Vector3(0,0,maze.mazeSize * maze.cubeSize),
            vector + Vector3.up * walls.wallHeight,
            vector += new Vector3(halfMaze * maze.cubeSize + 2*walls.wallWidth, 0, 0),
            vector + Vector3.up * walls.wallHeight,};
        Vector2[] uvs = new Vector2[verticies.Length];
        for (int i = 0;i < verticies.Length; i++)
        {
            uvs[i] = verticies[i];
        }
        int[] triangles = new int[(verticies.Length/2 - 1)*6];
        int triangleIndex = 0;
        for (int i = 0; i < verticies.Length/2 - 1; i++)
        {
            triangles[triangleIndex] = i * 2;
            triangles[triangleIndex + 1] = i * 2 + 2;
            triangles[triangleIndex + 2] = i * 2 + 1;
            triangles[triangleIndex + 3] = i * 2 + 1;
            triangles[triangleIndex + 4] = i * 2 + 2;
            triangles[triangleIndex + 5] = i * 2 + 3;
            triangleIndex += 6;
        }
        mesh.vertices = verticies;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        outsideWall.GetComponent<MeshFilter>().mesh = mesh;
        outsideWall.GetComponent<MeshCollider>().sharedMesh = mesh;
        outsideWall.GetComponent<MeshRenderer>().material = material;
    }
    public void CreateEntrance(MazeSettings maze)
    {
        for (int i = (int)(maze.mazeSize / 2); i < maze.mazeSize; i++)
        {
            int posX = Mathf.CeilToInt(maze.mazeSize / 2);
            int posY = i;
            matrix[posX, posY].sides[0] = false;
            matrix[posX, posY].sides[2] = false;
            matrix[posX, posY].canSpawn = false;
            entranceCubes.Add(new Vector2Int(posX, posY));
        }
    }
    public void CreateBase(MazeSettings maze, int xPos, int yPos)
    {
        SquareData[,] result = new SquareData[maze.baseSize,maze.baseSize];
        for (int x = 0; x < maze.baseSize; x++)
        {
            for (int y = 0; y < maze.baseSize; y++)
            {
                SquareData square = new SquareData(xPos - Mathf.FloorToInt(maze.baseSize / 2) + x, yPos - Mathf.FloorToInt(maze.baseSize / 2) + y, maze.cubeSize, maze.mazeSize);
                if (x == 0) { square.sides[3] = true; } else { square.sides[3] = false; }
                if (x == maze.baseSize - 1) { square.sides[1] = true; } else { square.sides[1] = false; }
                if (y == 0) { square.sides[2] = true; } else { square.sides[2] = false; }
                if (y == maze.baseSize - 1) { square.sides[0] = true; } else { square.sides[0] = false; }
                square.canSpawn = false;
                result[x,y] = square;
            }
        }
        foreach(SquareData sqr in result)
        {
            matrix[sqr.xIndex, sqr.yIndex] = sqr;
        }
    }

    public void CalculatePlacesOfIntrest(MazeSettings maze, List<Vector2Int> placesOfInterest, List<string> regions)
    {
        Vector2Int centerPoint = new Vector2Int(Mathf.CeilToInt(maze.mazeSize/2), Mathf.CeilToInt(maze.mazeSize / 2));
        bossLocations = new Vector2Int[maze.amountOfBosses];
        int count = 0;
        foreach(Vector2Int point in placesOfInterest)
        {
            if(count < maze.amountOfBosses && point.x != Mathf.CeilToInt(maze.mazeSize/2) && point.y != Mathf.CeilToInt(maze.mazeSize / 2))
            {
                if(centerPoint.x >= maze.bossDst || centerPoint.x <= -maze.bossDst || centerPoint.y >= maze.bossDst || centerPoint.y <= maze.bossDst && matrix[point.x, point.y].canSpawn)
                {
                    int percent = UnityEngine.Random.Range(0, 100);
                    if(percent <= maze.bossChance)
                    {
                        bossLocations[count] = new Vector2Int(point.x, point.y);
                        string region = regions[UnityEngine.Random.Range(0, regions.Count)];
                        foreach (Vector3Int regVal in LoadRegion(point.x, point.y, maze.regionSpread))
                        {
                            if (matrix[regVal.x, regVal.y].regionValue == 0 || matrix[regVal.x, regVal.y].regionValue > regVal.z)
                            {
                                matrix[regVal.x, regVal.y].region = region;
                                matrix[regVal.x, regVal.x].regionValue = regVal.z;
                            }
                        }
                        count++;
                    }
                }
            }
            else
            {
                matrix[point.x, point.y].regions.Add("PlaceOfIntrest", maze.regionSpread+10);
            }
        }
    }

    List<Vector2Int> AnalizeOptions(int xpos, int ypos)
    {
        List<Vector2Int> options = new List<Vector2Int>();
        if (matrix.GetLength(0) - 1 >= xpos + 1 && matrix[xpos + 1, ypos].visited == false)//To the right
        {
            options.Add(new Vector2Int(xpos + 1,ypos));
        }
        if (ypos - 1 >= 0 && matrix[xpos, ypos - 1].visited == false)//Down
        {
            options.Add(new Vector2Int(xpos, ypos - 1));
            options.Add(new Vector2Int(xpos, ypos - 1));
        }
        if (xpos - 1 >= 0 && matrix[xpos - 1, ypos].visited == false)//Left
        {
            options.Add(new Vector2Int(xpos - 1, ypos));
        }
        if (matrix.GetLength(1) - 1 >= ypos + 1 && matrix[xpos, ypos + 1].visited == false)//Up
        {
            options.Add(new Vector2Int(xpos, ypos + 1));
            options.Add(new Vector2Int(xpos, ypos + 1));
        }
        return options;
    }

    List<Vector3Int> LoadRegion(int xPos, int yPos , int regionSpread)
    {
        regionLoadIndex++;
        Stack<Vector2Int> visited = new Stack<Vector2Int>();
        List<Vector3Int> regionSquares = new List<Vector3Int>();
        bool mazeComplete = false;
        int visitCount = 0;
        bool end = true;
        regionSquares.Add(new Vector3Int(xPos, yPos, 0));
        while (mazeComplete == false)
        {
            int regionvalue = visited.Count + 1;
            matrix[xPos, yPos].regionIndex = regionLoadIndex;
            Vector2Int option = RegionOptions(xPos, yPos, regionLoadIndex);
            bool hasOptions = (option.x!=xPos || option.y!=yPos);
            Vector3Int oldSquare = new Vector3Int(xPos, yPos, regionvalue);
            if (visited.Count == 0 && hasOptions == false)
            {
                mazeComplete = true;

            }
            if (visited.Count < regionSpread && hasOptions)
            {
                end = true;
                visitCount++;
                visited.Push(new Vector2Int(xPos, yPos));
                xPos = option.x;
                yPos = option.y;
                regionSquares.Add(new Vector3Int(xPos, yPos, regionvalue));
            }
            else if(!regionSquares.Contains(oldSquare) && visited.Count < regionSpread)
            {
                regionSquares.Add(oldSquare);
            }
            else if (visited.Count > 0)
            {
                if (end == true)
                {

                }
                Vector2Int oldVector = visited.Pop();
                xPos = oldVector.x;
                yPos = oldVector.y;
                end = false;
            }
            else { mazeComplete = true; }
        }
        return regionSquares;
    }

    Vector2Int RegionOptions(int xPos, int yPos, int regionIndex)
    {
        Vector2Int result = new Vector2Int(xPos, yPos);
        if(xPos != matrix.GetLength(0) - 1)
        {
            if (matrix[xPos, yPos].sides[1] == false && matrix[xPos + 1, yPos].sides[3] == false && matrix[xPos + 1, yPos].regionIndex != regionIndex)//right
            {
                result = new Vector2Int(xPos + 1, yPos);
                return result;
            }
        }
        if(yPos != 0)
        {
            if (matrix[xPos, yPos].sides[2] == false && matrix[xPos, yPos - 1].sides[0] == false && matrix[xPos, yPos - 1].regionIndex != regionIndex)//down
            {
                result = new Vector2Int(xPos, yPos - 1);
                return result;
            }
        }
        if(xPos != 0)
        {
            if (matrix[xPos, yPos].sides[3] == false && matrix[xPos - 1, yPos].sides[1] == false && matrix[xPos - 1, yPos].regionIndex != regionIndex)//left
            {
                result = new Vector2Int(xPos - 1, yPos);
                return result;
            }
        }
        if(yPos != matrix.GetLength(1) - 1)
        {
            if (matrix[xPos, yPos].sides[0] == false && matrix[xPos, yPos + 1].sides[2] == false && matrix[xPos,yPos + 1].regionIndex != regionIndex)//up
            {
                result = new Vector2Int(xPos, yPos + 1);
                return result;
            }
        }
        return result;
    }

    public bool[] CalcualateExtends(bool[] sides, int x, int y)
    {
        bool[] extends = new bool[sides.Length * 2];
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
        try
        {
            if (x < matrix.GetLength(0) - 1)
            {
                if (sides[0] && sides[1] == false && matrix[x + 1, y].sides[0] == false && matrix[x + 1, y + 1].extends[6] == false)//top
                {
                    extends[1] = true;
                }
                if (sides[2] && sides[1] == false && matrix[x + 1, y].sides[2] == false && matrix[x + 1, y - 1].extends[7] == false)
                {
                    extends[4] = true;
                }
            }
            if (x > 0)
            {
                if (sides[0] && sides[3] == false && matrix[x - 1, y].sides[0] == false && matrix[x - 1, y + 1].extends[3] == false)
                {
                    extends[0] = true;
                }
                if (sides[2] && sides[3] == false && matrix[x - 1, y].sides[2] == false && matrix[x - 1, y - 1].extends[2] == false)//bot
                {
                    extends[5] = true;
                }
            }
            if (y > 0)
            {
                if (sides[3] && sides[2] == false && matrix[x, y - 1].sides[3] == false && matrix[x - 1, y - 1].extends[1] == false)
                {
                    extends[6] = true;
                }
                if (sides[1] && sides[2] == false && matrix[x, y - 1].sides[1] == false && matrix[x + 1, y - 1].extends[0] == false)//right
                {
                    extends[3] = true;
                }
            }
            if (y < matrix.GetLength(1) - 1)
            {
                if (sides[1] && sides[0] == false && matrix[x, y + 1].sides[1] == false && matrix[x + 1, y + 1].extends[5] == false)
                {
                    extends[2] = true;
                }
                if (sides[3] && sides[0] == false && matrix[x, y + 1].sides[3] == false && matrix[x - 1, y + 1].extends[4] == false)//left
                {
                    extends[7] = true;
                }
            }
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
        
        return extends;
    }
}

