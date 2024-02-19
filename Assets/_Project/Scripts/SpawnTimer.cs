using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTimer : MonoBehaviour
{
    [SerializeField] private Transform spawnerTarget;
    [SerializeField] private BoidManager boidManager;

    [SerializeField] private GameObject spawnerPrefab;
    [SerializeField] private List<float> spawnTimes;

    [SerializeField] private float distanceFromOrigin;
    [SerializeField] private Vector3 spawnPositionOffset;


    private float timer;
    private int currentSpawnState;
    // Start is called before the first frame update
    void Start()
    {
        timer = 0f;
        currentSpawnState = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer > spawnTimes[currentSpawnState])
        {
            if (currentSpawnState < (spawnTimes.Count - 1))
            {
                currentSpawnState++;
                SpawnSpawner();
            }
        }
    }

    private void SpawnSpawner()
    {
        Vector3 randomDirection = new Vector3(Random.insideUnitCircle.x, 0f, Random.insideUnitCircle.y).normalized;
        Vector3 spawnPosition = randomDirection * distanceFromOrigin + spawnPositionOffset;
        GameObject spawnerEnemy = Instantiate(spawnerPrefab, spawnPosition, Quaternion.identity);

        spawnerEnemy.GetComponent<SimpleFollow>().target = spawnerTarget;
        spawnerEnemy.GetComponentInChildren<BoidSpawner>().boidManager = boidManager;
    }


}
