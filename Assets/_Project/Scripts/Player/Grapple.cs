using UnityEngine;

public class Grapple : MonoBehaviour
{
    private CharacterController characterController;
    private Camera playerCamera;
    private Vector3 grapplePoint;
    private Vector3 grappleStartPoint;
    private bool isGrappling;
    private LineRenderer lineRenderer;
    private CPMPlayer cpmPlayer;
    private float grappleTravelTime;
    private float grappleCooldownTime;

    [SerializeField] private Transform grappleLineStart;
    [SerializeField] private float maxGrappleDistance = 100f;
    [SerializeField] private LayerMask grappleLayerMask;
    [SerializeField] private AnimationCurve grappleCurve;
    [SerializeField] private float grappleStrength;
    [SerializeField] private float maxGrappleTravelTime;
    [SerializeField] private float grappleCooldown;
    [SerializeField] private RectTransform grappleIconRect;
    [SerializeField] private AnimationCurve grappleIconCurve;
    [SerializeField] private float coyoteTimeWindow = 0.2f;
    private bool isCoyoteTimeActive;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        cpmPlayer = GetComponent<CPMPlayer>();
        playerCamera = Camera.main;
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
        grappleCooldownTime = grappleCooldown;
    }

    void Update()
    {
        if (grappleCooldownTime < grappleCooldown)
        {
            grappleCooldownTime += Time.deltaTime;
            UpdateGrappleIconRotation();
        }

        // Activate coyote time within the specified window before the cooldown ends
        if (grappleCooldownTime >= grappleCooldown - coyoteTimeWindow && grappleCooldownTime < grappleCooldown)
        {
            isCoyoteTimeActive = true;
        }

        if (Input.GetButtonDown("Fire2") && (grappleCooldownTime >= grappleCooldown || isCoyoteTimeActive))
        {
            ShootGrapple();
            isCoyoteTimeActive = false;
        }

        if (Input.GetButtonUp("Fire2"))
        {
            StopGrapple();
        }

        if (isGrappling)
        {
            GrappleMovement();
            DrawGrappleLine();
        }

        // Reset coyote time after cooldown is complete
        if (grappleCooldownTime >= grappleCooldown)
        {
            isCoyoteTimeActive = false;
        }
    }

    private void UpdateGrappleIconRotation()
    {
        float cooldownProgress = grappleCooldownTime / grappleCooldown;
        float easedProgress = grappleIconCurve.Evaluate(cooldownProgress);
        float angle = easedProgress * 360f * 3f;
        grappleIconRect.localEulerAngles = new Vector3(0f, 0f, -angle);
    }


    private void ShootGrapple()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxGrappleDistance, grappleLayerMask))
        {
            grapplePoint = hit.point;
            grappleStartPoint = transform.position;
            isGrappling = true;
            grappleTravelTime = 0f;
            grappleCooldownTime = 0f;
            if (lineRenderer != null)
            {
                lineRenderer.enabled = true;
                lineRenderer.positionCount = 2;
            }
        }
    }

    private void StopGrapple()
    {
        isGrappling = false;
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    private void GrappleMovement()
    {
        grappleTravelTime += Time.deltaTime;
        grappleStartPoint = transform.position;

        if (grappleTravelTime < maxGrappleTravelTime)
        {
            float curveValue = grappleCurve.Evaluate(grappleTravelTime / maxGrappleTravelTime);
            Vector3 direction = (grapplePoint - grappleStartPoint).normalized;
            Vector3 grappleForce = direction * grappleStrength * curveValue;
            cpmPlayer.AddExternalVelocity((grappleForce + grappleForce.GetAxis(Axis.Y)) * Time.deltaTime);
        }
        else
        {
            StopGrapple();
        }
    }

    private void DrawGrappleLine()
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, grappleLineStart.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }
    }
}
