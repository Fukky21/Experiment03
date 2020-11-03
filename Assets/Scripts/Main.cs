using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;

public class Main : MonoBehaviour
{   
    [Header("Settings")]
    public string fileName; // 拡張子は付けずに指定

    public Camera MainCamera;
    public SteamVR_Input_Sources HandType;
    public SteamVR_Action_Boolean RespUp;
    public SteamVR_Action_Boolean RespDown;
    public SteamVR_Action_Boolean RespTrigger;
    public Text GuideText;
    public CommonConfig config;
    public GameObject FixationPoint;
    OutputResult outputResult;
    private int phase = 0;
    private float globalTime = 0;

    void Start()
    {
        config = new CommonConfig();
        outputResult = new OutputResult(fileName);
        SetupCamera();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (phase == 0)
        {
            // キャリブレーション終了～スイングモード選択
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (outputResult.initialize() == -1)
                {
                    Application.Quit();
                }
                GuideText.text = "Press Up => Free\n\nPress Down => Fix";
                GuideText.enabled = true;
                phase++;
            }
        }
        else if (phase == 1)
        {
            // スイングモード選択～データ記録開始
            if (RespUp.GetState(HandType))
            {
                GuideText.enabled = false;
                FixationPoint.transform.position = new Vector3(0, 0, config.distance);
                FixationPoint.SetActive(true);
                PracticeSwingManager.StartSwing(50, 2);
                SwitchCamFix(false);
                phase++;
            }

            if (RespDown.GetState(HandType))
            {
                GuideText.enabled = false;
                FixationPoint.transform.position = new Vector3(0, 0, config.distance);
                FixationPoint.SetActive(true);
                // TODO: Fix実装
                SwitchCamFix(true);
                phase++;
            }
        }
        else if (phase == 2)
        {
            // データ記録開始～データ記録終了
            outputResult.writeData(
                PupilLabs.EyeTrackingDataManager.getEyeTrackingData(),
                new HeadTrackingData(MainCamera.transform),
                globalTime
            );
            globalTime += Time.deltaTime;

            if (RespTrigger.GetState(HandType))
            {
                FixationPoint.SetActive(false);
                PracticeSwingManager.StopSwing();
                GuideText.text = "Finish";
                GuideText.enabled = true;
                phase++;
            }
        }
    }

    void SetupCamera()
    {
        MainCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        MainCamera.GetComponent<Camera>().backgroundColor = Color.black;
    }

    void SwitchCamFix(bool isFixed)
    {
        MainCamera.transform.position = new Vector3(0, 0, 0);
        MainCamera.transform.rotation = Quaternion.Euler(0, 0, 0);
        XRDevice.DisableAutoXRCameraTracking(MainCamera, isFixed);
    }
}
