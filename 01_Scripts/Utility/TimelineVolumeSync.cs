using UnityEngine;

public class TimelineVolumeSync : MonoBehaviour
{
    private AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (SoundEffect.Instance == null || _audioSource == null) return;

        // Calculate volume exactly how SoundEffect does it
        float targetVolume = SoundEffect.Instance.allSoundVolume 
                           * SoundEffect.Instance.seVolAdjust 
                           * AudioManager.Instance.globalVolume 
                           * 0.35f;

        // Force the AudioSource volume to match
        _audioSource.volume = targetVolume;
    }
}

