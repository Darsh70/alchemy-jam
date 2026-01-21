using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 0.7f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;
    }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Sound Library")]
    public List<Sound> sounds;

    void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        PlayMusic("Theme");
        
    }

    public void PlayMusic(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null) return;
        Debug.Log(s.name);
        musicSource.clip = s.clip;
        musicSource.volume = s.volume;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null) 
        {
            Debug.LogWarning("Sound not found: " + name);
            return;
        }

        sfxSource.pitch = s.pitch * Random.Range(0.9f, 1.1f);
        sfxSource.PlayOneShot(s.clip, s.volume);
    }
}