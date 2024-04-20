using UnityEngine;

public class ToggleWaveSpawner : MonoBehaviour
{
    [SerializeField] private GameObject waveSpawner;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (waveSpawner != null)
            {
                waveSpawner.SetActive(!waveSpawner.activeSelf);
            }
        }
    }
}