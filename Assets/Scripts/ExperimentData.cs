using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperimentData
{
    public ExperimentData(EyeTrackingData eyeTrackingData, float globalTime)
    {
        this.eyeTrackingData = eyeTrackingData;
        this.globalTime = globalTime;
    }

    public EyeTrackingData eyeTrackingData;
    public float globalTime;
}
