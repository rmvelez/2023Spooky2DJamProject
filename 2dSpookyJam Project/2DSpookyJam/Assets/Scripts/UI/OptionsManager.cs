using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsManager : MonoBehaviour
{



    public static OptionsManager _instance;
    public static OptionsManager Instance { get { return _instance; } }

    [SerializeField] AudioMixerGroup musicMixerGroup;
    [SerializeField] AudioMixerGroup sfxMixerGroup;

    public float musicVolume;
    public float sfxVolume;

    // Start is called before the first frame update
    void Start()
    {

        //SINGLETON PATTERN - ensures that there only ever exists a single gamemanager

        //is this the first time we've created this singleton
        if (_instance == null)
        {
            //we're the first gameManager, so assign ourselves to this instance
            _instance = this;
            DontDestroyOnLoad(this);

            // don't keep ourselves between levels
        }
        else
        {
            //if there's another one, then destroy this one
            Destroy(this.gameObject);
        }

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
