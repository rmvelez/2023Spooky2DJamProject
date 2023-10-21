using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigidBody;

    [SerializeField] private Vector2 target;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject lamp;

    private const float SLOW_SPEED = 1;
    private const float FAST_SPEED = 0.5f;

    private int state;
    private bool lampLit = false;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (lampLit) return;
        switch (state)
        {
            case 0:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, SLOW_SPEED);
                break;
            case 1:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, SLOW_SPEED);
                break;
            case 2:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, FAST_SPEED);
                break;
            case 3:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, FAST_SPEED);
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "player outer circle")
        {
            switch (state)
            {
                case 0:
                    state = 1;
                    //target = player.position;
                    break;
                case 3:
                    state = 2;
                    //target = player.position;
                    break;
            }
        }
        else if (collider.gameObject.tag == "player inner circle")
        {
            switch (state)
            {
                case 0:
                    state = 2;
                    //target = player.position;
                    break;
                case 1:
                    ; state = 2;
                    //target = player.position;
                    break;
                case 3:
                    state = 2;
                    //target = player.position;
                    break;
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "player outer circle")
        {
            switch (state)
            {
                case 1:
                    state = 0;
                    //target = lamp.position;
                    break;
                case 3:
                    state = 2;
                    //target = player.position;
                    break;
            }
        }
    }

    public void SetLampLit()
    {
        lampLit = true;
        //target = lamp.position;
    }
}
