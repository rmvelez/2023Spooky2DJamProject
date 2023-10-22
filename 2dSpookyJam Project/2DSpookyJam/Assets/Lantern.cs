using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Lantern : MonoBehaviour
{
    [SerializeField] private Light2D light;       
    [SerializeField] private float intensityMax;
    [SerializeField] private float intensityMin = 0;

    [SerializeField] private float outerMax;
    [SerializeField] private float outerMin;
    private float outerRange;
    
    [SerializeField] private float lanternOilLevelCurrent;
    [SerializeField] private float lanternOilLevelMax;
    [SerializeField] private float lanternOilDecreasePerSecond;
    [Tooltip("how much oil is added to the lantern when refilled")]
    [SerializeField] private float lanternRefillAmount;



    // Start is called before the first frame update
    void Start()
    {
        intensityMax = light.intensity;
        lanternOilLevelCurrent = lanternOilLevelMax;
        outerRange = outerMax - outerMin;
    }

    // Update is called once per frame
    void Update()
    {
        light.intensity = (lanternOilLevelCurrent/ lanternOilLevelMax) * intensityMax;
        light.pointLightOuterRadius= ((lanternOilLevelCurrent / lanternOilLevelMax) * outerRange ) + outerMin;
        lanternOilLevelCurrent = Mathf.Max( lanternOilLevelCurrent-  lanternOilDecreasePerSecond * Time.deltaTime ,  0); 
        
    }

    public void Refill(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            lanternOilLevelCurrent = Mathf.Min(lanternOilLevelMax, lanternOilLevelCurrent + lanternRefillAmount);
            GameManager.Instance.RefillLantern();

        }
    }
}
