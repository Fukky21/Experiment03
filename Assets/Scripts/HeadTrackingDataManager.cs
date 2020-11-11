using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HeadTrackingDataManager : MonoBehaviour
{
    public Camera MainCamera;
    private static bool isRecording;
    private static string filePath;
    private static StreamWriter writer;
    private float time;

    public static void InitializeDataFile(string fileName)
    {
        // Resultsフォルダが存在しないときは作成する
        string folderPath = Application.dataPath + "/Results";
        if (!Directory.Exists(folderPath))
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            di.Create();
        }

        filePath = Application.dataPath + $"/Results/{fileName}_head_data.txt";
        FileInfo fi = new FileInfo(filePath);
        string header = "";
        header += "pos_x pos_y pos_z";
        header += " ro_x ro_y ro_z";
        header += " time";

        using (StreamWriter sw = fi.CreateText())
        {
            sw.WriteLine(header);
            sw.Close();
        }
    }

    public static void StartRecording()
    {
        writer = new StreamWriter(filePath, true);
        isRecording = true;
    }

    public static void StopRecording()
    {
        isRecording = false;
        writer.Close();
    }

    private async void WriteData()
    {
        Transform transform =  MainCamera.transform;
        string dataRow = "";
        dataRow += $"{transform.position.x} {transform.position.y} {transform.position.z}";
        dataRow += $" {TransformAngle180(transform.eulerAngles.x)} {TransformAngle180(transform.eulerAngles.y)} {TransformAngle180(transform.eulerAngles.z)}";
        dataRow += $" {time}";

        await writer.WriteLineAsync(dataRow);
    }

    // [0, 360]degから[-180, 180]degへ変換する
    private float TransformAngle180(float angle)
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
    }


    void Update()
    {
        if (isRecording)
        {
            WriteData();
            time += Time.deltaTime;
        }
    }
}
