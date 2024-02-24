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
    [SerializeField] private float scatterAngleMultiplier = 0.5f;
    [SerializeField] private float scatterPosition = 0.5f;

    [Header("Shotgun Shot")]
    [SerializeField] private float shotgunFireRate = 0.5f;
    [SerializeField] private float shotgunShootingForce = 2000f;
    [SerializeField] private float shotgunScatterAngleMultiplier = 0.5f;
    [SerializeField] private float shotgunScatterPosition = 0.5f;
    [SerializeField] private int shotgunPellets = 10;
    [SerializeField] private float tapThreshold = 0.2f;

    private float fireCooldown = 0f;
    private float shotgunFireCooldown = 0f;
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
        HandleShootingInput();
        UpdateCooldowns();
    }

    private void HandleShootingInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            isButtonPressed = true;
            buttonPressedTime = Time.time;
        }

        if (isButtonPressed && Time.time - buttonPressedTime > tapThreshold && fireCooldown <= 0f)
        {
            ShootProjectile();
            fireCooldown = 1f / fireRate;
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (Time.time - buttonPressedTime <= tapThreshold && shotgunFireCooldown <= 0f)
            {
                ShootShotgunBlast();
                shotgunFireCooldown = 1f / shotgunFireRate;
            }
            isButtonPressed = false;
        }
    }

    private void UpdateCooldowns()
    {
        if (fireCooldown > 0f)
        {
            fireCooldown -= Time.deltaTime;
        }

        if (shotgunFireCooldown > 0f)
        {
            shotgunFireCooldown -= Time.deltaTime;
        }
    }

    private void ShootProjectile()
    {
        AudioController.Instance.PlaySound(projectileSFX, 0.1f);
        InstantiateAndShoot(projectileSpawnPoint, shootingForce, scatterAngleMultiplier, scatterPosition);
    }

    private void ShootShotgunBlast()
    {
        AudioController.Instance.PlaySound(shotgunSFX, 0.5f);

        for (int i = 0; i < shotgunPellets; i++)
        {
            InstantiateAndShoot(projectileSpawnPoint, shotgunShootingForce, shotgunScatterAngleMultiplier, shotgunScatterPosition);
        }
    }

    private void InstantiateAndShoot(Transform spawnPoint, float force, float scatterAngleMultiplier, float scatterPosition)
    {
        if (projectilePrefab == null || spawnPoint == null) return;

        Vector3 positionOffset = MovementCompensation();
        Vector3 positionVariance = AddVarianceToPosition(scatterPosition);
        Vector3 compensatedPosition = spawnPoint.position + positionOffset + positionVariance;
        Quaternion scatteredAngle = AddVarianceToRotation(spawnPoint.rotation, scatterAngleMultiplier);
        GameObject projectile = Instantiate(projectilePrefab, compensatedPosition, scatteredAngle);
        Rigidbody projectileRigidbody = projectile.GetComponent<Rigidbody>();
        if (projectileRigidbody != null)
        {
            projectileRigidbody.AddForce(projectile.transform.forward * force);
        }
    }


    private Vector3 MovementCompensation()
    {
        if (characterController == null || characterController.velocity.sqrMagnitude < Mathf.Epsilon)
            return Vector3.zero;

        Vector3 positionCompensation = movementPositionCompensationStrength * characterController.velocity * Time.deltaTime;

        return positionCompensation;
    }

    private Vector3 AddVarianceToPosition(float variance)
    {
        float varianceX = Random.Range(-variance, variance);
        float varianceY = Random.Range(-variance, variance);

        return new Vector3(varianceX, varianceY, 0f);
    }

    private Quaternion AddVarianceToRotation(Quaternion originalRotation, float variance)
    {
        Vector3 euler = originalRotation.eulerAngles;
        euler.x += Random.Range(-variance, variance);
        euler.y += Random.Range(-variance, variance);
        return Quaternion.Euler(euler);
    }


}
