using UnityEngine;

[System.Serializable]
public class SpawnWave
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnTime;

    public GameObject EnemyPrefab { get { return enemyPrefab; } }
    public float SpawnTime { get { return spawnTime; } }
}
