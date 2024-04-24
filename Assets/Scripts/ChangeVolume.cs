using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeVolume : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    public void Volume()
    {
        AudioListener.volume = volumeSlider.value;
    }
    private void OnEnable()
    {
        volumeSlider.value = AudioListener.volume;
    }
}
