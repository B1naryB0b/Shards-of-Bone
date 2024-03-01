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

    [SerializeField] private float maxGrappleDistance = 100f;
    [SerializeField] private LayerMask grappleLayerMask;
    [SerializeField] private AnimationCurve grappleCurve;
    [SerializeField] private float grappleStrength;
    [SerializeField] private float maxGrappleTravelTime;


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
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            ShootGrapple();
        }

        if (Input.GetButtonUp("Fire2"))
        {
            StopGrapple();
        }

        if (isGrappling)
        {
            GrappleMovement();
            DrawGrappleLine();
            Debug.Log("Works");

        }
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
            cpmPlayer.AddExternalMovementForce(grappleForce * Time.deltaTime);
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
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, grapplePoint);
        }
    }
}
