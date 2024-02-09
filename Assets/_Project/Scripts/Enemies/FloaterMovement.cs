using System.Collections;
using UnityEngine;

public class FloaterMovement : MonoBehaviour
{
    [SerializeField] private float followForceMagnitude = 5f;
    [SerializeField] private float crawlFrequency = 5f; // Frequency of the sine wave
    [SerializeField] private float crawlAmplitude = 1f; // Amplitude of the sine wave

    [SerializeField] private Transform playerTransform;
    private Rigidbody rb;
    private float offset;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        offset = Random.value * crawlFrequency; // Randomize start offset for variety
    }

    public void SetPlayerTransform(Transform transform)
    {
        playerTransform = transform;
    }

    private void FixedUpdate()
    {
        if (playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            Vector3 followForce = direction * followForceMagnitude;

            // Calculate the crawl force using a sine wave for a dynamic effect
            float crawlForceMagnitude = Mathf.Sin(Time.time * crawlFrequency + offset) * crawlAmplitude;
            Vector3 crawlForce = direction * crawlForceMagnitude;

            // Apply the combined forces
            rb.AddForce(followForce + crawlForce);
        }

    }

}
