using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PracticeSwingManager : MonoBehaviour
{
    public GameObject FixationPoint;
    public AudioClip sound;
    AudioSource audioSource;
    public CommonConfig config;
    private static bool isMoving;
    private static float movingAngle; // 片側の動く角度[deg]
    private static float movingOneWayTime; // 片道の時間[sec]
    private bool isMovingToRight = false;
    private float currentAngle = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        config = new CommonConfig();
        isMoving = false;
    }

    void Update()
    {
        if (isMoving)
        {
            if (isMovingToRight)
            {
                FixationPoint.transform.RotateAround(config.cameraPosition, Vector3.up, movingSpeed() * Time.deltaTime);
                currentAngle += movingSpeed() * Time.deltaTime;

                if (currentAngle > movingAngle)
                {
                    audioSource.PlayOneShot(sound);
                    isMovingToRight = false;
                }
            }
            else
            {
                FixationPoint.transform.RotateAround(config.cameraPosition, Vector3.up, -movingSpeed() * Time.deltaTime);
                currentAngle -= movingSpeed() * Time.deltaTime;

                if (currentAngle < -movingAngle)
                {
                    audioSource.PlayOneShot(sound);
                    isMovingToRight = true;
                }
            }
        }
    }

    private float movingSpeed()
    {
        return movingAngle * 2 / movingOneWayTime; // [deg/s]
    }

    public static void StartSwing(float movingAngle, float movingOneWayTime)
    {
        PracticeSwingManager.movingAngle = movingAngle;
        PracticeSwingManager.movingOneWayTime = movingOneWayTime;
        PracticeSwingManager.isMoving = true;
    }

    public static void StopSwing()
    {
        PracticeSwingManager.isMoving = false;
    }
}
