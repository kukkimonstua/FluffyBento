using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static bool tutorialActive;
    private TutorialObject[] tutorialObjects;
    private SpriteRenderer[] controlPrompts;

    public MeteorManager meteorManager;
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
        if (gameObject.activeSelf)
        {
            tutorialActive = true;
            gui.helpText.gameObject.SetActive(true);
            foreach (TutorialObject to in tutorialObjects)
            {
                if (to.myObject.GetComponent<MeteorController>() != null)
                {
                    GameObject meteor = Instantiate(to.myObject, to.gameObject.transform.position, to.gameObject.transform.rotation);
                    gui.AddMinimapMeteor(meteor, 0);
                }
                else
                {
                    to.SpawnMyObject();
                }
            }
            ToggleControlPrompts(true);
            CameraController.tutorialCameraTimer = 3.0f;
            StartCoroutine(StartTutorialSequence());
        }
    }
    public void EndTutorial()
    {
        gui.helpText.gameObject.SetActive(false);
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
        switch (PlayerController.currentLevel) {
            case GameManager.LEVEL_3:
                yield return SayDialogue("I suppose it's not surprising that you would choose to fight.");
                yield return SayDialogue("Even though you taught me everything I know, your ideas of justice are probably still different than mine.");
                yield return SayDialogue("Well, it won't matter soon enough. Once all of us are gone, ideals in constant conflict will be a thing of the past!");
                yield return SayDialogue("So Master, hurry and get out of here! Or else my swarm of meteors will end you before everyone else!");
                gui.UpdateSubtitles("");
                break;
            case GameManager.LEVEL_2:
                yield return SayDialogue("Yashiro! What in the world are you doing up here?!");
                yield return SayDialogue("...Wait. Are you trying to get to those swords?");
                yield return SayDialogue("I see. It appears safe to assume that you have encountered meteors like these before, and repelled them using your swordplay prowess.");
                yield return SayDialogue("Regardless, please exercise caution. I doubt falling from these structures will hurt you, but you will lose precious time trying to get back up.");
                gui.UpdateSubtitles("");
                break;
            default:
                yield return SayDialogue("Master Yashiro, up there! Do you see that meteor falling from the sky?");
                yield return SayDialogue("If we let it hit the ground, that’s it for us! We’re all done for!");
                yield return SayDialogue("We have to do something about it... If only we had some way to destroy it before it lands...");
                yield return SayDialogue("...Huh? Master, over there! That thing shining a pillar of light! It looks like...a sword?");
                yield return SayDialogue("It looks pretty unusual for a sword... Maybe... Maybe that sword could do something about the meteor?");
                yield return SayDialogue("Master, go get that sword! I’m sure someone with your awesome sword skills could do something with it!");
                gui.UpdateSubtitles("");
                break;
        }
    }
    private IEnumerator TutorialSequencePart2()
    {
        switch (PlayerController.currentLevel)
        {
            case GameManager.LEVEL_3:
                yield return SayDialogue("Master Yashiro, you know I couldn't have done this without you, right?");
                yield return SayDialogue("Ten no Ikari... This relic requires a mysterious energy supplied only by the rocks it summons.");
                yield return SayDialogue("I started small, and kept a cycle going until I could make them nice and big.");
                yield return SayDialogue("And then I thought hey, why not summon a whole bunch to speed things along?");
                yield return SayDialogue("I figured I might as well do the world a favour and wipe out Taneshima while I was at it.");
                yield return SayDialogue("But it wasn't enough. There was barely any energy left when they hit the ground.");
                gui.UpdateSubtitles("");
                break;
            case GameManager.LEVEL_2:
                yield return SayDialogue("Yashiro. That meteor over there seems to be different from the others.");
                yield return SayDialogue("It appears to be much more durable. And it’s glowing a rather unnatural colour. Hmm...");
                yield return SayDialogue("Some of the swords scattered around here seem to glowing the same colour as that meteor.");
                yield return SayDialogue("Perhaps there’s a connection?");
                gui.UpdateSubtitles("");
                break;
            default:
                yield return SayDialogue("Okay, you got it! Now let’s try to get closer to that meteor! Quick, before it lands!");
                yield return SayDialogue("Once you’re close enough, get ready to attack! You need to hit it with good timing if you’re gonna destroy it!");
                gui.UpdateSubtitles("");
                break;
        }
    }
    private IEnumerator TutorialSequencePart3()
    {
        switch (PlayerController.currentLevel)
        {
            case GameManager.LEVEL_3:
                yield return SayDialogue("Then I realized. I need to destroy them before they land. But there's no way I accomplish something so superhuman.");
                yield return SayDialogue("But you, Master. I knew you would be able to do it. All I did was make some small, thin meteors appear, and you used them like swords, just as I had hoped.");
                yield return SayDialogue("You blew up the meteors and produced so much energy, the town was covered by it as though it were mist!");
                yield return SayDialogue("I have more than enough now. More than enough to summon a colossus that will bring an end to humanity!");
                yield return SayDialogue("Not even YOU can stop it, Master!");
                gui.UpdateSubtitles("");
                break;
            case GameManager.LEVEL_2:
                yield return SayDialogue("Yes, I’m sure of it now. Those unusual meteors require a sword glowing the same colour in order for you to destroy it.");
                yield return SayDialogue("Be sure to take a good look at the meteor and the sword you have equipped before going in for the attack.");
                yield return SayDialogue("Attacking those special meteors with the wrong sword will do nothing, regardless of your swordsmanship.");
                yield return SayDialogue("I know you will hate to hear this but...I am counting on you. The future of this nation is in your hands.");
                yield return SayDialogue("You must prevail.");
                gui.UpdateSubtitles("");
                break;
            default:
                yield return SayDialogue("Ah, the sword broke! It looks like it could only handle hitting a meteor once...");
                yield return SayDialogue("M-Master! More meteors are coming!");
                yield return SayDialogue("...Oh! It looks like more swords are falling down too!");
                yield return SayDialogue("Master, hurry and get to those swords! We can’t let a single meteor hit the ground!");
                yield return SayDialogue("I’ll go evacuate the townspeople! Master, be careful! If you get hurt from the meteors and pass out, that will mean the end for us too!");
                gui.UpdateSubtitles("");
                break;
        }
    }
}
