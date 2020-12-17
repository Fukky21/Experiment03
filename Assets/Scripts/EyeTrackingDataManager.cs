﻿using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PupilLabs
{
    public class EyeTrackingDataManager : MonoBehaviour
    {
        public GazeController gazeController;
        private bool isGazing = false;
        private static string filePath;
        private static StreamWriter writer;
        private static bool isRecording;
        private Vector3 gazeNormalLeft;
        private Vector3 gazeNormalRight;
        private float gazeConfidence;
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
            gazeConfidence = gazeData.Confidence;

            if (gazeData.IsEyeDataAvailable(0))
            {
                gazeNormalLeft = gazeData.GazeNormal0;
            }

            if (gazeData.IsEyeDataAvailable(1))
            {
                gazeNormalRight = gazeData.GazeNormal1;
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
            filePath = Application.dataPath + $"/Results/{fileName}_eyedata.txt";
            FileInfo fi = new FileInfo(filePath);
            string header = "";
            header += "gaze_normal_left_x gaze_normal_left_y gaze_normal_left_z";
            header += " gaze_normal_right_x gaze_normal_right_y gaze_normal_right_z";
            header += " gaze_confidence";
            header += " time";
            using (StreamWriter sw = fi.CreateText())
            {
                sw.WriteLine(header);
                sw.Close();
            }
        }

        private async void WriteData()
        {
            string data = "";
            data += $"{gazeNormalLeft.x} {gazeNormalLeft.y} {gazeNormalLeft.z}";
            data += $" {gazeNormalRight.x} {gazeNormalRight.y} {gazeNormalRight.z}";
            data += $" {gazeConfidence}";
            data += $" {time}";
            await writer.WriteLineAsync(data);
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

        void Start()
        {
            time = 0;
            isRecording = false;
        }

        void Update()
        {
            if (isRecording)
            {
                time += Time.deltaTime;
                WriteData();
            }
        }
    }
}