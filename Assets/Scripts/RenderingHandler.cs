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
    static Queue<Vector3Int> squareRenderQueue = new Queue<Vector3Int>();
    static Queue<Vector3Int> squaresRenderedQueue = new Queue<Vector3Int>();
    static List<Vector3Int> currentRender;
    public TextureSettings settings;
    public GameObject parent;
    public bool useQueue = true;
    int renderThreadCount = 0;
    int renderThreadLimit = 6;
    bool newSquares = true;

    private void Awake()
    {
        squareRenderQueue = new Queue<Vector3Int>();
        instance = this;
    }

    private void Update()
    {
        Debug.Log(renderThreadCount);
        if(squareRenderQueue.Count > 0) {
            ThreadStart render = delegate { RenderSquares(); };
            renderThreadCount++;
            render.Invoke();
        }
        Debug.Log("current render count"+currentRender.Count);
        Debug.Log("to be rendered count"+squareRenderQueue.Count);
        Debug.Log("all current rendered"+squaresRenderedQueue.Count);
    }

    void RenderCubes(Queue<Vector3Int> renderSquares)
    {
        lock (squareRenderQueue)
        {
            int count = squareRenderQueue.Count;
            for (int i = 0; i < count; i++)
            {
                Vector3Int point = renderSquares.Dequeue();
                Mazegeneration.matrix[point.x, point.y].RenderMesh(settings, parent);
            }
        }
    }

    public static void Render(List<Vector3Int> renderSquares)
    {
        instance.newSquares = true;
        foreach (Vector3Int point in renderSquares)
        {
            if (!Mazegeneration.matrix[point.x, point.y].isRendered)
            {
                squaresRenderedQueue.Enqueue(point);
                squareRenderQueue.Enqueue(point);
            }
        }
        currentRender = renderSquares;
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
                    ThreadStart render = delegate { Mazegeneration.matrix[point.x, point.y].RenderMesh(settings, parent); };
                    render.Invoke();
                }
            }
        }
        if (newSquares == true && squareRenderQueue.Count == 0)
        {
            Debug.Log("derender");
            lock (squaresRenderedQueue)
            {
                int count = squaresRenderedQueue.Count;
                for (int i = 0; i < count; i++)
                {
                    Vector3Int point = squaresRenderedQueue.Dequeue();
                    if (!currentRender.Contains(point))
                    {
                        Debug.Log("HideMesh");
                        Mazegeneration.matrix[point.x, point.y].HideMesh();
                    }
                }
                foreach(Vector3Int point in currentRender)
                {
                    squaresRenderedQueue.Enqueue(point);
                }
                newSquares= false;
            }
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
