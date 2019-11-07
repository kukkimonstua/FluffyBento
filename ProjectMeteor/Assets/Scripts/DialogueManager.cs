using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBackground;
    //public Button btn;
    //public EventSystem es;

    public Image fullscreenBlack;
    public Text dialogueName;             //character name
    public Text dialogueText;             //dialogue text
    public Image leftPortrait;        //character sprite
    public Image rightPortrait;
    private bool firstSpriteAppeared;
    public Image button;
    public float typeDelay = 0.001f;

    private IEnumerator fadeCoroutine;
    private bool fadingToNext;

    private bool isCurrentlyTyping;
    private string completeText;

    public Color activeShade;
    public Color inactiveShade;

    public static DialogueManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("Fix This" + gameObject.name);
        }
        else {
            instance = this;
            firstSpriteAppeared = false;
            fadingToNext = false;
        }
    }


    //FIFO collection (type of list)
    public Queue<DialogueBase.Info> dialogueInfo = new Queue<DialogueBase.Info>();   

    public void EnqueueDialogue(DialogueBase db)
    {
        dialogueInfo.Clear();     //clear to not carry over older queue data

        //dialogueBackground.SetActive(true);
        // es.SetSelectedGameObject(null);
        // es.SetSelectedGameObject(btn.gameObject);

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

        if(dialogueInfo.Count == 0){
            EndOfDialogue();
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
                fadeCoroutine = FadeBlackScreen(0.0f, 2.0f); // create an IEnumerator object
            }
        }
        
        StartCoroutine(TypeText(info));
    }

    IEnumerator TypeText(DialogueBase.Info info)
    {
        //yield return new WaitForSeconds(3.0f);
        button.enabled = false;
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
        button.enabled = true;
    }

    private void CompleteText()
    {
        if (fadeCoroutine != null)
        {
            fullscreenBlack.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        }
        Debug.Log("Stop all!");
        StopAllCoroutines();
        fadeCoroutine = null;
        isCurrentlyTyping = false;

        button.enabled = true;
        dialogueText.text = completeText;
    }

    public void EndOfDialogue()
    {
        if (!fadingToNext)
        {
            fadingToNext = true;
            Debug.Log("conversation over");

            dialogueBackground.SetActive(false);
            dialogueName.gameObject.SetActive(false);
            dialogueText.gameObject.SetActive(false);
            button.enabled = false;

            StartCoroutine(LoadNextPart());
        }
    }
    private IEnumerator LoadNextPart()
    {
        yield return new WaitForSeconds(1.0f);
        yield return StartCoroutine(FadeBlackScreen(1.0f, 2.0f));
        Debug.Log("LOAD NEXT PART");
        fadingToNext = false; // unreachable
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
