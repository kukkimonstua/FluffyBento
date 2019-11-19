using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameObject controls;
    public Button initialButton;
    public PlayerController player;
    public EventSystem eventSystem;

    //Waits for the button event to pause the game.
    void Update()
    {
       if (PlayerController.playerState == 1)
       {
            if (Input.GetButtonDown("Cancel"))
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
    }

    public void Resume()
    {
        ToggleMenuUI(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }
    public void ToggleMenuUI(bool state)
    {
        if (state)
        {
            pauseMenuUI.SetActive(true);
            eventSystem.SetSelectedGameObject(initialButton.gameObject); //Select initial menu option
        }
        else
        {
            eventSystem.SetSelectedGameObject(null); //Deselect all menu options
            controls.SetActive(false); //If they're open
            pauseMenuUI.SetActive(false);
        }
    }

    public void Pause()
    {
        GameIsPaused = true;
        Time.timeScale = 0f;
        ToggleMenuUI(true);
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

    public void ShowControls()
    {
        controls.SetActive(true);
    }   
}
