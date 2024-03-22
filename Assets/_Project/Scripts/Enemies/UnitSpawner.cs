using System.Collections;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    [SerializeField] private WaveSO waveSO;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float initialDelay = 5f;
    [SerializeField] private float waveCooldown = 10f;
    [SerializeField] private float spawnRadius = 2f;

    private Transform _playerTransform;

    private void Start()
    {
        _playerTransform = FindObjectOfType<CharacterController>().transform;
        StartCoroutine(Co_SpawnWaves());
    }

    private IEnumerator Co_SpawnWaves()
    {
        yield return new WaitForSeconds(initialDelay);

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
        IPlayerTracker playerTracker = enemy.GetComponent<IPlayerTracker>();
        if (playerTracker != null)
        {
            playerTracker.SetPlayerTransform(_playerTransform);
        }
    }

}
