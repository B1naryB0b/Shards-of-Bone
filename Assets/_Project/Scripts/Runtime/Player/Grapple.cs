using UnityEngine;

public class Grapple : MonoBehaviour
{
    private Camera _playerCamera;
    private CPMPlayer _cpmPlayer;
    
    private Vector3 _grapplePoint;
    private Vector3 _grappleStartPoint;
    private bool _isGrappling;
    private float _grappleTravelTime;
    private float _grappleCooldownTime;
    private bool _isCoyoteTimeActive;
    
    private LineRenderer _lineRenderer;

    [Header("Grapple Settings")]
    [SerializeField] private LayerMask grappleLayerMask;
    [SerializeField] private float maxGrappleDistance = 100f;
    [SerializeField] private float grappleStrength;
    [SerializeField] private float maxGrappleTravelTime;
    [SerializeField] private float grappleCooldown;
    [SerializeField] private float coyoteTimeWindow = 0.2f;
    [SerializeField] private AnimationCurve grappleCurve;

    [Header("Visual Settings")]
    [SerializeField] private Transform grappleLineStart;
    [SerializeField] private RectTransform grappleIconRect;
    [SerializeField] private AnimationCurve grappleIconCurve;

    void Start()
    {
        _cpmPlayer = GetComponent<CPMPlayer>();
        _playerCamera = Camera.main;
        _lineRenderer = GetComponent<LineRenderer>();
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }
        _grappleCooldownTime = grappleCooldown;
    }

    void Update()
    {
        GrappleCooldown();
        HandleInput();

        if (_isGrappling)
        {
            GrappleMovement();
            DrawGrappleLine();
        }
    }

    private void GrappleCooldown()
    {
        if (_grappleCooldownTime < grappleCooldown)
        {
            _grappleCooldownTime += Time.deltaTime;
            UpdateGrappleIconRotation();
        }

        // Activate coyote time within the specified window before the cooldown ends
        if (_grappleCooldownTime >= grappleCooldown - coyoteTimeWindow && _grappleCooldownTime < grappleCooldown)
        {
            _isCoyoteTimeActive = true;
        }

        // Reset coyote time after cooldown is complete
        if (_grappleCooldownTime >= grappleCooldown)
        {
            _isCoyoteTimeActive = false;
        }
    }

    private void HandleInput()
    {
        if (Input.GetButtonDown("Fire2") && (_grappleCooldownTime >= grappleCooldown || _isCoyoteTimeActive))
        {
            ShootGrapple();
            _isCoyoteTimeActive = false;
        }

        if (Input.GetButtonUp("Fire2"))
        {
            StopGrapple();
        }
    }

    private void UpdateGrappleIconRotation()
    {
        float cooldownProgress = _grappleCooldownTime / grappleCooldown;
        float easedProgress = grappleIconCurve.Evaluate(cooldownProgress);
        float angle = easedProgress * 360f * 3f;
        grappleIconRect.localEulerAngles = new Vector3(0f, 0f, -angle);
    }


    private void ShootGrapple()
    {
        Transform playerCameraTransform = _playerCamera.transform;
        Ray ray = new Ray(playerCameraTransform.position, playerCameraTransform.forward);
        
        if (!Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance, grappleLayerMask)) return;
        
        _grapplePoint = hit.point;
        _grappleStartPoint = transform.position;
        _isGrappling = true;
        _grappleTravelTime = 0f;
        _grappleCooldownTime = 0f;
        
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = true;
            _lineRenderer.positionCount = 2;
        }
    }

    private void StopGrapple()
    {
        _isGrappling = false;
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }
    }

    private void GrappleMovement()
    {
        _grappleTravelTime += Time.deltaTime;
        _grappleStartPoint = transform.position;

        if (_grappleTravelTime < maxGrappleTravelTime)
        {
            float curveValue = grappleCurve.Evaluate(_grappleTravelTime / maxGrappleTravelTime);
            Vector3 direction = (_grapplePoint - _grappleStartPoint).normalized;
            Vector3 grappleForce = direction * (grappleStrength * curveValue);
            _cpmPlayer.AddExternalVelocity((grappleForce + grappleForce.GetAxis(Axis.Y)) * Time.deltaTime);
        }
        else
        {
            StopGrapple();
        }
    }

    private void DrawGrappleLine()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.SetPosition(0, grappleLineStart.position);
            _lineRenderer.SetPosition(1, _grapplePoint);
        }
    }
}
