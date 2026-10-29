using UnityEngine;

// Plays a footstep on the "OnFootstep" animation events. Needs to be on the Animator's object.
[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;

    public float minPitch = 0.92f;
    public float maxPitch = 1.08f;
    public float minVolume = 0.8f;
    public float maxVolume = 1f;

    AudioSource source;
    int lastIndex = -1;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    public void OnFootstep(AnimationEvent evt)
    {
        // blend trees fire events from every clip in the blend, only play the main one
        if (evt.animatorClipInfo.weight < 0.5f) return;

        source.pitch = Random.Range(minPitch, maxPitch);
        source.PlayOneShot(clips[RandomClipIndex()], Random.Range(minVolume, maxVolume));
    }

    // random clip, but never the same one twice in a row
    int RandomClipIndex()
    {
        if (clips.Length == 1) return 0;

        int i = Random.Range(0, clips.Length - 1);
        if (lastIndex >= 0 && i >= lastIndex) i++;
        lastIndex = i;
        return i;
    }
}
