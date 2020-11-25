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
    private string experimentDataFilePath;
    private static StreamWriter experimentDataFileWriter;
    private int phase = 0;
    private bool isPracticeMode = false;
    private int step = 0;
    private int currentTrial = 1;
    private int shotCount = 0;
    private double[] beltSpeedRatio;
    private float[] beltSpeedToLeft;
    private float[] beltSpeedToRight;
    private bool[] topBeltMoveToRight;
    private float time = 0;
    private float localTime = 0;
    private float stimulusPresentationStartTime = 0;
    private bool isMovingToRight = false;
    private float currentAngle = 0;
    private string selectModeText = "Press Up: Start Practice\n\nPress Down: Start Experiment";
    private string finishRecordingText = "Finish\n\nPress ESC: Quit App";

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
                PupilLabs.EyeTrackingDataManager.StartRecording();
                HeadTrackingDataManager.StartRecording();
                isPracticeMode = true;
                ChangeGuideText(null, false);
                phase++;
            }

            if (RespDown.GetState(HandType))
            {
                // Start Experiment
                InitializeExprimentDataFile();
                SetupParameters();
                experimentDataFileWriter = new StreamWriter(experimentDataFilePath, true);
                PupilLabs.EyeTrackingDataManager.StartRecording();
                HeadTrackingDataManager.StartRecording();
                isPracticeMode = false;
                ChangeGuideText(trialText(currentTrial), true);
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
                    PupilLabs.EyeTrackingDataManager.StopRecording();
                    HeadTrackingDataManager.StopRecording();
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
                // 規定のトライアル数を終えたとき、stepが6になる
                if (step == 6)
                {
                    PupilLabs.EyeTrackingDataManager.StopRecording();
                    HeadTrackingDataManager.StopRecording();
                    experimentDataFileWriter.Close();
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
        System.Random rnd = new System.Random();
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

    // Setup beltSpeedRatio, beltSpeedToLeft, beltSpeedToRight, topBeltMoveToRight
    private void SetupParameters()
    {
        System.Random rnd = new System.Random();
        double[] tmpSpeedRatio = new double[Settings.nTrials()];
        beltSpeedRatio = new double[Settings.nTrials()];
        beltSpeedToLeft = new float[Settings.nTrials()];
        beltSpeedToRight = new float[Settings.nTrials()];
        topBeltMoveToRight = new bool[Settings.nTrials()];

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
        for (int i = 0; i < Settings.nTrials(); i++)
        {
            if (rnd.NextDouble() < 0.5)
            {
                topBeltMoveToRight[i] = false;
            }
            else
            {
                topBeltMoveToRight[i] = true;
            }
        }
    }

    private void InitializeExprimentDataFile()
    {
        // Resultsフォルダが存在しないときは作成する
        string folderPath = Application.dataPath + "/Results";
        if (!Directory.Exists(folderPath))
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            di.Create();
        }

        experimentDataFilePath = Application.dataPath + $"/Results/{fileName}_ex_data.txt";
        FileInfo fi = new FileInfo(experimentDataFilePath);
        string header = "trial ratio top_belt_move_to_left resp_up presentation_start_time";
        using (StreamWriter sw = fi.CreateText())
        {
            sw.WriteLine(header);
            sw.Close();
        }
    }

    private async void WriteExData(bool respUp)
    {
        string dataRow = "";
        dataRow += $"{currentTrial} {beltSpeedRatio[currentTrial - 1]}";
        dataRow += $" {ConvertBoolToInt(topBeltMoveToRight[currentTrial - 1])} {ConvertBoolToInt(respUp)}";
        dataRow += $" {stimulusPresentationStartTime}";
        await experimentDataFileWriter.WriteLineAsync(dataRow);
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
                if (++shotCount == Settings.shotCount)
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
                stimulusPresentationStartTime = time;
                localTime = 0;
                TopBelt.SetActive(true);
                BottomBelt.SetActive(true);
                step++;
            }
        }
        else if (step == 3) // 刺激提示中
        {
            if (topBeltMoveToRight[currentTrial - 1])
            {
                TopBelt.transform.Translate(Tan(beltSpeedToRight[currentTrial - 1]) * Time.deltaTime, 0, 0);
                BottomBelt.transform.Translate(-Tan(beltSpeedToLeft[currentTrial - 1]) * Time.deltaTime, 0, 0);
            }
            else
            {
                TopBelt.transform.Translate(-Tan(beltSpeedToLeft[currentTrial - 1]) * Time.deltaTime, 0, 0);
                BottomBelt.transform.Translate(Tan(beltSpeedToRight[currentTrial - 1]) * Time.deltaTime, 0, 0);
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
                WriteExData(true);
                step++;
            }

            if (RespDown.GetState(HandType))
            {
                WriteExData(false);
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
                TopBelt.transform.localPosition = Settings.topBeltPosition;
                BottomBelt.transform.localPosition = Settings.bottomBeltPosition;
                localTime = 0;
                shotCount = 0;
                currentTrial++;
                ChangeGuideText(trialText(currentTrial), true);
                step = 0;
            }
        }
    }
}
