using UnityEngine;
using DG.Tweening;
using System.Collections;

public class SignController : MonoBehaviour
{
    [Header("Animasyon Ayarlarý")]
    public Vector3 openRotation = new Vector3(0, 90, 0);
    public Vector3 closeRotation = new Vector3(0, 0, 0);
    public float duration = 1f;
    public float autoCloseTime = 3f;
    public Ease easeType = Ease.OutBack;

    [Header("Ses Ayarlarý")]
    public AudioClip[] scareSounds;   // Birden fazla ses
    public float volume = 1f;
    [SerializeField] private AudioSource audioSource;

    [Header("Referanslar")]
    [SerializeField] private Transform signModel;   // Döndürülecek tabela objesi

    private int playerCount = 0;
    private Coroutine closeRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Eðer içeride hiç oyuncu yoksa (ilk kez giriyorsa) ses çal
            if (playerCount == 0)
            {
                PlayRandomSound();

                if (signModel != null)
                {
                    signModel.DORotate(openRotation, duration)
                             .SetEase(easeType);
                }

                if (closeRoutine != null)
                    StopCoroutine(closeRoutine);

                closeRoutine = StartCoroutine(AutoClose());
            }

            playerCount++;
        }
    }

    private void PlayRandomSound()
    {
        if (scareSounds != null && scareSounds.Length > 0 && audioSource != null)
        {
            if (!audioSource.isPlaying) // zaten ses çalmýyorsa
            {
                int randomIndex = Random.Range(0, scareSounds.Length);
                AudioClip chosenClip = scareSounds[randomIndex];
                audioSource.PlayOneShot(chosenClip, volume);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount = Mathf.Max(0, playerCount - 1);
        }
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(autoCloseTime);

        if (playerCount == 0 && signModel != null)
        {
            signModel.DORotate(closeRotation, duration)
                     .SetEase(easeType);
        }

        closeRoutine = null;
    }
}
