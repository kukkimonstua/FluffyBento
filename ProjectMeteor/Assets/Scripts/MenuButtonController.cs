using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuButtonController : MonoBehaviour {

    public GameObject mainMenuUI;
    public Button initialButton;
    public EventSystem eventSystem;
	public AudioSource audioSource;

    private MenuButton previousSelection;

	void Start () {
		audioSource = GetComponent<AudioSource>();
        eventSystem.SetSelectedGameObject(initialButton.gameObject); //Select initial menu option
        previousSelection = initialButton.gameObject.GetComponent<MenuButton>();
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

}
