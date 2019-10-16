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
    public Button btn;
    public EventSystem es;



    // Update is called once per frame
    void Update()
    {
       if (Input.GetKeyDown(KeyCode.Escape))
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


    public bool Paused()
    {
        return GameIsPaused;
    }
   
}
