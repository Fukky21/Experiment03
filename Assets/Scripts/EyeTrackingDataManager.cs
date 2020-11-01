using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PupilLabs
{
    public class EyeTrackingDataManager : MonoBehaviour
    {
        public GazeController gazeController;
        private bool isGazing = false;
        public static Vector3 localGazeDirection;
        public static float gazeDistance;
        public static float lastConfidence;
        public static Vector3 gazeNormalLeft;
        public static Vector3 gazeNormalRight;
        public static Vector3 eyeCenterLeft;
        public static Vector3 eyeCenterRight;

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

        public static EyeTrackingData getEyeTrackingData()
        {
            return new EyeTrackingData(
                localGazeDirection,
                gazeDistance,
                lastConfidence,
                gazeNormalLeft,
                gazeNormalRight,
                eyeCenterLeft,
                eyeCenterRight
            );
        }

        void Start() {}

        void Update() {}
    }
}