﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour, ISpawnable 
{

    private BoidSettings settings;

    // State
    [HideInInspector] public Vector3 position;
    [HideInInspector] public Vector3 forward;
    private Vector3 velocity;

    // To update:
    private Vector3 acceleration;
    [HideInInspector] public Vector3 avgFlockHeading;
    [HideInInspector] public Vector3 avgAvoidanceHeading;
    [HideInInspector] public Vector3 centreOfFlockmates;
    [HideInInspector] public int numPerceivedFlockmates;

    // Cached
    private Material material;
    private Transform cachedTransform;
    private Transform target;
    

    void Awake () {
        material = transform.GetComponentInChildren<MeshRenderer> ().material;
        cachedTransform = transform;
        
    }

    public void Initialize (BoidSettings settings, Transform target) {
        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = (settings.minSpeed + settings.maxSpeed) / 2;
        velocity = transform.forward * startSpeed;
    }
    
    
    private void OnDestroy()
    {
        BoidManager boidManager = FindObjectOfType<BoidManager>();
        if (boidManager != null)
        {
            boidManager.RemoveBoid(this);
        }
    }

    public void SetColour (Color col) {
        if (material != null) {
            material.color = col;
        }
    }

    public void UpdateBoid ()
    {
        acceleration = Vector3.zero;
        
        if (target != null) {
            Vector3 offsetToTarget = (target.position - position);
            acceleration = SteerTowards (offsetToTarget) * settings.targetWeight;
        }

        if (numPerceivedFlockmates != 0) {
            centreOfFlockmates /= numPerceivedFlockmates;

            Vector3 offsetToFlockmatesCentre = (centreOfFlockmates - position);

            var alignmentForce = SteerTowards (avgFlockHeading) * settings.alignWeight;
            var cohesionForce = SteerTowards (offsetToFlockmatesCentre) * settings.cohesionWeight;
            var seperationForce = SteerTowards (avgAvoidanceHeading) * settings.seperateWeight;

            acceleration += alignmentForce;
            acceleration += cohesionForce;
            acceleration += seperationForce;
        }
        
        if (IsHeadingForCollision ()) {
            Vector3 collisionAvoidDir = ObstacleRays ();
            Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.avoidCollisionWeight;
            acceleration += collisionAvoidForce;
        }

        Vector3 gravityForce = new Vector3(0, settings.gravity, 0);
        acceleration += gravityForce;

        if (position.y < settings.buoyancyThreshold)
        {
            Vector3 buoyancyForce = new Vector3(0, settings.buoyancyStrength, 0);
            acceleration += buoyancyForce;
        }

        velocity += acceleration * Time.deltaTime;
        float speed = velocity.magnitude;
        Vector3 dir = speed > 0 ? velocity / speed : Vector3.zero;
        speed = Mathf.Clamp (speed, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }

    bool IsHeadingForCollision () {
        RaycastHit hit;
        return Physics.SphereCast (position, settings.boundsRadius, forward, out hit, settings.collisionAvoidDst, settings.obstacleMask);
    }

    Vector3 ObstacleRays() {
    Vector3[] rayDirections = BoidHelper.Directions;

    for (int i = 0; i < rayDirections.Length; i++) {
        Vector3 dir = cachedTransform.TransformDirection(rayDirections[i]);
        Ray ray = new Ray(position, dir);

        if (!Physics.Raycast(ray, settings.collisionAvoidDst, settings.obstacleMask)) {
            return dir;
        }
    }

    return forward;
}


    Vector3 SteerTowards (Vector3 vector) {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude (v, settings.maxSteerForce);
    }

    public void OnSpawn(Transform spawnerTarget, BoidManager boidManager)
    {
        boidManager.AddBoid(this);
    }

}