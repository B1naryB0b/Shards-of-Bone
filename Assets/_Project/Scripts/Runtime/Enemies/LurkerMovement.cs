using UnityEngine;

public class LurkerMovement : MonoBehaviour, IPlayerTracker
{
    [SerializeField] private float movementSpeed = 10f;
    [SerializeField] private float hopFrequency = 5f;
    [SerializeField] private float hopAmplitude = 1f;
    [SerializeField] private float lookingAtAngleThreshold;
    [SerializeField] private Transform playerTransform;

    private Camera playerCamera;
    private bool isLookedAt;
    private float timeSinceLookedAt;

    private AudioSource audioSource;

    void Start()
    {
        playerCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (!isLookedAt)
        {
            CheckIfLookedAt();
        }
        else
        {
            HopToPlayer();
        }
    }

    private void HopToPlayer()
    {
        Vector3 horizontalDirection = (playerTransform.position - transform.position).normalized;
        horizontalDirection.y = 0;

        float timeAdjusted = Time.time - timeSinceLookedAt;
        float hopHeight = Mathf.Abs(Mathf.Sin((timeAdjusted * hopFrequency * Mathf.PI * 2))) * hopAmplitude;
        Vector3 newPosition = transform.position + (horizontalDirection * movementSpeed * Time.fixedDeltaTime);
        newPosition.y = hopHeight + (transform.localScale.y / 2);

        transform.position = newPosition;
    }

    private void CheckIfLookedAt()
    {
        if (playerCamera == null) return;
        float angle = CalculateAngleBetweenTransforms(playerCamera.transform, transform);
        if (angle < lookingAtAngleThreshold && !isLookedAt)
        {
            audioSource.Play();
            isLookedAt = true;
            timeSinceLookedAt = Time.time;
        }
    }

    private float CalculateAngleBetweenTransforms(Transform firstTransform, Transform secondTransform)
    {
        Vector3 directionToSecond = secondTransform.position - firstTransform.position;
        return Vector3.Angle(firstTransform.forward, directionToSecond);
    }

    public void SetPlayerTransform(Transform transform)
    {
        playerTransform = transform;
    }
}
