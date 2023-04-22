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
    static Queue<SquareData> squareRenderQueue;
    static Queue<SquareData> squaresRenderedQueue;
    static List<SquareData> currentRender;
    TextureSettings settings;
    public GameObject parent;
    public bool useQueue = true;
    int renderThreadCount = 0;
    int renderThreadLimit = 6;
    bool newSquares = true;

    private void Awake()
    {
        squareRenderQueue = new Queue<SquareData>();
        instance = this;
        settings = new TextureSettings();
    }

    private void Update()
    {
        if(squareRenderQueue.Count > 0) {
            ThreadStart render = delegate { RenderSquares(); };
            renderThreadCount++;
            render.Invoke();
        }
    }

    void RenderCubes(Queue<SquareData> renderSquares)
    {
        lock (squareRenderQueue)
        {
            int count = squareRenderQueue.Count;
            for (int i = 0; i < count; i++)
            {
                renderSquares.Dequeue().RenderMesh(settings, parent);
            }
        }
    }

    static void Render(List<SquareData> renderSquares)
    {
        foreach (SquareData square in renderSquares)
        {
            if (!square.isRendered)
            {
                squaresRenderedQueue.Enqueue(square);
                squareRenderQueue.Enqueue(square);
            }
        }
        currentRender = renderSquares;
    }

    static void RenderSquares()
    {
        if(instance.renderThreadCount >= instance.renderThreadLimit){
            return;
        }
        if (instance.useQueue)
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
                    if (instance.renderThreadCount >= instance.renderThreadLimit)
                    {
                        return;
                    }
                    ThreadStart render = delegate { squareRenderQueue.Dequeue().RenderMesh(instance.settings, instance.parent); };
                    instance.renderThreadCount ++;
                    render.Invoke();
                }
            }
        }
        if (instance.renderThreadCount < instance.renderThreadLimit && instance.newSquares == true && squareRenderQueue.Count == 0)
        {
            lock (squaresRenderedQueue)
            {
                int count = squareRenderQueue.Count;
                for (int i = 0; i < count; i++)
                {
                    SquareData square = squaresRenderedQueue.Dequeue();
                    if (!currentRender.Contains(square))
                    {
                        square.HideMesh();
                    }
                }
                foreach(SquareData square in currentRender)
                {
                    squaresRenderedQueue.Enqueue(square);
                }
                instance.newSquares= false;
            }
        }
    }

    static void EndRender()
    {
        if (Thread.CurrentThread.Name == "render")
        {
            Thread.CurrentThread.Abort();
            instance.renderThreadCount--;
        }
    }
}
