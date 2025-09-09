using UnityEngine;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private Camera lobbyCamera;
    [SerializeField] private GameObject GateDoor; // Kilitli kapý
    [SerializeField] private int totalCollectables = 3; // Kaç obje toplanmasý gerekiyor

    [Networked]
    public int collectedCount { get; set; } // Network senkronize edilen sayac

    // Oyuncular için referans
    private Dictionary<PlayerRef, NetworkObject> players => SpawnPlayer.Instance.spawnedCharacters;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void KameraKapatRpc()
    {
        // Lobby kamerasýný kapat
        lobbyCamera.enabled = false;
        lobbyCamera.GetComponent<AudioListener>().enabled = false;

        // Oyuncularýn kameralarýný aktifleþtir
        foreach (var player in players)
        {
            Camera playerCamera = player.Value.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                playerCamera.GetComponent<MyCam>().oyundaMý = false;
                Debug.Log($"Kamera kapatýldý: {player.Value}");
            }
        }
    }

    // Collectable alýndýðýnda çaðýrýlýr
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void CollectItemRpc()
    {
        collectedCount++;
        Debug.Log($"Toplanan eþya sayýsý: {collectedCount}");

        if (collectedCount >= totalCollectables)
        {
            UnlockDoorRpc();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void UnlockDoorRpc()
    {
        Debug.Log("Kapý açýlýyor!");
        if (GateDoor != null)
        {
            GateDoor.transform
            .DOLocalMoveY(GateDoor.transform.localPosition.y + 5f, 2f)
             .SetEase(Ease.OutQuad);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                KameraKapatRpc();
            }
        }
    }
}
