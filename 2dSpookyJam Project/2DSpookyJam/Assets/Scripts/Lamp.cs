using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Lamp : MonoBehaviour, IInteractable
{
    public bool isLit = false;
    public Light2D lampLight;
    [SerializeField] private Ghost ghost;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    private GameManager gameManager;

    private float intensity;

    //[SerializeField] private 


    void OnEnable()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        intensity = lampLight.intensity;
        lampLight.intensity = 0;

        gameManager = GameManager.Instance;
        gameManager.numLamps++;

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
        lampLight.intensity = intensity;

        animator.SetBool("isLit", true);

        ghost.SetLampLit();
        gameManager.LightLamp();
    }
}
