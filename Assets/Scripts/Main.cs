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
    public GameObject FixationPoint;
    public GameObject MovePoint;
    public AudioClip sound;
    AudioSource audioSource;
    private int phase = 0;
    private bool isFreeMode = false;
    private float time = 0;
    private float localTime = 0;
    private bool isMovingToRight = false;
    private float currentAngle = 0;
    private string selectModeText = "Press Up => Start Recording in Free mode\n\nPress Down => Start Recording in Fix mode";
    private string finishRecordingText = "Finish Recording!\n\nPress ESC => Quit App";

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        PupilLabs.EyeTrackingDataManager.InitializeDataFile(fileName);
        HeadTrackingDataManager.InitializeDataFile(fileName);
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
            // キャリブレーション終了後
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeGuideText(selectModeText, true);
                phase++;
            }
        }
        else if (phase == 1)
        {
            // モード選択中
            if (RespUp.GetState(HandType))
            {
                // Free mode
                ChangeGuideText(null, false);
                MovePoint.SetActive(true);
                PupilLabs.EyeTrackingDataManager.StartRecording();
                HeadTrackingDataManager.StartRecording();
                isFreeMode = true;
                phase++;
            }

            if (RespDown.GetState(HandType))
            {
                // Fix mode
                ChangeGuideText(null, false);
                FixationPoint.SetActive(true);
                PupilLabs.EyeTrackingDataManager.StartRecording();
                HeadTrackingDataManager.StartRecording();
                isFreeMode = false;
                phase++;
            }
        }
        else if (phase == 2)
        {
            // 記録中
            Do();
            time += Time.deltaTime;

            if (RespTrigger.GetState(HandType))
            {
                PupilLabs.EyeTrackingDataManager.StopRecording();
                HeadTrackingDataManager.StopRecording();
                MovePoint.SetActive(false);
                FixationPoint.SetActive(false);
                ChangeGuideText(finishRecordingText, true);
                phase++;
            }
        }
    }

    private void SetupCamera()
    {
        MainCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        MainCamera.GetComponent<Camera>().backgroundColor = Color.black;
    }

    private void ChangeGuideText(string text, bool isEnabled)
    {
        GuideText.text = text;
        GuideText.enabled = isEnabled;
    }

    private void Do()
    {
        if (isFreeMode)
        {
            if (isMovingToRight)
            {
                MovePoint.transform.RotateAround(Settings.cameraPosition, Vector3.up, Settings.movingSpeed() * Time.deltaTime);
                currentAngle += Settings.movingSpeed() * Time.deltaTime;

                if (currentAngle > Settings.movingAngle)
                {
                    audioSource.PlayOneShot(sound);
                    isMovingToRight = false;
                }
            }
            else
            {
                MovePoint.transform.RotateAround(Settings.cameraPosition, Vector3.up, -Settings.movingSpeed() * Time.deltaTime);
                currentAngle -= Settings.movingSpeed() * Time.deltaTime;

                if (currentAngle < -Settings.movingAngle)
                {
                    audioSource.PlayOneShot(sound);
                    isMovingToRight = true;
                }
            }
        }
        else
        {
            localTime += Time.deltaTime;
            if (localTime > Settings.movingOneWayTime)
            {
                audioSource.PlayOneShot(sound);
                localTime = 0;
            }
        }
    }
}
