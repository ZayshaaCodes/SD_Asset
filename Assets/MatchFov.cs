using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//based on a target camera, copy the fov to this camera, null checking
[ExecuteInEditMode, RequireComponent(typeof(Camera))]
public class MatchFov : MonoBehaviour
{
    public Camera targetCamera;

    private void Update()
    {
        if (targetCamera)
            GetComponent<Camera>().fieldOfView = targetCamera.fieldOfView;
    }
}

