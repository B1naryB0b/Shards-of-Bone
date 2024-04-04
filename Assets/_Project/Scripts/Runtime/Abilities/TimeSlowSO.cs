using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

[CreateAssetMenu]
public class TimeSlowSO : AbilitySO
{
    [Range(0f,1f)]
    public float timeScale;
    
    public override void Activate()
    {
        Time.timeScale = timeScale;
    }

    public override void Deactivate()
    {
        Time.timeScale = 1f;
    }
}
