using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[Tooltip("a custom class, subclass of GameManager, used to circumvent a bug in Unity WebGL builds when pausing audio sources")]
public class AudioPauser
{
    public AudioSource source;
    [Tooltip("the time stored in the AudioStruct before the game paused")]
    public float prevTime;
    public bool isPaused;
    public bool showMapPrompt;
    //volume?

    public AudioPauser(AudioSource source)
    {
        this.source = source;
        prevTime = this.source.time;
        isPaused = false;
    }


    public void PauseSound(out AudioSource outSource)
    {
        if (source.isPlaying)
        {
            source.Pause();
            prevTime = source.time;
            isPaused =true;

        }
        outSource = source;
    }

    public void ResumeSound(out AudioSource outSource)
    {
        if (isPaused){
            source.time = prevTime;
            source.UnPause();
            isPaused = false;
        }

        outSource = source;
    }

}
public class GameManager : MonoBehaviour
{

    public static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private static ScoreKeeper _scoreKeeper;

    public bool paused;

    public UnityEvent onGamePause;
    public UnityEvent onGameResume;

    public float gameTime;
    [SerializeField] AudioSource backGroundMusic;
    private AudioPauser musicPauser;

    [Header("lamps")]
    public int numLamps;
    public int numLitLamps;

    public bool showInteractPrompt;
    public bool showRefillPrompt;

    [Header("Oil")]
    [SerializeField] public float OilLevel;
    [SerializeField] private int lampLightCost;
    [SerializeField] private int OilIncrement;
    [SerializeField] public int oilMax;
    [SerializeField] private int oilStartingLevel;
    [SerializeField] private float OilLossOverTime;
    public float refillCost;


    public HUDControl hudController;
    public PlayerController playerController;
    public Lantern lantern;

    public const string LOSESCENE = "LoseScene";
    public const string WINSCENE = "WinScene";
    public const bool GHOST = false;
    public const bool LANTERN = true;


    private void Awake()
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

    }


    // Start is called before the first frame update
    void Start()
    {
        OilLevel = oilStartingLevel;
        musicPauser = new AudioPauser(backGroundMusic);
    }

    // Update is called once per frame
    void Update()
    {
        gameTime = Time.timeSinceLevelLoad;
        if(gameTime >= (60 * 12))
        {
            SwitchToScene(LOSESCENE, ScoreKeeper.LossReason.Timeout);
        } 
    }

    public void LightLamp()
    {
        numLitLamps++;
        //lantern.lanternOilLevelCurrent += lampLightCost;

        //MathF.Max(lantern.lanternOilLevelCurrent, 0);
        //MathF.Min(lantern.lanternOilLevelCurrent, lantern.lanternOilLevelMax);

        lantern.lanternOilLevelCurrent =Mathf.Min(lantern.lanternOilLevelCurrent+  lampLightCost, lantern.lanternOilLevelMax);
        
        if(numLitLamps == numLamps)
        {
            SwitchToScene(WINSCENE);
        }

    }

    public void RefillLantern()
    {
        if(OilLevel > 0)
        {
            OilLevel = Mathf.Max(OilLevel- refillCost, 0);
        }
    }

    public void PauseGame()
    {

        paused = true;
        Time.timeScale = 0f;
        //backGroundMusic.Pause();
        musicPauser.PauseSound(out backGroundMusic);
        onGamePause.Invoke();
    }

    public void ResumeGame()
    {
        paused = false;
        Time.timeScale = 1f;
        //backGroundMusic.UnPause();
        musicPauser.ResumeSound(out backGroundMusic);
        onGameResume.Invoke();
    }

    public void SwitchToScene(string sceneName)
    {
       
        SwitchToScene(sceneName, ScoreKeeper.LossReason.Win);
    }

    //ghost is false, lantern is true
    public void SwitchToScene(string sceneName, ScoreKeeper.LossReason reason){

        _scoreKeeper = ScoreKeeper.Instance;

        _scoreKeeper.score = numLitLamps;
        _scoreKeeper.loadEnding(numLitLamps, reason, numLamps);

        SceneManager.LoadScene(sceneName);
    }

}
