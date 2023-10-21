using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }
    public int numLitLamps;


    [Header("Oil")]
    [SerializeField] public float OilLevel;
    [SerializeField] private int lampLightCost;
    [SerializeField] private int OilIncrement;
    [SerializeField] private int oilMax;
    [SerializeField] private int oilStartingLevel;
    [SerializeField] private float OilLossOverTime;
    [SerializeField] private float refillCost;

    public PlayerController playerController;


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

    }

    public void RefillLantern()
    {
        OilLevel -= refillCost;
    }
}