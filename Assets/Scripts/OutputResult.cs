using System.IO;
using UnityEngine;

public class OutputResult
{
    public static int initialize(string fileName)
    {
        // Resultsフォルダが存在しないときは作成する
        string folderPath = Application.dataPath + "/Results";
        if (!Directory.Exists(folderPath))
        {
            DirectoryInfo di = new DirectoryInfo(folderPath);
            di.Create();
        }

        string filePath = Application.dataPath + "/Results/" + fileName;
        FileInfo fi = new FileInfo(filePath);
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
            Debug.Log("File already exists");
            return -1;
        }
    }

    public static void writeResult(string fileName, ExperimentData exData)
    {
        string filePath = Application.dataPath + "/Results/" + fileName;
        FileInfo fi = new FileInfo(filePath);
        string dataRow = "";
        dataRow += $"{exData.eyeTrackingData.localGazeDirection.x} {exData.eyeTrackingData.localGazeDirection.y} {exData.eyeTrackingData.localGazeDirection.z}";
        dataRow += $" {exData.eyeTrackingData.gazeDistance}";
        dataRow += $" {exData.eyeTrackingData.lastConfidence}";
        dataRow += $" {exData.eyeTrackingData.gazeNormalLeft.x} {exData.eyeTrackingData.gazeNormalLeft.y} {exData.eyeTrackingData.gazeNormalLeft.z}";
        dataRow += $" {exData.eyeTrackingData.gazeNormalRight.x} {exData.eyeTrackingData.gazeNormalRight.y} {exData.eyeTrackingData.gazeNormalRight.z}";
        dataRow += $" {exData.eyeTrackingData.eyeCenterLeft.x} {exData.eyeTrackingData.eyeCenterLeft.y} {exData.eyeTrackingData.eyeCenterLeft.z}";
        dataRow += $" {exData.eyeTrackingData.eyeCenterRight.x} {exData.eyeTrackingData.eyeCenterRight.y} {exData.eyeTrackingData.eyeCenterRight.z}";
        dataRow += $" {exData.globalTime}";

        using (StreamWriter writer = fi.AppendText())
        {
            writer.WriteLine(dataRow);
            writer.Close();
        }
    }
}