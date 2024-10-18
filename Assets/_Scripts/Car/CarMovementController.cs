using BS.Pathfinding;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using static BS.Pathfinding.AStarPathfinding;

public class CarMovementController : MonoBehaviour
{
    [SerializeField] private AStarNavigation navigationComponent = null;
    [SerializeField] private PathType pathType = PathType.Linear;
    [SerializeField] private float carSpeed = 5f;
    [SerializeField] private float bezierSmoothingFactor = 0.2f;

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
        if(pathType == PathType.CubicBezier) 
        {
            pathCoordinates = CreateBezierControlPoints(pathCoordinates);
        }

        currentRouteTween = gameObject.transform.DOPath(pathCoordinates, CalculatePathTime(pathCoordinates), pathType)
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

    //TODO pls god, help me refactor this mess
    private Vector3[] CreateBezierControlPoints(Vector3[] pathCoordinates)
    {
        List<Vector3> pathCoordinatesWithControlPoints = new();
        Vector3[] pathCoordinatesOffset = new Vector3[pathCoordinates.Length];

        //offset coordinate to the inside of the road
        //for (int i = 0; i < pathCoordinates.Length; i++)
        //{
        //    if (i - 1 >= 0 && i + 1 < pathCoordinates.Length) //if previous and next point exist
        //    {
        //        Vector3 directionToPreviousPoint = (pathCoordinates[i - 1] - pathCoordinates[i]).normalized;

        //        Vector3 directionToNextPoint = (pathCoordinates[i + 1] - pathCoordinates[i]).normalized;

        //        Vector3 directionSum = directionToPreviousPoint + directionToNextPoint;

        //        if (directionSum.magnitude < float.Epsilon) continue;

        //        pathCoordinatesOffset[i] = directionSum.normalized * 0.25f;
        //    }
        //}

        for (int i = 0; i < pathCoordinates.Length; i++)
        {
            //pathCoordinates[i] += pathCoordinatesOffset[i];

            if (i - 1 >= 0 && i + 1 < pathCoordinates.Length) //if previous and next point exist
            {
                Vector3 directionFromPreviousPoint = (pathCoordinates[i] - pathCoordinates[i - 1]).normalized;
                
                Vector3 directionFromNextPoint = (pathCoordinates[i] - pathCoordinates[i + 1]).normalized;

                Vector3 directionForControlPoint = (directionFromNextPoint - directionFromPreviousPoint).normalized * bezierSmoothingFactor;

                pathCoordinatesWithControlPoints.Add(pathCoordinates[i] + directionForControlPoint);
                pathCoordinatesWithControlPoints.Add(pathCoordinates[i + 1]);
                pathCoordinatesWithControlPoints.Add(pathCoordinates[i] - directionForControlPoint);
            }
            else if (i - 1 >= 0) //if previous point exist
            {
                Vector3 directionBackward = (pathCoordinates[i - 1] - pathCoordinates[i]).normalized * bezierSmoothingFactor;
                pathCoordinatesWithControlPoints.Add(pathCoordinates[i] + directionBackward);

            }
            else if (i + 1 < pathCoordinates.Length) //if next point exist
            {
                Vector3 directionForward = (pathCoordinates[i + 1] - pathCoordinates[i]).normalized * bezierSmoothingFactor;
                pathCoordinatesWithControlPoints.Add(pathCoordinates[i + 1]);
                pathCoordinatesWithControlPoints.Add(pathCoordinates[i] + directionForward);
            }
        }

        return pathCoordinatesWithControlPoints.ToArray();
    }

    //TODO Refactor this
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
