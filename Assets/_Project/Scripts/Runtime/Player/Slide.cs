using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slide : MonoBehaviour
{

    [SerializeField] private float slideFriction;

    private float originalFriction;

    private CPMPlayer _cpmPlayer;
    
        // Start is called before the first frame update
    void Start()
    {
        _cpmPlayer = GetComponent<CPMPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
