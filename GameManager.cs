using UnityEngine;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private Camera lobbyCamera;
    [SerializeField] private GameObject GateDoor; // Kilitli kap�
    [SerializeField] private int totalCollectables = 3; // Ka� obje toplanmas� gerekiyor

    [Networked]
    public int collectedCount { get; set; } // Network senkronize edilen sayac

    // Oyuncular i�in referans
    private Dictionary<PlayerRef, NetworkObject> players => SpawnPlayer.Instance.spawnedCharacters;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void KameraKapatRpc()
    {
        // Lobby kameras�n� kapat
        lobbyCamera.enabled = false;
        lobbyCamera.GetComponent<AudioListener>().enabled = false;

        // Oyuncular�n kameralar�n� aktifle�tir
        foreach (var player in players)
        {
            Camera playerCamera = player.Value.GetComponentInChildren<Camera>();
            if (playerCamera != null)
            {
                playerCamera.GetComponent<MyCam>().oyundaM� = false;
                Debug.Log($"Kamera kapat�ld�: {player.Value}");
            }
        }
    }

    // Collectable al�nd���nda �a��r�l�r
    [Rpc(RpcSources.All, RpcTargets.All)]
    public void CollectItemRpc()
    {
        collectedCount++;
        Debug.Log($"Toplanan e�ya say�s�: {collectedCount}");

        if (collectedCount >= totalCollectables)
        {
            UnlockDoorRpc();
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void UnlockDoorRpc()
    {
        Debug.Log("Kap� a��l�yor!");
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
