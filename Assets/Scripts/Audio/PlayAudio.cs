using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAudio : MonoBehaviour
{
    // Reference to audio source component
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize audio source component
        audioSource = GetComponent<AudioSource>();
    }

    private void Play(AudioClip audio)
    {
        if (audioSource) 
        {
            audioSource.clip = audio;
            audioSource.Play(); 
        }
    }
}
