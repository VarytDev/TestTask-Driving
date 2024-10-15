using BS.Pathfinding;
using DG.Tweening;
using UnityEngine;
using static BS.Pathfinding.AStarPathfinding;

public class CarMovementController : MonoBehaviour
{
    [SerializeField] private AStar aStarComponent = null;
    [SerializeField] private float carSpeed = 5f;

    //TODO Remove
    [SerializeField] private Node debugTargetNode = null;

    private Node currentNode = null;

    private void Start()
    {
        if(!aStarComponent)
        {
            Debug.LogWarning($"{this} :: Can't initialize");
            return;
        }

        currentNode = aStarComponent.FindClosestNodeToPosition(transform.position);
        transform.position = currentNode.transform.position;
    }

    //TODO When starting path, car starts with node 0
    public void GoToNode(Node target)
    {
        if(currentNode == null || target == null)
        {
            return;
        }

        Vector3[] pathCoordinates = GetPathCoordinates(currentNode, target);
        gameObject.transform.DOPath(pathCoordinates, CalculatePathTime(pathCoordinates));
        //gameObject.transform.DOPath(pathCoordinates, CalculatePathTime(pathCoordinates), PathType.CubicBezier);
    }

    private float CalculatePathTime(Vector3[] coordinates)
    {
        return GetPathLength(coordinates) / carSpeed;
    }

    //TODO remove
    private void Update()
    {
        if(debugTargetNode != null)
        {
            GoToNode(debugTargetNode);
            debugTargetNode = null;
        }
    }
}
