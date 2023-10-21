using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigidBody;

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject lamp;

    [SerializeField] private float slowSpeed = 1;
    [SerializeField] private float fastSpeed = 0.5f;

    [SerializeField] private float outerCircle = 10;
    [SerializeField] private float innerCircle = 5;

    [SerializeField] private float patrolRange = 5; // the maximum distance the target can be from the ghost (if lamp is lit) or the lamp (if not lit)

    private enum States
    {
        Idle,
        Curious,
        Hostile,
        Wary
    }

    private Vector2 target;
    
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
            case (int)States.Idle:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, slowSpeed);
                break;
            case (int)States.Curious:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, slowSpeed);
                break;
            case (int)States.Hostile:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, fastSpeed);
                break;
            case (int)States.Wary:
                rigidBody.position += Vector2.MoveTowards(rigidBody.position, target, slowSpeed);
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "player outer circle")
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
        else if (collider.gameObject.tag == "player inner circle" && state != (int)States.Hostile)
        {
            state = (int)States.Hostile;
            target = player.transform.position;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "player outer circle")
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

    private void Patrol()
    {
        if (Vector2.Distance(rigidBody.position, target) >= 0.1f) return; // ALTERNATIVELY: if ((rigidBody.position - target).magnitude >= 0.1f)
        float bearing = Random.Range(-Mathf.PI, Mathf.PI);
        Vector3 patrolCentre = lampLit ? transform.position : lamp.transform.position;
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
        target = lamp.transform.position;
    }
}
