using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    private enum FollowType
    {
        Disabled,
        Snap,
        Smooth
    }

    [SerializeField] private FollowType followType;
    [SerializeField] private Transform target;
    [SerializeField] private bool lerpPosition;
    [SerializeField] private bool lerpRotation;

    [SerializeField] private float smoothFollowSpeed;

    void Start()
    {
        
    }

    void Update()
    {
        if (followType == FollowType.Disabled) return;

        if (followType == FollowType.Snap) SnapFollow();

        if (followType == FollowType.Smooth) SmoothFollow();
    }

    private void SnapFollow()
    {
        if (lerpPosition) gameObject.transform.position = target.position;

        if (lerpRotation) gameObject.transform.rotation = target.rotation;
    }

    private void SmoothFollow()
    {
        if (lerpPosition) gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, target.position, Time.deltaTime * smoothFollowSpeed);

        if (lerpRotation) gameObject.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, target.rotation, Time.deltaTime * smoothFollowSpeed);
    }
}
