using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance;

    [SerializeField] private AudioSource _musicSource, _sfxSource;

    private GameObject sfxGameObject;

    private void Start()
    {
        sfxGameObject = _sfxSource.gameObject;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (_sfxSource == null) { _sfxSource = sfxGameObject.GetComponent<AudioSource>(); }

        if (clip == null)
        {
            Debug.LogError("AudioClip is null!");
            return;
        }

        _sfxSource.PlayOneShot(clip, volume);
    }

    public void ChangeMusicVolume(float value)
    {
        _musicSource.volume = value;
    }

    public void ChangeSFXVolume(float value)
    {
        _sfxSource.volume = value;
    }

    public AudioController GetAudioControllerInstance()
    {
        return Instance;
    }
}
