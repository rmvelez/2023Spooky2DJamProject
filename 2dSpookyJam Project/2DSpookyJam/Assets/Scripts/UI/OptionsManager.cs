using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using static UnityEngine.Rendering.DebugUI;

public class OptionsManager : MonoBehaviour
{



    public static OptionsManager _instance;
    public static OptionsManager Instance { get { return _instance; } }

    [SerializeField] AudioMixerGroup masterMixerGroup;
    [SerializeField] AudioMixerGroup musicMixerGroup;
    [SerializeField] AudioMixerGroup sfxMixerGroup;

    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    public Toggle showToolTipsToggle;
    public bool showToolTips;



    private SceneManager sceneManager;

    public float masterVolume;
    public float musicVolume;
    public float sfxVolume;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    // Start is called before the first frame update
    void Start()
    {

        //SINGLETON PATTERN - ensures that there only ever exists a single optionsManager

        //is this the first time we've created this singleton
        if (_instance == null)
        {
            //we're the first optionsManager, so assign ourselves to this instance
            _instance = this;

            // don't keep ourselves between levels
        }
        else
        {
            //if there's another one, then destroy this one
            Destroy(this.gameObject);
        }

        if (!PlayerPrefs.HasKey("masterVolume"))
        {
            Debug.Log("uwu1");
            PlayerPrefs.SetFloat("masterVolume", .75f);

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

        if (!PlayerPrefs.HasKey("showToolTips"))
        {
            PlayerPrefs.SetInt("showToolTips", BoolToInt(true));
        }


        Load();
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnIGTTTogleChange(bool value)
    {
        Debug.Log("toggle changed1 " + value + " " + showToolTips);

        showToolTips = value;
        Debug.Log("toggle changed2 " + value + " " + showToolTips);
        PlayerPrefs.SetInt("showToolTips", BoolToInt(value));

        //set in-class var
        //sset out of class var
        //set playerprefs
    }

    private int BoolToInt(bool b)
    {
        return b ? 1 : 0;
    }

    private bool IntToBool(int i)
    {
        if (i == 0)
        {
            return false;
        }
        else if (i==1)
        {
            return true;
        } else
        {
            throw new System.Exception("int to bool was passed a value other than 0 or 1 - value: " + i);
        }
    }

    public void OnMasterSliderValueChange(float value)
    {
        masterVolume = masterSlider.value;
        masterMixerGroup.audioMixer.SetFloat("Master Volume", Mathf.Log10(masterSlider.value) * 20);
        PlayerPrefs.SetFloat("masterVolume", masterVolume);


        //set in-class var
        //sset out of class var
        //set playerprefs
    }

    public void OnMusicSliderValueChange(float value)
    {
        musicVolume = musicSlider.value;
        musicMixerGroup.audioMixer.SetFloat("Music Volume", Mathf.Log10(musicSlider.value) * 20 );
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
    }


    public void OnSfxSliderValueChange(float value)
    {
        sfxVolume = value;

        sfxMixerGroup.audioMixer.SetFloat("Sound Effects Volume", Mathf.Log10(value) * 20);

        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
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
        //Load();
        //Save();
    }

    public void Load()
    {
        masterSlider.value = PlayerPrefs.GetFloat("masterVolume");

        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");
        showToolTipsToggle.isOn = IntToBool(PlayerPrefs.GetInt("showToolTips"));

        masterVolume = PlayerPrefs.GetFloat("masterVolume");
        musicVolume = PlayerPrefs.GetFloat("musicVolume");
        sfxVolume = PlayerPrefs.GetFloat("sfxVolume");
        showToolTips = IntToBool(PlayerPrefs.GetInt("showToolTips"));

        masterMixerGroup.audioMixer.SetFloat("Master Volume", Mathf.Log10(masterSlider.value) * 20);
        musicMixerGroup.audioMixer.SetFloat("Music Volume", Mathf.Log10(musicSlider.value) * 20);
        sfxMixerGroup.audioMixer.SetFloat("Sound Effects Volume", Mathf.Log10(sfxSlider.value) * 20);
        //if we were storing tooltips elsewhere, then we'd set that here as well

    }

    private void Save()
    {
        PlayerPrefs.SetFloat("masterVolume", masterVolume);
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
        PlayerPrefs.SetInt("showToolTips", BoolToInt(showToolTips));


    }
}
