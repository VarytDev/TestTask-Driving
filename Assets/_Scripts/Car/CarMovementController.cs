using BS.Pathfinding;
using DG.Tweening;
using UnityEngine;
using static BS.Pathfinding.AStarPathfinding;

public class CarMovementController : MonoBehaviour
{
    [SerializeField] private AStarNavigation navigationComponent = null;
    [SerializeField] private CarBezierPathHandler bezierPathHandler = null;
    [SerializeField] private PathType pathType = PathType.Linear;
    [SerializeField] private float carSpeed = 5f;
    [SerializeField] private float distanceFromCenterOfRoad = 0.25f;

    private Node currentNode = null;
    private Tween currentRouteTween = null;

    private void Start()
    {
        if(!navigationComponent)
        {
            Debug.LogWarning($"{this} :: Can't initialize");
            return;
        }

        SnapToClosestNode();
    }

    public void GoToNode(Node target)
    {
        if(currentNode == null || target == null)
        {
            return;
        }

        if(currentRouteTween.IsActive() && currentRouteTween.IsPlaying())
        {
            return;
        }

        Vector3[] pathCoordinates = GetPathCoordinates(currentNode, target);
        AdjustRouteToRightSideOfTheRoad(pathCoordinates, distanceFromCenterOfRoad);
        if(pathType == PathType.CubicBezier && bezierPathHandler) 
        {
            pathCoordinates = bezierPathHandler.CreateCubicBezierPath(pathCoordinates);
        }

        currentRouteTween = gameObject.transform.DOPath(pathCoordinates, CalculatePathTime(pathCoordinates), pathType)
            .SetEase(Ease.InOutSine)
            .OnComplete(()=> OnRouteCompleated(target));
    }

    private void OnRouteCompleated(Node target)
    {
        currentNode = target;
    }

    private void SnapToClosestNode()
    {
        currentNode = navigationComponent.FindClosestNodeToPosition(transform.position);
        transform.position = currentNode.transform.position;
    }

    private float CalculatePathTime(Vector3[] coordinates)
    {
        return GetPathLength(coordinates) / carSpeed;
    }

    private void AdjustRouteToRightSideOfTheRoad(Vector3[] pathCoordinates, float offsetFromCenter)
    {
        Vector3[] offsets = new Vector3[pathCoordinates.Length];

        for (int i = 0; i < pathCoordinates.Length; i++)
        {
            Vector3 direction = Vector3.zero;

            if (pathCoordinates.IsInRange(i - 1))
            {
                direction += Quaternion.AngleAxis(-90, Vector3.forward) * pathCoordinates[i - 1].DirectionFromTo(pathCoordinates[i]);
            }

            if (pathCoordinates.IsInRange(i + 1))
            {
                direction += Quaternion.AngleAxis(90, Vector3.forward) * pathCoordinates[i + 1].DirectionFromTo(pathCoordinates[i]);
            }

            offsets[i] = direction.normalized * offsetFromCenter;
        }

        for (int i = 0;i < pathCoordinates.Length; i++) 
        {
            pathCoordinates[i] += offsets[i];
        }
    }
}
