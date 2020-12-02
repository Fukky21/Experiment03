using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
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
    public GameObject MovingPoint;
    public GameObject TopBelt;
    public GameObject BottomBelt;
    public AudioClip sound;
    AudioSource audioSource;
    System.Random rnd;
    private int phase = 0;
    private bool isPracticeMode = false;
    private int step = 0;
    private int currentTrial = 1;
    private int currentShotCount = 0;
    private bool topBeltMoveToRight;
    private int addedShotCount = 0;
    private double[] beltSpeedRatio;
    private float[] beltSpeedToLeft;
    private float[] beltSpeedToRight;
    private List<bool> topBeltMoveToRights;
    private List<bool> respUps;
    private List<float> onsetTimes;
    private float time = 0;
    private float localTime = 0;
    private float onsetTime = 0;
    private bool isMovingToRight = false;
    private float currentAngle = 0;
    private string selectModeText = "Press Up: Start Practice\n\nPress Down: Start Experiment";
    private string finishRecordingText = "Finish\n\nPress ESC: Quit App";

    Transform topBeltTransform;
    Transform bottomBeltTransform;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rnd = new System.Random();
        topBeltMoveToRights = new List<bool>();
        respUps = new List<bool>();
        onsetTimes = new List<float>();
        topBeltTransform = TopBelt.transform;
        bottomBeltTransform = BottomBelt.transform;
        SetupCamera();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PupilLabs.EyeTrackingDataManager.StopRecording(fileName);
            HeadTrackingDataManager.StopRecording(fileName);
            WriteData();
            Application.Quit();
        }

        if (phase == 0) // キャリブレーション終了後
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ChangeGuideText(selectModeText, true);
                phase++;
            }
        }
        else if (phase == 1)　// モード選択中
        {
            if (RespUp.GetState(HandType))
            {
                // Start Practice
                MovingPoint.SetActive(true);
                isPracticeMode = true;
                ChangeGuideText(null, false);
                PupilLabs.EyeTrackingDataManager.StartRecording();
                HeadTrackingDataManager.StartRecording();
                phase++;
            }

            if (RespDown.GetState(HandType))
            {
                // Start Experiment
                SetupParameters();
                isPracticeMode = false;
                ChangeGuideText(trialText(currentTrial), true);
                PupilLabs.EyeTrackingDataManager.StartRecording();
                HeadTrackingDataManager.StartRecording();
                phase++;
            }
        }
        else if (phase == 2) // 記録中
        {
            if (isPracticeMode)
            {
                DoPractice();
                time += Time.deltaTime;
                // トリガーを引いたら終了
                if (RespTrigger.GetState(HandType))
                {
                    PupilLabs.EyeTrackingDataManager.StopRecording(fileName);
                    HeadTrackingDataManager.StopRecording(fileName);
                    MovingPoint.SetActive(false);
                    FixationPoint.SetActive(false);
                    ChangeGuideText(finishRecordingText, true);
                    phase++;
                }
            }
            else
            {
                DoExperiment();
                time += Time.deltaTime;
                // 規定のトライアル数を終えたとき、stepの値は6になる
                if (step == 6)
                {
                    PupilLabs.EyeTrackingDataManager.StopRecording(fileName);
                    HeadTrackingDataManager.StopRecording(fileName);
                    WriteData();
                    ChangeGuideText(finishRecordingText, true);
                    phase++;
                }
            }
        }
    }

    int ConvertBoolToInt(bool b)
    {
        if (b)
        {
            return 1;
        }
        return 0;
    }

    float Tan(float x)
    {
        return (float)Math.Tan(x * (Math.PI / 180));
    }

    Type[] FisherYates<Type>(Type[] arr)
    {
        int n = arr.Length;
        for (int i = n - 1; i > 0; i--)
        {
            int j = (int)Math.Floor(rnd.NextDouble() * (i + 1));
            Type tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }
        return arr;
    }

    Dictionary<string, float> ConvertRatioToSpeed(double ratio)
    {

        double fast = Settings.beltBaseSpeed * Math.Sqrt(Math.Pow(2, Math.Abs(ratio)));
        double slow = Settings.beltBaseSpeed / Math.Sqrt(Math.Pow(2, Math.Abs(ratio)));

        if (ratio > 0)
        {
            return new Dictionary<string, float>() {
                {"speedToLeft", (float)Math.Round(slow, 1)},
                {"speedToRight", (float)Math.Round(fast, 1)}
            };
        }
        else
        {
            return new Dictionary<string, float>() {
                {"speedToLeft", (float)Math.Round(fast, 1)},
                {"speedToRight", (float)Math.Round(slow, 1)}
            };
        }
    }

    private void SetupCamera()
    {
        MainCamera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
        MainCamera.GetComponent<Camera>().backgroundColor = Color.black;
    }

    // Setup beltSpeedRatio, beltSpeedToLeft, beltSpeedToRight
    private void SetupParameters()
    {
        double[] tmpSpeedRatio = new double[Settings.nTrials()];
        beltSpeedRatio = new double[Settings.nTrials()];
        beltSpeedToLeft = new float[Settings.nTrials()];
        beltSpeedToRight = new float[Settings.nTrials()];

        for (int i = 0; i < Settings.repeat; i++)
        {
            for (int j = 0; j < Settings.ratio.Length; j++)
            {
                tmpSpeedRatio[Settings.ratio.Length * i + j] = Settings.ratio[j];
            }
        }
        Array.Copy(tmpSpeedRatio, beltSpeedRatio, tmpSpeedRatio.Length);
        beltSpeedRatio = FisherYates(beltSpeedRatio);
        for (int i = 0; i < beltSpeedRatio.Length; i++)
        {
            beltSpeedToLeft[i] = ConvertRatioToSpeed(beltSpeedRatio[i])["speedToLeft"];
            beltSpeedToRight[i] = ConvertRatioToSpeed(beltSpeedRatio[i])["speedToRight"];
        }
    }

    private void WriteData()
    {
        // Resultsフォルダが存在しないときは作成する
        string folderPath = Application.dataPath + "/Results";
        if (!Directory.Exists(folderPath))
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            di.Create();
        }
        string filePath = Application.dataPath + $"/Results/{fileName}_ex_data.txt";
        FileInfo fi = new FileInfo(filePath);
        string header = "trial ratio top_belt_move_to_right resp_up onset_time";
        using (StreamWriter sw = fi.CreateText())
        {
            sw.WriteLine(header);
            for (int i = 0; i < onsetTimes.Count; i++)
            {
                string dataTxt = $"{i+1} {beltSpeedRatio[i]} {ConvertBoolToInt(topBeltMoveToRights[i])}";
                dataTxt += $" {ConvertBoolToInt(respUps[i])} {onsetTimes[i]}";
                sw.WriteLine(dataTxt);
            }
            sw.Close();
        }
    }

    private void ChangeGuideText(string text, bool isEnabled)
    {
        GuideText.text = text;
        GuideText.enabled = isEnabled;
    }

    private string trialText(int currentTrial)
    {
        return $"{currentTrial}/{Settings.nTrials()}";
    }

    private void DoPractice()
    {
        if (isMovingToRight)
        {
            MovingPoint.transform.RotateAround(Settings.cameraPosition, Vector3.up, Settings.movingSpeed() * Time.deltaTime);
            currentAngle += Settings.movingSpeed() * Time.deltaTime;

            if (currentAngle > Settings.movingAngle)
            {
                audioSource.PlayOneShot(sound);
                isMovingToRight = false;
            }
        }
        else
        {
            MovingPoint.transform.RotateAround(Settings.cameraPosition, Vector3.up, -Settings.movingSpeed() * Time.deltaTime);
            currentAngle -= Settings.movingSpeed() * Time.deltaTime;

            if (currentAngle < -Settings.movingAngle)
            {
                audioSource.PlayOneShot(sound);
                isMovingToRight = true;
            }
        }
    }

    private void DoExperiment()
    {
        if (step == 0) // 現在のトライアル数を表示中
        {
            if (RespTrigger.GetState(HandType))
            {
                ChangeGuideText(null, false);
                FixationPoint.SetActive(true);
 
                // addedShotCountを決定
                if (rnd.NextDouble() < 0.3333)
                {
                    addedShotCount = 1;
                }
                else if (rnd.NextDouble() < 0.6666)
                {
                    addedShotCount = 2;
                }
                else
                {
                    addedShotCount = 3;
                }
                // topBeltMoveToRightを決定
                if (rnd.NextDouble() < 0.5)
                {
                    topBeltMoveToRight = false;
                }
                else
                {
                    topBeltMoveToRight = true;
                }

                step++;
            }
        }
        else if (step == 1) // 一定回数反復中
        {
            localTime += Time.deltaTime;
            if (localTime > Settings.movingOneWayTime)
            {
                audioSource.PlayOneShot(sound);
                localTime = 0;
                if (++currentShotCount == Settings.shotCount + addedShotCount)
                {
                    step++;
                }
            }
        }
        else if (step == 2) // 刺激提示直前(反復中心で刺激を提示するための調整を行う)
        {
            localTime += Time.deltaTime;
            if (localTime > (Settings.movingOneWayTime - Settings.stimulusPresentationTime) / 2)
            {
                onsetTime = time;
                localTime = 0;
                TopBelt.SetActive(true);
                BottomBelt.SetActive(true);
                step++;
            }
        }
        else if (step == 3) // 刺激提示中
        {
            if (topBeltMoveToRight)
            {
                topBeltTransform.Translate(Tan(beltSpeedToRight[currentTrial - 1]) * Time.deltaTime, 0, 0);
                bottomBeltTransform.Translate(-Tan(beltSpeedToLeft[currentTrial - 1]) * Time.deltaTime, 0, 0);
            }
            else
            {
                topBeltTransform.Translate(-Tan(beltSpeedToLeft[currentTrial - 1]) * Time.deltaTime, 0, 0);
                bottomBeltTransform.Translate(Tan(beltSpeedToRight[currentTrial - 1]) * Time.deltaTime, 0, 0);
            }
            localTime += Time.deltaTime;
            if (localTime > Settings.stimulusPresentationTime)
            {
                TopBelt.SetActive(false);
                BottomBelt.SetActive(false);
                FixationPoint.SetActive(false);
                step++;
            }
        }
        else if (step == 4) // 応答待ち
        {
            if (RespUp.GetState(HandType))
            {
                topBeltMoveToRights.Add(topBeltMoveToRight);
                respUps.Add(true);
                onsetTimes.Add(onsetTime);
                step++;
            }

            if (RespDown.GetState(HandType))
            {
                topBeltMoveToRights.Add(topBeltMoveToRight);
                respUps.Add(false);
                onsetTimes.Add(onsetTime);
                step++;
            }
        }
        else if (step == 5)
        {
            if (currentTrial == Settings.nTrials())
            {
                step++;
            }
            else
            {
                // 初期化処理
                topBeltTransform.localPosition = Settings.topBeltPosition;
                bottomBeltTransform.localPosition = Settings.bottomBeltPosition;
                localTime = 0;
                currentShotCount = 0;
                currentTrial++;
                ChangeGuideText(trialText(currentTrial), true);
                step = 0;
            }
        }
    }
}
