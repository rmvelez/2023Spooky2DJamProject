using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Lamp : MonoBehaviour, IInteractable
{
    public bool isLit = false;
    [SerializeField] private Light2D light;
    [SerializeField] private Ghost ghost;

    private float intensity;

    //[SerializeField] private 

    // Start is called before the first frame update
    void Start()
    {
        intensity = light.intensity;
        light.intensity = 0;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Interact()
    {
        Light();
    }

    public void Light()
    {
        isLit = true;
        light.intensity = intensity;

        ghost.SetLampLit();
    }
}
