using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueBox;
    //public Button btn;
    //public EventSystem es;
    public Text dialogueName;             //character name
    public Text dialogueText;             //dialogue text
    public Image dialoguePortrait;        //character sprite
    public float delay = 0.001f;
    
    private bool isCurrentlyTyping;
    private string completeText;

    public static DialogueManager instance;
    private void Awake()
    {
        if (instance !=null)
        {
            Debug.LogWarning("Fix This" + gameObject.name);
        }
        else {
            instance = this;
        }
    }


    //FIFO collection (type of list)
    public Queue<DialogueBase.Info> dialogueInfo = new Queue<DialogueBase.Info>();   

    public void EnqueueDialogue(DialogueBase db)
    {
        dialogueBox.SetActive(true);
        dialogueInfo.Clear();     //clear to not carry over older queue data

       // es.SetSelectedGameObject(null);
       // es.SetSelectedGameObject(btn.gameObject);

        foreach(DialogueBase.Info info in db.dialogueInfo)
        {
            dialogueInfo.Enqueue(info);
        }

        DequeueDialogue();
    }

    public void DequeueDialogue()
    {
       if (isCurrentlyTyping == true)
        {
            CompleteText();
            StopAllCoroutines();
            isCurrentlyTyping = false;
            return;
        }

        if(dialogueInfo.Count == 0){
            EndOfDialogue();
            return;
        }

        DialogueBase.Info info = dialogueInfo.Dequeue();
        completeText = info.myText;

        dialogueName.text = info.myName;
        dialogueText.text = info.myText;
        dialoguePortrait.sprite = info.portrait;
        
        dialogueText.text = "";
        StartCoroutine(TypeText(info));
    }

    IEnumerator TypeText(DialogueBase.Info info)
    {
        isCurrentlyTyping = true;     
        foreach (char c in info.myText.ToCharArray())
        {
            yield return new WaitForSeconds(delay);
            dialogueText.text += c;
            yield return null;
        }
        isCurrentlyTyping = false;
    }

    private void CompleteText()
    {
        dialogueText.text = completeText;

    }

    public void EndOfDialogue()
    {
        dialogueBox.SetActive(false);
    }

}
