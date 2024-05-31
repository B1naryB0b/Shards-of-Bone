using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ShootBones : MonoBehaviour
{
    private PlayerInputActions _playerControls;
    private InputAction _fireInput;
    
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float movementPositionCompensationStrength;
    [SerializeField] private float movementAngleCompensationStrength;

    [Header("Continuous Shot")]
    [SerializeField] private float fireRate = 0.5f;
    public float FireRate
    {
        get => fireRate;
        set => fireRate = value;
    }
    
    [SerializeField] private float shootingForce = 1000f;
    public float ShootingForce
    {
        get => shootingForce;
        set => shootingForce = value;
    }
    
    [SerializeField] private float scatterAngleMultiplier = 0.5f;
    [SerializeField] private float scatterPosition = 0.5f;

    [Header("Shotgun Shot")]
    [SerializeField] private float shotgunFireRate = 0.5f;
    public float ShotgunFireRate
    {
        get => shotgunFireRate;
        set => shotgunFireRate = value;
    }
    
    [SerializeField] private float shotgunShootingForce = 2000f;
    public float ShotgunShootingForce
    {
        get => shotgunShootingForce;
        set => shotgunShootingForce = value;
    }
    
    [SerializeField] private float shotgunScatterAngleMultiplier = 0.5f;
    [SerializeField] private float shotgunScatterPosition = 0.5f;
    [SerializeField] private int shotgunPellets = 10;
    [SerializeField] private float tapThreshold = 0.2f;
    
    [HideInInspector] public float shotgunJumpDelay;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject handAnchor;
    [SerializeField] private float continuousRecoil;
    [SerializeField] private AnimationCurve continuousRecoilAnimationCurve;
    [SerializeField] private float shotgunRecoil;
    [SerializeField] private AnimationCurve shotgunRecoilAnimationCurve;



    [Header("Audio Feedback")]
    [SerializeField] private AudioClip projectileSFX;
    [SerializeField] private AudioClip shotgunSFX;

    private CharacterController _characterController;

    private float _fireCooldown = 0f;
    private float _shotgunFireCooldown = 0f;
    private bool _isFirePressed = false;
    private float _firePressedTime = 0f;
    

    [HideInInspector] public bool isShotgunFired;


    private void Awake()
    {
        _playerControls = new PlayerInputActions();
    }

    private void OnEnable()
    {
        _fireInput = _playerControls.Player.Fire;
        _fireInput.Enable();
        _fireInput.performed += OnFirePerformed;
        _fireInput.canceled += OnFireCanceled;
    }

    private void OnDisable()
    {
        _fireInput.Disable();
        _fireInput.performed -= OnFirePerformed;
        _fireInput.canceled -= OnFireCanceled;
    }

    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (_isFirePressed && Time.time - _firePressedTime > tapThreshold && _fireCooldown <= 0f)
        {
            ShootProjectile();
            _fireCooldown = 1f / fireRate;
        }
        
        UpdateCooldowns();
        
    }

    private void OnFirePerformed(InputAction.CallbackContext context)
    {
        _isFirePressed = true;
        _firePressedTime = Time.time;
    }

    private void OnFireCanceled(InputAction.CallbackContext context)
    {
        if (Time.time - _firePressedTime <= tapThreshold && _shotgunFireCooldown <= 0f)
        {
            ShootShotgunBlast();
            _shotgunFireCooldown = 1f / shotgunFireRate;
        }
        _isFirePressed = false;
    }
    
    /*private void HandleShootingInput()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            _isFirePressed = true;
            _firePressedTime = Time.time;
        }

        if (_isFirePressed && Time.time - _firePressedTime > tapThreshold && _fireCooldown <= 0f)
        {
            ShootProjectile();
            _fireCooldown = 1f / fireRate;
        }

        if (Input.GetButtonUp("Fire1"))
        {
            if (Time.time - _firePressedTime <= tapThreshold && _shotgunFireCooldown <= 0f)
            {
                ShootShotgunBlast();
                _shotgunFireCooldown = 1f / shotgunFireRate;
            }
            _isFirePressed = false;
        }
    }
    */

    private void UpdateCooldowns()
    {
        if (_fireCooldown > 0f)
        {
            _fireCooldown -= Time.deltaTime;
        }

        if (_shotgunFireCooldown > 0f)
        {
            _shotgunFireCooldown -= Time.deltaTime;
        }
    }

    private void ShootProjectile()
    {
        AudioController.Instance?.PlaySound(projectileSFX, 0.1f);
        InstantiateAndShoot(projectileSpawnPoint, shootingForce, scatterAngleMultiplier, scatterPosition);
    }

    private void ShootShotgunBlast()
    {
        AudioController.Instance?.PlaySound(shotgunSFX, 0.5f);

        for (int i = 0; i < shotgunPellets; i++)
        {
            InstantiateAndShoot(projectileSpawnPoint, shotgunShootingForce, shotgunScatterAngleMultiplier, shotgunScatterPosition);
        }
        
        isShotgunFired = true;
        StartCoroutine(Co_ResetShotgunJump());
    }
    
    private IEnumerator Co_ResetShotgunJump()
    {
        yield return new WaitForSeconds(shotgunJumpDelay);
        isShotgunFired = false;
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
        if (_characterController == null || _characterController.velocity.sqrMagnitude < Mathf.Epsilon)
            return Vector3.zero;

        Vector3 positionCompensation = movementPositionCompensationStrength * _characterController.velocity * Time.deltaTime;

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
