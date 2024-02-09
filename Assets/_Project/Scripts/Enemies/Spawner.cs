using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private WaveSO waveSO;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float waveCooldown = 10f;
    [SerializeField] private float spawnRadius = 2f;

    private Transform playerTransform;

    private void Start()
    {
        playerTransform = FindObjectOfType<CharacterController>().transform;
        StartCoroutine(Co_SpawnWaves());
    }

    private IEnumerator Co_SpawnWaves()
    {
        while (true)
        {
            StartCoroutine(Co_SpawnWave());
            yield return new WaitForSeconds(waveCooldown);
        }
    }

    private IEnumerator Co_SpawnWave()
    {
        foreach (Wave wave in waveSO.waves)
        {
            for (int i = 0; i < wave.quantity; i++)
            {
                Vector3 offsetPosition = Random.insideUnitSphere * spawnRadius + spawnPoint.position;
                offsetPosition.y = spawnPoint.position.y;
                SpawnUnit(wave.unit, offsetPosition);
                yield return null; // Optional: to spread out instantiation over multiple frames
            }
        }
    }

    private void SpawnUnit(GameObject unit, Vector3 position)
    {
        GameObject enemy = Instantiate(unit, position, spawnPoint.rotation);
        if (enemy.TryGetComponent(out FloaterMovement floater))
        {
            floater.SetPlayerTransform(playerTransform);
        }
        if (enemy.TryGetComponent(out CrawlerMovement crawler))
        {
            crawler.SetPlayerTransform(playerTransform);
        }
        if (enemy.TryGetComponent(out LurkerMovement lurker))
        {
            lurker.SetPlayerTransform(playerTransform);
        }
    }
}
