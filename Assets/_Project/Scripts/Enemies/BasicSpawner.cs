using System.Collections;
using UnityEngine;

public class BasicSpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float waveCooldown = 10f;
    [SerializeField] private float spawnRadius = 2f;

    private bool isSpawning;
    private Transform playerTransform;

    private void Start()
    {
        isSpawning = false;
        playerTransform = FindObjectOfType<CharacterController>().transform;
    }

    private void Update()
    {
        if (!isSpawning)
        {
            StartCoroutine(Co_SpawnWave());
        }
    }

    private IEnumerator Co_SpawnWave()
    {
        isSpawning = true;
        for (int i = 0; i < enemiesPerWave; i++)
        {
            Vector3 offsetPosition = Random.insideUnitSphere * spawnRadius + spawnPoint.position;
            offsetPosition.y = spawnPoint.position.y; // Keep the y position consistent with the spawn point
            SpawnEnemy(offsetPosition);
        }
        yield return new WaitForSeconds(waveCooldown);
        isSpawning = false;
    }

    private void SpawnEnemy(Vector3 position)
    {
        GameObject enemy = Instantiate(enemyPrefab, position, spawnPoint.rotation);
        if (enemy.TryGetComponent(out BasicFloater floater))
        {
            floater.SetPlayerTransform(playerTransform);
        }
        if (enemy.TryGetComponent(out CrawlerMovement crawler))
        {
            crawler.SetPlayerTransform(playerTransform);
        }
    }
}
