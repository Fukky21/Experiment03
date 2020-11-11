using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PupilLabs
{
    public class EyeTrackingDataManager : MonoBehaviour
    {
        public GazeController gazeController;
        private bool isGazing = false;
        private Vector3 localGazeDirection;
        private float gazeDistance;
        private float lastConfidence;
        private Vector3 gazeNormalLeft;
        private Vector3 gazeNormalRight;
        private Vector3 eyeCenterLeft;
        private Vector3 eyeCenterRight;
        private static bool isRecording;
        private static string filePath;
        private static StreamWriter writer;
        private float time;

        void OnEnable()
        {
            if (gazeController == null)
            {
                Debug.LogWarning("Required GazeController missing");
                enabled = false;
                return;
            }

            if (isGazing)
            {
                Debug.Log("Already gazing!");
                return;
            }

            gazeController.OnReceive3dGaze += ReceiveGaze;
            isGazing = true;
        }
        
        void OnDisable()
        {
            if (!isGazing || !enabled)
            {
                Debug.Log("Nothing to stop.");
                return;
            }

            isGazing = false;
            gazeController.OnReceive3dGaze -= ReceiveGaze;
        }

        void ReceiveGaze(GazeData gazeData)
        {
            lastConfidence = gazeData.Confidence;

            localGazeDirection = gazeData.GazeDirection;
            gazeDistance = gazeData.GazeDistance;

            if (gazeData.IsEyeDataAvailable(0))
            {
                gazeNormalLeft = gazeData.GazeNormal0;
                eyeCenterLeft = gazeData.EyeCenter0;
            }

            if (gazeData.IsEyeDataAvailable(1))
            {
                gazeNormalRight = gazeData.GazeNormal1;
                eyeCenterRight = gazeData.EyeCenter1;
            }
        }

        public static void InitializeDataFile(string fileName)
        {
            // Resultsフォルダが存在しないときは作成する
            string folderPath = Application.dataPath + "/Results";
            if (!Directory.Exists(folderPath))
            {
                DirectoryInfo di = new DirectoryInfo(folderPath);
                di.Create();
            }

            filePath = Application.dataPath + $"/Results/{fileName}_eye_data.txt";
            FileInfo fi = new FileInfo(filePath);
            string header = "";
            header += "local_gaze_direction_x local_gaze_direction_y local_gaze_direction_z";
            header += " gaze_distance";
            header += " last_confidence";
            header += " gaze_normal_left_x gaze_normal_left_y gaze_normal_left_z";
            header += " gaze_normal_right_x gaze_normal_right_y gaze_normal_right_z";
            header += " eye_center_left_x eye_center_left_y eye_center_left_z";
            header += " eye_center_right_x eye_center_right_y eye_center_right_z";
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
            string dataRow = "";
            dataRow += $"{localGazeDirection.x} {localGazeDirection.y} {localGazeDirection.z}";
            dataRow += $" {gazeDistance}";
            dataRow += $" {lastConfidence}";
            dataRow += $" {gazeNormalLeft.x} {gazeNormalLeft.y} {gazeNormalLeft.z}";
            dataRow += $" {gazeNormalRight.x} {gazeNormalRight.y} {gazeNormalRight.z}";
            dataRow += $" {eyeCenterLeft.x} {eyeCenterLeft.y} {eyeCenterLeft.z}";
            dataRow += $" {eyeCenterRight.x} {eyeCenterRight.y} {eyeCenterRight.z}";
            dataRow += $" {time}";

            await writer.WriteLineAsync(dataRow);
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
}