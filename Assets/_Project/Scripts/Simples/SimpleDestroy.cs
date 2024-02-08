using UnityEngine;

public class SimpleDestroy : MonoBehaviour
{
    [SerializeField] private string targetTag = "TargetTag";

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(targetTag))
        {
            Destroy(collision.gameObject);
        }
    }
}
