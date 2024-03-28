using UnityEngine;

public class DistanceBasedAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float cutoffDistance = 10f;
    [SerializeField] private float maxDistance = 50f;

    [SerializeField] private Transform playerTransform;
    private bool isPlaying = false;

    private void Start()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component not set on DistanceBasedAudio script.");
        }
        else
        {
            audioSource.loop = true;
            audioSource.volume = 0f; // Start with volume at 0
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceFromOrigin = Vector3.Distance(playerTransform.position, Vector3.zero);
        if (distanceFromOrigin > cutoffDistance)
        {
            if (!isPlaying)
            {
                audioSource.Play();
                isPlaying = true;
            }

            float volume = Mathf.Clamp01((distanceFromOrigin - cutoffDistance) / (maxDistance - cutoffDistance));
            audioSource.volume = volume;
        }
        else
        {
            if (isPlaying)
            {
                audioSource.Stop();
                isPlaying = false;
            }
        }
    }
}
