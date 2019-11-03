using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static bool tutorialActive;
    private TutorialObject[] tutorialObjects;
    private SpriteRenderer[] controlPrompts;

    public GUIController gui;
    public bool firstActionCleared;
    public bool secondActionCleared;
    private IEnumerator tutorialInstance;

    void Start()
    {
        tutorialObjects = GetComponentsInChildren<TutorialObject>();
        controlPrompts = GetComponentsInChildren<SpriteRenderer>();
        ResetTutorial();
    }

    public void ResetTutorial()
    {
        tutorialActive = true;
        foreach (TutorialObject to in tutorialObjects)
        {
            
            if (to.myObject.GetComponent<MeteorController>() != null)
            {
                //Debug.Log("a tutorial meteor!");
                GameObject meteor = Instantiate(to.myObject, to.gameObject.transform.position, to.gameObject.transform.rotation);

                gui.AddMinimapMeteor(meteor);
            }
            else
            {
                to.SpawnMyObject();
            }
        }
        ToggleControlPrompts(true);


        StartCoroutine(StartTutorialSequence());
    }
    public void EndTutorial()
    {
        tutorialActive = false;
        ToggleControlPrompts(false);
    }

    public void ToggleControlPrompts(bool toggle)
    {
        foreach (SpriteRenderer prompt in controlPrompts)
        {
            prompt.gameObject.SetActive(toggle);
        }
    }
    private IEnumerator StartTutorialSequence()
    {
        firstActionCleared = false;
        secondActionCleared = false;
        if (tutorialInstance != null) StopCoroutine(tutorialInstance);
        tutorialInstance = TutorialSequencePart1();
        StartCoroutine(tutorialInstance);
        while (!firstActionCleared)
        {
            yield return null;
        }
        StopCoroutine(tutorialInstance);
        tutorialInstance = TutorialSequencePart2();
        StartCoroutine(tutorialInstance);
        while (!secondActionCleared)
        {
            yield return null;
        }
        EndTutorial();
        StopCoroutine(tutorialInstance);
        tutorialInstance = TutorialSequencePart3();
        StartCoroutine(tutorialInstance);
    }

    private IEnumerator SayDialogue(string dialogue)
    {
        gui.UpdateSubtitles(dialogue);
        yield return new WaitForSeconds(4.5f);
    }
    private IEnumerator TutorialSequencePart1()
    {
        yield return SayDialogue("Master Yashiro, look up! Do you see that meteor falling down?");
        yield return SayDialogue("If we let it hit the ground, that’s it for us! We’re all done for!");
        yield return SayDialogue("We have to do something about it... If only we had some way to destroy it before it lands...");
        yield return SayDialogue("...Huh? Master, over there! Something fell! It looks like...a sword?");
        yield return SayDialogue("It looks pretty unusual for a sword... Maybe... Maybe that sword could do something about the meteor?");
        yield return SayDialogue("Master, go get that sword! I’m sure someone with your awesome sword skills could do something with it!");
        gui.UpdateSubtitles("");
    }

    private IEnumerator TutorialSequencePart2()
    {
        yield return SayDialogue("Okay, you got it! Now let’s try to get closer to that meteor! Quick, before it lands!");
        yield return SayDialogue("Once you’re close enough, get ready to attack! You need to hit it with good timing if you’re gonna destroy it!");
        gui.UpdateSubtitles("");
    }
    private IEnumerator TutorialSequencePart3()
    {
        yield return SayDialogue("Ah, the sword broke! It looks like it could only handle hitting a meteor once...");
        yield return SayDialogue("M-Master! More meteors are coming!");
        yield return SayDialogue("...Oh! It looks like more swords are falling down too!");
        yield return SayDialogue("Master, hurry and get to those swords! We can’t let a single meteor hit the ground!");
        yield return SayDialogue("I’ll go evacuate the townspeople! Master, be careful! If you get hurt from the meteors and pass out, that will mean the end for us too!");
        gui.UpdateSubtitles("");
    }

}
