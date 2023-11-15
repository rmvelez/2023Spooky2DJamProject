using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public struct AudioStruct
{
    public AudioSource source;
    [Tooltip("the time stored in the AudioStruct before the game paused")]
    public float prevTime;
    public bool isPaused;
    public AudioClip clip;
    //volume?

    public AudioStruct(AudioSource source, AudioClip clip)
    {
        this.source = source;
        prevTime = this.source.time;
        isPaused = false;
        this.clip = clip;
    }

    public void SetClip(AudioClip clip)
    {
        this.clip = clip;
    }

    public void Pause(out AudioSource outSource)
    {
        source.Pause();
        prevTime = this.source.time;
        isPaused =true;
        outSource = source;
    }

    public void Resume(out AudioSource outSource)
    {
        source.time = prevTime;
        source.UnPause();
        isPaused = false;
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
    private AudioStruct musicStruct;

    [Header("lamps")]
    public int numLamps;
    public int numLitLamps;

    public bool showInterractPrompt;
    public bool showRefillPrompt;

    [Header("Oil")]
    [SerializeField] public float OilLevel;
    [SerializeField] private int lampLightCost;
    [SerializeField] private int OilIncrement;
    [SerializeField] public int oilMax;
    [SerializeField] private int oilStartingLevel;
    [SerializeField] private float OilLossOverTime;
    public float refillCost;



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
        musicStruct = new AudioStruct(backGroundMusic, backGroundMusic.clip);
    }

    // Update is called once per frame
    void Update()
    {
        gameTime = Time.timeSinceLevelLoad;
        if(gameTime >= (60 * 10))
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
        musicStruct.Pause(out backGroundMusic);
        onGamePause.Invoke();
    }

    public void ResumeGame()
    {
        paused = false;
        Time.timeScale = 1f;
        //backGroundMusic.UnPause();
        musicStruct.Resume(out backGroundMusic);
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
