using BS.Pathfinding;
using UnityEngine;

public class ParkingSpot : MonoBehaviour
{
    public Node AssignedNode => assignedNode;

    [SerializeField] private Node assignedNode;
}
