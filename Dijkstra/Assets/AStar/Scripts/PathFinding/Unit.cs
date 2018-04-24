using RockVR.Video;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    const float MinPathUpdateTime = .2f;
    const float PathUpdateMoveTreshold = .5f;
    [SerializeField]
    Transform _target;
    [SerializeField]
    private float _speed = 20;
    [SerializeField]
    private float turnDst = 5;
    private Path _path;
    [SerializeField]
    private float _turnSpeed = 3;
    private VideoCaptureCtrl _video;

    [SerializeField] private float _stoppingDistance = 10f;

    void Start()
    {
        _video = GetComponentInChildren<VideoCaptureCtrl>();
        _video.StartCapture();
        StartCoroutine(UpdatePath());
    }
    public void OnPathFound(Vector3[] wayPoints, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            _path = new Path(wayPoints, transform.position, turnDst,_stoppingDistance);
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator UpdatePath()
    {
        if(Time.timeSinceLevelLoad<.3f)
            yield return new WaitForSeconds(.3f);
        PathRequestManager.RequestPath(new PathRequest(transform.position, _target.position, OnPathFound));
        float sqrtMoveTreshold = PathUpdateMoveTreshold * PathUpdateMoveTreshold;
        Vector3 targetPosOld = _target.position;
        while (true)
        {
            yield return new WaitForSeconds(MinPathUpdateTime);
            if ((_target.position - targetPosOld).sqrMagnitude > sqrtMoveTreshold)
                PathRequestManager.RequestPath(new PathRequest(transform.position, _target.position, OnPathFound));
            targetPosOld = _target.position;
        }
    }
    IEnumerator FollowPath()
    {
        bool followingPath = true;
        int pathIndex = 0;
        transform.LookAt(_path.LookPoints[0]);

        float speedPercent = 1;
        while (followingPath)
        {
            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            while (_path.TurnBoundaries[pathIndex].HasCrossedLine((pos2D)))
            {
                if (pathIndex == _path.FinishLineIndex)
                {
                    followingPath = false;
                    break;
                }
                else
                {
                    pathIndex++;
                }
            }
            if (followingPath)
            {
                if (pathIndex >= _path.SlowDownIndex && _stoppingDistance > 0)
                {
                    speedPercent =
                        Mathf.Clamp01(_path.TurnBoundaries[_path.FinishLineIndex].DistanceFromPoint((pos2D))/
                                      _stoppingDistance);
                    if (speedPercent<0.01f)
                    {
                        followingPath = false;
                    }
                }
                Quaternion targetRotation = Quaternion.LookRotation(_path.LookPoints[pathIndex] - transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * _turnSpeed);
                transform.Translate(Vector3.forward * Time.deltaTime * _speed*speedPercent, Space.Self);
            }
            yield return null;
        }
        _video.StopCapture();
    }
    public void OnDrawGizmos()
    {
        if (_path != null)
        {
            _path.DrawWithGizmos();
        }
    }
}
