using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private Rigidbody2D rigidBody;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D hitBoxCollider;
    [SerializeField] private Light2D spotLight; //spotlight?

    [SerializeField] private Lantern lantern;



    [Header("movement")]

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private float playerSpeed;
    [SerializeField] private Vector2 moveInput;
    [SerializeField] private Vector2 prevInput;

    //public bool touchingWall = false;
    enum PlayerDirection { Left, Right, Up, Down };
    [SerializeField] private PlayerDirection playerDirection;

    [SerializeField] private Animator playerAnimator;

    [Tooltip("used to set the direction in the animator")]
    private int movementAnimationDirection;

    private const int WALK_SIDE_DIRECTION = 1;
    private const int WALK_UP_DIRECTION = 2;
    private const int WALK_DOWN_DIRECTION = 3;
    //private const int IDLE_LEFT_DIRECTION = 0;
    //private const int IDLE_RIGHT_DIRECTION = 0;
    private const int IDLE_SIDE_DIRECTION = 4;
    private const int IDLE_UP_DIRECTION = 5;
    private const int IDLE_DOWN_DIRECTION = 6;

    #region Unity Built in
    private void Awake()
    {

        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("player controller added a gameObject that doesn't have a PlayerInput on it -- which is definitely a bug");
        }
        movementAnimationDirection = IDLE_DOWN_DIRECTION;
        rigidBody.interpolation = RigidbodyInterpolation2D.Extrapolate;
    }

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;


        gameManager.onGamePause.AddListener(SwitchActionMapUI);
        gameManager.onGameResume.AddListener(SwitchActionMapPlayer);
    }

    // Update is called once per frame
    void FixedUpdate()
    {


        Move(moveInput);

    }

    private void OnDestroy()
    {
        gameManager.onGamePause.RemoveListener(SwitchActionMapUI);
        gameManager.onGameResume.RemoveListener(SwitchActionMapPlayer);
    }
    #endregion Unity Built in

    #region move
    public void Move(Vector2 direction)
    {

        if (playerInput != null)
        {
            rigidBody.velocity = direction * playerSpeed;
        }

        if (!Mathf.Approximately(direction.x, 0) || !Mathf.Approximately(direction.y, 0))//if we're inputting movement
        {
            Vector3 targetPosition = new Vector3(this.transform.position.x + direction.y, this.transform.position.y - direction.x, 0);



            //override if moving sideways
            if (direction.x < 0)// override if we're moving left
            {

                GetComponent<SpriteRenderer>().flipX = true;
                movementAnimationDirection = WALK_SIDE_DIRECTION;

                //flip sprite left
            }
            else if (direction.x > 0) // or if we're moving right
            {
                GetComponent<SpriteRenderer>().flipX = false;

                //flip sprite right

                movementAnimationDirection = WALK_SIDE_DIRECTION;
            }

            if (direction.y > 0) //if the player is moving up
            {
                GetComponent<SpriteRenderer>().flipX = false;
                movementAnimationDirection = WALK_UP_DIRECTION;
            }
            else if (direction.y < 0) //if we're moving down
            {
                GetComponent<SpriteRenderer>().flipX = false;
                movementAnimationDirection = WALK_DOWN_DIRECTION;
            }

        } else { //not moving
        
            //if we were in a moving anim direction, switch to that respective idle direction
            switch (movementAnimationDirection)
            {
                case (IDLE_DOWN_DIRECTION):
                case (IDLE_SIDE_DIRECTION):
                case (IDLE_UP_DIRECTION):
                    break;

                default:
                case (WALK_SIDE_DIRECTION):
                    movementAnimationDirection = IDLE_SIDE_DIRECTION;
                    break;
                case (WALK_UP_DIRECTION):
                    movementAnimationDirection = IDLE_UP_DIRECTION;
                    break;
                case (WALK_DOWN_DIRECTION):
                    movementAnimationDirection = IDLE_DOWN_DIRECTION;
                    break;
            }
        }
            //prev anim direction?

            playerAnimator.SetInteger("movement", movementAnimationDirection);        
    }
    #endregion move

    #region input actions

    public void MoveActionPerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void MoverVertActionPerformed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            moveInput.y = context.ReadValue<float>();
            prevInput.y = context.ReadValue<float>();
            moveInput.x = 0; //move input gets overridden, prev input does not
        }

        if (context.canceled)
        {
            moveInput.x = prevInput.x; // set horizontal to how we were moving before this
            prevInput.y = 0; //remove vertical inputs on both
            moveInput.y = 0;
        }
    }


    public void MoverHorizActionPerformed(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            moveInput.x = context.ReadValue<float>();
            prevInput.x = context.ReadValue<float>();
            moveInput.y = 0;
        }

        if (context.canceled)
        {
            moveInput.y = prevInput.y;
            moveInput.x = 0;
            prevInput.x = 0;
        }
    }

    public void PauseActionPerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (gameManager.paused) //if we're already paused
            {
                gameManager.ResumeGame(); //resume
            }
            else
            {
                gameManager.PauseGame();
            }

        }
    }
    #endregion input actions


    #region action maps

    public void SwitchActionMapPlayer() { SwitchActionMap("Moving"); }
    public void SwitchActionMapUI() { SwitchActionMap("PauseMenu"); }
    public void SwitchActionMap(string mapName)
    {
        playerInput.currentActionMap.Disable();
        playerInput.SwitchCurrentActionMap(mapName);

        switch (mapName)
        {
            case "PauseMenu":
                //UnityEngine.Cursor.visible = true;
                //UnityEngine.Cursor.lockState = CursorLockMode.None;
                break;
            default:
            case "Moving":
                //UnityEngine.Cursor.visible = false;
                //UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                break;
        }
    }
    #endregion action maps

    #region collisions

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ghost"))
        {
            gameManager.SwitchToScene(GameManager.LOSESCENE, ScoreKeeper.LossReason.Ghost);

        }
    }

    #endregion collisions
}
