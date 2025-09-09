using UnityEngine;

public class ZombieAudio : MonoBehaviour
{
    private Animator anim;
    [Header("Footstep Sesleri")]
    public AudioClip[] footstepClips;
    public AudioSource audioSourceFootsteps;

    [Header("Scream Sesleri")]
    public AudioClip[] screamClips;
    public AudioSource audioSourceScream;

    [Range(0f, 1f)] public float screamChance = 0.3f; // %30 ihtimal


    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Animation Event'ten çaðrýlýyor
    public void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        audioSourceFootsteps.PlayOneShot(footstepClips[index]);

        // Eðer koþma/chase modundaysa bazen scream ekle
        if (anim.GetFloat("speed") > 0.4f && screamClips.Length > 0)
        {
            float random = Random.Range(0f, 1f);
            if (random <= screamChance && !audioSourceScream.isPlaying)
            {
                int screamIndex = Random.Range(0, screamClips.Length);
                audioSourceScream.PlayOneShot(screamClips[screamIndex]);
            }
        }
    }
}
