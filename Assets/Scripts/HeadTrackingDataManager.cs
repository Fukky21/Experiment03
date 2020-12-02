using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HeadTrackingDataManager : MonoBehaviour
{
    public Camera MainCamera;
    private static bool isRecording;
    private float time;
    private static List<Vector3> rotations;
    private static List<float> times;

    public static void WriteData(string fileName)
    {
        // Resultsフォルダが存在しないときは作成する
        string folderPath = Application.dataPath + "/Results";
        if (!Directory.Exists(folderPath))
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            di.Create();
        }
        string filePath = Application.dataPath + $"/Results/{fileName}_head_data.txt";
        FileInfo fi = new FileInfo(filePath);
        string header = "ro_y time";
        using (StreamWriter sw = fi.CreateText())
        {
            sw.WriteLine(header);
            for (int i = 0; i < times.Count; i++)
            {
                string dataTxt = $"{TransformAngle180(rotations[i].y)} {times[i]}";
                sw.WriteLine(dataTxt);
            }
            sw.Close();
        }
    }

    public static void StartRecording()
    {
        isRecording = true;
    }

    public static void StopRecording(string fileName)
    {
        isRecording = false;
        WriteData(fileName);
    }

    // [0, 360]degから[-180, 180]degへ変換する
    private static float TransformAngle180(float angle)
    {
        if (angle > 180)
        {
            return angle - 360;
        }
        return angle;
    } 

    void Start()
    {
        time = 0;
        isRecording = false;
        rotations = new List<Vector3>();
        times = new List<float>();
    }


    void Update()
    {
        if (isRecording)
        {
            rotations.Add(MainCamera.transform.eulerAngles);
            times.Add(time);
            time += Time.deltaTime;
        }
    }
}
