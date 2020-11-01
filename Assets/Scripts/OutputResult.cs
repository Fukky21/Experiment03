using System.IO;
using UnityEngine;

public class OutputResult
{
    public OutputResult(string fileName)
    {
        this.fileName = fileName;
    }

    private string fileName;
    private string eyeDataFilePath;
    private string headDataFilePath;

    public int initialize()
    {
        // Resultsフォルダが存在しないときは作成する
        string folderPath = Application.dataPath + "/Results";
        if (!Directory.Exists(folderPath))
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            di.Create();
        }

        if (initializeEyeDataFile() == -1 || initializeHeadDataFile() == -1)
        {
            Debug.Log("File already exists");
            return -1;
        }

        return 0;
    }

    public void writeData(
        EyeTrackingData eyeTrackingData,
        HeadTrackingData headTrackingData,
        float globalTime
    )
    {
        writeEyeData(eyeTrackingData, globalTime);
        writeHeadData(headTrackingData, globalTime);
    }

    private int initializeEyeDataFile()
    {
        eyeDataFilePath = Application.dataPath + $"/Results/{fileName}_eye_data.txt";
        FileInfo fi = new FileInfo(eyeDataFilePath);
        string header = "";
        header += "local_gaze_direction_x local_gaze_direction_y local_gaze_direction_z";
        header += " gaze_distance";
        header += " last_confidence";
        header += " gaze_normal_left_x gaze_normal_left_y gaze_normal_left_z";
        header += " gaze_normal_right_x gaze_normal_right_y gaze_normal_right_z";
        header += " eye_center_left_x eye_center_left_y eye_center_left_z";
        header += " eye_center_right_x eye_center_right_y eye_center_right_z";
        header += " global_time";

        if (!fi.Exists)
        {
            using (StreamWriter writer = fi.CreateText())
            {
                writer.WriteLine(header);
                writer.Close();
            }
            return 0;
        }
        else
        {
            // ファイルが既に存在するときは、-1を返す
            return -1;
        }
    }

    private int initializeHeadDataFile()
    {
        headDataFilePath = Application.dataPath + $"/Results/{fileName}_head_data.txt";
        FileInfo fi = new FileInfo(headDataFilePath);
        string header = "";
        header += "pos_x pos_y pos_z";
        header += " ro_x ro_y ro_z";
        header += " global_time";

        if (!fi.Exists)
        {
            using (StreamWriter writer = fi.CreateText())
            {
                writer.WriteLine(header);
                writer.Close();
            }
            return 0;
        }
        else
        {
            // ファイルが既に存在するときは、-1を返す
            return -1;
        }
    }

    public void writeEyeData(EyeTrackingData data, float globalTime)
    {
        FileInfo fi = new FileInfo(eyeDataFilePath);
        string dataRow = "";
        dataRow += $"{data.localGazeDirection.x} {data.localGazeDirection.y} {data.localGazeDirection.z}";
        dataRow += $" {data.gazeDistance}";
        dataRow += $" {data.lastConfidence}";
        dataRow += $" {data.gazeNormalLeft.x} {data.gazeNormalLeft.y} {data.gazeNormalLeft.z}";
        dataRow += $" {data.gazeNormalRight.x} {data.gazeNormalRight.y} {data.gazeNormalRight.z}";
        dataRow += $" {data.eyeCenterLeft.x} {data.eyeCenterLeft.y} {data.eyeCenterLeft.z}";
        dataRow += $" {data.eyeCenterRight.x} {data.eyeCenterRight.y} {data.eyeCenterRight.z}";
        dataRow += $" {globalTime}";

        using (StreamWriter writer = fi.AppendText())
        {
            writer.WriteLine(dataRow);
            writer.Close();
        }
    }

    public void writeHeadData(HeadTrackingData data, float globalTime)
    {
        FileInfo fi = new FileInfo(headDataFilePath);
        string dataRow = "";
        dataRow += $"{data.positionX} {data.positionY} {data.positionZ}";
        dataRow += $" {data.rotationX} {data.rotationY} {data.rotationZ}";
        dataRow += $" {globalTime}";

        using (StreamWriter writer = fi.AppendText())
        {
            writer.WriteLine(dataRow);
            writer.Close();
        }
    }
}