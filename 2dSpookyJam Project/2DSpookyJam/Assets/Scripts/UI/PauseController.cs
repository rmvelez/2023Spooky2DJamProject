using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseController : MonoBehaviour
{

    [SerializeField] private UIDocument UIdoc;

    private VisualElement root;
    private Button resumeButton;
    private Button mainMenuButton;

    private GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;

        root = UIdoc.rootVisualElement;
        resumeButton = root.Q<Button>("ResumeButton");
        mainMenuButton = root.Q<Button>("MainMenuButton");

        gameManager.onGamePause.AddListener(pauseMenuPause);


        resumeButton.clicked += resumeFromPause;
        mainMenuButton.clicked += GoToMainMenu;

        root.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GoToMainMenu()
    {
        gameManager.SwitchToScene("MenuScene");
    }

    private void resumeFromPause()
    {
        gameManager.ResumeGame();
    }

    private void pauseMenuPause()
    {
        if (gameManager.paused)
        {
            root.visible = true;
            root.SetEnabled(true);
        } else
        {
            root.visible = false;
            root.SetEnabled(false);
        }
    }
}
