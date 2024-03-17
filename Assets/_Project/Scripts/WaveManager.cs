using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private Transform spawnerTarget;
    [SerializeField] private BoidManager boidManager;
    [SerializeField, DataTable] private List<SpawnWave> waves;
    [SerializeField] private float distanceFromOrigin;

    private float timer;
    private int currentWave;

    private void Start()
    {
        timer = 0f;
        currentWave = 0;
    }


    private void Update()
    {
        timer += Time.deltaTime;

        if (!(timer > waves[currentWave].SpawnTime)) return;
        
        if (currentWave >= waves.Count - 1) return;
        
        currentWave++;
        SpawnWave();
    }


    private void SpawnWave()
    {
        Vector3 randomDirection = new Vector3(Random.insideUnitCircle.x, 0f, Random.insideUnitCircle.y).normalized;
        Vector3 spawnPosition = randomDirection * distanceFromOrigin + waves[currentWave].EnemyPrefab.transform.position;

        GameObject enemy = Instantiate(waves[currentWave].EnemyPrefab, spawnPosition, Quaternion.identity);

        if (enemy.GetComponent<SimpleFollow>() != null)
        {
            enemy.GetComponent<SimpleFollow>().target = spawnerTarget;
        }

        if (enemy.GetComponentInChildren<BoidSpawner>() != null)
        {
            enemy.GetComponentInChildren<BoidSpawner>().boidManager = boidManager;
        }
    }
}




