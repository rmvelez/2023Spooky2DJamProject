using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuControl : MonoBehaviour
{
    // a list of te avaliable states that the menu can be in
    public enum GameState {MenuState,LearnState,CreditState, Options}

    // reference to the current menu screen that the user is on
    public GameState currentGameState;

    // references to the various screens that make up the menu scene
    public GameObject menu;
    public GameObject learn;
    public GameObject credit;
    public GameObject options;

    private OptionsManager optionsManager;

    // Start is called before the first frame update
    void Start()
    {
        currentGameState = GameState.MenuState;
        ShowScreen(menu);
        options.SetActive(true);
        optionsManager = OptionsManager.Instance;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("LevelDesign");
    }

    // displays the main menu
    public void ShowMainMenu()
    {
        currentGameState = GameState.MenuState;
        ShowScreen(menu);
    }

    // displays the screen that contains the instructions for the game
    public void ShowInstructions()
    {
        currentGameState = GameState.LearnState;
        ShowScreen(learn);
    }

    // displays the credits
    public void ShowCredits() 
    {
        currentGameState = GameState.CreditState;
        ShowScreen(credit);
    }

    public void ShowOptions()
    {
        currentGameState = GameState.Options;
        ShowScreen(options);
    }


    // used to determine which screen to show based on the current game state
    private void ShowScreen(GameObject gameObjectToShow)
    {
        menu.SetActive(false);
        learn.SetActive(false);
        credit.SetActive(false);
        options.SetActive(false);

        gameObjectToShow.SetActive(true);
    }
}
