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
    public Button btn;
    public EventSystem es;

    // Update is called once per frame
    void Update()
    {
       if (Input.GetButtonDown("Cancel"))
        {
            if (GameIsPaused)
            {
                Resume();
            } else
            {
                Pause();
            }
        }

    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
        controls.SetActive(false);
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;

        //highlight initial menu option
        es.SetSelectedGameObject(null);
        es.SetSelectedGameObject(btn.gameObject);

        GameIsPaused = true;
    }

    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void ShowControls()
    {
        controls.SetActive(true);
    }


    public bool Paused()
    {
        return GameIsPaused;
    }
   
}
