using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System;
using Unity.VisualScripting;

public class SessionInfoUI : MonoBehaviour
{
    public TextMeshProUGUI sessionNameText;
    public TextMeshProUGUI PlayerCountText;
    public Button JoinButton;

    SessionInfo sessionInfo;

    public event Action<SessionInfo> OnJoinSession;

    public void SetInformation(SessionInfo sessionInfo)
    {
        this.sessionInfo = sessionInfo;
        sessionNameText.text = sessionInfo.Name;
        PlayerCountText.text = $"{sessionInfo.PlayerCount.ToString()}/{sessionInfo.MaxPlayers.ToString()}";

        bool IsJoinButtonActive = true;

        if(sessionInfo.PlayerCount >= sessionInfo.MaxPlayers)
        {
            IsJoinButtonActive = false;
        }
        JoinButton.gameObject.SetActive(IsJoinButtonActive);
    }

    public void OnClick()
    {
        OnJoinSession?.Invoke(sessionInfo);
    }

}
