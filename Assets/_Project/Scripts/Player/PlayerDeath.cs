using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private GameObject retryScreen;

    private bool isDead = false;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
    }

    private void Die()
    {
        gameObject.GetComponent<CPMPlayer>().enabled = false;
        retryScreen.SetActive(true);

        UnlockCursor();
    }

    private void UnlockCursor()
    {
        if (Cursor.lockState != CursorLockMode.Confined)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }

    private void FellOffMapCheck()
    {
        if (!isDead && gameObject.transform.position.y < -10f)
        {
            Die();
            isDead = true;
        }
    }

    private void Update()
    {
        FellOffMapCheck();
    }

}
