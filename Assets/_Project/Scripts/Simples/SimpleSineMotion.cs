using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleSineMotion : MonoBehaviour
{
    [SerializeField] private Vector3 oscillationAxis;
    [SerializeField] private float amplitude;
    [SerializeField] private float frequency;
    [SerializeField] private float phaseDifference;

    private Vector3 startingPosition;

    // Start is called before the first frame update
    void Start()
    {
        startingPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = startingPosition + (oscillationAxis * CalculateSine(amplitude, frequency, phaseDifference));
    }

    private float CalculateSine(float amplitude, float frequency, float phaseDifference)
    {
        return amplitude * Mathf.Sin(frequency * Time.time + phaseDifference);
    }

}
