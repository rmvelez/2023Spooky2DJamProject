using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    private GameManager gameManager;

    private enum GhostState { idle, curious, hostile, wary} //consider refactoring idle to patrol
    [SerializeField] private GhostState ghostState;
    

    //[SerializeField] private Rigidbody2D rigidBody;

    [SerializeField] private Vector2 target;
    [Tooltip("theoretically set by referencing GameManager.playerController")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject lamp;
    private Transform lastSeenPlayerPos;

    [SerializeField] private float innerRange;
    [SerializeField] private float outerRange;

    [SerializeField] private float fastSpeed = 1;
    [SerializeField] private float slowSpeed = 0.5f;
    private float currentSpeed ;

    private int state;
    [SerializeField] private bool lampLit = false;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        //rigidBody = GetComponent<Rigidbody2D>();

        currentSpeed = slowSpeed;

        player = gameManager.playerController.gameObject;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        checkPlayerPos();
        if(ghostState == GhostState.idle)
        {
            //call patrol();
        }
        Vector2 moveTo = Vector2.MoveTowards(transform.position, target, currentSpeed);
        if(moveTo.magnitude >= .01)
        {
            this.transform.position = moveTo;
        }
    }
    

    //void FixedUpdate()
    //{
        /*
        rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, currentSpeed);



        if (lampLit) return;
        switch (state)
        {
            case 0: //idle
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, SLOW_SPEED);
                break;
            case 1: //curious
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, SLOW_SPEED);
                break;
            case 2: // hostile?
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, FAST_SPEED);
                break;
            case 3:  // wary?
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, FAST_SPEED);
                break;
        }*/

    //}


    private void checkPlayerPos()
    {
        float distanceToPlayer = Vector2.Distance(player.transform.position, this.transform.position);
        switch (ghostState)
        {
            case GhostState.idle: //if we're in idle state
                if (distanceToPlayer <= outerRange) //player goes from being idle to entering outer circle
                {
                    ghostState = GhostState.curious;
                    target = player.transform.position;
                    currentSpeed = slowSpeed;

                    break;
                }
                else if (distanceToPlayer <= innerRange)
                { //and we enter the inner circle
                    ghostState = GhostState.hostile;
                    currentSpeed = fastSpeed;

                    target = player.transform.position;
                    break;
                }

                break;
            case GhostState.curious: //if we're in curious state (state 1)
                if (distanceToPlayer >= outerRange) //player exits outer range 
                {
                    Debug.Log("this should only happen once when player exits outer range");
                    ghostState = GhostState.idle;
                    currentSpeed = slowSpeed;

                    target = this.transform.position;
                }
                else if (distanceToPlayer <= innerRange) //player enters inner range
                {
                    ghostState = GhostState.hostile; //curious to hostile upon entering inner range
                    target = player.transform.position;
                    currentSpeed = fastSpeed;
                    
                    break;
                }
                break;
            case GhostState.hostile:
                if (distanceToPlayer >= outerRange) //player exits outer range 
                {
                    ghostState = GhostState.wary; //hostile to wary upon exiting outer range
                    lastSeenPlayerPos = player.transform;
                    currentSpeed = slowSpeed;

                    target = lastSeenPlayerPos.position;
                    break;
                }
                break;
            case GhostState.wary: //in wary state (state 3)
                if (distanceToPlayer <= outerRange) //entering outer range
                {
                    ghostState = GhostState.hostile; //wary to hostile upon entering outer range 

                    currentSpeed = fastSpeed;   
                    target = player.transform.position;
                    break;
                }
                break;

        }
    }

    /*
    void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider.gameObject.CompareTag("player")){
            if(Vector2.Distance(player.transform.position, this.transform.position) <= innerRange) //player is in inner circle
            {
                switch (ghostState)
                {

                    case GhostState.idle:
                        //state = 2;
                        ghostState = GhostState.hostile;
                        target = player.transform.position;
                        //target = player.position;
                        break;
                    case GhostState.curious: //if curious
                        ghostState = GhostState.hostile;
                        target = player.transform.position;

                        //set to state
                        state = 2;
                        //target = player.position;
                        break;
                    case GhostState.wary: //if wary
                        ghostState = GhostState.hostile;
                            //other version of this statement sets it to curious

                        //set to hostile
                        state = 2;
                        //target = player.position;
                        break;

                            
                    //case (GhostState.wary):
                    //    ghostState = GhostState.curious;
                    //    target = player.transform.position;
                    //    break;
                    //    case (ghostState.)

                }
            }
        
        }
        //if (collider.gameObject.CompareTag("player outer circle"))
        //if(collider.gameObject.transform.position )
        if(true) //originally this was going to use vec2.distance() to probably check player pos, swapping to if(true) to avoid compile errors
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
        else if (collider.gameObject.CompareTag("player inner circle"))
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
        if (collider.gameObject.CompareTag("player outer circle"))
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
    */

    public void SetLampLit()
    {
        lampLit = true;
        //target = lamp.position;
    }
}
