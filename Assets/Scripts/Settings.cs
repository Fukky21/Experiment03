﻿using UnityEngine;

public class Settings
{
    public static Vector3 cameraPosition = new Vector3(0, 0, 0);
    public static Vector3 topBeltPosition = new Vector3(0, 0.1f, 1);
    public static Vector3 bottomBeltPosition = new Vector3(0, -0.1f, 1);
    public static float movingAngle = 50; // 片側の動く角度[deg]
    public static float movingOneWayTime = 1.0f; // 片道の時間[sec]
    public static float stimulusPresentationTime = 0.5f; // 刺激提示時間[sec]

    public static float movingSpeed()
    {
        return movingAngle * 2 / movingOneWayTime; // [deg/s]
    }

    public static int repeat = 3;
    public static double[] ratio = new double[] {-0.5, -0.4, -0.3, -0.2, -0.1, 0, 0.1, 0.2, 0.3, 0.4, 0.5};
    //public static double[] ratio = new double[] {-0.4, -0.32, -0.24, -0.16, -0.08, 0, 0.08, 0.16, 0.24, 0.32, 0.4};
    //public static double[] ratio = new double[] {-1.0, -0.8, -0.6, -0.4, -0.2, 0, 0.2, 0.4, 0.6, 0.8, 1.0};
    //public static double[] ratio = new double[] {-5.0, -4.0, -3.0, -2.0, -1.0, 0, 1.0, 2.0, 3.0, 4.0, 5.0};

    public static int nTrials()
    {
        return ratio.Length * repeat;
    }

    public static int shotCount = 4; // この回数分だけ反復した後に刺激が提示される
    //public static int shotCount = 1;
    public static float beltBaseSpeed = 12.0f; // [deg/s]
}