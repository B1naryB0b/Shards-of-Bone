using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class AbilitySO : ScriptableObject
{
    public abstract void Initialise();
    public abstract void Activate();
    public abstract void Deactivate();
    [HideInInspector] public bool isUnlocked;
}
