using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResultsMenu : MonoBehaviour
{
    public const int VICTORY = 1;
    public const int METEOR_DEATH = 2;
    public const int HEALTH_DEATH = 3;

    public GameObject resultsMenuUI;
    public Image decal;
    public PlayerController player;
    public EventSystem eventSystem;

    public Text header;
    public Text explanation;
    public Text advice;
    public Text finalScore;

    public Button initialButton;
    public Button altInitialButton;
    private GameObject previousSelection;
    private bool resultsMenuUnlocked;

    void Start()
    {
        previousSelection = initialButton.gameObject;
    }

    void Update()
    {
        //decal.GetComponent<RectTransform>().Rotate(new Vector3(0, 0, -30.0f * Time.deltaTime));
        if (resultsMenuUnlocked)
        {
            if (eventSystem.currentSelectedGameObject != null)
            {
                if (eventSystem.currentSelectedGameObject != previousSelection)
                {
                    previousSelection = eventSystem.currentSelectedGameObject;
                }
            }
            else
            {
                eventSystem.SetSelectedGameObject(previousSelection.gameObject);
            }
        }
    }

    public void ShowResultsMenu(int currentScore, int resultType, float duration, float delay)
    {
        finalScore.text = currentScore.ToString("000000");

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
                    advice.text = "If you're not confident with your timing, press the button a little sooner. You lose some style, but you will definitely destroy the meteor!";
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
        resultsMenuUnlocked = true;
    }

    public void Continue()
    {
        resultsMenuUnlocked = false;
        eventSystem.SetSelectedGameObject(null); //Deselect all menu options
        player.GoToNextLevel();
    }

    public void RestartGame()
    {
        resultsMenuUnlocked = false;
        eventSystem.SetSelectedGameObject(null); //Deselect all menu options
        player.RestartLevel();
    }
    public void QuitGame()
    {
        resultsMenuUnlocked = false;
        eventSystem.SetSelectedGameObject(null); //Deselect all menu options
        player.QuitGame();
    }

}
