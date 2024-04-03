using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private GameObject retryScreen;
    [SerializeField] private GameObject crosshair;

    private bool isDead = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Die();
        }
    }


    private void Die()
    {
        gameObject.GetComponent<CPMPlayer>().enabled = false;
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
        if (!isDead && gameObject.transform.position.y < -50f)
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