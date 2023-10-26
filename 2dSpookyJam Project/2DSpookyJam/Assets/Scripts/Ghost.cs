using System;
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

    [SerializeField] private float innerRange = 4;
    [SerializeField] private float outerRange = 7;

    [SerializeField] private float fastSpeed = 1;
    [SerializeField] private float slowSpeed = 0.5f;
    private float currentSpeed ;
    
    [SerializeField] private float patrolRange = 3; // the maximum distance the target can be from the ghost (if lamp is lit) or the lamp (if not lit)

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;



    private const int SIDE_DIRECTION = 1;
    private const int UP_DIRECTION = 2;
    private const int DOWN_DIRECTION = 3;
    //private const int IDLE_LEFT_DIRECTION = 0;
    //private const int IDLE_RIGHT_DIRECTION = 0;
    
    //private const int AGGRO_SIDE_DIRECTION = 4;
    //private const int AGGRO_UP_DIRECTION = 5;
    //private const int AGGRO_DOWN_DIRECTION = 6;
    

    private enum States
    {
        Idle,
        Curious,
        Hostile,
        Wary
    }

    private int state;
    [SerializeField] private bool lampLit = false;


    // Start is called before the first frame update
    void Start()
    {
        animator.SetFloat("CameraMovement", 1);

        gameManager = GameManager.Instance;
        //rigidBody = GetComponent<Rigidbody2D>();

        currentSpeed = slowSpeed;

        player = gameManager.playerController.gameObject;

        gameManager.numLamps++;

        target = lamp.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CheckPlayerPos();
        //if(ghostState == GhostState.idle)
        //{
        //    Patrol();
        //    //call patrol();
        //}

        animator.SetBool("isAggro", ghostState == GhostState.hostile);


        Vector2 moveTo = Vector2.MoveTowards(transform.position, target, currentSpeed);
        Vector2 direction = moveTo - (Vector2) transform.position;

        if(moveTo.magnitude >= .01)
        {
            this.transform.position = moveTo;

            if( MathF.Abs( direction.x) > MathF.Abs(direction.y)) //if we're moving horizontally more than vertically
            {
                animator.SetInteger("movement", SIDE_DIRECTION);
                

            } else //we're moving vertically more than horizontally
            {
                if(direction.y > 0) //we're moving up
                {
                    animator.SetInteger("movement", UP_DIRECTION);
                } else // we're moving down - defaults to down when equal
                {
                    animator.SetInteger("movement", DOWN_DIRECTION);
                }
            }

            if (direction.x >= 0)
            {
                spriteRenderer.flipX = false; //when x is flipped, it's looking left
            }
            else
            {
                spriteRenderer.flipX = true;
            }



        } else
        {
            if (ghostState == GhostState.wary) ghostState = GhostState.idle;
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
            case (int)States.Idle:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, slowSpeed);
                break;
            case 1: //curious
            case (int)States.Curious:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, slowSpeed);
                break;
            case 2: // hostile?
            case (int)States.Hostile:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, fastSpeed);
                break;
            case 3:  // wary?
            case (int)States.Wary:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, slowSpeed);
                break;
        }*/

    //}


    private void CheckPlayerPos()
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
                }  else if (Vector2.Distance(transform.position, target) < .1) //if we've reached our destination
                {
                    Patrol();
                }
                //else if (distanceToPlayer <= innerRange) //inside (entering) the inner circle
                //{ //and we enter the inner circle
                //    ghostState = GhostState.hostile;
                //    currentSpeed = fastSpeed;

                //    target = player.transform.position;
                //    break;
                //}
                // ^ commented out the above code as it is redundant - if distance > outer range it will certainly be > inner range!
                break;
            case GhostState.curious: //if we're in curious state (state 1)
                if (distanceToPlayer > outerRange) //player exits outer range 
                {
                    //Debug.Log("this should only happen once when player exits outer range");
                    ghostState = GhostState.idle;
                    currentSpeed = slowSpeed;
                    lastSeenPlayerPos = player.transform;
                    target = lastSeenPlayerPos.position + (transform.position - lastSeenPlayerPos.position).normalized * innerRange;
                }
                else if (distanceToPlayer <= innerRange) //player enters inner range
                {
                    ghostState = GhostState.hostile; //curious to hostile upon entering inner range
                    target = player.transform.position;
                    currentSpeed = fastSpeed;

                    //GetComponent<SpriteRenderer>().color = Color.green;

                    break;
                }
                else target = player.transform.position;
                break;
            case GhostState.hostile:
                if (distanceToPlayer > outerRange) //player exits outer range 
                {
                    ghostState = GhostState.wary; //hostile to wary upon exiting outer range
                    lastSeenPlayerPos = player.transform;
                    currentSpeed = slowSpeed;

                    target = lastSeenPlayerPos.position;

                    //GetComponent<SpriteRenderer>().color = Color.white;

                    break;
                }
                target = player.transform.position;
                break;
            case GhostState.wary: //in wary state (state 3)
                if (distanceToPlayer <= outerRange) //entering outer range
                {
                    ghostState = GhostState.hostile; //wary to hostile upon entering outer range 

                    currentSpeed = fastSpeed;   
                    target = player.transform.position;

                    //GetComponent<SpriteRenderer>().color = Color.green;

                    break;
                }
                if(Vector2.Distance(transform.position, target) < .1)
                {
                    Debug.Log("ping");
                    ghostState = GhostState.idle;
                    currentSpeed = slowSpeed;
                    Patrol();
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
                case (int)States.Idle:
                    state = (int)States.Curious;
                    target = player.transform.position;
                    break;
                case (int)States.Wary:
                    state = (int)States.Hostile;
                    target = player.transform.position;
                    break;
            }
        }
        else if (collider.gameObject.CompareTag("player inner circle"))
        {

        //following lines were a merge conflict, following contains each copy of the code
        
        //start of original code (merge source B
        
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
            
            //meswif's code, brought from patrol branch (merge source C)
            state = (int)States.Hostile;
            target = player.transform.position;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.CompareTag("player outer circle"))
        {
            switch (state)
            {
                case (int)States.Curious:
                    state = (int)States.Idle;
                    target = lamp.transform.position;
                    break;
                case (int)States.Hostile:
                    state = (int)States.Wary;
                    target = player.transform.position;
                    break;
            }
        }
    }
    */

    //sets target to a new random position within range of the relevant point 
    private void Patrol()
    {
        //if (Vector2.Distance(rigidBody.position, target) >= 0.1f) return; // ALTERNATIVELY: if ((rigidBody.position - target).magnitude >= 0.1f)
        //if (Vector2.Distance(transform.position, target) >= 0.1f) return; // ALTERNATIVELY: if ((rigidBody.position - target).magnitude >= 0.1f)
        
        float bearing = UnityEngine.Random.Range(-Mathf.PI, Mathf.PI);
        Vector3 patrolCentre = lampLit ? lastSeenPlayerPos.position : lamp.transform.position;
        //Vector2 patrolCentre = lampLit ? lastSeenPlayerPos.position : lamp.transform.position;
        target = new Vector2(patrolCentre.x, patrolCentre.y) + patrolRange * new Vector2(Mathf.Cos(bearing), Mathf.Sin(bearing));

        // ----------------------------------------------------------------
        //                      COME BACK TO THIS:
        //                      -----------------
        //
        //                      lastSeenPlayerPos
        // ----------------------------------------------------------------
    }

    public void SetLampLit()
    {
        lampLit = true;
        Debug.Log("ghost received lamp lit signal");
        // target = lamp.transform.position; //do we want to set this? I thought when the lamp gets lit the ghost would go into idle and start patrolling randomly
        //target = lamp.position;
    }
}
