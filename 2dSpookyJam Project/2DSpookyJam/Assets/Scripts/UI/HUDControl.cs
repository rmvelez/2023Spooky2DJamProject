using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDControl : MonoBehaviour
{
    // reference to the sprite for the oil meter
    public SpriteRenderer barSprite;

    // reference to the starting position of the oil meter
    public Vector3 barStartPos;

    // the total amount of oil that the player currently has
    private float oilLevel;

    // the amount of oil that the player starts with
    private float oilMax;

    // reference to the game manager
    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        // sets this variable to an instance of the game manager
        gameManager = GameManager.Instance;

        // sets the value of oilTotal and oilMax to the value of oilMax from the game manager script
        oilLevel = oilMax = gameManager.oilMax;
    }

    // Update is called once per frame
    void Update()
    {
        // sets the oilLevel to the oilLevel in game manager, which decreases in value over time
        oilLevel = gameManager.OilLevel;

        // the percentage of oil to be displayed in the bar
        float progressPercent = (oilLevel / oilMax) * 9.25f;

        // updates the position and scale of the bar based on the amount of oil the player has
        barSprite.transform.localPosition = new Vector3(barStartPos.x - (progressPercent / 2), 4, 0);
        barSprite.transform.localScale = new Vector3(progressPercent, 1, 1);
    }
}
