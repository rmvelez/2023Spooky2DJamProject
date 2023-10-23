using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilCan : MonoBehaviour , IInteractable
{
    [SerializeField] private float oilIncreaseLevel;

    //[SerializeField] private Collider2D collider;
    private GameManager gameManager;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
    }




    public void Interact()
    {
        Debug.Log("called");
        gameManager.OilLevel += oilIncreaseLevel;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
