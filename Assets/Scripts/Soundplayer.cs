using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soundplayer : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip[] clips;
    [SerializeField] AudioClip[] rareclips;
    [SerializeField] float rarety = 0;

    public void playRandom()
    {
        if(Random.value < rarety)
            source.clip = rareclips[Random.Range(0, rareclips.Length)];
        else
            source.clip = clips[Random.Range(0, clips.Length)];
        source.Play();
    }

    public void playSound(AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }
    public void playSound(int i)
    {
        if (i > clips.Length-1)
            Debug.LogWarning("play sound index out of bounds");
        source.clip = clips[i];
        source.Play();
    }
}
