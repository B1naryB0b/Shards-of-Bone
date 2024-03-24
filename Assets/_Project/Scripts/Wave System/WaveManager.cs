using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private Transform spawnerTarget;
    [SerializeField] private BoidManager boidManager;
    [SerializeField, DataTable] private List<TimedWave> waves;
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
        
        SpawnWave();
        currentWave++;
    }


    private void SpawnWave()
    {
        Debug.Log(waves[currentWave].UnitBatches.units);
        List<Units> units = waves[currentWave].UnitBatches.units;
        

        foreach (Units unit in units)
        {
            for (int i = 0; i < unit.quantity; i++)
            {
                Vector3 randomDirection = new Vector3(Random.insideUnitCircle.x, 0f, Random.insideUnitCircle.y).normalized;
                Vector3 spawnPosition = randomDirection * distanceFromOrigin + unit.unitObject.transform.position;

                GameObject enemy = Instantiate(unit.unitObject, spawnPosition, Quaternion.identity);

                var simpleFollow = enemy.GetComponent<SimpleFollow>();
                
                if (simpleFollow != null)
                {
                    simpleFollow.target = spawnerTarget;
                }

                var boidSpawner = enemy.GetComponentInChildren<BoidSpawner>();
                
                if (boidSpawner != null)
                {
                    boidSpawner.boidManager = boidManager;
                }

                var boid = enemy.GetComponent<Boid>();

                if (boid != null)
                {
                    boidManager.AddBoid(boid);
                }
            }
        }
        
    }
}




