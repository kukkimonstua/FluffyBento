using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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
        eventSystem.SetSelectedGameObject(null); //Deselect all menu options
        controls.SetActive(false); //If they're open
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void Pause()
    {
        GameIsPaused = true;
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        eventSystem.SetSelectedGameObject(initialButton.gameObject); //Select initial menu option
    }
    public void RestartGame()
    {
        Resume();
        player.RestartLevel();
    }
    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowControls()
    {
        controls.SetActive(true);
    }   
}
