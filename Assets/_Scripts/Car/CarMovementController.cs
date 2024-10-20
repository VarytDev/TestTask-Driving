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
    [SerializeField] private float bezierControlPointDistance = 0.4f;
    [SerializeField] private float offsetToSmoothCorners = 0.25f;

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
        AdjustRouteToRightSideOfTheRoad(pathCoordinates, 0.25f);
        if(pathType == PathType.CubicBezier) 
        {
            pathCoordinates = CreateCubicBezierPath(pathCoordinates);
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

    private Vector3[] CreateCubicBezierPath(Vector3[] pathCoordinates)
    {
        Vector3[] pathCoordinatesOffset = CalculateOffsetToSmoothCorners(pathCoordinates);
        BezierPoint[] bezierPoints = GenerateBezierControlPoints(pathCoordinates);

        for (int i = 0; i < bezierPoints.Length; i++)
        {
            bezierPoints[i].Offset(pathCoordinatesOffset[i]);
        }

        return ConvertBezierPointsToCubicBezierPathCoordinates(bezierPoints, pathCoordinatesOffset);
    }

    private Vector3[] ConvertBezierPointsToCubicBezierPathCoordinates(BezierPoint[] bezierPoints, Vector3[] pathCoordinatesOffset)
    {
        List<Vector3> pathCoordinatesWithControlPoints = new();

        for (int i = 0; i < bezierPoints.Length; i++)
        {
            if (bezierPoints.IsInRange(i - 1))
            {
                pathCoordinatesWithControlPoints.Add(bezierPoints[i].ControlPointTowardsPrevious);
            }

            if (bezierPoints.IsInRange(i + 1))
            {
                pathCoordinatesWithControlPoints.Add(bezierPoints[i + 1].Point);
                pathCoordinatesWithControlPoints.Add(bezierPoints[i].ControlPointTowardsNext);
            }
        }

        return pathCoordinatesWithControlPoints.ToArray();
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

    private BezierPoint[] GenerateBezierControlPoints(Vector3[] pathCoordinates)
    {
        BezierPoint[] bezierPoints = new BezierPoint[pathCoordinates.Length];

        HandleFirstPoint(0);

        for (int i = 1; i < pathCoordinates.Length - 1; i++)
        {
            Vector3 directionFromPreviousPoint = pathCoordinates[i - 1].DirectionFromTo(pathCoordinates[i]);
            Vector3 directionFromNextPoint = pathCoordinates[i + 1].DirectionFromTo(pathCoordinates[i]);
            Vector3 directionForControlPoint = directionFromPreviousPoint.DirectionFromTo(directionFromNextPoint) * bezierControlPointDistance;

            CreateBezierPoint(i, pathCoordinates[i] - directionForControlPoint, pathCoordinates[i] + directionForControlPoint);
        }

        HandleLastPoint(pathCoordinates.Length - 1);

        return bezierPoints;

        void HandleFirstPoint(int index)
        {
            Vector3 directionToNextPoint = pathCoordinates[index].DirectionFromTo(pathCoordinates[index + 1]) * bezierControlPointDistance;
            CreateBezierPoint(index, pathCoordinates[index] + directionToNextPoint);
        }

        void HandleLastPoint(int index)
        {
            Vector3 directionToPreviousPoint = pathCoordinates[index].DirectionFromTo(pathCoordinates[index - 1]) * bezierControlPointDistance;
            CreateBezierPoint(index, controlPointTowardsPrevious: pathCoordinates[index] + directionToPreviousPoint);
        }

        void CreateBezierPoint(int index, Vector3 controlPointTowardsNext = default, Vector3 controlPointTowardsPrevious = default)
        {
            BezierPoint bezierPoint = new BezierPoint(pathCoordinates[index]);
            bezierPoint.ControlPointTowardsPrevious = controlPointTowardsPrevious;
            bezierPoint.ControlPointTowardsNext = controlPointTowardsNext;
            bezierPoints[index] = bezierPoint;
        }
    }

    private Vector3[] CalculateOffsetToSmoothCorners(Vector3[] pathCoordinates)
    {
        Vector3[] pathCoordinatesOffset = new Vector3[pathCoordinates.Length];

        //offset coordinate to the inside of the road (only for points that have point before and after)
        for (int i = 1; i < pathCoordinates.Length - 1; i++)
        {
            Vector3 directionToPreviousPoint = pathCoordinates[i].DirectionFromTo(pathCoordinates[i - 1]);
            Vector3 directionToNextPoint = pathCoordinates[i].DirectionFromTo(pathCoordinates[i + 1]);

            Vector3 directionSum = directionToPreviousPoint + directionToNextPoint;
            pathCoordinatesOffset[i] = directionSum.normalized * offsetToSmoothCorners * GetNormalizedTurnSharpness(directionToPreviousPoint, directionToNextPoint);
        }

        return pathCoordinatesOffset;
    }

    /// <summary>
    /// Returns normalized turn sharpness where the more perpendicular the turn the bigger value is returned
    /// </summary>
    /// <returns></returns>
    private float GetNormalizedTurnSharpness(Vector3 directionToPreviousPoint, Vector3 directionToNextPoint)
    {
        return (Mathf.Abs(Vector3.Dot(directionToPreviousPoint, directionToNextPoint)) - 1) * -1;
    }

    private struct BezierPoint
    {
        public BezierPoint(Vector3 point)
        {
            Point = point;
            ControlPointTowardsPrevious = Vector3.zero;
            ControlPointTowardsNext = Vector3.zero;
        }

        public Vector3 Point;
        public Vector3 ControlPointTowardsPrevious;
        public Vector3 ControlPointTowardsNext;

        public void Offset(Vector3 offset)
        {
            Point += offset;
            ControlPointTowardsPrevious += offset;
            ControlPointTowardsNext += offset;
        }
    }
}
