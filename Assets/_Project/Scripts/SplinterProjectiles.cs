using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplinterProjectiles : MonoBehaviour
{
    [SerializeField] private GameObject splinterObject;
    [SerializeField] private int splintersPerBullet;

    [SerializeField] private float scatterAngle;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Vector3 originalVelocity = gameObject.GetComponent<Rigidbody>().velocity;
            float speed = originalVelocity.magnitude;

            for (int i = 0; i < splintersPerBullet; i++)
            {
                GameObject splinter = Instantiate(splinterObject, transform.position, transform.rotation);
                Rigidbody rb = splinter.GetComponent<Rigidbody>();

                Quaternion variedRotation = AddVarianceToRotation(transform.rotation, scatterAngle);
                Vector3 variedDirection = variedRotation * Vector3.forward;
                rb.velocity = variedDirection * speed;
            }

            Destroy(gameObject);
        }
    }

    private Quaternion AddVarianceToRotation(Quaternion originalRotation, float variance)
    {
        Vector3 euler = originalRotation.eulerAngles;
        euler.x += UnityEngine.Random.Range(-variance, variance);
        euler.y += UnityEngine.Random.Range(-variance, variance);
        return Quaternion.Euler(euler);
    }


}
