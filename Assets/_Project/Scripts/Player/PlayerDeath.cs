using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private GameObject retryScreen;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            gameObject.GetComponent<CPMPlayer>().enabled = false;
            retryScreen.SetActive(true);

            UnlockCursor();
        }
    }

    private void UnlockCursor()
    {
        if (Cursor.lockState != CursorLockMode.Confined)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}
