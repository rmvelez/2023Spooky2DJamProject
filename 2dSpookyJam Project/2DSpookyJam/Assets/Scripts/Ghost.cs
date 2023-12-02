using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
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

    private AudioPauser loopPauser;

    [SerializeField] private Vector2 target;
    [Tooltip("theoretically set by referencing GameManager.playerController")]
    [SerializeField] private GameObject player;
    [SerializeField] private Lamp lamp;
    private Vector3 lastSeenPlayerPos;

    [SerializeField] private float innerRange = 4;
    [SerializeField] private float outerRange = 7;

    [SerializeField] private float fastSpeed = 1;
    [SerializeField] private float slowSpeed = 0.5f;
    [SerializeField] private float currentSpeed ;
    
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
    private AudioPauser stingerPauser;

    //= lampLit ? lastSeenPlayerPos.position : lamp.transform.position;

    private float maxLoopVol;

    [SerializeField] private new  Rigidbody2D rigidbody; 

    [SerializeField] private float loopVolInc;
    private bool isCoroutineRunning;
    IEnumerator increaseVolCoroutine;

    [Tooltip("the angle used to calculate how much to rotate our movement vector by so as to go around the lamp collider when our destination is on the other side")]
    float rotationAngle;

    private bool newPointMightBeInRangeOfLight; //unity throws a warning that this variable isn't being used, but it is

    private Vector2 moveTo;

    private bool goingAround = false;
    private Vector2 goingAroundDest;
    // Start is called before the first frame update
    void Start()
    {
        //animator.SetFloat("CameraMovement", 1);

        gameManager = GameManager.Instance;
        //rigidBody = GetComponent<Rigidbody2D>();

        currentSpeed = slowSpeed;

        player = gameManager.playerController.gameObject;


        target = lamp.gameObject.transform.position;

        rotationAngle = 0; 

        patrolCentre = lamp.transform.position;

        maxLoopVol = loopSource.volume;

        increaseVolCoroutine = IncreaseVolume();
        isCoroutineRunning = false;

        gameManager.onGamePause.AddListener(PauseSounds);
        gameManager.onGameResume.AddListener(UnPauseSounds);

        Physics2D.queriesHitTriggers = true;
        lampCollider = lamp.GetComponent<CircleCollider2D>();
        
        loopPauser = new AudioPauser(loopSource);
        stingerPauser = new AudioPauser(stingerSound);

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

        //moveTo = Vector2.MoveTowards(transform.position, target, step);
        moveTo = target - (Vector2) transform.position;


        Vector2 direction = moveTo;
        //direction sets the direction of the sprite, so we want to do that before any exit Circle stuff, as that tends to throw the ghost in a few weird directions
        moveTo = Quaternion.Euler(0, 0, rotationAngle) * moveTo;

        ExitCircle();

        moveTo = goingAround ? goingAroundDest: moveTo;

        if (ghostState != GhostState.idle)
        {
            newPointMightBeInRangeOfLight = false;
        }

        if (moveTo.magnitude >= .001)// if we're moving 
        {
            rigidbody.velocity = (moveTo.normalized * currentSpeed);
            //if (goingAround)
            //{
            //    rigidbody.velocity = (goingAroundDest.normalized * currentSpeed);
            //} else
            //{

            //    rigidbody.velocity = (moveTo.normalized * currentSpeed) ;
            //}


            #region animation var setting
            if ( MathF.Abs( direction.x) > MathF.Abs(direction.y)) //if we're moving horizontally more than vertically
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
                Debug.Log("should be transitioning to idle");
                //if we implement a wait coroutine, start it here.
            }
        }

    }


    private void ExitCircle()
    {
        RaycastHit2D RaycastToDestination = Physics2D.Raycast(transform.position, moveTo, Vector2.Distance(transform.position, target) );


        if(RaycastToDestination.collider != null && RaycastToDestination.collider.CompareTag("Lamp"))
        {
            if (lampCollider = (CircleCollider2D)RaycastToDestination.collider)
            {
                //check that the ghost is not already fleeing - as if it's doing that then we just want it to keep moving to it's target rather than changing it's behavior as it does so 
                if (ghostState != GhostState.fleeing)
                {
                    if (lampCollider.OverlapPoint(transform.position))
                    //this will be true if the center of the box is within the collider, i.e. the ghost is starting inside the collider rather than entering the side
                    {   // this happens when the player lights the lamp with the ghost inside
                        // if the ghost is (fully) inside the light, then we need to get out
                        CheckIfPointIsInLight(lampCollider, transform.position, out Vector2 closestPointToGhost);

                        

                        //the following lines tell the ghost to start moving to a point directly out of the lamp range, plus a little extra
                        Vector2 exitDirection = closestPointToGhost - (Vector2)lampCollider.transform.position;
                        exitDirection = (exitDirection + (exitDirection.normalized));

                        //exitDirection = (exitDirection + cornerToTransform) * 1.1f;
                        //Debug.Log("exitDirection: " + exitDirection);
                        target = (Vector2)lampCollider.transform.position + (exitDirection);

                        patrolCentre = target;//have the ghost patrol around this new point just outside the circle

                        ghostState = GhostState.fleeing;
                    }
                    else // if the boxcast succeeded but the ghost isn't already inside the collider, then it will move into it sooner or later, so adjust course
                    {
                        CheckIfPointIsInLight(lampCollider, target, out target);

                        //don't run these calculations repeatedly, only do so if we haven't already done so 
                        if (!goingAround)
                        {
                            Vector3 normal = RaycastToDestination.normal;
                            //left is normal x up 



                            Vector3 leftTangent = Vector3.Cross(normal, Vector3.forward );
                            Vector3 rightTangent = Vector3.Cross(Vector3.forward, normal);

                            float leftAngle = Vector3.Angle(leftTangent, moveTo);
                            float rightAngle = Vector3.Angle(rightTangent, moveTo);

                            goingAroundDest = moveTo;

                            Quaternion rotation;

                            rotation = (rightAngle >= leftAngle) ? Quaternion.Euler(0f, 0f, 5f) : Quaternion.Euler(0f, 0f, -5f);

                            RaycastHit2D aroundRaycast = Physics2D.Raycast(transform.position, goingAroundDest, Vector2.Distance(transform.position, goingAroundDest));
                            while (aroundRaycast.collider != null && aroundRaycast.collider.CompareTag("Lamp"))
                            {
                                //rotate the vector until it's no longer hitting the collider 
                                goingAroundDest = rotation * goingAroundDest;
                                aroundRaycast = Physics2D.Raycast(transform.position, goingAroundDest, Vector2.Distance(transform.position, goingAroundDest));
                            }
                            goingAround = true;
                        }

                        //goingAroundDest = Quaternion.Euler(0, 0, rotationAngle) * moveTo;

                        //the tangent with the larger angle tells us which direction is closer to the edge
                    }

                }

            }
            else
            {
                Debug.LogError("Cast from collider with lamp tag to circleCollider failed");
            }
        } else
        {
            goingAround = false;
            rotationAngle = 0;
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
        if(ghostState != GhostState.idle && ghostState != GhostState.idle) //if we're chasing the player
        {
            if(target != (Vector2) player.transform.position)
            {//if the player has moved
                goingAround = false;
            }
        }

        switch (ghostState)
        {
            case GhostState.idle: //if the ghost is in idle state

                if (distanceToPlayer <= outerRange && !(playerIsInLight)) //player goes from being idle to entering outer circle
                {
                    ghostState = GhostState.curious;
                    target = player.transform.position;

                    currentSpeed = slowSpeed;

                    break;
                }  else if (Vector2.Distance(transform.position, target) < 1) //if the ghost has reached its destination
                {
                    Patrol();
                } else if (loopSource.isPlaying)
                {
                    StopPlayingSound();
                    //loopSource.Stop();
                }

                break;
            case GhostState.curious: //if the ghost is in curious state (state 1)
                if (distanceToPlayer > outerRange || playerIsInLight) //player exits outer range 
                {
                    //Debug.Log("this should only happen once when player exits outer range");
                    ghostState = GhostState.idle; //idle to curious when player exits outer range
                    StopPlayingSound();
                    //loopSource.Stop();
                    currentSpeed = slowSpeed;
                    lastSeenPlayerPos = player.transform.position;
                    //patrolCentre = lastSeenPlayerPos; //uncomment this line if we want the ghost to patrol around where it last saw the player
                    //I removed it because it tended to cause the ghosts to "drift" and confuse the player regarding the position of their associated lamp
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
                else //if player isn't in light 
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
                if (distanceToPlayer > outerRange || playerIsInLight ) //player exits outer range 
                {
                    ghostState = GhostState.wary; //hostile to wary upon exiting outer range
                    lastSeenPlayerPos = player.transform.position;
                    if (playerIsInLight && lampCollider != null) // and lampcollider 
                    {
                        CheckIfPointIsInLight( lampCollider, lastSeenPlayerPos, out Vector2 closestPointToPlayer );
                        lastSeenPlayerPos = closestPointToPlayer;
                    } 

                    currentSpeed = slowSpeed;
                    //patrolCentre = lastSeenPlayerPos; //uncomment this line if we want the ghost to patrol around where it last saw the player
                    //I removed it because it tended to cause the ghosts to "drift" and confuse the player regarding the position of their associated lamp
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
                    ghostState = GhostState.idle;//wary to idle upon reaching last seen location
                    currentSpeed = slowSpeed;
                    Patrol();
                    break;
                }


                break;
            case GhostState.fleeing:

                if(Vector2.Distance(transform.position, target) < .1) //if the ghost is running away and reach the outside of the circle
                {
                    if(!CheckIfPointIsInLight(lampCollider, transform.position, out target))
                    {

                        ghostState = GhostState.idle;
                        currentSpeed = slowSpeed;
                    }
                }

                if(stingerSound.isPlaying) 
                { 
                    stingerSound.Stop(); 
                }

                if (loopSource.isPlaying)
                {
                    StopPlayingSound();
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
        loopPauser.PauseSound(out loopSource);
        stingerPauser.PauseSound(out stingerSound);

    }

    private void UnPauseSounds()
    {
        loopPauser.ResumeSound(out loopSource);
        stingerPauser.ResumeSound(out stingerSound);
    }

    //sets target to a new random position within range of the relevant point 
    private void Patrol()
    {

        //if (Vector2.Distance(rigidBody.position, target) >= 0.1f) return; // ALTERNATIVELY: if ((rigidBody.position - target).magnitude >= 0.1f)
        //if (Vector2.Distance(transform.position, target) >= 0.1f) return; // ALTERNATIVELY: if ((rigidBody.position - target).magnitude >= 0.1f)
        


        float bearing = UnityEngine.Random.Range(-Mathf.PI, Mathf.PI);
        //Vector2 patrolCentre = lampLit ? lastSeenPlayerPos.position : lamp.transform.position;
        target = new Vector2(patrolCentre.x, patrolCentre.y) + patrolRange * new Vector2(Mathf.Cos(bearing), Mathf.Sin(bearing));

        //if lampcollider isn't null then we want to make sure the new patrol point isn't inside the light
        if(lampCollider != null)
        {
            CheckIfPointIsInLight(lampCollider, target, out target);
        }


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

    
     
    [Tooltip("checks if the point is within the range of the light. returns closest point not within light as vec2 out var, or the initial vector if it's not within light")]
    private bool CheckIfPointIsInLight(CircleCollider2D collider , Vector2 point, out Vector2 closestPointOutsideLight)
    {       



        //most likely issues are with local to global point translation, or just plain linear algebra

        Vector2 distToLight = point - (Vector2)collider.transform.position;

        float radius = collider.radius * 1.6f;
        //we're multiplying the radius by 1.6 because the scale of the prefab root is 1.5, and an addition .1 just helps to ensure this point is
        //absolutely outside of the circle, and likely far enough that the box collider likely won't overlap


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
