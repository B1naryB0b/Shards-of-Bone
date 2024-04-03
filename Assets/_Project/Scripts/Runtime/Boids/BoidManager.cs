using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BoidManager : MonoBehaviour
{

    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    public Transform target;
     
    private List<Boid> boids = new List<Boid>();

    public void AddBoid(Boid boid)
    {
        boids.Add(boid);
        boid.Initialize(settings, target);
    }

    public void RemoveBoid(Boid boid)
    {
        boids.Remove(boid);
    }

    void Update()
    {
        
        if (boids.Count > 0)
        {
            int numBoids = boids.Count;
            var boidData = new BoidData[numBoids];
            Debug.Log(numBoids);

            for (int i = 0; i < numBoids; i++)
            {
                boidData[i].position = boids[i].position;
                boidData[i].direction = boids[i].forward;
            }

            var boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
            boidBuffer.SetData(boidData);

            compute.SetBuffer(0, "boids", boidBuffer);
            compute.SetInt("numBoids", numBoids);
            compute.SetFloat("viewRadius", settings.perceptionRadius);
            compute.SetFloat("avoidRadius", settings.avoidanceRadius);

            int threadGroups = Mathf.CeilToInt(numBoids / (float)threadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(boidData);

            for (int i = 0; i < numBoids; i++)
            {
                boids[i].avgFlockHeading = boidData[i].flockHeading;
                boids[i].centreOfFlockmates = boidData[i].flockCentre;
                boids[i].avgAvoidanceHeading = boidData[i].avoidanceHeading;
                boids[i].numPerceivedFlockmates = boidData[i].numFlockmates;

                boids[i].UpdateBoid();
            }

            boidBuffer.Release();
        }

        boids.RemoveAll(boid => boid == null);
    }

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size
        {
            get { return sizeof(float) * 3 * 5 + sizeof(int); }
        }
    }
}