using UnityEngine;

public class ShootBones : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float movementPositionCompensationStrength;
    [SerializeField] private float movementAngleCompensationStrength;


    [Header("Continuous Shot")]
    [SerializeField] private float fireRate = 0.5f;
    [SerializeField] private float shootingForce = 1000f;
    [SerializeField] private float scatterIntensity = 0.5f;

    [Header("Shotgun Shot")]
    [SerializeField] private float shotgunFireRate = 0.5f;
    [SerializeField] private float shotgunShootingForce = 2000f;
    [SerializeField] private float shotgunScatterIntensity = 0.5f;
    [SerializeField] private int shotgunPellets = 10;
    [SerializeField] private float tapThreshold = 0.2f;

    private float nextFireTime = 0f;
    private float shotgunNextFireTime = 0f;
    private bool isButtonPressed = false;
    private float buttonPressedTime = 0f;

    [SerializeField] private AudioClip projectileSFX;
    [SerializeField] private AudioClip shotgunSFX;

    private CharacterController characterController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            isButtonPressed = true;
            buttonPressedTime = Time.time;
        }

        if (isButtonPressed && Time.time - buttonPressedTime > tapThreshold)
        {
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + (1f / fireRate);
                ShootProjectile();
            }
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (Time.time - buttonPressedTime <= tapThreshold)
            {
                if (Time.time >= shotgunNextFireTime)
                {
                    shotgunNextFireTime = Time.time + (1f / shotgunFireRate);
                    ShootShotgunBlast();
                }
            }
            isButtonPressed = false; // Reset the button pressed state
        }
    }

    private void ShootProjectile()
    {
        AudioController.Instance.PlaySound(projectileSFX, 0.3f);
        InstantiateAndShoot(projectileSpawnPoint, shootingForce, scatterIntensity);
    }

    private void ShootShotgunBlast()
    {
        AudioController.Instance.PlaySound(shotgunSFX);

        for (int i = 0; i < shotgunPellets; i++)
        {
            InstantiateAndShoot(projectileSpawnPoint, shotgunShootingForce, shotgunScatterIntensity);
        }
    }

    private void InstantiateAndShoot(Transform spawnPoint, float force, float scatter)
    {
        if (projectilePrefab == null || spawnPoint == null) return;

        (Vector3 positionOffset, Vector3 eulerRotationOffset) = MovementCompensation();
        Vector3 compensatedPosition = spawnPoint.position + positionOffset;
        Quaternion scatteredAngle = AddVarianceToRotation(spawnPoint.rotation /** Quaternion.Euler(eulerRotationOffset)*/, scatter);

        GameObject projectile = Instantiate(projectilePrefab, compensatedPosition, scatteredAngle);
        Rigidbody projectileRigidbody = projectile.GetComponent<Rigidbody>();

        if (projectileRigidbody != null)
        {
            projectileRigidbody.AddForce(projectile.transform.forward * force);
        }
    }


    private (Vector3, Vector3) MovementCompensation()
    {
        if (characterController == null || characterController.velocity.sqrMagnitude < Mathf.Epsilon)
            return (Vector3.zero, Vector3.zero);

        Vector3 angleCompensation = projectileSpawnPoint.eulerAngles;

        Vector3 positionCompensation = movementPositionCompensationStrength * characterController.velocity * Time.deltaTime;
        angleCompensation = new Vector3(angleCompensation.x, angleCompensation.y + (movementAngleCompensationStrength * characterController.velocity.x * Time.deltaTime), angleCompensation.z);

        return (positionCompensation, angleCompensation);
    }



    public Quaternion AddVarianceToRotation(Quaternion originalRotation, float variance)
    {
        // Convert the original rotation to Euler angles
        Vector3 euler = originalRotation.eulerAngles;

        // Add variance to the x and y angles
        euler.x += UnityEngine.Random.Range(-variance, variance);
        euler.y += UnityEngine.Random.Range(-variance, variance);

        // Return the new rotation as a Quaternion
        return Quaternion.Euler(euler);
    }

}
