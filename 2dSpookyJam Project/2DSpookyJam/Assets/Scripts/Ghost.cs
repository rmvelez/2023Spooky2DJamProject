using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    private GameManager gameManager;

    private enum GhostState { idle, curious, hostile, wary, fleeing} //consider refactoring idle to patrol
    [SerializeField] private GhostState ghostState;

    [SerializeField] private BoxCollider2D boxCollider;

    //[SerializeField] private Rigidbody2D rigidBody;

    [SerializeField] AudioSource loopSource;
    [SerializeField] AudioClip hostileLoop;
    [SerializeField] AudioClip curiousLoop;

    [SerializeField] private Vector2 target;
    [Tooltip("theoretically set by referencing GameManager.playerController")]
    [SerializeField] private GameObject player;
    [SerializeField] private Lamp lamp;
    private Vector3 lastSeenPlayerPos;

    [SerializeField] private float innerRange = 4;
    [SerializeField] private float outerRange = 7;

    [SerializeField] private float fastSpeed = 1;
    [SerializeField] private float slowSpeed = 0.5f;
    private float currentSpeed ;
    
    [SerializeField] private float patrolRange = 3; // the maximum distance the target can be from the ghost (if lamp is lit) or the lamp (if not lit)

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("how far from the light's outer range the ghost can enter - as a percent of the range")]
    [SerializeField] private float lightRange;

    private const int SIDE_DIRECTION = 1;
    private const int UP_DIRECTION = 2;
    private const int DOWN_DIRECTION = 3;
    //private const int IDLE_LEFT_DIRECTION = 0;
    //private const int IDLE_RIGHT_DIRECTION = 0;
    
    //private const int AGGRO_SIDE_DIRECTION = 4;
    //private const int AGGRO_UP_DIRECTION = 5;
    //private const int AGGRO_DOWN_DIRECTION = 6;
    
    private Vector3 patrolCentre;

    [SerializeField] private AudioSource stingerSound;

    //= lampLit ? lastSeenPlayerPos.position : lamp.transform.position;

    private int state;
    [SerializeField] private bool lampLit = false;

    private float maxLoopVol;

    [SerializeField] float loopVolInc;
    private bool isCoroutineRunning;
    IEnumerator increaseVolCoroutine;


    // Start is called before the first frame update
    void Start()
    {
        //animator.SetFloat("CameraMovement", 1);

        gameManager = GameManager.Instance;
        //rigidBody = GetComponent<Rigidbody2D>();

        currentSpeed = slowSpeed;

        player = gameManager.playerController.gameObject;


        target = lamp.gameObject.transform.position;


        patrolCentre = lamp.lampLight.transform.position;

        maxLoopVol = loopSource.volume;

        increaseVolCoroutine = IncreaseVolume();
        isCoroutineRunning = false;
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

        animator.SetBool("aggroSprite", ghostState == GhostState.hostile || ghostState ==GhostState.fleeing); //activate aggro sprites if the ghost is in either the aggro or fleeing states


        float step = currentSpeed * Time.deltaTime * 100;

        Vector2 moveTo = Vector2.MoveTowards(transform.position, target, step);
        Vector2 direction = moveTo - (Vector2) transform.position;


        
        if (lampLit)//the ghost only runs from the light if the lamp is lit
        {
            if (CheckIfPointIsInLight(moveTo, out Vector2 MoveToCPOL)) //if we're about to move into the circle
            {
                if(ghostState == GhostState.hostile || ghostState == GhostState.curious) //if we're chasing the player
                {
                    if (CheckIfPointIsInLight(target, out Vector2 targetCPOL))
                    { //if we're going toa point in the light... 
                        //the point is impossible to reach, get out of the light and find a new target, the closest point to us that's outside of the light
                        ghostState = GhostState.fleeing;
                        patrolCentre = targetCPOL;
                        target = targetCPOL;
                        if (CheckIfPointIsInLight(transform.position, out Vector2 posCPOL))
                        {
                            target = posCPOL;
                            //if we're currently in the light, walk to the edge, rather than teleporting straight there
                        }
                        else
                        {
                            //otherwise, go to the closest point of the target
                            target = targetCPOL;
                        }

                    }
                    else
                    {//if the target isn't in the light, then just go around the light to get to them
                        if(CheckIfPointIsInLight(transform.position, out Vector2 posCPOL))
                        {
                            target = posCPOL;
                            //if we're currently in the light, walk to the edge, rather than teleporting straight there
                        } else
                        {

                            moveTo = MoveToCPOL;
                        }
                    }
                } 
            }

        }



        if (moveTo.magnitude >= .01)
        {
            this.transform.position = moveTo;



            #region animation var setting
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

            #endregion animation var setting


            //Vector2 ignore = new Vector2();
            //if(CheckIfPointIsInLight(transform.position, out ignore))
            //{
            //    Debug.Log(transform.parent.gameObject.name +  " ghost is in light " + ignore.ToString());
            //}
        }
        else //if we're not moving
        {
            if (ghostState == GhostState.wary)
            {
                ghostState = GhostState.idle; //could theoretically do this by checking if we've reached our destination?
            }
        }
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ghost gotcha");
            //gameManager.SwitchToScene(GameManager.LOSESCENE, ScoreKeeper.LossReason.Ghost);

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
        bool playerIsInLight = CheckIfPointIsInLight(player.transform.position, out _);
        switch (ghostState)
        {
            case GhostState.idle: //if we're in idle state

                if (distanceToPlayer <= outerRange && !(playerIsInLight && lampLit)) //player goes from being idle to entering outer circle
                {
                    ghostState = GhostState.curious;
                    target = player.transform.position;
                    currentSpeed = slowSpeed;

                    break;
                }  else if (Vector2.Distance(transform.position, target) < .1) //if we've reached our destination
                {
                    Patrol();
                } else if (loopSource.isPlaying)
                {
                    StopVolumeCoroutine();
                    loopSource.Stop();
                }


                /*
                //else if (distanceToPlayer <= innerRange) //inside (entering) the inner circle
                //{ //and we enter the inner circle
                //    ghostState = GhostState.hostile;
                //    currentSpeed = fastSpeed;

                //    target = player.transform.position;
                //    break;
                //}
                // ^ commented out the above code as it is redundant - if distance > outer range it will certainly be > inner range!
                */

                break;
            case GhostState.curious: //if we're in curious state (state 1)
                if (distanceToPlayer > outerRange) //player exits outer range 
                {
                    //Debug.Log("this should only happen once when player exits outer range");
                    ghostState = GhostState.idle;
                    StopVolumeCoroutine();
                    loopSource.Stop();
                    currentSpeed = slowSpeed;
                    lastSeenPlayerPos = player.transform.position;
                    patrolCentre = lampLit ? player.transform.position : lamp.transform.position;
                    target = lastSeenPlayerPos + (transform.position - lastSeenPlayerPos).normalized * innerRange;
                }
                else if (distanceToPlayer <= innerRange && !(playerIsInLight && lampLit)) //player enters inner range
                {
                    ghostState = GhostState.hostile; //curious to hostile upon entering inner range
                    target = player.transform.position;
                    currentSpeed = fastSpeed;
                    stingerSound.Play();
                    loopSource.clip = hostileLoop;
                    loopSource.Play();
                    StartVolumeCoroutine();

                    Debug.Log("stinger started playing");

                    //GetComponent<SpriteRenderer>().color = Color.green;

                    break;
                }
                else
                {
                    target = player.transform.position;

                    if (!loopSource.isPlaying || loopSource.clip != curiousLoop) //if it's not playing, or it's not set to the correct loop
                    {
                        loopSource.clip = curiousLoop;
                        StopVolumeCoroutine();
                        loopSource.volume = maxLoopVol;
                        loopSource.Play();
                    } 
                }   

                break;
            case GhostState.hostile:
                if (distanceToPlayer > outerRange) //player exits outer range 
                {
                    ghostState = GhostState.wary; //hostile to wary upon exiting outer range
                    lastSeenPlayerPos = player.transform.position;
                    currentSpeed = slowSpeed;

                    patrolCentre = lampLit? player.transform.position : lamp.transform.position;
                    target = lastSeenPlayerPos;
                    loopSource.Stop();
                    StopVolumeCoroutine();

                    //GetComponent<SpriteRenderer>().color = Color.white;

                    break;
                }

                //if (!stingerSound.isPlaying) //wait until stingerSound stops playing
                //{
                //    Debug.Log("stinger stopped playing");

                //    if (!loopSource.isPlaying || loopSource.clip != hostileLoop) //if it's not playing, or it's not set to the correct loop
                //    {
                //        loopSource.clip = hostileLoop;
                //        loopSource.Play();
                //    }
                //} else
                //{
                //    Debug.Log("stinger continued playing");

                //}
                target = player.transform.position;
                break;
            case GhostState.wary: //in wary state (state 3)
                if (distanceToPlayer <= outerRange && !(playerIsInLight && lampLit)) //entering outer range
                {
                    stingerSound.Play();
                    StartVolumeCoroutine();
                    loopSource.clip = hostileLoop;
                    loopSource.Play();
                    ghostState = GhostState.hostile; //wary to hostile upon entering outer range 

                    currentSpeed = fastSpeed;   
                    target = player.transform.position;

                    //GetComponent<SpriteRenderer>().color = Color.green;

                    break;
                }
                if(Vector2.Distance(transform.position, target) < .1)
                {
                    ghostState = GhostState.idle;
                    currentSpeed = slowSpeed;
                    Patrol();
                    break;
                }

                //if (!loopSource.isPlaying || loopSource.clip != curiousLoop) //if it's not playing, or it's not set to the correct loop
                //{
                //    loopSource.clip = curiousLoop;
                //    loopSource.Play();
                //}

                break;
            case GhostState.fleeing:
                if((Vector2) transform.position == (Vector2)target)
                {
                    patrolCentre = lampLit ? transform.position : lamp.transform.position;
                    ghostState = GhostState.idle;
                    currentSpeed = slowSpeed;
                }

                if (loopSource.isPlaying)
                {
                    StopVolumeCoroutine();
                    loopSource.Stop();
                }

                break;
        }
    }

    private void StartVolumeCoroutine()
    {
        if(!isCoroutineRunning)
        {
            loopSource.volume = 0;
            increaseVolCoroutine = IncreaseVolume();
            StartCoroutine(increaseVolCoroutine);
        }
    }

    private void StopVolumeCoroutine()
    {
        if(increaseVolCoroutine != null)
        {
            loopSource.volume = maxLoopVol;
            StopCoroutine(increaseVolCoroutine);
            isCoroutineRunning = false;

        }
    }

    private IEnumerator IncreaseVolume()
    {
        isCoroutineRunning = true;
        while(loopSource.volume < maxLoopVol)
        {
            loopSource.volume = MathF.Min(loopSource.volume + loopVolInc, maxLoopVol);

            yield return new WaitForSeconds(.1f);
        }
        isCoroutineRunning = false;
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
        //Vector2 patrolCentre = lampLit ? lastSeenPlayerPos.position : lamp.transform.position;
        target = new Vector2(patrolCentre.x, patrolCentre.y) + patrolRange * new Vector2(Mathf.Cos(bearing), Mathf.Sin(bearing));


        if (lampLit)
        {

            CheckIfPointIsInLight(target, out target);
        }       
    }

    public void SetLampLit()
    {
        lampLit = true;

        if (stingerSound.isPlaying)
        {
            stingerSound.Stop();
        }

        // target = lamp.transform.position; //do we want to set this? I thought when the lamp gets lit the ghost would go into idle and start patrolling randomly
        //target = lamp.position;
    }

    [Tooltip("checks if the point is within the range of the light. returns closest point not within light as vec2 out var, or a zero vector if it's not within light")]
    private bool CheckIfPointIsInLight(Vector2 point, out Vector2 closestPointOutsideLight)
    {

        //most likely issues are with local to global point translation, or just plain linear algebra

        Vector2 distToLight = point - (Vector2)lamp.lampLight.transform.position;
        //if the distance to the light is less than how far it should be from the lamp (outer radius times lightrange). everything is squared cause it's faster
        //
        if (distToLight.sqrMagnitude < MathF.Pow(lamp.lampLight.pointLightOuterRadius * lightRange, 2))
        { //sqr magnitude is faster than magnitude because it avoids root operations, just make sure to square everything
            //Debug.Log("point is within light");

            closestPointOutsideLight = (distToLight.normalized * (lightRange * lamp.lampLight.pointLightOuterRadius) ) + (Vector2) lamp.lampLight.transform.position;

            return true;
        }
        else
        {
            //Debug.Log("point is NOT within light");

            closestPointOutsideLight = point;

            return false;
        }
    }

}
