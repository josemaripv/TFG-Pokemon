using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private List<SceneAudioConfig> sceneAudioConfigs;
    [SerializeField] private SceneAudioConfig battleAudioConfig; // Añadido para la música de batalla

    public static AudioManager Instance { get; private set; }

    private string currentSceneName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }

        if (musicPlayer == null)
        {
            musicPlayer = GetComponent<AudioSource>();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        PlayMusicForScene(currentSceneName);
    }

    public void PlayMusicForScene(string sceneName)
    {
        SceneAudioConfig config = sceneAudioConfigs.Find(config => config.sceneName == sceneName);
        if (config != null && config.musicClip != null)
        {
            PlayMusic(config.musicClip);
        }
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();
    }

    public void PlayBattleMusic()
    {
        if (battleAudioConfig != null && battleAudioConfig.musicClip != null)
        {
            PlayMusic(battleAudioConfig.musicClip);
        }
    }

    public void PlayMusicForCurrentScene()
    {
        PlayMusicForScene(currentSceneName);
    }
}