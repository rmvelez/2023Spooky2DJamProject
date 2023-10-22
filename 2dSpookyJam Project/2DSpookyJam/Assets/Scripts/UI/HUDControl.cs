using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDControl : MonoBehaviour
{
    // reference to the sprite for the oil meter

    public SpriteRenderer reserveBar;
    public SpriteRenderer lanternBar;


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

    }

    // Update is called once per frame
    void Update()
    {
        // sets the oilLevel to the oilLevel in game manager, which decreases in value over time
        reserveLevel = gameManager.OilLevel;

        // the percentage of oil to be displayed in the bar
        float reserveProgressPercent = (reserveLevel / reserveMax) ;

        // updates the position and scale of the bar based on the amount of oil the player has
        reserveBar.transform.localPosition = new Vector3( -2.15f * (1-reserveProgressPercent), 0,0);

        //reserveBar.transform.localPosition = new Vector3(ReserveBarStartPos.x - (reserveProgressPercent / 2), , 0);
        reserveBar.transform.localScale = new Vector3(reserveProgressPercent, 1, 1);


        lanternLevel = gameManager.lantern.lanternOilLevelCurrent;

        float lanternProgPercent = (lanternLevel / lanternMax);

        lanternBar.transform.localPosition = new Vector3(2.7f * (1-lanternProgPercent), 0, 0);
        lanternBar.transform.localScale = new Vector3(lanternProgPercent, 1, 1);

    }
}
