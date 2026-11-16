using UnityEngine;

public class TopDownCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Camera Offset")]
    [SerializeField] private Vector3 offset;

    [Header("Camera Follow")]
    [SerializeField] private float followSpeed = 10f;

    private void Awake()
    {
        if (target != null)
            transform.position = target.position + offset;
    }

    private void LateUpdate()
    {
        if (target == null)
            return;
        
        Vector3 targetPosition = target.position + offset;

        float followAmount = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followAmount);
    }
}
