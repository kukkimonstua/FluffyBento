using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    public Image fullscreenBlack;
    private IEnumerator fadeCoroutine;
    private bool cutsceneEnded;
    public Image backgroundImage;

    public GameObject dialogueBackground;
    public Text dialogueName;
    public Text dialogueText;

    public GameObject continueButton;
    private bool isCurrentlyTyping;
    private string completeText;
    private float typeDelay = 0.001f;

    public Image leftPortrait;
    public Image rightPortrait;
    public Sprite emptyPortrait;
    private bool firstSpriteAppeared;
    public Color activeShade;
    public Color inactiveShade;

    //FIFO collection (type of list)
    public Queue<DialogueBase.Info> dialogueInfo = new Queue<DialogueBase.Info>();
    public static DialogueManager instance;
    private DialogueBase selectedScript;
    public int debugSceneIndex = 0;

    public DialogueBase script1OP;
    public Sprite background1OP;
    public DialogueBase script1ED;
    public Sprite background1ED;
    public DialogueBase script2OP;
    public Sprite background2OP;

    private void Awake()
    {
        if (debugSceneIndex != 0) GameManager.sceneIndex = debugSceneIndex;

        if (instance != null)
        {
            Debug.LogWarning("Fix This" + gameObject.name);
        }
        else
        {
            instance = this; //Maybe move all code after this to a Start()?
            StartCutscene();
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            SkipToEnd();
        }

        //reset selection to continue button for non-button clicks
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(continueButton);
        }
        else
        {
            continueButton = EventSystem.current.currentSelectedGameObject;
        }
    }
    private void StartCutscene()
    {
        switch (GameManager.sceneIndex)
        {
            case GameManager.LEVEL_2_OP:
                selectedScript = script2OP;
                backgroundImage.sprite = background2OP;
                break;
            case GameManager.LEVEL_1_ED:
                selectedScript = script1ED;
                backgroundImage.sprite = background1ED;
                break;
            default:
                selectedScript = script1OP;
                backgroundImage.sprite = background1OP;
                break;
        }

        firstSpriteAppeared = false;
        cutsceneEnded = false;
        leftPortrait.sprite = emptyPortrait;
        rightPortrait.sprite = emptyPortrait;
        dialogueBackground.SetActive(true);
        dialogueName.gameObject.SetActive(true);
        dialogueText.gameObject.SetActive(true);
        continueButton.GetComponent<Image>().enabled = true;

        instance.EnqueueDialogue(selectedScript); // This triggers the dialogue, could possibly be used to restart entire scene?
    }

    private void EnqueueDialogue(DialogueBase db)
    {
        dialogueInfo.Clear(); //clear to not carry over older queue data
        foreach (DialogueBase.Info info in db.dialogueInfo)
        {
            dialogueInfo.Enqueue(info);
        }
        DequeueDialogue();
    }

    public void DequeueDialogue()
    {
        if (isCurrentlyTyping || fadeCoroutine != null)
        {
            CompleteText();
            return;
        }

        if (dialogueInfo.Count <= 0 || cutsceneEnded) {
            EndOfDialogue(1.0f);
            return;
        }

        DialogueBase.Info info = dialogueInfo.Dequeue();
        completeText = info.myText;

        dialogueName.text = info.myName;
        dialogueText.text = "";

        if (info.portrait != null)
        {
            if (info.isLeftPortrait)
            {
                leftPortrait.color = activeShade;
                rightPortrait.color = inactiveShade;
                leftPortrait.sprite = info.portrait;
            }
            else
            {
                leftPortrait.color = inactiveShade;
                rightPortrait.color = activeShade;
                rightPortrait.sprite = info.portrait;
            }
            if (!firstSpriteAppeared)
            {
                firstSpriteAppeared = true;
                fadeCoroutine = FadeBlackScreen(0.0f, 1.0f); // create an IEnumerator object
            }
        }
        
        StartCoroutine(TypeText(info));
    }

    private IEnumerator TypeText(DialogueBase.Info info)
    {
        continueButton.GetComponent<Image>().enabled = false;
        if (fadeCoroutine != null)
        {
            yield return StartCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        isCurrentlyTyping = true;     
        foreach (char c in info.myText.ToCharArray())
        {
            yield return new WaitForSeconds(typeDelay);
            dialogueText.text += c;
            yield return null;
        }
        isCurrentlyTyping = false;
        continueButton.GetComponent<Image>().enabled = true;
    }

    private void CompleteText()
    {
        if (fadeCoroutine != null)
        {
            fullscreenBlack.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        }
        //Debug.Log("Stop all!");
        StopAllCoroutines();
        fadeCoroutine = null;
        isCurrentlyTyping = false;

        continueButton.GetComponent<Image>().enabled = true;
        dialogueText.text = completeText;
    }
    private void SkipToEnd()
    {
        Debug.Log("End this cutscene!"); //Replace with audio feedback
        fullscreenBlack.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        CompleteText();
        EndOfDialogue(0.0f);
    }
    private void EndOfDialogue(float delay)
    {
        if (!cutsceneEnded)
        {
            cutsceneEnded = true;
            Debug.Log("conversation over");

            dialogueBackground.SetActive(false);
            dialogueName.gameObject.SetActive(false);
            dialogueText.gameObject.SetActive(false);
            continueButton.GetComponent<Image>().enabled = false;

            StartCoroutine(LoadNextPart(delay));
        }
    }

    private IEnumerator LoadNextPart(float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(FadeBlackScreen(1.0f, 2.0f));

        Debug.Log("LOAD NEXT PART");

        switch(GameManager.sceneIndex)
        {
            case GameManager.LEVEL_3_ED:
                Debug.Log("LOAD MAIN MENU OR CREDITS");
                break;

            case GameManager.LEVEL_2_ED:
                GameManager.sceneIndex = GameManager.LEVEL_3_OP;
                StartCutscene();
                break;
            case GameManager.LEVEL_2_OP:
                //These two lines should be loaded by lvl 2's victory screen
                GameManager.sceneIndex = GameManager.LEVEL_2_ED;
                StartCutscene();
                break;

            case GameManager.LEVEL_1_ED:
                GameManager.sceneIndex = GameManager.LEVEL_2_OP;
                StartCutscene();
                break;
            default:
                Debug.Log("LOAD LEVEL 1");

                //These two lines should be loaded by lvl 1's victory screen
                GameManager.sceneIndex = GameManager.LEVEL_1_ED;
                StartCutscene();

                break;
        }
    }

    private IEnumerator FadeBlackScreen(float alphaTarget, float duration)
    {
        float alpha = fullscreenBlack.GetComponent<CanvasRenderer>().GetAlpha();
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
        {
            fullscreenBlack.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Lerp(alpha, alphaTarget, t));
            yield return null;
        }
        fullscreenBlack.GetComponent<CanvasRenderer>().SetAlpha(alphaTarget);
    }

}
