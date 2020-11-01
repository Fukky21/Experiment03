using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    [Header("Settings")]
    public string fileName; // 拡張子は付けずに指定

    public Camera MainCamera;
    public GameObject Sphere;

    OutputResult outputResult;
    private int phase = 0;
    private float globalTime = 0f;

    void Start()
    {
        outputResult = new OutputResult(fileName);
        SetupCamera();
        SetupSphere();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (phase == 0)
        {
            // キャリブレーション終了～記録開始準備
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (outputResult.initialize() == -1)
                {
                    Application.Quit();
                }
                Sphere.GetComponent<Renderer>().material.color = Color.green;
                Sphere.SetActive(true);
                phase++;
            }
        }
        else if (phase == 1)
        {
            // 記録開始準備～記録開始
            if (Input.GetKeyDown(KeyCode.S))
            {
                Sphere.GetComponent<Renderer>().material.color = Color.red;
                phase++;
            }
        }
        else if (phase == 2)
        {
            // 記録開始～記録終了
            outputResult.writeEyeData(
                PupilLabs.EyeTrackingDataManager.getEyeTrackingData(),
                globalTime
            );
            globalTime += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.E))
            {
                Sphere.GetComponent<Renderer>().material.color = Color.black;
                phase++;
            }
        }
    }

    void SetupCamera()
    {
        MainCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        MainCamera.GetComponent<Camera>().backgroundColor = Color.gray;
    }

    void SetupSphere()
    {
        Sphere.SetActive(false);
    }
}
