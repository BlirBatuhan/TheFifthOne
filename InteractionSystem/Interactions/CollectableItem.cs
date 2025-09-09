using Fusion;
using UnityEngine;

public class CollectableItem : NetworkBehaviour, IInteractable
{
    public override void Spawned()
    {
        
    }

    public bool CanInteract() => true;

    public bool Interact(Interactor interactor)
    {
        var manager = FindObjectOfType<GameManager>();
        if (manager != null)
        {
            manager.CollectItemRpc();
        }

        RPC_RequestDespawn();
        return true;
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_RequestDespawn()
    {
        if (Object != null)
            Runner.Despawn(Object);
    }

}
