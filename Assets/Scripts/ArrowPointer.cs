using UnityEngine;

public class ArrowPointer : MonoBehaviour
{
    private Transform follow;
    private Transform pointAt;
    [SerializeField] private float followRadius;
    public void SetPointAt(Transform follow, Transform pointAt)
    {
        this.follow = follow;
        this.pointAt = pointAt;
    }

    private void Update()
    {
        if (pointAt == null) return;
        transform.position = SetPosition(follow, pointAt.position);
        transform.LookAt(pointAt.position, Vector3.up);
    }

    private Vector3 SetPosition(Transform anchor, Vector3 targetPos)
    {
        Vector3 centerPosition = anchor.position; // Center position
        float distance = Vector3.Distance(targetPos, centerPosition); // Distance from anchor to position
        Vector3 position = targetPos; // Default position to targetPos

        Vector3 fromOriginToObject = targetPos - centerPosition; // Find vector between objects
        fromOriginToObject *= followRadius / distance; //Multiply by radius, then Divide by distance
        position = centerPosition + fromOriginToObject; // Add new vector to anchor position

        return position;
    }
}
