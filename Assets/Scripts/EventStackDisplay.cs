using UnityEngine;
using System.Collections.Generic;

public class EventStackDisplay : MonoBehaviour
{
    private struct EventObjAndAudioClip
    {
        public GameObject GameObject;
        public AudioClip AudioClip;

        public EventObjAndAudioClip(GameObject gameObject, AudioClip audioClip)
        {
            GameObject = gameObject;
            AudioClip = audioClip;
        }
    }

    [SerializeField] private TriggerEventDisplay displayPrefab;
    [SerializeField] private Transform list;

    private Queue<EventObjAndAudioClip> spawnedDisplays = new Queue<EventObjAndAudioClip>();

    [SerializeField] private AudioSource source;

    [SerializeField] private Vector2 minMaxVolume = new Vector2(.6f, .9f);
    [SerializeField] private Vector2 minMaxPitch = new Vector2(.7f, .9f);

    public void Push(StoredTriggerEventDisplayInfo info)
    {
        TriggerEventDisplay spawned = Instantiate(displayPrefab, list);
        spawnedDisplays.Enqueue(new EventObjAndAudioClip(spawned.gameObject, info.OnPopClip));
        spawned.Set(info);
    }

    public void Dequeue()
    {
        EventObjAndAudioClip spawned = spawnedDisplays.Dequeue();
        source.pitch = RandomHelper.RandomFloat(minMaxPitch);
        source.PlayOneShot(spawned.AudioClip, RandomHelper.RandomFloat(minMaxVolume));
        Destroy(spawned.GameObject);
    }
}
