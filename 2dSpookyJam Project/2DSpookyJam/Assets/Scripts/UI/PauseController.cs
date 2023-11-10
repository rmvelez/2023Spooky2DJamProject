using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PauseController : MonoBehaviour
{

    [SerializeField] private UIDocument UIdoc;
    [SerializeField] private GameObject OptionsMenu;
    [SerializeField] private GameObject HUD;

    private VisualElement root;
    private Button resumeButton;
    private Button mainMenuButton;
    private Button optionsButton;

    private GameManager gameManager;
    private AudioSource pauseSound;

    
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;

        root = UIdoc.rootVisualElement;
        resumeButton = root.Q<Button>("ResumeButton");
        mainMenuButton = root.Q<Button>("MainMenuButton");
        optionsButton = root.Q<Button>("OptionsButton");

        gameManager.onGamePause.AddListener(pauseMenuPause);
        gameManager.onGameResume.AddListener(pauseMenuPause);


        resumeButton.clicked += resumeFromPause;
        mainMenuButton.clicked += GoToMainMenu;
        optionsButton.clicked += ShowOptionsMenu;

        root.visible = false;

        pauseSound = this.gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void GoToMainMenu()
    {
        pauseSound.Play();
        gameManager.SwitchToScene("MenuScene");
    }

    private void resumeFromPause()
    {
        pauseSound.Play();
        gameManager.ResumeGame();
    }

    private void pauseMenuPause()
    {
        if (gameManager.paused)
        {//show pause menu
            root.visible = true;
            root.SetEnabled(true);
        } else
        { //hide pause menu
            root.visible = false;
            root.SetEnabled(false);
        }
    }

    private void ShowOptionsMenu()
    {
        HUD.SetActive(false);

        pauseSound.Play();

        OptionsMenu.SetActive(true);

        //hide pause menu
        root.visible = false;
        root.SetEnabled(false);
    }
    
    public void HideOptionsMenu()
    {
        HUD.SetActive(true);
        //show pause menu
        root.visible = true;
        root.SetEnabled(true);

        pauseSound.Play();

        OptionsMenu.SetActive(false);
    }
}
