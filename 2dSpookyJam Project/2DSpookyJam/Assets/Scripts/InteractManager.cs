using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InteractManager : MonoBehaviour
{
    [SerializeField] private Collider2D InteractCollider;


    List<GameObject> trackedObjects = new List<GameObject>();
    GameObject trackedObject = null;

    [Tooltip("Event called when we go from 0 to 1 at least 1 interactable object in range")]
    public UnityEvent OnInteractablesExist;
    [Tooltip("event called when we go from some number to 0 interactables in range")]
    public UnityEvent OnInteractablesDoNotExist;

    [Tooltip("the player controller on the player (drag it here)")]
    [SerializeField] private PlayerController playerController;

    private GameManager gameManager;

    [SerializeField] private Collider2D[] interactableColliders;

    IEnumerator objectFinderCoroutine;
    bool lookingForObjects = true;


    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;

        //objectFinderCoroutine = LookForObjectsCoroutine();
        //StartCoroutine(objectFinderCoroutine);
    }

    //at one point I had tried refactoring the interact code to rely on overlapCircle rather than colliders, this was meant to circumvent some issues 
    //with the interact colliders and the player hitbox colliders being treated as a composite (so the player's hitbox was as large as their interact range
    //I got it working but am keeping the old methods just in case they end up being needed

    //private IEnumerator LookForObjectsCoroutine()
    //{
    //    WaitForSeconds tenPerSecond = new WaitForSeconds(0.1f);
    //    while (lookingForObjects)
    //    {
    //        LookForObjects();
    //        yield return tenPerSecond;
    //    }
    //}

    //old code = - see comments on line 41
    //private void LookForObjects()
    //{
    //    trackedObjects.Clear();
    //    interactableColliders = Physics2D.OverlapCircleAll(transform.position, 1.75f, 5);

    //    foreach(Collider2D other in interactableColliders)
    //    {
    //        if (other.CompareTag("Interactable"))
    //        {
    //            //then track it
    //            TrackObject(other.gameObject);
    //        }
    //    }
    //}


    private void Update()
    {
        //LookForObjects();
    }


    public void InteractActionPerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            CallInteract();
        }

    }

    private void TrackObject(GameObject objectToTrack)
    {
        //original iteration of this code used a list of gameObjects to provide support for tracking multiple objects. 
        //since the player will (theoretically) only ever be in range of one object at a time, using a single object is sufficient
        //it also provides easier and more reliable means of checking for interactable objects, which also circumvent potential
        //performance issues with a list
        //if the player does ever somehow track two objects simultaneously, then the code should default to whichever was tracked first,
        //then track the second once the first is removed

        //only bother if we don't yet have a tracked object
        if(trackedObject == null)
        {
            Vector3 distToLight = objectToTrack.transform.position - transform.position;
            if (Mathf.Pow(playerController.lantern.LightRange, 2) > (distToLight.sqrMagnitude))
            {//object is within range
                Debug.Log("object is within range");
                Lamp lamp;
                //next, check if it's a lamp
                if (lamp = objectToTrack.GetComponent<Lamp>())
                {
                    //if it is a lamp, the player should only be able to interact while it's lit
                    if (!lamp.isLit)
                    {
                        trackedObject = objectToTrack;
                        gameManager.showInteractPrompt = true;

                    }
                }
                else//it must not be a lamp, no need to check if it's lit or anything, cause oil cans are always interactable until they're collected (and destroyed)
                {
                    trackedObject = objectToTrack;
                    gameManager.showInteractPrompt = true;
                }
                //trackedObject = lamp.gameObject;

            }
            else
            {
                Debug.Log("light is NOT within range");

            }

        }

        //old version of code, see line comments on line 89
        //if (!trackedObjects.Contains(objectToTrack))
        //{
        //    //TODO: convert lamp display/track code over to new method, call that method here and when the player refils their lantern
        //    Lamp lamp;

        //    if(lamp = objectToTrack.GetComponent<Lamp>())
        //    {
        //        CheckIfLampIsInRange(Lamp)
        //    } else//it must not be a lamp, or we've somehow messed up on this check
        //    {
        //        trackedObjects.Add(objectToTrack);
        //        gameManager.showInterractPrompt = true;
        //    }
        //}        
    }

   

    //public so we can call it from another object after we "destroy" this one
    public void UntrackObject(GameObject trackedObject)
    {
        if(trackedObject == this.trackedObject)
        {

            if (this.trackedObject != null)
            {
                trackedObject = null;
                this.trackedObject = null;

                gameManager.showInteractPrompt = false;
            }

        } else
        {
            //Debug.LogWarning("UntrackObject was passed an object different from the currently tracked object");
        }

        ////old code, see line 89 for details
        ////only try to remove it if it's already in there
        //if (trackedObjects.Contains(trackedObject))
        //{
        //    trackedObjects.Remove(trackedObject);
        //    gameManager.showInterractPrompt = trackedObjects.Count > 0;
        //}
    }
    private void CheckIfLampIsInRange(Lamp lamp)
    {

        //the player can only interact with the lamp if the lamp is lit...
        if (!lamp.isLit)
        {
            Vector3 distToLight = lamp.transform.position - transform.position;

            //...AND if the distance between ourselves and the lantern is less than that of the lightRange
            if (Mathf.Pow(playerController.lantern.LightRange, 2) > (distToLight.sqrMagnitude))
            {
                trackedObject = lamp.gameObject;
                gameManager.showInteractPrompt = true;

                Debug.Log("light is within range");
            }
            else
            {
                Debug.Log("light is NOT within range");

            }
        }
    }

    #region collision code


    private void OnTriggerStay2D(Collider2D other)
    {
        //if an interactable enters our trigger area
        if (other.CompareTag("Interactable"))
        {
            //Debug.Log("trigger enter");
            //then track it
            TrackObject(other.gameObject);

        }
        //else if (other.CompareTag("Ghost"))
        //{
        //    gameManager.SwitchToScene(GameManager.LOSESCENE, ScoreKeeper.LossReason.Ghost);
        //}
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("untracking object");
        // If the object exiting the trigger is an interactable object
        if (other.CompareTag("Interactable"))
        {
            UntrackObject(other.gameObject);
        }
    }
    #endregion collision code

    public void CallInteract()
    {
        if(trackedObject != null)
        {
            trackedObject.GetComponent<IInteractable>().Interact();
            UntrackObject(trackedObject);
        }

        //old code from previous interaction system which used a list -see comments in line 89 for more information
        //make sure we still have objects
        //if (trackedObjects.Count > 0)
        //    {

        //        if (trackedObjects[0] != null)
        //        {
        //            //retrieve the first one
        //            GameObject gameObj = trackedObjects[0];

        //            //interact only with the first one

        //            UntrackObject(gameObj);
        //            gameObj.GetComponent<IInteractable>().Interact();
        //        }
        //    }
    }

}
