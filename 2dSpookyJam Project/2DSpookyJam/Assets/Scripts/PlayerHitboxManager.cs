using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitboxManager : MonoBehaviour
{
    private GameManager gameManager;

    [SerializeField] private Rigidbody2D rigidBody;
    [SerializeField] private Collider2D hitBoxCollider;

    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;

        playerController = gameManager.playerController;
    }

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ghost"))
        {
            gameManager.SwitchToScene(GameManager.LOSESCENE, ScoreKeeper.LossReason.Ghost);

        }
    }
}
