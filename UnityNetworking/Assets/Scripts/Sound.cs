using UnityEngine.Audio;
using UnityEngine;

// DOES NOT NEED TO BE ATTACHED TO ANYTHING

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    public AudioMixerGroup audioMixerGroup;

    [HideInInspector]// this lets us populate sounds via code, and will not change via inspector even though it is public
    public AudioSource source;
}
