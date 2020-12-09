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
    public bool isFixationPointMode;

    public Camera MainCamera;
    public SteamVR_Input_Sources HandType;
    public SteamVR_Action_Boolean RespUp;
    public SteamVR_Action_Boolean RespDown;
    public SteamVR_Action_Boolean RespTrigger;
    public Text GuideText;
    public GameObject FixationPoint;
    public GameObject FixationLine;
    public GameObject TopBelt;
    public GameObject BottomBelt;
    public AudioClip sound;
    AudioSource audioSource;
    System.Random rnd;
    private int phase = 0;
    private int step = 0;
    private int currentTrial = 1;
    private double currentRatio;
    private float currentBeltSpeedToRight;
    private float currentBeltSpeedToLeft;
    private int currentCorrect = 0;
    private int currentShotCount = 0;
    private bool topBeltMoveToRight;
    private int addedShotCount = 0;
    private List<double> ratios;
    private List<bool> topBeltMoveToRights;
    private List<bool> respUps;
    private List<float> onsetTimes;
    private float time = 0;
    private float localTime = 0;
    private float onsetTime = 0;

    Transform topBeltTransform;
    Transform bottomBeltTransform;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        currentRatio = Settings.initialRatio;
        rnd = new System.Random();
        ratios = new List<double>();
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
            if (RespUp.GetState(HandType) || RespDown.GetState(HandType))
            {
                ChangeGuideText(trialText(currentTrial), true);
                PupilLabs.EyeTrackingDataManager.StartRecording();
                HeadTrackingDataManager.StartRecording();
                phase++;
            }
        }
        else if (phase == 1) // 記録中
        {
            DoExperiment();
            time += Time.deltaTime;
            // 規定のトライアル数を終えたとき、stepの値は6になる
            if (step == 6)
            {
                PupilLabs.EyeTrackingDataManager.StopRecording(fileName);
                HeadTrackingDataManager.StopRecording(fileName);
                WriteData();
                ChangeGuideText(finishText(currentRatio), true);
                phase++;
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
                string dataTxt = $"{i+1} {ratios[i]} {ConvertBoolToInt(topBeltMoveToRights[i])}";
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
        return $"{currentTrial}/{Settings.trials}";
    }

    private string finishText(double currentRatio)
    {
        return $"Finish!\n\nLast Ratio: {Math.Abs(currentRatio)}\n\nQuit: Press ESC";
    }

    private void DoExperiment()
    {
        if (step == 0) // 現在のトライアル数を表示中
        {
            if (RespTrigger.GetState(HandType))
            {
                // 次のratio(絶対値)を決定する
                if (currentCorrect == Settings.consecutiveCorrentAnswers)
                {
                    // 規定回数連続で正答した場合は次のstepへ以降する
                    currentCorrect = 0;
                    currentRatio = Math.Abs(currentRatio) - Settings.ratioStep;
                    if (currentRatio < 0)
                    {
                        // ここにはほとんど来ないはず
                        currentRatio = 0;
                    }
                }
                // 次のratioの符号を決定する
                if (rnd.NextDouble() < 0.5)
                {
                    currentRatio = -currentRatio;
                }
                // 次のベルトの速度を決定する
                currentBeltSpeedToLeft = ConvertRatioToSpeed(currentRatio)["speedToLeft"];
                currentBeltSpeedToRight = ConvertRatioToSpeed(currentRatio)["speedToRight"];
                // addedShotCountを決定する
                if (rnd.NextDouble() < 0.5)
                {
                    addedShotCount = 1;
                }
                else
                {
                    addedShotCount = 2;
                }
                // topBeltMoveToRightを決定する
                if (rnd.NextDouble() < 0.5)
                {
                    topBeltMoveToRight = false;
                }
                else
                {
                    topBeltMoveToRight = true;
                }

                ChangeGuideText(null, false);
                if (isFixationPointMode)
                {
                    FixationPoint.SetActive(true);
                }
                else
                {
                    FixationLine.SetActive(true);
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
                topBeltTransform.Translate(Tan(currentBeltSpeedToRight) * Time.deltaTime, 0, 0);
                bottomBeltTransform.Translate(-Tan(currentBeltSpeedToLeft) * Time.deltaTime, 0, 0);
            }
            else
            {
                topBeltTransform.Translate(-Tan(currentBeltSpeedToLeft) * Time.deltaTime, 0, 0);
                bottomBeltTransform.Translate(Tan(currentBeltSpeedToRight) * Time.deltaTime, 0, 0);
            }
            localTime += Time.deltaTime;
            if (localTime > Settings.stimulusPresentationTime)
            {
                TopBelt.SetActive(false);
                BottomBelt.SetActive(false);
                FixationPoint.SetActive(false);
                FixationLine.SetActive(false);
                step++;
            }
        }
        else if (step == 4) // 応答待ち
        {
            if (RespUp.GetState(HandType))
            {
                ratios.Add(currentRatio);
                topBeltMoveToRights.Add(topBeltMoveToRight);
                respUps.Add(true);
                onsetTimes.Add(onsetTime);
                step++;
            }

            if (RespDown.GetState(HandType))
            {
                ratios.Add(currentRatio);
                topBeltMoveToRights.Add(topBeltMoveToRight);
                respUps.Add(false);
                onsetTimes.Add(onsetTime);
                step++;
            }
        }
        else if (step == 5)
        {
            if (currentTrial == Settings.trials)
            {
                // 規定のトライアル数を終えたら終了
                step++;
            }
            else
            {
                // 応答が正解かどうかチェックする
                if (currentRatio > 0)
                {
                    // 右へ動く刺激の方が速いとき
                    if (topBeltMoveToRight == respUps[currentTrial - 1])
                    {
                        currentCorrect++;
                    }
                    else
                    {
                        // 間違えていたとき
                        currentCorrect = 0;
                        currentRatio = Math.Abs(currentRatio) + Settings.ratioStep;
                    }
                }
                else
                {
                    // 左へ動く刺激の方が速いとき
                    if (topBeltMoveToRight != respUps[currentTrial - 1])
                    {
                        currentCorrect++;
                    }
                    else
                    {
                        // 間違えていたとき
                        currentCorrect = 0;
                        currentRatio = Math.Abs(currentRatio) + Settings.ratioStep;
                    }
                }
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
