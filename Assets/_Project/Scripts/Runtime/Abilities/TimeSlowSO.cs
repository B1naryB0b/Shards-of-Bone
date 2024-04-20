using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class TimeSlowSO : AbilitySO
{
    [Range(0f,1f)]
    public float timeScale;

    [HideInInspector] public float timeScaleCompensation;

    public override void Initialise()
    {
        Time.timeScale = 1f;
        timeScaleCompensation = 1f / timeScale;
    }
    
    public override void Activate()
    {
        Time.timeScale *= timeScale;
    }

    public override void Deactivate()
    {
        Time.timeScale /= timeScale;
    }
}
