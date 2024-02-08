using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [SerializeField] private Vector3 _rotationAxis = Vector3.up; 
    [SerializeField] private float _rotationSpeed = 30f; 

    private void Update()
    {
        float degrees = _rotationSpeed * Time.deltaTime;

        transform.Rotate(_rotationAxis, degrees);
    }
}