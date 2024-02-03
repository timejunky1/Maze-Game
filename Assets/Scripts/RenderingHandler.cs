using Palmmedia.ReportGenerator.Core;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class RenderingHandler : MonoBehaviour
{
    static RenderingHandler instance;
    static Queue<Vector3Int> squareRenderQueue;
    static Queue<Vector2Int> squaresRendered = new Queue<Vector2Int>();
    static List<Vector2Int> currentRender = new List<Vector2Int>();
    public TextureSettings textureSettings;
    public GameObject parent;
    public bool useQueue = true;
    public bool setRerender = true;
    bool installing = false;
    int renderThreadCount = 0;
    int renderThreadLimit = 6;
    bool newSquares = false;

    private void Awake()
    {
        squareRenderQueue = new Queue<Vector3Int>();
        instance = this;
    }

    private void Update()
    {
        //Debug.Log(renderThreadCount);
        if(squareRenderQueue.Count > 0) {
            ThreadStart render = delegate { RenderSquares(); };
            renderThreadCount++;
            render.Invoke();
        }
    }

    void RenderCubes(Queue<Vector3Int> renderSquares)//Third vector value is for distance and used for render quality
    {
        Vector3Int point;
        while (squareRenderQueue.Count > 0)
        {
            lock (squareRenderQueue)
            {
                try
                {
                    point = renderSquares.Dequeue();
                }
                catch
                {
                    break;
                }
            }
            Mazegeneration.matrix[point.x, point.y].RenderMesh(textureSettings, parent);
        }
    }

    public static void Render(List<Vector3Int> renderSquares)
    {
        instance.newSquares = true;
        currentRender.Clear();
        foreach (Vector3Int point in renderSquares)
        {
            lock (squareRenderQueue)
            {
                squareRenderQueue.Enqueue(point);
            }
            currentRender.Add(new Vector2Int(point.x, point.y));
        }
    }

    void RenderSquares()
    {
        //if(renderThreadCount >= renderThreadLimit){
        //    Debug.Log("Thread Limit Reached");
        //    return;
        //}
        if (useQueue)
        {
            instance.RenderCubes(squareRenderQueue);
        }
        else
        {
            lock(squareRenderQueue)
            {
                int count = squareRenderQueue.Count;
                for( int i = 0; i < count; i++ )
                {
                    //if (renderThreadCount >= renderThreadLimit)
                    //{
                    //    return;
                    //}
                    Vector3Int point = squareRenderQueue.Dequeue();
                    ThreadStart render = delegate { Mazegeneration.matrix[point.x, point.y].RenderMesh(textureSettings, parent); };
                    render.Invoke();
                }
            }
        }
        if (squareRenderQueue.Count == 0 && newSquares)
        {
            int count = squaresRendered.Count;
            for (int i = 0; i < count; i++)
            {
                Vector2Int point = squaresRendered.Dequeue();
                if (!currentRender.Contains(point))
                {
                    Mazegeneration.matrix[point.x, point.y].HideMesh();
                }

            }
            foreach(Vector2Int point in currentRender)
            {
                squaresRendered.Enqueue(point);
            }
            newSquares = false;
        }
    }

    static void EndRender()
    {
        if (Thread.CurrentThread.Name == "render")
        {
            Thread.CurrentThread.Abort();
            instance.renderThreadCount--;
            Debug.Log("Thread Ended");
        }
    }
}
