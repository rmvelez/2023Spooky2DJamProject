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

    [SerializeField] private Collider2D collider;

    private GameManager gameManager;

    private float intensity;

    private AudioSource lampSound;

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

        lampSound = this.gameObject.GetComponent<AudioSource>();

        collider.gameObject.SetActive(false);

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
        if (!isLit)
        {
            isLit = true;
            lampLight.intensity = intensity;

            animator.SetBool("isLit", true);

            gameManager.LightLamp();
            lampSound.Play();
            collider.gameObject.SetActive(true);
        }        
    }
}
