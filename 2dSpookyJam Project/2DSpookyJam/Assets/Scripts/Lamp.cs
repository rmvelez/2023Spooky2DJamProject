using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Lamp : MonoBehaviour, IInteractable
{
    public bool isLit = false;
    [SerializeField] private GameObject lights;
    [SerializeField] private Ghost ghost;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    [SerializeField] private new Collider2D collider;

    private GameManager gameManager;


    private AudioSource lampSound;

    //[SerializeField] private 


    void OnEnable()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        lights.SetActive(false);

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

            lights.SetActive(true);

            animator.SetBool("isLit", true);

            gameManager.LightLamp();
            lampSound.Play();
            collider.gameObject.SetActive(true);
        }        
    }
}
