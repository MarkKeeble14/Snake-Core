using UnityEngine;

[System.Serializable]
public class AudioClipContainer
{
    [SerializeField] private AudioClip clip;
    public AudioClip Clip => clip;
    [SerializeField] private float volume = 1;
    public float Volume => volume;
    [SerializeField] private float pitch = 1;
    public float Pitch => pitch;
    [SerializeField] private AudioSource source;
    public AudioSource Source { get { return source; } set { source = value; } }

    [SerializeField] private bool randomizeVolume;
    [SerializeField] private Vector2 minMaxVolume;
    [SerializeField] private bool randomizePitch;
    [SerializeField] private Vector2 minMaxPitch;
    [SerializeField] private bool useTemporaryAudioSource;

    public void SetRandoms()
    {
        if (randomizePitch)
        {
            source.pitch = RandomHelper.RandomFloat(minMaxPitch);
        }
        else
        {
            source.pitch = pitch;
        }

        if (randomizeVolume)
        {
            volume = RandomHelper.RandomFloat(minMaxVolume);
        }
    }

    public void PlayOneShot()
    {
        if (useTemporaryAudioSource)
        {
            GridGenerator._Instance.PlayFromTemporaryAudioSource(this);
        }
        else
        {
            if (!source) return;
            if (!clip) return;
            SetRandoms();
            source.PlayOneShot(clip, volume);
        }
    }
}
