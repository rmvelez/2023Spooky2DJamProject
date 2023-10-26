using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private static ScoreKeeper _scoreKeeper;

    public bool paused;

    public UnityEvent onGamePause;
    public UnityEvent onGameResume;

    [Header("lamps")]
    public int numLamps;
    public int numLitLamps;



    [Header("Oil")]
    [SerializeField] public float OilLevel;
    [SerializeField] private int lampLightCost;
    [SerializeField] private int OilIncrement;
    [SerializeField] public int oilMax;
    [SerializeField] private int oilStartingLevel;
    [SerializeField] private float OilLossOverTime;
    [SerializeField] private float refillCost;

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
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LightLamp()
    {
        numLitLamps++;
        OilLevel -= lampLightCost;
        if(numLitLamps == numLamps)
        {
            SwitchToScene(WINSCENE);
        }

    }

    public void RefillLantern()
    {
        OilLevel = Mathf.Max(OilLevel- refillCost, 0);
    }

    public void PauseGame()
    {

        paused = true;
        Time.timeScale = 0f;
        onGamePause.Invoke();
    }

    public void ResumeGame()
    {
        paused = false;
        Time.timeScale = 1f;
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
