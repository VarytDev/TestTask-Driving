using UnityEngine;

public class CarInteractionHandler : MonoBehaviour
{
    [SerializeField] private CarMovementController movementController;
    [SerializeField] private LayerMask interactionLayerMask = 1 << 3;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && TryFindParkingSpot(out ParkingSpot foundSpot) && foundSpot.AssignedNode != null)
        {
            movementController.GoToNode(foundSpot.AssignedNode);
        }
    }

    //TODO implement interactable interface
    private bool TryFindParkingSpot(out ParkingSpot foundSpot)
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPoint, interactionLayerMask);
        if (hitCollider != null && hitCollider.TryGetComponent(out foundSpot))
        {
            return true;
        }

        foundSpot = null;
        return false;
    }
}
