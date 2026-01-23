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
        public bool loop = true; 
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

    // --- MUSIC CONTROLS ---

    public void PlayMusic(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null)
        {
            Debug.LogWarning("Music track not found: " + name);
            return;
        }

        if (musicSource.clip == s.clip && musicSource.isPlaying) return;

        musicSource.clip = s.clip;
        musicSource.volume = s.volume;
        musicSource.loop = s.loop; 
        musicSource.Play();
    }

    public void StartBattleMusic()
    {
        
        PlayMusic("Battle1"); 
    }

    public void PlayRestMusic()
    {
        PlayMusic("Rest"); 
    }

    public void PlayBossMusic()
    {
        PlayMusic("Boss"); 
    }

    public void PlayGameOverMusic()
    {
        PlayMusic("GameOver");
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // --- SFX CONTROLS ---
    public void PlaySFX(string name)
    {
        Sound s = sounds.Find(sound => sound.name == name);
        if (s == null) 
        {
            Debug.LogWarning("SFX not found: " + name);
            return;
        }
        sfxSource.volume = s.volume;
        sfxSource.pitch = s.pitch * Random.Range(0.9f, 1.1f); // Slight variation
        sfxSource.PlayOneShot(s.clip, s.volume);
    }
}