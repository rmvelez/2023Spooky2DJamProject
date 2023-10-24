using System.Collections;
using System.Collections.Generic;
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

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
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
        
        trackedObjects.Add(objectToTrack);
        
    }

    //public so we can call it from another object after we "destroy" this one
    public void UntrackObject(GameObject trackedObject)
    {
        //only try to remove it if it's already in there
        if (trackedObjects.Contains(trackedObject))
        {
            trackedObjects.Remove(trackedObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //if an interactable enters our trigger area
        if (other.CompareTag("Interactable"))
        {
            //Debug.Log("trigger enter");
            //then track it
            TrackObject(other.gameObject);

        } else if (other.CompareTag("Ghost"))
        {
            gameManager.SwitchToScene(GameManager.LOSESCENE, ScoreKeeper.LossReason.Ghost);
        }
    }


    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("untrackign object");
        // If the object exiting the trigger is an interactable object
        if (other.CompareTag("Interactable"))
        {
            UntrackObject(other.gameObject);
        }
    }

    public void CallInteract()
    {
        //make sure we still have objects
        if (trackedObjects.Count > 0)
        {
            //retrieve the first one
            GameObject gameObj = trackedObjects[0];


            UntrackObject(gameObj);


            //interact only with the first one
            gameObj.GetComponent<IInteractable>().Interact();

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
