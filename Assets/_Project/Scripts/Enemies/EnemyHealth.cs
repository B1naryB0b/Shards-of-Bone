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

    [SerializeField] private GameObject enemyObject;

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
        Projectile projectile = collision.gameObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            TakeDamage(collision, projectile.damage);
        }
    }

    private void TakeDamage(Collision collision, int damage)
    {
        Flash(flashDuration);
        health -= damage;
        Destroy(collision.gameObject);

        if (health <= 0)
        {
            AudioController.Instance.PlaySound(hitSFX);

            if (enemyObject != null)
            {
                Destroy(enemyObject);
            }
            else
            {
                Debug.LogError("No enemyObject assigned to EnemyHealth.cs");
            }
        }
    }
}
