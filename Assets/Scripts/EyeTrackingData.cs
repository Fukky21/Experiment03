using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTrackingData {
    public EyeTrackingData(
        Vector3 localGazeDirection,
        float gazeDistance,
        float lastConfidence,
        Vector3 gazeNormalLeft,
        Vector3 gazeNormalRight,
        Vector3 eyeCenterLeft,
        Vector3 eyeCenterRight
    )
    {
        this.localGazeDirection = localGazeDirection;
        this.gazeDistance = gazeDistance;
        this.lastConfidence = lastConfidence;
        this.gazeNormalLeft = gazeNormalLeft;
        this.gazeNormalRight = gazeNormalRight;
        this.eyeCenterLeft = eyeCenterLeft;
        this.eyeCenterRight = eyeCenterRight;
    }

    public Vector3 localGazeDirection;
    public float gazeDistance;
    public float lastConfidence;
    public Vector3 gazeNormalLeft;
    public Vector3 gazeNormalRight;
    public Vector3 eyeCenterLeft;
    public Vector3 eyeCenterRight;
}