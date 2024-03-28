using UnityEngine;

public class Wander : MonoBehaviour
{
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private float minDistance = 30f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float rotationSpeed = 2f;

    private Vector3 targetPosition;
    private bool isTurning;

    private void Start()
    {
        PickNewTargetPosition();
    }

    private void Update()
    {
        RotateTowardsTarget();
        MoveTowardsTarget();
    }

    private void PickNewTargetPosition()
    {
        Vector3 randomDirection;
        float randomDistance;
        Vector3 potentialTarget;

        do
        {
            randomDirection = Random.onUnitSphere;
            randomDirection.y = 0; // Keep the wandering on the horizontal plane
            randomDistance = Random.Range(0, maxDistance);
            potentialTarget = randomDirection.normalized * randomDistance;
            potentialTarget.y = transform.position.y;
        } while (Vector3.Distance(transform.position, potentialTarget) < minDistance);

        targetPosition = potentialTarget;
    }


    private void RotateTowardsTarget()
    {
        Vector3 targetDirection = targetPosition - transform.position;
        if (targetDirection == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        float step = rotationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);

        isTurning = !AreAnglesClose(transform.eulerAngles.y, targetRotation.eulerAngles.y, 0.1f);
    }


    private void MoveTowardsTarget()
    {
        if (isTurning) return;

        float step = moveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            PickNewTargetPosition();
        }
    }

    private bool AreAnglesClose(float angle1, float angle2, float tolerance)
    {
        float difference = Mathf.Abs(Mathf.DeltaAngle(angle1, angle2));
        return difference <= tolerance;
    }
}
