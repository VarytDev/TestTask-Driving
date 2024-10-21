using System.Collections.Generic;
using UnityEngine;

public class CarBezierPathHandler : MonoBehaviour
{
    [SerializeField] private float bezierControlPointDistance = 0.4f;
    [SerializeField] private float offsetToSmoothCorners = 0.25f;

    public Vector3[] CreateCubicBezierPath(Vector3[] pathCoordinates)
    {
        Vector3[] pathCoordinatesOffset = CalculateOffsetToSmoothCorners(pathCoordinates);
        BezierPoint[] bezierPoints = GenerateBezierControlPoints(pathCoordinates);

        for (int i = 0; i < bezierPoints.Length; i++)
        {
            bezierPoints[i].Offset(pathCoordinatesOffset[i]);
        }

        return ConvertBezierPointsToCubicBezierPathCoordinates(bezierPoints, pathCoordinatesOffset);
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
