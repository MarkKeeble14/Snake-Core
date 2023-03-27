using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class TogglableAudioSource
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;
    [SerializeField] private float volume;
    [SerializeField] private float pitch;

    private bool hasSetClip;

    public bool GetActive()
    {
        return audioSource.enabled;
    }

    public void SetActive(bool value)
    {
        if (!hasSetClip)
        {
            audioSource.clip = clip;
            hasSetClip = true;
        }
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.enabled = value;
    }
}