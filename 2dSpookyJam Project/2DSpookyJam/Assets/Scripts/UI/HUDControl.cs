using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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

    // Start is called before the first frame update
    void Start()
    {
        // sets this variable to an instance of the game manager
        gameManager = GameManager.Instance;

        // sets the value of oilTotal and oilMax to the value of oilMax from the game manager script
        reserveLevel = gameManager.OilLevel;
        reserveMax = gameManager.oilMax;

        lanternMax = gameManager.lantern.lanternOilLevelMax;
        lanternLevel = gameManager.lantern.lanternOilLevelCurrent;

        ReserveBarStartPos = reserveBar.transform.position;
        LanternBarStartPos = lanternBar.transform.position;


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

        // sets the oilLevel to the oilLevel in game manager, which decreases in value over time
        reserveLevel = gameManager.OilLevel;

        // the percentage of oil to be displayed in the bar
        float reserveProgressPercent = (reserveLevel / reserveMax) ;

        // updates the position and scale of the bar based on the amount of oil the player has
        reserveBar.transform.localPosition = new Vector3(-63f * (1-reserveProgressPercent), 0,0);

        //reserveBar.transform.localPosition = new Vector3(ReserveBarStartPos.x - (reserveProgressPercent / 2), , 0);
        reserveBar.transform.localScale = new Vector3(reserveProgressPercent, 1, 1);


        lanternLevel = gameManager.lantern.lanternOilLevelCurrent;

        float lanternProgPercent = (lanternLevel / lanternMax);

        lanternBar.transform.localPosition = new Vector3(-63f * (1-lanternProgPercent), 0, 0);
        lanternBar.transform.localScale = new Vector3(lanternProgPercent, 1, 1);

        UILitNumber.text = string.Concat(gameManager.numLitLamps.ToString(), UITotNumber);
        //UILitNumber.text.Append<string>(UITotNumber);

    }
}
