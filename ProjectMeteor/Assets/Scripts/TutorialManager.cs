using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static bool tutorialActive;
    private static TutorialObject[] tutorialObjects;
    private static SpriteRenderer[] controlPrompts;

    void Start()
    {
        tutorialObjects = GetComponentsInChildren<TutorialObject>();
        controlPrompts = GetComponentsInChildren<SpriteRenderer>();
        ResetTutorial();
    }

    public static void ResetTutorial()
    {
        tutorialActive = true;
        foreach (TutorialObject to in tutorialObjects)
        {
            to.SpawnMyObject();
        }
        ToggleControlPrompts(true);
    }
    public static void EndTutorial()
    {
        tutorialActive = false;
        ToggleControlPrompts(false);
    }

    public static void ToggleControlPrompts (bool toggle)
    {
        foreach (SpriteRenderer prompt in controlPrompts)
        {
            prompt.gameObject.SetActive(toggle);
        }
    }
}
