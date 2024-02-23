using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour {

    public BoidManager boidManager;

    [Header("Waves")]
    [SerializeField] private WaveSO waveSO;
    [SerializeField] private float initialDelay;
    [SerializeField] private float waveCooldown = 10f;

    [Header("Spawn")]
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private Color colour;
    private enum GizmoType { Never, SelectedOnly, Always }
    [SerializeField] private GizmoType showSpawnRegion;

    [Header("Spawn Direction")]
    [SerializeField] private bool randomSpawn;
    [SerializeField] private Vector3 startDirection = Vector3.forward;

    private void Start()
    {
        StartCoroutine(Co_SpawnWaves());
    }

    private IEnumerator Co_SpawnWaves()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            yield return StartCoroutine(Co_SpawnWave());
            yield return new WaitForSeconds(waveCooldown);
        }
    }

    private IEnumerator Co_SpawnWave()
    {
        foreach (Wave wave in waveSO.waves)
        {
            for (int i = 0; i < wave.quantity; i++)
            {
                SpawnBoid(wave);
                yield return null;
            }
        }
    }

    private void SpawnBoid(Wave wave)
    {
        Vector3 pos = transform.position + Random.insideUnitSphere * spawnRadius;
        GameObject boidObject = Instantiate(wave.unit, pos, Quaternion.identity);
        Boid boid = boidObject.GetComponent<Boid>();

        if (boid != null)
        {
            Vector3 direction = randomSpawn ? Random.insideUnitSphere.normalized : startDirection;
            boid.transform.forward = direction;
            boid.SetColour(colour);
            boidManager.AddBoid(boid);
        }
        else
        {
            Debug.LogError("The instantiated object does not have a Boid component.");
        }
    }

    private void OnDrawGizmos () {
        if (showSpawnRegion == GizmoType.Always) {
            DrawGizmos ();
        }
    }

    void OnDrawGizmosSelected () {
        if (showSpawnRegion == GizmoType.SelectedOnly) {
            DrawGizmos ();
        }
    }

    void DrawGizmos () {

        Gizmos.color = new Color (colour.r, colour.g, colour.b, 0.3f);
        Gizmos.DrawSphere (transform.position, spawnRadius);
    }

}