using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slowed : MonoBehaviour
{
    [SerializeField] private float slowStrength;
    
    private CPMPlayer _cpmPlayer;

    private bool _isSlowed;
    private void Start()
    {
        _cpmPlayer = GetComponent<CPMPlayer>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Disabler"))
        {
            _isSlowed = true;
        }
        else
        {
            _isSlowed = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Disabler"))
        {
            _isSlowed = false;
        }
    }

    private void Update()
    {
        if (_isSlowed)
        {
            Vector3 velocity = _cpmPlayer.Controller.velocity;
            velocity = Vector3.Lerp(velocity, Vector3.zero, slowStrength * Time.deltaTime);
            _cpmPlayer.SetVelocity(velocity);
        }
    }
}
