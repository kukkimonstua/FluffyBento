using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public PlayerController player;
    public EventSystem eventSystem;

    public static bool GameIsPaused = false;
    public GameObject pauseMenuPanel;
    public Button initialButton;
    private GameObject previousSelection;

    public HelpMenu helpPanel;
    private bool helpOpen;
    
    //Waits for the button event to pause the game.
    void Update()
    {
        if (PlayerController.playerState == PlayerController.ACTIVELY_PLAYING)
        {
            if (Input.GetButtonDown("Cancel")) //when Start is pressed
            {
                if (GameIsPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }
        if (GameIsPaused)
        {
            if (eventSystem.currentSelectedGameObject != null)
            {
                if (eventSystem.currentSelectedGameObject != previousSelection)
                {
                    Debug.Log("NEW previous");
                    previousSelection = eventSystem.currentSelectedGameObject;
                }
            }
            else
            {
                Debug.Log("set to previous");
                eventSystem.SetSelectedGameObject(previousSelection.gameObject);
            }
        }
        if (helpOpen && !helpPanel.gameObject.activeSelf)
        {
            helpOpen = false;
            eventSystem.SetSelectedGameObject(initialButton.gameObject); //Select initial menu option
        }
    }

    public void Resume()
    {
        GameIsPaused = false;
        ToggleMenuUI(false);
        Time.timeScale = 1f;
    }
    public void ToggleMenuUI(bool state)
    {
        if (state)
        {
            pauseMenuPanel.SetActive(true);
            eventSystem.SetSelectedGameObject(initialButton.gameObject); //Select initial menu option
            previousSelection = initialButton.gameObject;
        }
        else
        {
            helpPanel.CloseHelp(); //If it's open
            pauseMenuPanel.SetActive(false);
            eventSystem.SetSelectedGameObject(null); //Deselect all menu options
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        ToggleMenuUI(true);
        GameIsPaused = true;
    }
    public void RestartGame()
    {
        ToggleMenuUI(false);
        player.RestartLevel();
    }
    public void QuitGame()
    {
        ToggleMenuUI(false);
        player.QuitGame();
    }

    public void ShowHelp()
    {
        helpOpen = true;
        helpPanel.gameObject.SetActive(true);
        helpPanel.GoToFirstHelpPage();
        eventSystem.SetSelectedGameObject(helpPanel.GetComponentInChildren<Button>().gameObject); //Select initial menu option
    }

}
