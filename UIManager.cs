using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private TextMeshProUGUI itemPickupText; // gameUI altýndaki text

    private void Awake() => Instance = this;

    public void ShowPickupPrompt(string text)
    {
        if (itemPickupText == null) return;
        itemPickupText.text = text;
        itemPickupText.gameObject.SetActive(true);
    }

    public void HidePickupPrompt()
    {
        if (itemPickupText == null) return;
        itemPickupText.gameObject.SetActive(false);
    }
}

