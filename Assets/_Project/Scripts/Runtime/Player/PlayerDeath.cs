using UnityEngine;
using UnityEngine.UI;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private GameObject retryScreen;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private float fallDistance;
    [SerializeField] private float fadeDistance;
    [SerializeField] private Image fadeImage; // Assign this in the inspector

    private bool _isDead = false;
    public bool IsDead => _isDead;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead) return;

        _isDead = true;
        
        if (gameObject.TryGetComponent(out CPMPlayer cpmPlayer))
            cpmPlayer.enabled = false;
        
        if (gameObject.TryGetComponent(out MovementController movementController))
            movementController.enabled = false;
        
        gameObject.GetComponent<ShootBones>().enabled = false;
        crosshair.SetActive(false);
        retryScreen.SetActive(true);

        UnlockCursor();
    }

    private void UnlockCursor()
    {
        if (Cursor.lockState != CursorLockMode.Confined)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void FellOffMapCheck()
    {
        float playerHeight = gameObject.transform.position.y;
        if (playerHeight < -fallDistance)
        {
            Die();
        }
        else if (playerHeight < -fadeDistance)
        {
            float fadeAmount = 1 - Mathf.InverseLerp(-fallDistance, -fadeDistance, playerHeight);
            SetScreenFade(fadeAmount);
        }
    }

    private void SetScreenFade(float alpha)
    {
        Color fadeColor = fadeImage.color;
        fadeColor.a = Mathf.Clamp01(alpha);
        fadeImage.color = fadeColor;
    }

    private void Update()
    {
        if (!_isDead)
        {
            FellOffMapCheck();
        }
    }
}