using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrappleController : MonoBehaviour
{
    private Camera _playerCamera;
    private MovementController _movementController;
    
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


    private Vector3 _currentDisplacement;

    public void OnGrapplePerformed()
    {
        if ((_grappleCooldownTime >= grappleCooldown || _isCoyoteTimeActive))
        {
            ShootGrapple();
            _isCoyoteTimeActive = false;
        }
    }
    
    public void OnGrappleCanceled()
    {
        StopGrapple();
    }

    void Start()
    {
        _movementController = GetComponent<MovementController>();
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

        _currentDisplacement = _grapplePoint - _grappleStartPoint;
    }

    private void StopGrapple()
    {
        _isGrappling = false;
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = false;
        }

        _currentDisplacement = Vector3.zero;
    }

    private void GrappleMovement()
    {
        _grappleTravelTime += Time.deltaTime;
        _grappleStartPoint = transform.position;
        Vector3 normalizedPlayerVelocity = _movementController.PlayerVelocity.normalized;

        if (_grappleTravelTime < maxGrappleTravelTime)
        {
            float curveValue = grappleCurve.Evaluate(_grappleTravelTime / maxGrappleTravelTime);
            Vector3 displacement = _grapplePoint - _grappleStartPoint;
            Vector3 direction = displacement.normalized;
            float dotProduct = Vector3.Dot(normalizedPlayerVelocity, direction);
            Debug.Log(dotProduct);
            
            if (dotProduct < 0f)
            {
                Vector3 perpendicular = GetPerpendicularInPlane(normalizedPlayerVelocity, direction);
                Vector3 backStoppedVelocity = Vector3.Lerp(normalizedPlayerVelocity, perpendicular, backStopStrength * Time.deltaTime) * _movementController.PlayerVelocity.magnitude;
                _movementController.SetVelocity(backStoppedVelocity);
            }
            Vector3 grappleForce = direction * (grappleStrength * curveValue);
            Vector3 compensatedGrappleFroce =
                (grappleForce + (_currentDisplacement.y > 0f
                    ? grappleForce.GetAxis(Axis.Y) * Mathf.Lerp(0f, 0.1f, _currentDisplacement.y / 10f)
                    : Vector3.zero)) * Time.deltaTime;
            
            _movementController.AddExternalVelocity(compensatedGrappleFroce);
            Debug.Log(compensatedGrappleFroce);
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
