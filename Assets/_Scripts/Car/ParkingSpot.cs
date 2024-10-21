using BS.Pathfinding;
using UnityEngine;

public class ParkingSpot : MonoBehaviour, IInteractable
{
    [SerializeField] private Node assignedNode;

    public void OnInteract(CarInteractionHandler sender)
    {
        if (!assignedNode)
        {
            Debug.LogWarning($"{this} :: Assigned node is null!");
            return;
        }

        sender.GoToNode(assignedNode);
    }

    public Node GetAssignedNode()
    {
        return assignedNode;
    }
}
