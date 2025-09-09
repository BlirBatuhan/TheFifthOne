using DG.Tweening;
using Fusion;
using UnityEngine;

public class ItemPickUp : NetworkBehaviour, IInteractable
{
    [SerializeField] private float animDuration = 0.4f;
    [SerializeField] private AudioClip pickupSound;

    [Networked] private bool pickedUp { get; set; }

    public bool CanInteract()
    {
        return !pickedUp;
    }

    public bool Interact(Interactor interactor)
    {
        if (pickedUp || interactor == null) return false;

        pickedUp = true;

        // collider kapat
        var col = GetComponent<Collider>();
        if (col) col.enabled = false;

        // animasyonu ve sesi tüm clientlara gönder
        RPC_PlayPickupEffects(interactor.Object);

        return true;
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_PlayPickupEffects(NetworkObject interactorObj)
    {
        // ses
        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position, 1f);

        if (interactorObj != null)
        {
            var interactor = interactorObj.GetComponent<Interactor>();
            if (interactor != null)
            {
                // Torch animasyonunu tetikle (tüm clientler görecek)
                var anim = interactor.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetBool("HasTorch", true);
                    interactor.torch.SetActive(true);
                }

                // item interactor'a doðru uçsun
                transform.DOMove(interactor.transform.position, animDuration)
                         .SetEase(Ease.OutQuad)
                         .OnComplete(() =>
                         {
                             if (Object != null && Object.HasStateAuthority)
                                 Runner.Despawn(Object);
                         });
            }
        }
    }
}
