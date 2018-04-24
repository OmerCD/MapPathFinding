using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneAdjust : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
#if UNITY_EDITOR
        UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
#endif
    }
}
