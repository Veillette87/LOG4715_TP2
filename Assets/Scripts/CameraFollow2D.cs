using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    public float smoothTime = 0.25f;
    public Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    [SerializeField] private Transform target;
    void Update()
    {
        if (target == null) return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
