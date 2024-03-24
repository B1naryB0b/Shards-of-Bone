using UnityEngine;

[System.Serializable]
public class TimedWave
{
    [SerializeField] private UnitBatchesSO unitBatches;
    [SerializeField] private float spawnTime;

    public UnitBatchesSO UnitBatches => unitBatches;
    public float SpawnTime => spawnTime;
}
