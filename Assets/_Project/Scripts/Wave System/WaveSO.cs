using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class WaveSO : ScriptableObject
{
    public List<Wave> waves;
}

[System.Serializable]
public class Wave
{
    public GameObject unit;
    public int quantity;
}
