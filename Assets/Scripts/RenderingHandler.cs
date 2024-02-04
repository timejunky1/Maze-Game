using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class RenderingHandler
{
    static Queue<Vector3Int> squareRenderQueue;
    static List<Vector2Int> squaresRendered;
    static Queue<Vector2Int> squareDerenderQueue;
    static List<Vector2Int> currentRender;
    TextureSettings textureSettings;
    GameObject parent;

    public RenderingHandler(GameObject parent, TextureSettings textureSettings)
    {
        squareRenderQueue = new Queue<Vector3Int>();
        squaresRendered = new List<Vector2Int>();
        squareDerenderQueue = new Queue<Vector2Int>();
        currentRender = new List<Vector2Int>();
        this.parent = parent;
        this.textureSettings = textureSettings;
    }

    public void AddRender(List<Vector3Int> cubes)
    {
        currentRender.Clear();
        foreach (var c in cubes)
        {
            Vector2Int v = new Vector2Int(c.x, c.y);
            currentRender.Add(v);
            if (squaresRendered.Contains(v))
            {
                continue;
            }
            squareRenderQueue.Enqueue(c);
        }
        foreach (var c in squaresRendered)
        {
            if(currentRender.Contains(c))
            {
                continue;
            }
            squareDerenderQueue.Enqueue(c);
        }
    }
    public void RenderCubes()//Third vector value is for distance and used for render quality
    {
        Vector3Int point;
        while (squareRenderQueue.Count > 0)
        {
            try
            {
                point = squareRenderQueue.Dequeue();
                Mazegeneration.matrix[point.x, point.y].RenderMesh(textureSettings, parent);
                squaresRendered.Add(new Vector2Int(point.x, point.y));
                Debug.Log($"Render Cube");
            }
            catch
            {
                break;
            }
        }
    }

    public void DerenderCubes()//Third vector value is for distance and used for render quality
    {
        Vector2Int point;
        while (squareDerenderQueue.Count > 0)
        {
            try
            {
                point = squareDerenderQueue.Dequeue();
                Mazegeneration.matrix[point.x, point.y].HideMesh();
                squaresRendered.Remove(new Vector2Int(point.x, point.y));
                Debug.Log($"Derender Cube");
            }
            catch
            {
                break;
            }
        }
    }
}
