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

    [Tooltip("the minimum amount a target destination can be placed outside of the lamp collider to ensure the ghost's transform can still reach it")]
    private const float GRACE_RANGE = 1.53f; // calculated as the magnitude from the center of the ghosts transform to the furthest corner on the box collider, rounded up slightly

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
        //direction sets the direction of the sprite, so we want to do that before any exit Circle stuff, as that tends to throw the ghost in a few weird directions

        if (lampCollider != null)
        {
            ExitCircle();
        }

        if(ghostState != GhostState.idle)
        {
            newPointMightBeInRangeOfLight = false;
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

    private void ExitCircle()
{
        //check that the ghost is not already fleeing - as if it's doing that then we just want it to keep moving to it's target rather than changing it's behavior as it does so 
        if (ghostState != GhostState.fleeing)
        {
            Vector2 boxCorner = boxCollider.ClosestPoint(lampCollider.transform.position); //this is the corner closest to the center of the circle
            Vector2 cornerToTransform = (Vector2)transform.position - boxCorner; // whenever we set a destination, we want to shift it by this much - otherwise it may not be reachable by the ghost 

            if (lampCollider.OverlapPoint(boxCollider.bounds.center)) //this will be true if the center of the box is within the collider, i.e. the ghost is starting inside the collider rather than entering the side
            {   // this happens when the player lights the lamp with the ghost inside
                // if the ghost is (fully) inside the light, then we need to get out
                CheckIfPointIsInLight(lampCollider, boxCollider.bounds.center, out Vector2 closestPointToGhost);

                //the following lines tell the ghost to start moving to a point directly out of the lamp range, plus a little extra
                Vector2 exitDirection = closestPointToGhost - (Vector2) lampCollider.transform.position;
                exitDirection = (exitDirection + (exitDirection.normalized * GRACE_RANGE));
                
                //exitDirection = (exitDirection + cornerToTransform) * 1.1f;
                //Debug.Log("exitDirection: " + exitDirection);
                target = (Vector2) lampCollider.transform.position + (exitDirection); 

                patrolCentre = target;//have the ghost patrol around this new point just outside the circle

                ghostState = GhostState.fleeing;

            }
            else //if the ghost is touching the lamp collider, but not fully inside it, then the ghost is on the edge. 
            {
                //if the ghost is fleeing though, then the ghost must be either trying to pursue a point either within the collider or on the other side of it
                //checkPlayerPos should theoretically be checking if the player is in the light already (playerIsInLight bool), so this should only
                //happen if the ghost has chosen to patrol to a point that happens to be inside the light, or on the very edge - such that reaching the point
                //would require passing through the light 

                //first step is to make sure the ghost is not trying to reach a patrol point that's already inside the light 
                //this variable will keep track of when the ghost has already moved a point outside the light, so that CheckIfPointIsInLight() isn't being called unnecessarily
                if (newPointMightBeInRangeOfLight)
                {
                    newPointMightBeInRangeOfLight = false;
                    Debug.Log("newPointMightBeInRangeOfLight");


                    if (CheckIfPointIsInLight(lampCollider, target, out Vector2 closestPointToTarget))
                    {
                        //the following lines tell the ghost to start moving directly out of the lamp range, plus a little extra
                        Vector2 targetDirection = closestPointToTarget - (Vector2) lampCollider.transform.position;

                        targetDirection = (targetDirection + (targetDirection.normalized * GRACE_RANGE));
                        //if the ghost goes to a point just on the edge of the lamp collider, then their collider will probably still be overlapping, so move it just a bit further out
                        //Debug.Log("targetDirection: " + targetDirection);
                        target = (Vector2)lampCollider.transform.position + (targetDirection);
                    }
                    else
                    {
                        Debug.Log("CheckIfPointIsInLight called unnecessarily");
                    }
                }
                else //now that we've confirmed the target is outside of the light, we want to just have the ghost circle around the light 
                {

                    if (CheckIfPointIsInLight(lampCollider, boxCorner, out Vector2 cornerOutsideLight)) //retrieve the closest point to that corner that is outside the light
                    {

                        //the out variable will be where the corner will be after the ghost gets out of the light, and will therefore be useful for calculating the direction to get out of the light

                        Vector2 shiftBy = (cornerOutsideLight - boxCorner)  ;
                        Vector2 newDir = shiftBy - (Vector2) transform.position;
                        newDir = newDir.normalized + moveTo;
                        float step = currentSpeed * Time.deltaTime * 100;
                        moveTo = Vector2.MoveTowards(transform.position, newDir, step);

                        //moveTo += shiftBy;//instead of moving directly towards the point, adjust the ghosts direction such that it'll will stay outside of the circle 

                    }
                    else
                    {
                        Debug.LogWarning("something's wrong I can feel it");
                        //somehow lampcollider is null but not a single point in our boxcollider is inside the lamp collider
                    }
                }
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
            lampCollider = (CircleCollider2D)other;
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
            case GhostState.idle: //if the ghost is in idle state

                if (distanceToPlayer <= outerRange && !(playerIsInLight)) //player goes from being idle to entering outer circle
                {
                    ghostState = GhostState.curious;
                    target = player.transform.position;
                    currentSpeed = slowSpeed;

                    break;
                }  else if (Vector2.Distance(transform.position, target) < .1) //if the ghost has reached its destination
                {
                    Patrol();
                } else if (loopSource.isPlaying)
                {
                    StopPlayingSound();
                    //loopSource.Stop();
                }

                break;
            case GhostState.curious: //if the ghost is in curious state (state 1)
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
                if((Vector2) transform.position ==target) //if the ghost is running away and reach the outside of the circle
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

        float radius = collider.radius * 1.51f;
        //we're multiplying the radius by 1.5 because the scale of the prefab root is 1.5, and an addition .01 just helps to ensure this point is absolutely outside of the circle (rather than exactly on the border)


        if (distToLight.sqrMagnitude < MathF.Pow(radius, 2))
        { //sqr magnitude is faster than magnitude because it avoids root operations, just make sure to square everything else 
            //Debug.Log("point is within light");

            closestPointOutsideLight = (distToLight.normalized * (radius) ) + (Vector2) collider.transform.position;

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
