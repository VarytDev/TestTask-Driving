using BS.Pathfinding;
using DG.Tweening;
using UnityEngine;
using static BS.Pathfinding.AStarPathfinding;

public class CarMovementController : MonoBehaviour
{
    [SerializeField] private AStarNavigation navigationComponent = null;
    [SerializeField] private float carSpeed = 5f;

    private Node currentNode = null;
    private Node requestedRouteChange = null;
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
            requestedRouteChange = target;
            return;
        }

        Vector3[] pathCoordinates = GetPathCoordinates(currentNode, target);
        AdjustRouteToRightSideOfTheRoad(pathCoordinates, 0.25f);

        currentRouteTween = gameObject.transform.DOPath(pathCoordinates, CalculatePathTime(pathCoordinates), PathType.CatmullRom)
            .SetEase(Ease.InOutSine)
            .OnStepComplete(OnRouteStepCompleated)
            .OnComplete(()=> OnRouteCompleated(target));
    }

    private void OnRouteStepCompleated()
    {
        if(requestedRouteChange != null)
        {
            AbortRoute();
            GoToNode(requestedRouteChange);
            requestedRouteChange = null;
        }
    }

    private void AbortRoute()
    {
        currentRouteTween.Kill();
        SnapToClosestNode();
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

            if (i - 1 >= 0) //if previous point exist
            {
                direction += Quaternion.AngleAxis(-90, Vector3.forward) * (pathCoordinates[i] - pathCoordinates[i - 1]).normalized;
            }

            if (i + 1 < pathCoordinates.Length) //if next point exist
            {
                direction += Quaternion.AngleAxis(90, Vector3.forward) * (pathCoordinates[i] - pathCoordinates[i + 1]).normalized;
            }

            offsets[i] = direction.normalized * offsetFromCenter;
        }

        for (int i = 0;i < pathCoordinates.Length; i++) 
        {
            pathCoordinates[i] += offsets[i];
        }
    }
}
