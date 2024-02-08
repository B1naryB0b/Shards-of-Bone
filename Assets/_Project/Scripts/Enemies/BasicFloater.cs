using System.Collections;
using UnityEngine;

public class BasicFloater : MonoBehaviour
{
    [SerializeField] private float followForceMagnitude = 5f;
    [SerializeField] private float crawlFrequency = 5f; // Frequency of the sine wave
    [SerializeField] private float crawlAmplitude = 1f; // Amplitude of the sine wave

    [SerializeField] private Transform playerTransform;
    private Rigidbody rb;
    private float offset;

    [SerializeField] private int health;
    [SerializeField] private AudioClip hitSFX;


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
