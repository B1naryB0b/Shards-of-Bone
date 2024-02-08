using System.Collections;
using UnityEngine;

public class CrawlerMovement : MonoBehaviour
{
    [SerializeField] private float followForceMagnitude = 5f;
    [SerializeField] private float floatFrequency = 5f; // Frequency of the sine wave
    [SerializeField] private float floatAmplitude = 1f; // Amplitude of the sine wave

    [SerializeField] private Transform playerTransform;
    private Rigidbody rb;
    private Vector3 offset;

    [SerializeField] private int health;
    [SerializeField] private AudioClip hitSFX;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        offset = new Vector3(Random.value, Random.value, 0) * floatFrequency; // Randomize start offset for variety
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
            Vector3 force = direction * followForceMagnitude;

            // Apply the sine wave force along the x and y axes separately
            force.x += Mathf.Sin(Time.time * floatFrequency + offset.x) * floatAmplitude;
            force.y += Mathf.Sin(Time.time * floatFrequency + offset.y) * floatAmplitude;

            rb.AddForce(force);
        }
    }

    public void Flash(GameObject target, float duration)
    {
        StartCoroutine(Co_Flash(target, duration));
    }

    private IEnumerator Co_Flash(GameObject target, float duration)
    {
        Renderer targetRenderer = target.GetComponent<Renderer>();
        if (targetRenderer == null)
        {
            yield break; // Exit if the target has no Renderer
        }

        Color originalColor = targetRenderer.material.color; // Store the original color
        targetRenderer.material.color = Color.white; // Change to white

        yield return new WaitForSeconds(duration); // Wait for the specified duration

        targetRenderer.material.color = originalColor; // Revert to the original color
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullets"))
        {
            Flash(gameObject, 0.02f);
            health -= 1;
            Destroy(collision.gameObject);
        }

        if (health <= 0)
        {
            AudioController.Instance.PlaySound(hitSFX);
            Destroy(gameObject);
        }
    }
}
