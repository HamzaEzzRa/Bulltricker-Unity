using UnityEngine;

[System.Serializable]
public class Sound
{
    #region Members
    public AudioClip clip;
    public string name;
    [Range(0f, 1f)] public float volume;
    [Range(0.1f, 3f)] public float pitch;
    public bool loop;
    [HideInInspector] public bool isPaused;
    [HideInInspector] public AudioSource source;
    #endregion
}
