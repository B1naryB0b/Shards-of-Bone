using UnityEngine;
using System.Collections.Generic;

public class BoidManager : MonoBehaviour
{
    const int ThreadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    public Transform target;

    private List<Boid> boids = new List<Boid>();
    private ComputeBuffer boidBuffer;
    private BoidData[] boidData;

    void Update()
    {
        int numBoids = boids.Count;
        if (numBoids <= 0)
        {
            boids.RemoveAll(boid => boid == null);
            return;
        }

        if (boidData == null || boidData.Length != numBoids)
        {
            if (boidBuffer != null)
                boidBuffer.Release();

            boidData = new BoidData[numBoids];
            boidBuffer = new ComputeBuffer(numBoids, BoidData.Size);
        }

        for (int i = 0; i < numBoids; i++)
        {
            boidData[i].position = boids[i].position;
            boidData[i].direction = boids[i].forward;
        }

        boidBuffer.SetData(boidData);

        compute.SetBuffer(0, "boids", boidBuffer);
        compute.SetInt("numBoids", numBoids);
        compute.SetFloat("viewRadius", settings.perceptionRadius);
        compute.SetFloat("avoidRadius", settings.avoidanceRadius);

        int threadGroups = Mathf.CeilToInt(numBoids / (float)ThreadGroupSize);
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

        boids.RemoveAll(boid => boid == null);
    }

    public void AddBoid(Boid boid)
    {
        boids.Add(boid);
        boid.Initialize(settings, target);
        UpdateBuffer();
    }

    public void RemoveBoid(Boid boid)
    {
        boids.Remove(boid);
        UpdateBuffer();
    }

    private void UpdateBuffer()
    {
        if (boidBuffer != null)
            boidBuffer.Release();

        boidData = new BoidData[boids.Count];
        boidBuffer = new ComputeBuffer(boids.Count, BoidData.Size);
    }

    void OnDestroy()
    {
        if (boidBuffer != null)
            boidBuffer.Release();
    }

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;
        public Vector3 flockHeading;
        public Vector3 flockCentre;
        public Vector3 avoidanceHeading;
        public int numFlockmates;

        public static int Size => sizeof(float) * 3 * 5 + sizeof(int);
    }
}
