using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestManager : MonoBehaviour
{
    Queue<PathResult> results = new Queue<PathResult>();
    private static PathRequestManager _instance;
    private PathFinding _pathFinding;
    void Awake()
    {
        _instance = this;
        _pathFinding = GetComponent<PathFinding>();
    }

    void Update()
    {
        if (results.Count>0)
        {
            int itemsInQueue = results.Count;
            lock (results)
            {
                for (int i = 0; i < itemsInQueue; i++)
                {
                    PathResult result = results.Dequeue();
                    result.Callback(result.Path, result.Success);
                }
            }
        }
    }
    public static void RequestPath(PathRequest request)
    {
        ThreadStart threadStart = delegate
         {
             _instance._pathFinding.FindPath(request, _instance.FinishedProcessingPath);
         };
        threadStart.Invoke();
    }
    public void FinishedProcessingPath(PathResult result)
    {
        lock (results)
        {
            results.Enqueue(result);
        }
    }

}

public struct PathResult
{
    public Vector3[] Path;
    public bool Success;
    public Action<Vector3[], bool> Callback;

    public PathResult(Vector3[] path, bool success, Action<Vector3[], bool> callback)
    {
        Path = path;
        Success = success;
        Callback = callback;
    }
}
public struct PathRequest
{
    public Vector3 PathStart;
    public Vector3 PathEnd;
    public Action<Vector3[], bool> Callback;

    public PathRequest(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        PathStart = pathStart;
        PathEnd = pathEnd;
        Callback = callback;
    }
}