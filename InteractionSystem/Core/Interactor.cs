using UnityEngine;
using Fusion;

public class Interactor : NetworkBehaviour
{
    [SerializeField] private float interactionRange = 1f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask interactableMask;

    public GameObject torch;
    private IInteractable currentInteractable = null;

    private void Update()
    {
        if (!HasInputAuthority) return;

        CheckForInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }

    private void CheckForInteractable()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactableMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null && interactable.CanInteract())
            {
                currentInteractable = interactable;
                UIManager.Instance?.ShowPickupPrompt("Press E to pick up");
                return;
            }
        }

        currentInteractable = null;
        UIManager.Instance?.HidePickupPrompt();
    }
}
