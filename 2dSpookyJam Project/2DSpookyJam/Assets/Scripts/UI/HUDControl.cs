using System.Collections;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class HUDControl : MonoBehaviour
{

    //[SerializeField] private UIDocument UIDoc;
    [SerializeField] private TMP_Text UILitNumber;
    private string UITotNumber;
    //[SerializeField] private TextMeshPro UITotNumber;

    // reference to the sprite for the oil meter

    //public CanvasRenderer reserveBar;
    [SerializeField] private CanvasRenderer reserveBar;


    [SerializeField] private CanvasRenderer lanternBar;


    [SerializeField] private TMP_Text interactPrompt;
    [SerializeField] private TMP_Text refillPrompt;
    [SerializeField] private TMP_Text mapPrompt;
    [SerializeField] private RawImage map;
    [SerializeField] private MapCamera mapCamController;

    private OptionsManager optionsManager;

    // reference to the starting position of the oil meter
    public Vector3 ReserveBarStartPos;
    public Vector3 LanternBarStartPos;

    // the total amount of oil that the player currently has
    private float lanternLevel;
    private float lanternMax;

    // the amount of oil that the player starts with
    private float reserveLevel;
    private float reserveMax;

    // reference to the game manager
    private GameManager gameManager;
    private bool init = true;

    [Tooltip("resets when the player lights a new lamp or they check the map")]
    public float timeSinceMapPromptChecked;

    //prompts the player to use the map the first time they acquire enough lamps
    [Tooltip("used to keep track of whether we've prompted the player due to the initial passing of the threshold")]
    private bool hasShownMapPromptByThreshold;
    //prompts the player the first time a set amount of time passes without them either finding a lamp or using the map
    [Tooltip("(in seconds) the amount of time since the player has lit a lamp or used the map")]
    [SerializeField] private int timeToWaitBeforeShowingPrompt;
    [Tooltip("used to keep track of whether we've prompted the player due to them not finding the lamp within a certain amount of time")]
    private bool hasShownMapPromptByTime; //we probably only want to prompt them for this reason once, repetative prompts could get annoying

    // Start is called before the first frame update
    void Start()
    {
        optionsManager = OptionsManager.Instance;
        // sets this variable to an instance of the game manager
        gameManager = GameManager.Instance;

        // sets the value of oilTotal and oilMax to the value of oilMax from the game manager script
        reserveLevel = gameManager.OilLevel;
        reserveMax = gameManager.oilMax;

        lanternMax = gameManager.lantern.lanternOilLevelMax;
        lanternLevel = gameManager.lantern.lanternOilLevelCurrent;

        ReserveBarStartPos = reserveBar.transform.position;
        LanternBarStartPos = lanternBar.transform.position;

        HideMap();
        mapPrompt.enabled = false;

        //UILitNumber = UIDoc.rootVisualElement.Q<Label>("litNumber");
        //UITotNumber = UIDoc.rootVisualElement.Q<Label>("totNumber");
        

    }

    // Update is called once per frame
    void Update()
    {
        if (init)
        {
            //UILitNumber.text= string.Concat("/", gameManager.numLamps.ToString());
            UITotNumber = string.Concat("/", gameManager.numLamps.ToString());
            init = false;
        }

        // -- RESERVE BAR CALCULATIONS --
        // sets the oilLevel to the oilLevel in game manager, which decreases in value over time
        reserveLevel = gameManager.OilLevel;
        // the percentage of oil to be displayed in the bar
        float reserveProgressPercent = (reserveLevel / reserveMax);
        // updates the position and scale of the bar based on the amount of oil the player has
        reserveBar.transform.localPosition = new Vector3(-63f * (1-reserveProgressPercent), 0,0);
        reserveBar.transform.localScale = new Vector3(reserveProgressPercent, 1, 1);
        // -- LANTERN BAR CALCULATIONS --
        lanternLevel = gameManager.lantern.lanternOilLevelCurrent;
        float lanternProgPercent = (lanternLevel / lanternMax);
        lanternBar.transform.localPosition = new Vector3(-63f * (1-lanternProgPercent), 0, 0);
        lanternBar.transform.localScale = new Vector3(lanternProgPercent, 1, 1);

        refillPrompt.enabled = (gameManager.showRefillPrompt && optionsManager.showToolTips);
        interactPrompt.enabled = (gameManager.showInteractPrompt && optionsManager.showToolTips);

        if (gameManager.canViewMap)
        {
            if (!hasShownMapPromptByThreshold)
            {
                hasShownMapPromptByThreshold = true;
                mapPrompt.enabled = true;
            }

            if(!hasShownMapPromptByTime && (timeSinceMapPromptChecked +timeToWaitBeforeShowingPrompt <= Time.time))
            {
                hasShownMapPromptByTime = true;
                mapPrompt.enabled = true;
            }
        }               
    }


    public void LampLit()
    {
        UILitNumber.text = string.Concat(gameManager.numLitLamps.ToString(), UITotNumber);

        if (gameManager.canViewMap && !hasShownMapPromptByTime)
        {
            timeSinceMapPromptChecked = Time.time;
        }
    }

    public void ShowMap()
    {
        mapCamController.SetPosition();
        map.gameObject.SetActive(true);
        mapPrompt.enabled = false;
        timeSinceMapPromptChecked = Time.time;
    }

    public void HideMap()
    {
        map.gameObject.SetActive(false);
    }

}
