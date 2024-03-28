using UnityEngine;

public class EdgeCounterforce : MonoBehaviour
{
    [SerializeField] private float forceMultiplier = 10.0f;
    [SerializeField] private float cutoffDistance = 5.0f;
    [SerializeField] private Transform playerTransform;
    private CharacterController playerCharacterController;
    private Vector3 simulatedVelocity;

    private void Start()
    {
        if (playerTransform != null)
        {
            playerCharacterController = playerTransform.GetComponent<CharacterController>();
        }

        if (playerCharacterController == null)
        {
            Debug.LogError("EdgeCounterforce requires the player's CharacterController component to function.");
            enabled = false;
        }
    }

    private void Update()
    {
        Vector3 positionXZ = new Vector3(playerTransform.position.x, 0f, playerTransform.position.z);
        float distanceFromOrigin = positionXZ.magnitude;

        if (distanceFromOrigin > cutoffDistance)
        {
            Vector3 directionToOrigin = -positionXZ.normalized;
            float forceMagnitude = (distanceFromOrigin - cutoffDistance) * forceMultiplier;
            simulatedVelocity += directionToOrigin * forceMagnitude * Time.deltaTime;
        }
        else
        {
            simulatedVelocity = Vector3.Lerp(simulatedVelocity, Vector3.zero, Time.deltaTime * 1000f);
        }

        if (simulatedVelocity != Vector3.zero)
        {
            playerCharacterController.Move(simulatedVelocity * Time.deltaTime);
        }
    }
}
