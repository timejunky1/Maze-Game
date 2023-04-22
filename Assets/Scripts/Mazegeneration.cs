using System;
using System.Collections.Generic;
using UnityEngine;

public class Mazegeneration
{
    public static SquareData[,] matrix; 
    public List<Vector2Int> visitedSquares = new List<Vector2Int>();
    public List<Vector2Int> placesOfIntrest = new List<Vector2Int>();
    public Vector3Int[] bossLocations;
    public List<Vector2Int> entranceCubes = new List<Vector2Int>();
    public Stack<Vector2Int> lockedSquares;
    public List<Vector2Int> baseSquares;
    public List<Vector3> wallSquares = new List<Vector3>();
    MazeSettings maze;
    MazeWallsSettings walls;
    bool isFirstMaze = true;
    int regionLoadIndex;

    public Mazegeneration(MazeSettings _maze, MazeWallsSettings walls)
    {
        lockedSquares = new Stack<Vector2Int>();
        baseSquares = new List<Vector2Int>();
        regionLoadIndex = 0;
        isFirstMaze = true;
        maze = _maze;
        matrix = new SquareData[maze.mazeSize, maze.mazeSize];
        this.walls = walls;
    }
    public void LoadMaze() {
        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                if (matrix[x,y] != null && matrix[x,y].isLocked)
                {
                    matrix[x, y].visited = false;
                    Debug.Log("Locked");
                    continue;
                }
                Vector3 location = new Vector3((x + 1) * maze.cubeSize - (maze.mazeSize * maze.cubeSize / 2) - maze.cubeSize / 2, 0, (y + 1) * maze.cubeSize - (maze.mazeSize * maze.cubeSize / 2) - maze.cubeSize / 2);
                SquareData square = new SquareData(maze.cubeSize, location);
                matrix[x, y] = new SquareData(maze.cubeSize, location);
            }
        }
    }
    public void GenerateMaze()
    {
        regionLoadIndex= 0;
        placesOfIntrest.Clear();
        visitedSquares.Clear();
        int xpos = Mathf.CeilToInt(maze.mazeSize / 2);
        int ypos = Mathf.CeilToInt(maze.mazeSize / 2);
        if (isFirstMaze)
        {
            if (maze.baseSize > 0)
            {
                CreateBase(xpos, ypos);
            }
            isFirstMaze= false;
        }

        Stack<Vector2Int> visited = new Stack<Vector2Int>();
        List<Vector2Int> options = new List<Vector2Int>();
        bool mazeComplete = false;
        int visitCount = 1;
        int count = 0;
        bool end = true;
        Vector2Int option;
        visitedSquares.Add(new Vector2Int(xpos,ypos));
        while (mazeComplete == false)
        {
            count++;
            matrix[xpos, ypos].visited = true;
            options = AnalizeOptions(xpos, ypos, 1);
            if (visited.Count == 0 && options.Count == 0)
            {
                mazeComplete= true;
            }
            if (options.Count > 0 && visited.Count < maze.maxPathLength)
            {
                end = true;
                visitCount++;
                visited.Push(new Vector2Int(xpos, ypos));
                option = options[UnityEngine.Random.Range(0, options.Count - 1)];
                int newXPos = option.x;
                int newYPos = option.y;
                ManageTurn(xpos, ypos, newXPos, newYPos);
                if (visited.Count == 0 && visitCount>10)
                {
                    visited.Push(new Vector2Int(xpos, ypos));
                    option = options[options.Count - 1];
                    newXPos = option.x;
                    newYPos = option.y;
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
        if (maze.createEntrance)
        {
            CreateEntrance();
        }
        if(maze.createOuterWall)
        {
            CreateWrapedWall();
        }
        foreach (Vector2Int pos in visitedSquares)
        {
            matrix[pos.x,pos.y].extends = CalcualateExtends(matrix[pos.x, pos.y].sides, pos.x,pos.y);
        }
        foreach (Vector2Int pos in visitedSquares)
        {
            matrix[pos.x, pos.y].GetMeshData(walls);
        }
    }
    public void SetLockedSquares()
    {
        foreach(Vector2Int point in lockedSquares)
        {
            for (int i = 0; i < matrix[point.x, point.y].sides.Length; i++)
            {
                matrix[point.x, point.y].sides[i] = false;
            }
            if(point.y < matrix.GetLength(1) - 1 && matrix[point.x, point.y + 1].isLocked == false)
            {
                matrix[point.x, point.y].sides[0] = true;
            }
            if (point.x < matrix.GetLength(0) - 1 && matrix[point.x + 1, point.y].isLocked == false)
            {
                matrix[point.x, point.y].sides[1] = true;
            }
            if (point.y > 0 && matrix[point.x, point.y - 1].isLocked == false)
            {
                matrix[point.x, point.y].sides[2] = true;
            }
            if (point.x > 0 && matrix[point.x - 1, point.y].isLocked == false)
            {
                matrix[point.x, point.y].sides[3] = true;
            }
        }
        foreach (Vector2Int pos in lockedSquares)
        {
            matrix[pos.x, pos.y].extends = CalcualateExtends(matrix[pos.x, pos.y].sides, pos.x, pos.y);
        }
    }
    void ManageTurn(int xpos, int ypos, int newXPos, int newYPos)
    {
        if (ypos < newYPos)//up
        {
            matrix[xpos, ypos].sides[0] = false;
            matrix[newXPos, newYPos].sides[2] = false;
        }
        if (xpos < newXPos)//right
        {
            matrix[xpos, ypos].sides[1] = false;
            matrix[newXPos, newYPos].sides[3] = false;
        }
        if (ypos > newYPos)//down
        {
            matrix[xpos, ypos].sides[2] = false;
            matrix[newXPos, newYPos].sides[0] = false;
        }
        if (xpos > newXPos)//left
        {
            matrix[xpos, ypos].sides[3] = false;
            matrix[newXPos, newYPos].sides[1] = false;
        }
    }
    void CreateWrapedWall()// keeps on looping fix it
    {
        bool added = false;
        for (int x = 0; x < matrix.GetLength(0); x++)
        {
            for (int y = 0; y < matrix.GetLength(1); y++)
            {
                if (x == 0)
                {
                    matrix[x, y].sides[2] = false;
                    matrix[x,y].sides[3] = false;
                    matrix[x, y].sides[0] = false;
                    if (!added)
                    {
                        visitedSquares.Add(new Vector2Int(x,y));
                        added = true;
                    }
                }
                if (y == 0)
                {
                    matrix[x, y].sides[1] = false;
                    matrix[x, y].sides[2] = false;
                    matrix[x, y].sides[3] = false;
                    if (!added)
                    {
                        visitedSquares.Add(new Vector2Int(x, y));
                        added = true;
                    }
                }
                if (x == matrix.GetLength(0) - 1)
                {
                    matrix[x, y].sides[0] = false;
                    matrix[x, y].sides[1] = false;
                    matrix[x, y].sides[2] = false;
                    if (!added)
                    {
                        visitedSquares.Add(new Vector2Int(x, y));
                        added = true;
                    }
                }
                if (y == matrix.GetLength(1) - 1)
                {
                    matrix[x, y].sides[3] = false;
                    matrix[x, y].sides[0] = false;
                    matrix[x, y].sides[1] = false;
                    if (!added)
                    {
                        visitedSquares.Add(new Vector2Int(x, y));
                        added = true;
                    }
                }
                added = false;
            }
        }

    }
    public void CreateEntrance()
    {
        for (int i  = (int)(maze.mazeSize / 2) + (int)maze.baseSize / 2; i < maze.mazeSize; i++)
        {
            int posX = Mathf.CeilToInt(maze.mazeSize / 2);
            int posY = i;
            Vector3 location = new Vector3((posX + 1) * maze.cubeSize - (maze.mazeSize * maze.cubeSize / 2) - maze.cubeSize / 2, 0, (posY + 1) * maze.cubeSize - (maze.mazeSize * maze.cubeSize / 2) - maze.cubeSize / 2);
            SquareData square = new SquareData(maze.cubeSize, location);
            matrix[posX, posY].canSpawn = false;
            matrix[posX, posY].visited = true;
            matrix[posX, posY].sides[0] = false;
            matrix[posX, posY].sides[2] = false;
            AddLockedSquare(new Vector2Int(posX, posY));
        }
    }
    public void CreateBase(int xPos, int yPos)
    {
        for (int x = 0; x < maze.baseSize; x++)
        {
            for (int y = 0; y < maze.baseSize; y++)
            {
                
                int xIndex = xPos - Mathf.FloorToInt(maze.baseSize / 2) + x;
                int yIndex = yPos - Mathf.FloorToInt(maze.baseSize / 2) + y;
                Vector3 location = new Vector3((xIndex + 1) * maze.cubeSize - (maze.mazeSize * maze.cubeSize / 2) - maze.cubeSize / 2, 0, (yIndex + 1) * maze.cubeSize - (maze.mazeSize * maze.cubeSize / 2) - maze.cubeSize / 2);
                SquareData square = new SquareData(maze.cubeSize, location);
                if (x == 0) { square.sides[3] = true; } else { square.sides[3] = false; }
                if (x == maze.baseSize - 1) { square.sides[1] = true; } else { square.sides[1] = false; }
                if (y == 0) { square.sides[2] = true; } else { square.sides[2] = false; }
                if (y == maze.baseSize - 1) { square.sides[0] = true; } else { square.sides[0] = false; }
                square.canSpawn = false;
                square.isLocked = true;
                matrix[xIndex,yIndex] = square;
                baseSquares.Add(new Vector2Int(xIndex, yIndex));
            }
        }
    }

    public void AddLockedSquare(Vector2Int location)
    {
        if (lockedSquares.Count == 10)
        {
            Debug.Log("Overiding last locked square");
            Vector2Int lastLocation = lockedSquares.Pop();
            Debug.Log(lastLocation + "Overided");
        }
        if (lockedSquares.Contains(location)) return;
        lockedSquares.Push(location);
        matrix[location.x, location.y].isLocked = true;
        if (lockedSquares.Count == 10)
        {
            Debug.Log("Maximum locked squares reached");
        }
    }

    public void RemoveLockedSquares()
    {
        int count = lockedSquares.Count;
        for (int i = 0; i < count; i++)
        {
            Vector2Int location = lockedSquares.Pop();
            matrix[location.x, location.y].isLocked = false;
        }
    }

    public void CalculatePlacesOfIntrest(List<Vector2Int> placesOfInterest, List<string> regions)
    {
        Vector2Int centerPoint = new Vector2Int(Mathf.CeilToInt(maze.mazeSize/2), Mathf.CeilToInt(maze.mazeSize / 2));
        bossLocations = new Vector3Int[maze.amountOfBosses];
        int count = 0;
        foreach(Vector2Int point in placesOfInterest)
        {
            if(count < maze.amountOfBosses && point.x > 0 && point.y > 0 && point.x < matrix.GetLength(0) - 1 && point.y < matrix.GetLength(1) - 1)
            {
                if(point.x >= centerPoint.x + maze.bossDst ||  point.x <= centerPoint.x - maze.bossDst || point.y >= centerPoint.y + maze.bossDst || point.y <= centerPoint.y - maze.bossDst && matrix[point.x, point.y].canSpawn)
                {
                    int percent = UnityEngine.Random.Range(0, 100);
                    if(percent <= maze.bossChance)
                    {
                        int rndRegion = UnityEngine.Random.Range(0, regions.Count);
                        bossLocations[count] = new Vector3Int(point.x, point.y, rndRegion);
                        string region = regions[rndRegion];
                        List<Vector3Int> squareRegions = LoadRegion(point.x, point.y, maze.regionSpread, false);
                        foreach (Vector3Int regVal in squareRegions)
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
        }
    }

    List<Vector2Int> AnalizeOptions(int xpos, int ypos, int offSet)
    {
        List<Vector2Int> options = new List<Vector2Int>();
        if (matrix.GetLength(1) - (1+offSet) >= ypos + 1 && matrix[xpos, ypos + 1].visited == false)//Up
        {
            options.Add(new Vector2Int(xpos, ypos + 1));
        }
        if (matrix.GetLength(0) - (1 + offSet) >= xpos + 1 && matrix[xpos + 1, ypos].visited == false)//To the right
        {
            options.Add(new Vector2Int(xpos + 1,ypos));
            options.Add(new Vector2Int(xpos + 1,ypos));
        }
        if (ypos - 1 >= offSet && matrix[xpos, ypos - 1].visited == false)//Down
        {
            options.Add(new Vector2Int(xpos, ypos - 1));
        }
        if (xpos - 1 >= offSet && matrix[xpos - 1, ypos].visited == false)//Left
        {
            options.Add(new Vector2Int(xpos - 1, ypos));
            options.Add(new Vector2Int(xpos - 1, ypos));
        }
        return options;
    }

    public List<Vector3Int> LoadRegion(int xPos, int yPos , int regionSpread, bool loadOver)
    {
        int offSet = 0;
        if(loadOver)
        {
            offSet = regionSpread;
        }
        regionLoadIndex++;
        Stack<Vector2Int> visited = new Stack<Vector2Int>();
        List<Vector3Int> regionSquares = new List<Vector3Int>();
        bool mazeComplete = false;
        int visitCount = 0;
        bool end = true;
        regionSquares.Add(new Vector3Int(xPos, yPos, 0));
        while (mazeComplete == false)
        {
            int regionValue = visited.Count + 1 + offSet;
            matrix[xPos, yPos].regionIndex = regionLoadIndex;
            
            Vector2Int option = RegionOptions(xPos, yPos, regionLoadIndex, regionValue);
            bool hasOptions = (option.x!=xPos || option.y!=yPos);
            Vector3Int oldSquare = new Vector3Int(xPos, yPos, regionValue);
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
                regionSquares.Add(new Vector3Int(xPos, yPos, regionValue));
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

    Vector2Int RegionOptions(int xPos, int yPos, int regionIndex, int regionValue)
    {
        Vector2Int result = new Vector2Int(xPos, yPos);
        if(xPos != matrix.GetLength(0) - 1)
        {
            if (matrix[xPos, yPos].sides[1] == false && matrix[xPos + 1, yPos].sides[3] == false && matrix[xPos + 1, yPos].regionIndex != regionIndex && matrix[xPos + 1, yPos].regionValue <= regionValue)//right
            {
                result = new Vector2Int(xPos + 1, yPos);
                return result;
            }
        }
        if(yPos != 0)
        {
            if (matrix[xPos, yPos].sides[2] == false && matrix[xPos, yPos - 1].sides[0] == false && matrix[xPos, yPos - 1].regionIndex != regionIndex && matrix[xPos, yPos - 1].regionValue <= regionValue)//down
            {
                result = new Vector2Int(xPos, yPos - 1);
                return result;
            }
        }
        if(xPos != 0)
        {
            if (matrix[xPos, yPos].sides[3] == false && matrix[xPos - 1, yPos].sides[1] == false && matrix[xPos - 1, yPos].regionIndex != regionIndex && matrix[xPos - 1, yPos].regionValue <= regionValue)//left
            {
                result = new Vector2Int(xPos - 1, yPos);
                return result;
            }
        }
        if(yPos != matrix.GetLength(1) - 1)
        {
            if (matrix[xPos, yPos].sides[0] == false && matrix[xPos, yPos + 1].sides[2] == false && matrix[xPos,yPos + 1].regionIndex != regionIndex && matrix[xPos, yPos + 1].regionValue <= regionValue)//up
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

