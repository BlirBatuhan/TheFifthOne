using UnityEngine;
using Fusion;
public class FootstepController : NetworkBehaviour
{
    private AudioSource audioSource;

    [Header("Footstep Sesleri")]
    public AudioClip[] footstepClips;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }


    public void PlayFootstepEvent()
    {
        if (Object.HasInputAuthority)
        {
            RpcPlayFootstep();
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void RpcPlayFootstep()
    {
        if (footstepClips.Length > 0)
        {
            AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
            audioSource.PlayOneShot(clip);
        }
    }
}
