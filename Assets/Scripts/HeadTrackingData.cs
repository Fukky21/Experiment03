using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTrackingData
{
    public HeadTrackingData(Transform transform)
    {
        this.positionX = transform.position.x;
        this.positionY = transform.position.y;
        this.positionZ = transform.position.z;
        this.rotationX = transformAngle180(transform.eulerAngles.x);
        this.rotationY = transformAngle180(transform.eulerAngles.y);
        this.rotationZ = transformAngle180(transform.eulerAngles.z);
    }

    public float positionX;
    public float positionY;
    public float positionZ;
    public float rotationX;
    public float rotationY;
    public float rotationZ;

    // [0, 360]degから[-180, 180]degへ変換する
    private float transformAngle180(float angle)
    {
        if (angle > 180)
        {
            return angle - 360;
        }

        return angle;
    } 
}
