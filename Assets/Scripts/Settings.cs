using UnityEngine;

public class Settings
{
    public static Vector3 cameraPosition = new Vector3(0, 0, 0);
    public static Vector3 fixationPointPosition = new Vector3(0, 0, 20);
    public static float movingAngle = 50; // 片側の動く角度[deg]
    public static float movingOneWayTime = 2.0f; // 片道の時間[sec]

    public static float movingSpeed()
    {
        return movingAngle * 2 / movingOneWayTime; // [deg/s]
    }
}