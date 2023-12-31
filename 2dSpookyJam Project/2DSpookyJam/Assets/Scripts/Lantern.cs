using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Lantern : MonoBehaviour
{
    [SerializeField] private Light2D lampLight;

    [SerializeField] private AnimationCurve intensityCurve;

    [SerializeField] private float intensityMax;

    [SerializeField] private float outerMax;
    [SerializeField] private float outerMin;
    [SerializeField] private float outerRange;
    
    [SerializeField] public float lanternOilLevelCurrent;
    [SerializeField] public float lanternOilLevelMax;
    [SerializeField] private float lanternOilDecreasePerSecond;
    [Tooltip("how much oil is added to the lantern when refilled")]
    [SerializeField] private float lanternRefillAmount;

    [SerializeField] private int promptThreshold;

    private GameManager gameManager;

    public float LightRange { get { return lampLight.pointLightOuterRadius; } }

    // Start is called before the first frame update
    void Start()
    {
        intensityMax = lampLight.intensity;
        lanternOilLevelCurrent = lanternOilLevelMax;
        outerRange = outerMax - outerMin;
        gameManager = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        //light.intensity = (lanternOilLevelCurrent/ lanternOilLevelMax) * intensityMax;
        lampLight.intensity  = intensityCurve.Evaluate(lanternOilLevelCurrent / lanternOilLevelMax) * intensityMax;

        //light.pointLightOuterRadius= ((lanternOilLevelCurrent / lanternOilLevelMax) * outerRange ) + outerMin;
        lampLight.pointLightOuterRadius = (intensityCurve.Evaluate(lanternOilLevelCurrent / lanternOilLevelMax)  *outerRange) +outerMin;

        lanternOilLevelCurrent -= lanternOilDecreasePerSecond * Time.deltaTime;

            //Mathf.Max( lanternOilLevelCurrent-  lanternOilDecreasePerSecond * Time.deltaTime ,  0); 
        if(lanternOilLevelCurrent <= 0)
        {
            lanternOilLevelCurrent = 0;
            if (GameManager.Instance.OilLevel <=0)
            {
                GameManager.Instance.SwitchToScene(GameManager.LOSESCENE, ScoreKeeper.LossReason.Lantern);
            }
        }

        gameManager.showRefillPrompt = (lanternOilLevelCurrent < promptThreshold);

    }

    public void Refill(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if(GameManager.Instance.OilLevel > 0)
            {
                lanternOilLevelCurrent = Mathf.Min(lanternOilLevelMax, lanternOilLevelCurrent + lanternRefillAmount);
                GameManager.Instance.RefillLantern();
            }
        }
    }

    public void FillFromOverflow( float overflow)
    {
        float oilRatio =  (lanternRefillAmount / gameManager.refillCost) / 1.5f ;
        //float oilRatio = gameManager.refillCost / lanternRefillAmount;
        float oilIncrease = oilRatio * overflow;
        //Debug.Log("overflow amount: " + overflow + " amount of oil added to lantern: " + oilIncrease);
        lanternOilLevelCurrent = Mathf.Min(lanternOilLevelMax, lanternOilLevelCurrent + oilIncrease);
    }
}
