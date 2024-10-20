using UnityEngine;

public class CarRotationHandler : MonoBehaviour
{
    [SerializeField] private float maxRotationDelta = 0.3f;

    private Vector3 previousPosition = Vector3.zero;
    private Vector3 targetDirection = Vector3.zero;

    private void Start()
    {
        previousPosition = transform.position;
        targetDirection = transform.right;
    }

    private void Update()
    {
        if(previousPosition != transform.position)
        {
            UpdateTargetRotation();
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(targetDirection, Vector3.back), maxRotationDelta * 100 * Time.deltaTime);
    }

    private void UpdateTargetRotation()
    {
        targetDirection = (transform.position - previousPosition).normalized;

        previousPosition = transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + targetDirection);
    }
}
