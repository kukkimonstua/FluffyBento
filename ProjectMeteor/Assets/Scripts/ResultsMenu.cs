using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ResultsMenu : MonoBehaviour
{
    public const int VICTORY = 1;
    public const int METEOR_DEATH = 2;
    public const int HEALTH_DEATH = 3;

    //public static bool GameIsPaused = false;
    public GameObject resultsMenuUI;
    //public GameObject controls;

    public Text header;
    public Text explanation;
    public Text advice;

    public Button initialButton;
    public Button altInitialButton;
    public PlayerController player;
    public EventSystem eventSystem;

    public void ShowResultsMenu(int resultType, float duration, float delay)
    {
        resultsMenuUI.GetComponent<CanvasGroup>().alpha = 0.0f;
        resultsMenuUI.SetActive(true);

        switch (resultType)
        {
            case VICTORY:
                resultsMenuUI.GetComponent<CanvasRenderer>().SetAlpha(0.5f);
                header.text = "VICTORY!";
                explanation.text = "You protected everyone from annihilation!";
                advice.gameObject.SetActive(false);

                initialButton.gameObject.SetActive(true);
                StartCoroutine(FadeInResultsMenu(duration, delay, initialButton.gameObject));
                break;

            default:
                resultsMenuUI.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
                header.text = "GAME OVER";
                advice.gameObject.SetActive(true);
                if (resultType == HEALTH_DEATH)
                {
                    explanation.text = "You passed out, and the meteors wiped out the land...";
                    advice.text = "If you're not confident with your timing, play it safe and press the button a little sooner. You lose some style, but you will definitely destroy the meteor!";
                }
                else
                {
                    explanation.text = "A meteor landed and destroyed everything...";
                    advice.text = "Having trouble finding a sword? They tend to appear up in higher places.";
                }
                initialButton.gameObject.SetActive(false);
                StartCoroutine(FadeInResultsMenu(duration, delay, altInitialButton.gameObject));
                break;
        }
    }
    public void HideResultsMenu()
    {
        eventSystem.SetSelectedGameObject(null); //Deselect all menu options
        resultsMenuUI.SetActive(false);
    }

    private IEnumerator FadeInResultsMenu(float duration, float delay, GameObject buttonToSet)
    {
        yield return new WaitForSeconds(delay);

        float counter = 0.0f;
        while (counter < duration)
        {
            resultsMenuUI.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(0, 1.0f, counter / duration);
            counter += Time.deltaTime;
            yield return null;
        }

        resultsMenuUI.GetComponent<CanvasGroup>().alpha = 1.0f;
        eventSystem.SetSelectedGameObject(buttonToSet); //Select initial menu option
    }

    public void Continue()
    {
        Debug.Log("go to next level because current level is " + PlayerController.currentLevel);
        if (PlayerController.currentLevel == 3)
        {
            GameManager.sceneIndex = GameManager.LEVEL_3_ED;
            SceneManager.LoadScene(5);
        }
        else if (PlayerController.currentLevel == 2)
        {
            GameManager.sceneIndex = GameManager.LEVEL_2_ED;
            SceneManager.LoadScene(5);
        }
        else
        {
            GameManager.sceneIndex = GameManager.LEVEL_1_ED;
            SceneManager.LoadScene(5);
        }
    }

    public void RestartGame()
    {
        player.RestartLevel();
    }
    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
