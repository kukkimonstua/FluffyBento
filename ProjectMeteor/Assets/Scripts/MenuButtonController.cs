using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuButtonController : MonoBehaviour {

    public BGMController bgm;
    public AudioClip bgmClip;

    public Image fullscreenBlack;
    public Image backgroundImage;

    private int panelIndex = 0;

    public GameObject startScreenPanel;
    public GameObject mainMenuPanel;
    public GameObject levelSelectPanel;
    public CreditsController credits;

    public Button initialButton;
    public EventSystem eventSystem;
	public AudioSource audioSource;

    private MenuButton previousSelection;

	void Start () {
        GameManager.ResetRunningScore();
		audioSource = GetComponent<AudioSource>();
        eventSystem.SetSelectedGameObject(initialButton.gameObject); //Select initial menu option
        previousSelection = initialButton.gameObject.GetComponent<MenuButton>();

        startScreenPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);

        StartCoroutine(FadeLoadScreen(0.0f, 2.0f, 0.0f, GameManager.MAIN_MENU_INDEX));
        bgm.FadeInMusic(bgmClip, 2.0f);
    }
	void Update () {
        switch (panelIndex)
        {
            case 0:
                if (Input.GetButtonDown("Submit") || Input.GetButtonDown("Cancel"))
                {
                    if (eventSystem.enabled)
                    {
                        OpenMainMenu();
                    }
                }
                break;
            case 1:
            case 2:
                if (Input.GetButtonDown("buttonB"))
                {
                    ReturnToStart();
                }
                break;
        }
        
        if (eventSystem.currentSelectedGameObject != null && eventSystem.currentSelectedGameObject.GetComponent<MenuButton>() != null)
        {
            if (eventSystem.currentSelectedGameObject.GetComponent<MenuButton>() != previousSelection)
            {
                previousSelection.IsSelected(false);
                previousSelection.IsPressed(false);
                previousSelection = eventSystem.currentSelectedGameObject.GetComponent<MenuButton>();
                eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsSelected(true);
            }
        }
        else
        {
            eventSystem.SetSelectedGameObject(previousSelection.gameObject);
        }
	}

    public void ReturnToStart()
    {
        panelIndex = 0;
        startScreenPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(startScreenPanel.transform.GetChild(0).gameObject);

        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
    }
    public void OpenMainMenu()
    {
        eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsPressed(true);

        panelIndex = 1;
        mainMenuPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(mainMenuPanel.transform.GetChild(0).gameObject);

        startScreenPanel.SetActive(false);
        levelSelectPanel.SetActive(false);
    }
    public void OpenLevelSelect()
    {
        eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsPressed(true);

        panelIndex = 2;
        levelSelectPanel.SetActive(true);
        eventSystem.SetSelectedGameObject(levelSelectPanel.transform.GetChild(0).gameObject);

        startScreenPanel.SetActive(false);
        mainMenuPanel.SetActive(false);
    }

    public void StartStoryMode()
    {
        eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsPressed(true);
        StartCoroutine(FadeLoadScreen(1.0f, 2.0f, 1.0f, GameManager.START_CUTSCENE));
        bgm.FadeOutMusic(3.0f);
    }
    public void StartLevel2()
    {
        eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsPressed(true);
        StartCoroutine(FadeLoadScreen(1.0f, 2.0f, 1.0f, GameManager.LEVEL_2_OP));
        bgm.FadeOutMusic(3.0f);
    }
    public void StartLevel3()
    {
        eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsPressed(true);
        StartCoroutine(FadeLoadScreen(1.0f, 2.0f, 1.0f, GameManager.LEVEL_3_OP));
        bgm.FadeOutMusic(3.0f);
    }


    public void StartSurvivalMode()
    {
        eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsPressed(true);
        StartCoroutine(FadeLoadScreen(1.0f, 2.0f, 1.0f, GameManager.SURVIVAL_MODE));
        bgm.FadeOutMusic(3.0f);
    }
    public void ShowExtras()
    {
        eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsPressed(true);
        StartCoroutine(PlayCredits());
    }
    public void QuitTheGame()
    {
        eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsPressed(true);
        StartCoroutine(FadeToQuit());
        bgm.FadeOutMusic(3.0f);
    }
    private IEnumerator FadeToQuit()
    {
        eventSystem.enabled = false;
        yield return new WaitForSeconds(1.0f);

        float alpha = fullscreenBlack.GetComponent<CanvasRenderer>().GetAlpha();
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / 2.0f)
        {
            fullscreenBlack.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Lerp(alpha, 1.0f, t));
            yield return null;
        }
        fullscreenBlack.GetComponent<CanvasRenderer>().SetAlpha(1.0f);

        Debug.Log("GAME IS DEAD");
        Application.Quit();
    }

    private IEnumerator PlayCredits()
    {
        eventSystem.enabled = false;
        bgm.FadeOutMusic(1.0f);
        yield return new WaitForSeconds(1.0f);

        fullscreenBlack.GetComponent<CanvasRenderer>().SetAlpha(1.0f);

        credits.gameObject.SetActive(true);
        credits.StartCredits();
        while (credits.gameObject.activeSelf)
        {
            yield return null;
        }

        bgm.FadeInMusic(bgmClip, 1.0f);
        fullscreenBlack.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        eventSystem.enabled = true;
    }

    private IEnumerator FadeLoadScreen(float alphaTarget, float duration, float delay, int sceneTarget)
    {
        eventSystem.enabled = false;
        yield return new WaitForSeconds(delay);

        float alpha = fullscreenBlack.GetComponent<CanvasRenderer>().GetAlpha();
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
        {
            fullscreenBlack.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Lerp(alpha, alphaTarget, t));
            yield return null;
        }
        fullscreenBlack.GetComponent<CanvasRenderer>().SetAlpha(alphaTarget);

        if (sceneTarget != GameManager.MAIN_MENU_INDEX)
        {
            //use the scene target value to pick the scene to load
            GameManager.sceneIndex = sceneTarget;
            if (GameManager.sceneIndex >= GameManager.START_CUTSCENE)
            {
                SceneManager.LoadScene(GameManager.START_CUTSCENE); //The cutscene scene
            }
            else
            {
                SceneManager.LoadScene(GameManager.sceneIndex); //everything else
            }
            
        }
        eventSystem.enabled = true;
    }
}
