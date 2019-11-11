using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuButtonController : MonoBehaviour {

    public Image fullscreenBlack;
    public Image backgroundImage;

    public GameObject mainMenuUI;
    public Button initialButton;
    public EventSystem eventSystem;
	public AudioSource audioSource;

    private MenuButton previousSelection;

	void Start () {
		audioSource = GetComponent<AudioSource>();
        eventSystem.SetSelectedGameObject(initialButton.gameObject); //Select initial menu option
        previousSelection = initialButton.gameObject.GetComponent<MenuButton>();


        StartCoroutine(FadeLoadScreen(0.0f, 2.0f, 0.0f, GameManager.MAIN_MENU_INDEX));
    }
	void Update () {
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

    public void StartStoryMode()
    {
        eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsPressed(true);

        Debug.Log("start the story!");
        StartCoroutine(FadeLoadScreen(1.0f, 2.0f, 1.0f, GameManager.LEVEL_1_OP));

        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //Load itself, lol
    }
    public void StartSurvivalMode()
    {
        eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsPressed(true);
        Debug.Log("let the carnage begin!");
    }
    public void ShowExtras()
    {
        eventSystem.currentSelectedGameObject.GetComponent<MenuButton>().IsPressed(true);
        Debug.Log("show extras!");
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
            GameManager.sceneIndex = GameManager.LEVEL_1_OP;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); //THIS IS TEMP
        }
        eventSystem.enabled = true;
        
    }
}
