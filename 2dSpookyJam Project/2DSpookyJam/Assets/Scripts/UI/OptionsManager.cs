using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsManager : MonoBehaviour
{
    [SerializeField] AudioMixerGroup musicMixerGroup;
    [SerializeField] AudioMixerGroup sfxMixerGroup;

    public float musicVolume;
    public float sfxVolume;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMusicSliderValueChange(float value)
    {
        musicVolume = value;

        musicMixerGroup.audioMixer.SetFloat("Music Volume", Mathf.Log10(value) * 20 );
    }
    public void OnSfxSliderValueChange(float value)
    {
        sfxVolume = value;

        sfxMixerGroup.audioMixer.SetFloat("Sound Effects Volume", Mathf.Log10(value) * 20);
    }
}
