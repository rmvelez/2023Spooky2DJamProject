using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsManager : MonoBehaviour
{



    public static OptionsManager _instance;
    public static OptionsManager Instance { get { return _instance; } }

    [SerializeField] AudioMixerGroup musicMixerGroup;
    [SerializeField] AudioMixerGroup sfxMixerGroup;

    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    private SceneManager sceneManager;

    public float musicVolume;
    public float sfxVolume;

    private void OnEnable()
    {
        Debug.Log("asfx " + PlayerPrefs.GetFloat("sfxVolume"));
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    // Start is called before the first frame update
    void Start()
    {

        //SINGLETON PATTERN - ensures that there only ever exists a single gamemanager

        //is this the first time we've created this singleton
        if (_instance == null)
        {
            //we're the first gameManager, so assign ourselves to this instance
            _instance = this;

            // don't keep ourselves between levels
        }
        else
        {
            //if there's another one, then destroy this one
            Destroy(this.gameObject);
        }

        if (!PlayerPrefs.HasKey("musicVolume"))
        {
            Debug.Log("uwu1");
            PlayerPrefs.SetFloat("musicVolume", .75f);

        }

        if (!PlayerPrefs.HasKey("sfxVolume"))
        {
            Debug.Log("uwu2");
            PlayerPrefs.SetFloat("sfxVolume", .75f);
        }
        Load();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSliderValueChange(float value)
    {
        musicVolume = musicSlider.value;
        Debug.Log("omusic " + PlayerPrefs.GetFloat("musicVolume") + "value: " + musicSlider.value);

        musicMixerGroup.audioMixer.SetFloat("Music Volume", Mathf.Log10(musicSlider.value) * 20 );


        sfxVolume = sfxSlider.value;
        sfxMixerGroup.audioMixer.SetFloat("Sound Effects Volume", Mathf.Log10(sfxSlider.value) * 20);
        Save();
    }


    public void OnSfxSliderValueChange(float value)
    {
        Debug.Log("osfx " + PlayerPrefs.GetFloat("sfxVolume") + "value: " + value);
        sfxVolume = value;

        sfxMixerGroup.audioMixer.SetFloat("Sound Effects Volume", Mathf.Log10(value) * 20);
        Debug.Log("osfx " + PlayerPrefs.GetFloat("sfxVolume"));
        Save();
    }

    public void OnDestroy()
    {
        Save();
    }

    public void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("on scene loaded call --  playerpref's sfxvol: " + PlayerPrefs.GetFloat("sfxVolume")  + " inscript sfxVol variable: "+ sfxVolume);
        Load();
        //Save();
    }

    public void Load()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");

        musicVolume = PlayerPrefs.GetFloat("musicVolume");
        sfxVolume = PlayerPrefs.GetFloat("sfxVolume");

        Debug.Log("lmusic " + PlayerPrefs.GetFloat("musicVolume"));
        Debug.Log("lsfx " + PlayerPrefs.GetFloat("sfxVolume"));
    }

    private void Save()
    {
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);

        Debug.Log("called from save function -  music val in playerprefs" + PlayerPrefs.GetFloat("musicVolume"));
        Debug.Log("called from save function - sfx val in playerprefs " + PlayerPrefs.GetFloat("sfxVolume"));
    }
}
