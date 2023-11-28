using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Lamp : MonoBehaviour, IInteractable
{
    public bool isLit = false;
    public Light2D lampLight;
    [SerializeField] Light2D mapLight; 
    [SerializeField] private Ghost ghost;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [SerializeField] private new Collider2D collider;

    private GameManager gameManager;

    private float mainLightIntensity;
    private float mapLightIntensity;

    private AudioSource lampSound;

    //[SerializeField] private 


    void OnEnable()
    {

    }

    // Start is called before the first frame update
    void Start()
    {

        lampLight.enabled = false;
        mapLight.enabled = false;

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

            lampLight.enabled = true;
            mapLight.enabled = true;

            animator.SetBool("isLit", true);

            gameManager.LightLamp();
            lampSound.Play();
            collider.gameObject.SetActive(true);
        }        
    }
}
