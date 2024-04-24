using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeVolume : MonoBehaviour
{
    private Slider volumeSlider;
    public void Volume()
    {
        AudioListener.volume = volumeSlider.value;
        PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
    }
    private void Awake()
    {
        volumeSlider = GetComponent<Slider>();
    }
    private void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
    }
    private void OnEnable()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
    }
    
}
