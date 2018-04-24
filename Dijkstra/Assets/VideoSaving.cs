using RockVR.Video;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VideoSaving : MonoBehaviour {

	// Use this for initialization
	void Start () {
        var video = GetComponent<VideoCapture>();
        video.StartCapture();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
