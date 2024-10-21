using BS.Pathfinding;
using UnityEngine;

public class CarInteractionHandler : MonoBehaviour
{
    [SerializeField] private CarMovementController movementController;
    [SerializeField] private LayerMask interactionLayerMask = 1 << 3;

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) && TryFindInteractable(out IInteractable foundInteractable))
        {
            foundInteractable.OnInteract(this);
        }
    }

    public void GoToNode(Node targetNode)
    {
        if(!movementController)
        {
            Debug.LogWarning($"{this} :: Can't start a route, movement controller is null!");
            return;
        }

        movementController.GoToNode(targetNode);
    }

    private bool TryFindInteractable(out IInteractable foundInteractable)
    {
        Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPoint, interactionLayerMask);
        if (hitCollider != null && hitCollider.TryGetComponent(out foundInteractable))
        {
            return true;
        }

        foundInteractable = null;
        return false;
    }
}
