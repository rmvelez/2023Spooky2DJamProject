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

    [Tooltip("Event called when we go from 0 to 1 at least 1 interractable object in range")]
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

    private IEnumerator LookForObjectsCoroutine()
    {
        WaitForSeconds tenPerSecond = new WaitForSeconds(0.1f);
        while (lookingForObjects)
        {
            LookForObjects();
            yield return tenPerSecond;
        }
    }


    private void Update()
    {
        //LookForObjects();
    }

    private void LookForObjects()
    {
        trackedObjects.Clear();
        interactableColliders = Physics2D.OverlapCircleAll(transform.position, 1.75f, 5);

        foreach(Collider2D other in interactableColliders)
        {
            if (other.CompareTag("Interactable"))
            {
                //then track it
                TrackObject(other.gameObject);
            }
        }


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
        if (!trackedObjects.Contains(objectToTrack))
        {
            Lamp lamp = objectToTrack.GetComponent<Lamp>();
            //could hypothetically do this as one line
            if ( lamp != null)
            {
                if (!lamp.isLit)
                {
                    trackedObjects.Add(objectToTrack);
                    gameManager.showInterractPrompt = true;
                }
            } else//it must not be a lamp, or we've somehow messed up on this check
            {
                trackedObjects.Add(objectToTrack);
                gameManager.showInterractPrompt = true;
            }
        }

        
    }

   

    //public so we can call it from another object after we "destroy" this one
    public void UntrackObject(GameObject trackedObject)
    {
        //only try to remove it if it's already in there
        if (trackedObjects.Contains(trackedObject))
        {
            trackedObjects.Remove(trackedObject);
            gameManager.showInterractPrompt = trackedObjects.Count > 0;
        }
    }

    #region old collision code


    private void OnTriggerEnter2D(Collider2D other)
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
        //Debug.Log("untrackign object");
        // If the object exiting the trigger is an interactable object
        if (other.CompareTag("Interactable"))
        {
            UntrackObject(other.gameObject);
        }
    }
    #endregion old collision code

    public void CallInteract()
    {
        //make sure we still have objects
        if (trackedObjects.Count > 0)
        {

            if (trackedObjects[0] != null)
            {
                //retrieve the first one
                GameObject gameObj = trackedObjects[0];

                //interact only with the first one

                UntrackObject(gameObj);
                gameObj.GetComponent<IInteractable>().Interact();
            }
        }
    }

}
