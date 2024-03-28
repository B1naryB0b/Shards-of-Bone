
using UnityEngine;

public interface ISpawnable
{
    void OnSpawn(Transform spawnerTarget, BoidManager boidManager);
}
