using RockVR.Video;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private RockVR.Video.VideoCaptureCtrl _video;
    [SerializeField] private bool _debugStart;
    public static string HostName = "DebugYokAnam";
    // Use this for initialization
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (!_debugStart)
        {
            var cmd = System.Environment.GetCommandLineArgs();
            var obj = GameObject.Find(cmd[1]);
            var agentLoc = GameObject.Find(cmd[2]);
            HostName = cmd[3];
            transform.position = agentLoc.transform.position;
            transform.rotation = agentLoc.transform.rotation;
            _agent.destination = obj.transform.position;
        }
        else
        {
            _agent.destination = _door.position;
        }
        //if (agent.SetDestination(door.localPosition))
        //{
        _video = GetComponentInChildren<VideoCaptureCtrl>();
        _video.StartCapture();
        VideoCapture.ProcessFinished += VideoProcessFinished;
        //}
    }

    private void VideoProcessFinished()
    {
        Application.Quit();
    }

    //public Camera cam;
    [SerializeField]
    private Transform _door;
    private NavMeshAgent _agent;
    bool _stoppedCapture = false;
    // Update is called once per frame
    void Update()
    {

        if (!_agent.pathPending)
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                {
                    //agent.transform.LookAt(door.transform);
                    if (!_stoppedCapture)
                    {
                        _video.StopCapture();
                        _stoppedCapture = true;
                    }
                }
            }
        }

    }
}
