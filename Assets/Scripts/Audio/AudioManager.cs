using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    #region Members
    public Sound[] soundArray;
    public static AudioManager instance;
    #endregion

    #region Awake
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(transform.gameObject);
        foreach (Sound sound in soundArray)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.loop = sound.loop;
        }
    }
    #endregion

    #region Start
    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == (int)SceneIndex.MAIN_MENU)
        {
            PlaySound("Ambient BG");
        }
    }
    #endregion

    #region Update
    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex == (int)SceneIndex.MAIN_MENU)
        {
            PlaySound("Ambient BG");
        }
        else if (SceneManager.GetActiveScene().buildIndex == (int)SceneIndex.GAME_SCREEN)
        {
            StopSound("Ambient BG");
        }
    }
    #endregion

    #region SoundControl
    public void PlaySound(string name)
    {
        Sound currentSound = Array.Find(soundArray, sound => sound.name == name);
        if (currentSound != null && !currentSound.source.isPlaying)
        {
            currentSound.source.Play();
            currentSound.source.volume = currentSound.volume;
            currentSound.source.pitch = currentSound.pitch;
        }
    }

    public void StopSound(string name)
    {
        Sound currentSound = Array.Find(soundArray, sound => sound.name == name);
        if (currentSound != null && currentSound.source.isPlaying)
        {
            currentSound.source.Stop();
        }
    }

    public void PauseSound(string name)
    {
        Sound currentSound = Array.Find(soundArray, sound => sound.name == name);
        if (currentSound != null && currentSound.source.isPlaying && currentSound.isPaused == false)
        {
            currentSound.source.Pause();
            currentSound.isPaused = true;
        }
    }

    public void ResumeSound(string name)
    {
        Sound currentSound = Array.Find(soundArray, sound => sound.name == name);
        if (currentSound != null && !currentSound.source.isPlaying && currentSound.isPaused == true)
        {
            currentSound.source.UnPause();
            currentSound.source.volume = currentSound.volume;
            currentSound.source.pitch = currentSound.pitch;
            currentSound.isPaused = false;
        }
    }
    #endregion
}