using UnityEngine;
using TMPro;
using System.Collections.Generic; // Needed for the Queue

public class DisplaySpeed : MonoBehaviour
{
    [SerializeField] private GameObject target;
    [SerializeField] private TextMeshProUGUI currentSpeedText;
    [SerializeField] private TextMeshProUGUI topSpeedText;

    private Vector3 _prevPosition;
    private float _topSpeed = 0f;
    private Queue<float> _speeds = new Queue<float>(); // Queue to hold the last 10 speeds

    void Start()
    {
        if (target != null)
        {
            _prevPosition = target.transform.position;
        }
    }

    void Update()
    {
        if (target != null)
        {
            float distance = Vector3.Distance(target.transform.position, _prevPosition);
            float currentSpeed = distance / Time.deltaTime;
            _speeds.Enqueue(currentSpeed);

            // Keep only the last 10 speeds
            if (_speeds.Count > 10)
            {
                _speeds.Dequeue();
            }

            float averageSpeed = 0f;
            foreach (float speed in _speeds)
            {
                averageSpeed += speed;
            }
            averageSpeed /= _speeds.Count;

            if (averageSpeed > _topSpeed)
            {
                _topSpeed = averageSpeed;
            }

            _prevPosition = target.transform.position;

            if (currentSpeedText != null)
            {
                currentSpeedText.text = $"Speed: {averageSpeed:F2} u/s";
            }
            if (topSpeedText != null)
            {
                topSpeedText.text = $"T_Speed: {_topSpeed:F2} u/s";
            }
        }
    }
}