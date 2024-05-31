using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Range(0f, 1f)]
    public float soundVolume = 1f;
    [Range(0f, 1f)]
    public float musicVolume = 1f;
    public float soundFalloffDistance = 5f;
    public AudioClip backgroundAmbientNoise;

    private float updatedMaxDistance = 7.5f;
    private AudioSource musicAudioSource;
    private AudioSource ambientNoiseSource;
    private Camera mainCamera;
    private List<AudioSource> playingAudioSources = new List<AudioSource>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return; // Exit the method to prevent further execution
        }

        musicAudioSource = gameObject.AddComponent<AudioSource>();
        ambientNoiseSource = gameObject.AddComponent<AudioSource>();
        mainCamera = Camera.main;
    }
    private void Start()
    {
        PlayAmbientNoise();
    }

    public void PlaySound(AudioClip clip, Transform parent)
    {
        if (clip != null)
        {
            GameObject soundObject = new GameObject("Sound");
            soundObject.transform.SetParent(parent);
            soundObject.transform.localPosition = Vector3.zero;

            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.spatialBlend = 1f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.maxDistance = updatedMaxDistance;
            audioSource.dopplerLevel = 0f;
            audioSource.volume = soundVolume;

            // Add the audio source to the list
            playingAudioSources.Add(audioSource);

            audioSource.Play();
            Destroy(soundObject, clip.length);
        }
    }

    private void UpdateMaxDistance()
    {
        float cameraSize = mainCamera.orthographicSize;
        float maxDistance = cameraSize;

        // Update the maxDistance for all playing audio sources
        for (int i = playingAudioSources.Count - 1; i >= 0; i--)
        {
            AudioSource audioSource = playingAudioSources[i];
            if (audioSource == null)
            {
                // Remove the null AudioSource from the list
                playingAudioSources.RemoveAt(i);
            }
            else
            {
                audioSource.maxDistance = maxDistance;
                updatedMaxDistance = maxDistance;
                Debug.LogError("maxDistance: " + maxDistance);
            }
        }
    }

    private void Update()
    {
        float cameraSize = mainCamera.orthographicSize * 2f;
        float remainder = cameraSize % 15f;
        if (remainder == 0f)
        {
            UpdateMaxDistance();
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
        {
            return; // Exit the method if the audio clip is null
        }

        musicAudioSource.clip = clip;
        musicAudioSource.volume = musicVolume;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    public void PlayAmbientNoise()
    {
        if (backgroundAmbientNoise == null)
        {
            return; // Exit the method if the audio clip is null
        }

        ambientNoiseSource.clip = backgroundAmbientNoise;
        ambientNoiseSource.volume = soundVolume * 0.1f;
        ambientNoiseSource.loop = true;
        ambientNoiseSource.Play();
    }
    private void OnValidate()
    {
        // Update sound volume
        SetSoundVolume(soundVolume);

        // Update music volume
        SetMusicVolume(musicVolume);
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = Mathf.Clamp01(volume);
        // Update the volume of all playing audio sources
        foreach (AudioSource audioSource in playingAudioSources)
        {
            audioSource.volume = soundVolume;
        }
        ambientNoiseSource.volume = soundVolume * 0.1f;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicAudioSource.volume = musicVolume;
    }
}
