using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int health;
    [SerializeField] private AudioClip hitSFX;
    [SerializeField] private float flashDuration;

    [SerializeField] private Color hitColor;
    private Color _originalColor;

    [SerializeField] private GameObject enemyObject;

    private Renderer _enemyRenderer;
    private Collider _collider;

    private void Start()
    {
        _enemyRenderer = GetComponent<Renderer>();
        _enemyRenderer.material.EnableKeyword("_EmissiveColor");
        _originalColor = _enemyRenderer.material.GetColor("_EmissiveColor");

        _collider = GetComponent<Collider>();
    }

    private void Flash(float duration)
    {
        StartCoroutine(Co_Flash(duration));
    }

    private IEnumerator Co_Flash(float duration)
    {
        if (_enemyRenderer == null) yield break;

        _enemyRenderer.material.SetColor("_EmissiveColor", hitColor);

        yield return new WaitForSeconds(duration);

        _enemyRenderer.material.SetColor("_EmissiveColor", _originalColor);
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
            AudioController.Instance.PlaySound(hitSFX, 0.5f);

            if (enemyObject != null)
            {
                StartCoroutine(Co_Die());
            }
            else
            {
                Debug.LogError("No enemyObject assigned to EnemyHealth.cs");
            }
        }
    }

    private IEnumerator Co_Die()
    {
        _collider.enabled = false;
        yield return new WaitForSeconds(flashDuration);
        _enemyRenderer.material.SetColor("_EmissiveColor", hitColor);
        Destroy(enemyObject);
    }
}
