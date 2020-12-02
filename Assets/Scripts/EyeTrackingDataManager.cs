using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PupilLabs
{
    public class EyeTrackingDataManager : MonoBehaviour
    {
        public GazeController gazeController;
        private bool isGazing = false;
        private Vector3 gazeNormalLeft;
        private Vector3 gazeNormalRight;
        private static bool isRecording;
        private float time;
        private static List<Vector3> gazeNormalLefts;
        private static List<Vector3> gazeNormalRights;
        private static List<float> times;

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
            if (gazeData.IsEyeDataAvailable(0))
            {
                gazeNormalLeft = gazeData.GazeNormal0;
            }

            if (gazeData.IsEyeDataAvailable(1))
            {
                gazeNormalRight = gazeData.GazeNormal1;
            }
        }

        public static void WriteData(string fileName)
        {
            // Resultsフォルダが存在しないときは作成する
            string folderPath = Application.dataPath + "/Results";
            if (!Directory.Exists(folderPath))
            {
                DirectoryInfo di = new DirectoryInfo(folderPath);
                di.Create();
            }
            string filePath = Application.dataPath + $"/Results/{fileName}_eye_data.txt";
            FileInfo fi = new FileInfo(filePath);
            string header = "gaze_normal_left_x gaze_normal_right_x time";
            using (StreamWriter sw = fi.CreateText())
            {
                sw.WriteLine(header);
                for (int i = 0; i < times.Count; i++)
                {
                    string dataTxt = $"{gazeNormalLefts[i].x} {gazeNormalRights[i].x} {times[i]}";
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

        void Start()
        {
            time = 0;
            isRecording = false;
            gazeNormalLefts = new List<Vector3>();
            gazeNormalRights = new List<Vector3>();
            times = new List<float>();
        }

        void Update()
        {
            if (isRecording)
            {
                gazeNormalLefts.Add(gazeNormalLeft);
                gazeNormalRights.Add(gazeNormalRight);
                times.Add(time);
                time += Time.deltaTime;
            }
        }
    }
}