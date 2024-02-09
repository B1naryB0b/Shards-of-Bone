using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int health;
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private float flashDuration;

    [SerializeField] private Color hitColor;
    private Color originalColor;

    [SerializeField] private int damagePerHit;

    private Renderer enemyRenderer;

    private void Start()
    {
        enemyRenderer = gameObject.GetComponent<Renderer>();
        enemyRenderer.material.EnableKeyword("_EmissiveColor");
        originalColor = enemyRenderer.material.GetColor("_EmissiveColor");
    }

    public void Flash(float duration)
    {
        StartCoroutine(Co_Flash(duration));
    }

    private IEnumerator Co_Flash(float duration)
    {
        if (enemyRenderer == null)
        {
            yield break;
        }

        enemyRenderer.material.SetColor("_EmissiveColor", hitColor);

        yield return new WaitForSeconds(duration);

        enemyRenderer.material.SetColor("_EmissiveColor", originalColor);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullets"))
        {
            TakeDamage(collision);
        }
    }

    private void TakeDamage(Collision collision)
    {
        Flash(flashDuration);
        health -= damagePerHit;
        Destroy(collision.gameObject);

        if (health <= 0)
        {
            AudioController.Instance.PlaySound(hitSFX);
            Destroy(gameObject);
        }
    }
}
