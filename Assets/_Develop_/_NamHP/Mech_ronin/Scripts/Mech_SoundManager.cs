using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mech_SoundManager : MonoBehaviour
{
    [TextArea(5, 10)] public string Description = "";
    [Space]
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioSource audioLoopSource;

    [SerializeField] AudioClip[] audioClips;
    bool haveSound;

    private void OnEnable()
    {
        if (audioClips == null)
        {
            haveSound = false;
        }
        else
            haveSound = true;
    }

    public void PlayOneShot(int audioId)
    {
        if (!haveSound || audioSource == null) return;
        if (audioId >= 0 && audioId <= audioClips.Length)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(audioClips[audioId]);
        }
    }

    public void StopSound()
    {
        if (audioSource)
        {
            audioSource.Stop();
        }
    }

    public void PlayLoopSound()
    {
        if (!haveSound || audioLoopSource == null) return;
        audioLoopSource.Play();
    }

    public void StopLoopSound()
    {
        if (audioLoopSource)
        {
            audioLoopSource.Stop();
        }
    }

}
