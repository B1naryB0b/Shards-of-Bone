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
    [SerializeField] private float backStopStrength;

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
        _cpmPlayer.SetGrapplingState(_isGrappling);  // Set the grappling state when grappling starts
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
        _cpmPlayer.SetGrapplingState(_isGrappling);  // Reset the grappling state when grappling stops
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }
    }

    private void GrappleMovement()
    {
        _grappleTravelTime += Time.deltaTime;
        _grappleStartPoint = transform.position;
        Vector3 normalizedPlayerVelocity = _cpmPlayer.PlayerVelocity.normalized;

        if (_grappleTravelTime < maxGrappleTravelTime)
        {
            float curveValue = grappleCurve.Evaluate(_grappleTravelTime / maxGrappleTravelTime);
            Vector3 direction = (_grapplePoint - _grappleStartPoint).normalized;
            float dotProduct = Vector3.Dot(normalizedPlayerVelocity, direction);
            Debug.Log(dotProduct);
            
            if (dotProduct < 0f)
            {
                Vector3 perpendicular = GetPerpendicularInPlane(normalizedPlayerVelocity, direction);
                Vector3 backStoppedVelocity = Vector3.Lerp(normalizedPlayerVelocity, perpendicular, backStopStrength * Time.deltaTime) * _cpmPlayer.PlayerVelocity.magnitude;
                _cpmPlayer.SetVelocity(backStoppedVelocity);
            }
            
            
            Vector3 grappleForce = direction * (grappleStrength * curveValue);
            _cpmPlayer.AddExternalVelocity((grappleForce + (grappleForce.y > 0f ? grappleForce.GetAxis(Axis.Y) : Vector3.zero)) * Time.deltaTime);
            
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
    
    public Vector3 GetPerpendicularInPlane(Vector3 vectorA, Vector3 vectorB)
    {
        // Step 1: Find the vector perpendicular to the plane formed by A and B
        Vector3 normalToPlane = Vector3.Cross(vectorA, vectorB);

        // Step 2: Find the vector in the plane that is perpendicular to A
        // This vector will be on the "side" of A as determined by the cross product with B
        Vector3 perpendicularInPlane = Vector3.Cross(normalToPlane, vectorA);

        // Normalize the result to get a direction vector if needed
        perpendicularInPlane.Normalize();

        return perpendicularInPlane;
    }
}