using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.SocialPlatforms.GameCenter;
using static UnityEngine.InputSystem.Controls.AxisControl;

public class Ghost : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private float scaleamount;

    private enum GhostState { idle, curious, hostile, wary, fleeing} //consider refactoring idle to patrol
    [SerializeField] private GhostState ghostState;

    [SerializeField] private BoxCollider2D boxCollider;
    private CircleCollider2D lampCollider;

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

    private float maxLoopVol;

    [SerializeField] float loopVolInc;
    private bool isCoroutineRunning;
    IEnumerator increaseVolCoroutine;


    private bool loopIsPaused = false;
    private bool stingerIsPaused = false;

    private bool newPointMightBeInRangeOfLight;

    private Vector2 moveTo;

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

        gameManager.onGamePause.AddListener(PauseSounds);
        gameManager.onGameResume.AddListener(UnPauseSounds);
        
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

        animator.SetBool("aggroSprite", ghostState == GhostState.hostile || ghostState == GhostState.fleeing); //activate aggro sprites if the ghost is in either the aggro or fleeing states


        float step = currentSpeed * Time.deltaTime * 100;

        moveTo = Vector2.MoveTowards(transform.position, target, step);
        Vector2 direction = moveTo - (Vector2) transform.position;

        if(ghostState != GhostState.idle)
        {
            //newPointMightBeInRangeOfLight = false;
        }

        //move this to ontriggerstay{ comparetag("light")}
        if (lampCollider != null )//the ghost only runs from the light if the lamp is lit
        {

            
            if (CheckIfPointIsInLight(lampCollider, transform.position, out Vector2 closestPoint))//if we're in the circle
            {
                //this check is just to avoid calling CheckIfPointIsInLight() repeatedly when we're patrolling to a point on the other side of the circle 
                if (ghostState != GhostState.idle || newPointMightBeInRangeOfLight)
                {
                    newPointMightBeInRangeOfLight = false;
                    if (CheckIfPointIsInLight(lampCollider, target, out Vector2 closestPointToTarget))
                    {
                        if (ghostState != GhostState.idle) //if we're chasing the player
                        {//get out of the circle
                         //note - since we should only be chasing the player if they're not in the light, then this should only happen when the player activates the
                         //light while we're in the circle 
                            ghostState = GhostState.fleeing;
                            target = closestPoint;
                            patrolCentre = closestPoint; //patrol around where we exit the circle
                        }
                        else //if we're not chasing the player, we must be patrolling to a point that is inside the circle 
                        {//in which case we want to change our target to be on the edge of the circle 
                            target = closestPointToTarget;
                            moveTo = closestPoint;//move around the circle
                        }
                    } else
                    {
                        Debug.LogWarning("CheckIfPointInLight was called unnecessarily");
                    }

                } else //if we're patrolling to a point that's  known to not be in the circle
                { //then the point must be on the other side, so simply move around the circle 
                    moveTo = closestPoint;
                }         
                   

                //if ((ghostState == GhostState.hostile || ghostState == GhostState.curious) //if we're chasing the player
                //    && CheckIfPointIsInLight(lampCollider, player.transform.position, out Vector2 _)) //AND the player is in the circle  
                //{//since we should only be chasing the player if they're not in the light, then this should only happen when the player activates the
                // //light while we're in the circle 
                    
                //    if (CheckIfPointIsInLight(lampCollider, player.transform.position, out Vector2 _))//if the player is in the circle
                //    { 
                //    }
                //} else //if the point we're chasing (either patrol point or player position) is on the other side of the circle 
                //{
                //    moveTo = closestPoint; //then just move around the circle
                //}
            }
            

            //old version of the above code
            
            if (CheckIfPointIsInLight(lampCollider, moveTo, out Vector2 MoveToCPOL)) //if we're about to move into the circle
            {
                if (ghostState == GhostState.hostile || ghostState == GhostState.curious) //if we're chasing the player
                {
                    if (CheckIfPointIsInLight(lampCollider, target, out Vector2 targetCPOL)) //if our 'final' target is a point in light
                    { //if we're going toa point in the light... 
                        //the point is impossible to reach, get out of the light and find a new target, the closest point to us that's outside of the light
                        ghostState = GhostState.fleeing;
                        patrolCentre = targetCPOL;
                        target = targetCPOL;
                        if (CheckIfPointIsInLight(lampCollider, transform.position, out Vector2 posCPOL)) //if we're in the light 
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
                        if (CheckIfPointIsInLight(lampCollider, transform.position, out Vector2 posCPOL))
                        {
                            target = posCPOL;
                            //if we're currently in the light, walk to the edge, rather than teleporting straight there
                        }
                        else
                        {

                            moveTo = MoveToCPOL;
                        }
                    }
                }
            }
            

        }

        if (moveTo.magnitude >= .001)// if we're moving 
        {
            transform.position = moveTo; //set position to moveTo

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

        // -------------------------------------------------
        //   CLEANUP - we should be able to remove the code below as it should be redundant with the transition from wary to idle in CheckPlayerPos 
        //I don't want to test that though until we verify the other changes are working
        // -------------------------------------------------

        else //if we're not moving
        {

            if (ghostState == GhostState.wary)
            {
                ghostState = GhostState.idle; //could theoretically do this by checking if we've reached our destination?
                //if we implement a wait coroutine, start it here.
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        
        if (other.CompareTag("VertBuilding"))
        {
            spriteRenderer.sortingOrder = -1;
        } else if (other.CompareTag("Lamp"))
        {
            lampCollider = (CircleCollider2D) other;

            newPointMightBeInRangeOfLight = true;

            if (CheckIfPointIsInLight(lampCollider, transform.position, out Vector2 closestPoint))//if we're in the circle
            {
                if (ghostState != GhostState.fleeing) //if we're  not already fleeing
                {
                    if ((ghostState != GhostState.idle) || newPointMightBeInRangeOfLight)
                    {
                        newPointMightBeInRangeOfLight = false;
                        if (CheckIfPointIsInLight(lampCollider, target, out Vector2 closestPointToTarget))
                        {
                            if (ghostState != GhostState.idle) //if we're chasing the player
                            {//get out of the circle
                             //note - since we should only be chasing the player if they're not in the light, then this should only happen when the player activates the
                             //light while we're in the circle 
                                ghostState = GhostState.fleeing;
                                target = closestPoint;
                                patrolCentre = closestPoint; //patrol around where we exit the circle
                            }
                            else //if we're not chasing the player, we must be patrolling to a point that is inside the circle 
                            {//in which case we want to change our target to be on the edge of the circle 
                                target = closestPointToTarget;
                                moveTo = closestPoint;//move around the circle
                                Vector2 boxCorner = boxCollider.ClosestPoint(lampCollider.transform.position);
                                if (CheckIfPointIsInLight(lampCollider, boxCorner, out Vector2 pointOutsideLight))
                                {
                                    transform.position += (Vector3)(pointOutsideLight - boxCorner);
                                }
                                /*
                                while (CheckIfPointIsInLight(lampCollider, boxCorner, out Vector2 newVector)){
                                    Vector2 shiftBy = 
                                    shiftBy += (newVector - (Vector2) lampCollider.transform.position).normalized;
                                }
                                transform.position = (Vector3) shiftBy;*/
                            }
                        }
                        else
                        {
                            Debug.LogWarning("CheckIfPointInLight was called unnecessarily");
                        }

                    }
                    else //if we're patrolling to a point that's  known to not be in the circle
                    { //then the point must be on the other side, so simply move around the circle 
                        moveTo = closestPoint;
                        Vector2 shiftBy = boxCollider.ClosestPoint(lampCollider.transform.position);
                        while (CheckIfPointIsInLight(lampCollider, shiftBy, out Vector2 newVector))
                        {
                            shiftBy += (newVector.normalized - (Vector2)lampCollider.transform.position);
                        }
                        transform.position += (Vector3)shiftBy;
                    }
                }

                //this check is just to avoid calling CheckIfPointIsInLight() repeatedly when we're patrolling to a point on the other side of the circle 


                //if ((ghostState == GhostState.hostile || ghostState == GhostState.curious) //if we're chasing the player
                //    && CheckIfPointIsInLight(lampCollider, player.transform.position, out Vector2 _)) //AND the player is in the circle  
                //{//since we should only be chasing the player if they're not in the light, then this should only happen when the player activates the
                // //light while we're in the circle 

                //    if (CheckIfPointIsInLight(lampCollider, player.transform.position, out Vector2 _))//if the player is in the circle
                //    { 
                //    }
                //} else //if the point we're chasing (either patrol point or player position) is on the other side of the circle 
                //{
                //    moveTo = closestPoint; //then just move around the circle
                //}
            }

            /*
            if (CheckIfPointIsInLight(lampCollider, moveTo, out Vector2 MoveToCPOL)) //if we're about to move into the circle
            {
                if (ghostState == GhostState.hostile || ghostState == GhostState.curious) //if we're chasing the player
                {
                    if (CheckIfPointIsInLight(lampCollider, target, out Vector2 targetCPOL)) //if our 'final' target is a point in light
                    { //if we're going toa point in the light... 
                        //the point is impossible to reach, get out of the light and find a new target, the closest point to us that's outside of the light
                        ghostState = GhostState.fleeing;
                        patrolCentre = targetCPOL;
                        target = targetCPOL;
                        if (CheckIfPointIsInLight(lampCollider, transform.position, out Vector2 posCPOL)) //if we're in the light 
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
                        if (CheckIfPointIsInLight(lampCollider, transform.position, out Vector2 posCPOL))
                        {
                            target = posCPOL;
                            //if we're currently in the light, walk to the edge, rather than teleporting straight there
                        }
                        else
                        {

                            moveTo = MoveToCPOL;
                        }
                    }
                }
            }
            */

        }

    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("VertBuilding"))
        {
            spriteRenderer.sortingOrder = 0;
        }
        else if (other.CompareTag("Lamp"))
        {
            lampCollider = null;

        }
    }


    private void CheckPlayerPos()
    {
        float distanceToPlayer = Vector2.Distance(player.transform.position, this.transform.position);
        //rather than giving up immediately, probably just remove all this and have the ghost change its mind when it gets to the light?
        //alternatively, give the player it's own isInLightBool
        //which route we want to implement depends on how we want the ghosts to behave - should they give up as soon as the player goes into the light 
        //(implying they're blinded by the light), or should they give up when they get close enough to it (Implying a more physical aversion to the light)
        //ultimately this will depend on how they should respond to the player's lantern - whether they slow down because of it or have difficulty seeing the player
        //for now let's set the PlayerIsInLight bool to always be false. if we decide we want the ghosts to respond before reaching the light, then it's easy enough 
        //to change that always false to "read from the player.isIsInLight bool"
        //bool playerIsInLight = CheckIfPointIsInLight(player.transform.position, out _);
        bool playerIsInLight = gameManager.playerController.isInLight;
        switch (ghostState)
        {
            case GhostState.idle: //if we're in idle state

                if (distanceToPlayer <= outerRange && !(playerIsInLight)) //player goes from being idle to entering outer circle
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
                    StopPlayingSound();
                    //loopSource.Stop();
                }

                break;
            case GhostState.curious: //if we're in curious state (state 1)
                if (distanceToPlayer > outerRange) //player exits outer range 
                {
                    //Debug.Log("this should only happen once when player exits outer range");
                    ghostState = GhostState.idle; //idle to curious when player exits outer range
                    StopPlayingSound();
                    //loopSource.Stop();
                    currentSpeed = slowSpeed;
                    lastSeenPlayerPos = player.transform.position;
                    patrolCentre = lastSeenPlayerPos;
                    target = lastSeenPlayerPos + (transform.position - lastSeenPlayerPos).normalized * innerRange;
                }
                else if (distanceToPlayer <= innerRange && !(playerIsInLight )) //player enters inner range
                {
                    ghostState = GhostState.hostile; //curious to hostile upon entering inner range
                    target = player.transform.position;
                    currentSpeed = fastSpeed;
                    stingerSound.Play();
                    
                    StartPlayingSound(hostileLoop);


                    //GetComponent<SpriteRenderer>().color = Color.green;

                    break;
                }
                else
                {
                    target = player.transform.position;

                    if (!loopSource.isPlaying || loopSource.clip != curiousLoop) //if it's not playing, or it's not set to the correct loop
                    {
                        loopSource.clip = curiousLoop;
                        StartPlayingSound(curiousLoop);
                        //loopSource.volume = maxLoopVol;
                        //loopSource.Play();
                    } 
                }   

                break;
            case GhostState.hostile:
                if (distanceToPlayer > outerRange) //player exits outer range 
                {
                    ghostState = GhostState.wary; //hostile to wary upon exiting outer range
                    lastSeenPlayerPos = player.transform.position;
                    currentSpeed = slowSpeed;
                    patrolCentre = lastSeenPlayerPos;
                    target = lastSeenPlayerPos;
                    //loopSource.Stop();
                    StopPlayingSound();

                    if (stingerSound.isPlaying)
                    {
                        stingerSound.Stop();
                    }

                    //GetComponent<SpriteRenderer>().color = Color.white;

                    break;
                }

                target = player.transform.position;
                break;
            case GhostState.wary: //in wary state (state 3)
                if (distanceToPlayer <= outerRange && !(playerIsInLight)) //entering outer range
                {
                    stingerSound.Play();
                    StartPlayingSound(hostileLoop);
                    //loopSource.clip = hostileLoop;
                    //loopSource.Play();
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
                if((Vector2) transform.position ==target) //if we're running away and reach the outside of the circle
                {
                    //patrolCentre = lampLit ? transform.position : lamp.transform.position;
                    patrolCentre = transform.position;//probably redundant
                    ghostState = GhostState.idle;
                    currentSpeed = slowSpeed;
                }

                if (loopSource.isPlaying)
                {
                    StopPlayingSound();
                    //loopSource.Stop();
                }

                break;
        }
    }

    private void StartPlayingSound(AudioClip clip)
    {
        if (isCoroutineRunning)
        {//if it's already running
            StopPlayingSound(); //then stop
        }
        loopSource.clip = clip;
        loopSource.Play();
        
        loopSource.volume = 0;
        increaseVolCoroutine = IncreaseVolume();
        StartCoroutine(increaseVolCoroutine);        
    }

    private void StopPlayingSound()
    {
        loopSource.Stop();
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


    private void PauseSounds()
    {
        if(loopSource.isPlaying)
        {
            loopSource.Pause();
            loopIsPaused = true;
        }

        if (stingerSound.isPlaying)
        {
            stingerSound.Pause();
            stingerIsPaused = true;
        }
    }

    private void UnPauseSounds()
    {

        if (loopIsPaused)
        {
            loopSource.UnPause();
            loopIsPaused = false;
        }

        if (stingerIsPaused)
        {
            stingerIsPaused = false;
            stingerSound.UnPause();
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
        //Vector2 patrolCentre = lampLit ? lastSeenPlayerPos.position : lamp.transform.position;
        target = new Vector2(patrolCentre.x, patrolCentre.y) + patrolRange * new Vector2(Mathf.Cos(bearing), Mathf.Sin(bearing));
        newPointMightBeInRangeOfLight = true;
        
    }

    /*
    [Tooltip("checks if the point is within the range of the light. returns closest point not within light as vec2 out var, just the original point if it's not within light")]
    private bool CheckIfPointIsInLight( Collider2D light, Vector2 point, out Vector2 closestPointOutsideLight)
    {
        bool pointIsInLight = light.bounds.Contains(point);
        if (pointIsInLight)
        {
            closestPointOutsideLight = light.ClosestPoint(point);
        } else
        {
            closestPointOutsideLight = point;
        }
        return pointIsInLight;
    }*/

    
     // old version of CheckIfPointIsInLight, before I realized Unity had its own ways of doing this, and thus I tried to do it manually
    [Tooltip("checks if the point is within the range of the light. returns closest point not within light as vec2 out var, or a zero vector if it's not within light")]
    private bool CheckIfPointIsInLight(CircleCollider2D collider , Vector2 point, out Vector2 closestPointOutsideLight)
    {
        

        //most likely issues are with local to global point translation, or just plain linear algebra

        Vector2 distToLight = point - (Vector2)collider.transform.position;
        //if the distance to the light is less than how far it should be from the lamp (outer radius times lightrange). everything is squared cause it's faster
        //
        if (distToLight.sqrMagnitude < MathF.Pow(lamp.lampLight.pointLightOuterRadius * lightRange, 2))
        { //sqr magnitude is faster than magnitude because it avoids root operations, just make sure to square everything
            //Debug.Log("point is within light");

            closestPointOutsideLight = (distToLight.normalized * ((collider.radius * 1.5f)) ) + (Vector2) collider.transform.position;

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
