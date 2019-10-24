using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueTestScript : MonoBehaviour
{
    public DialogueBase dialogue;
    public GameObject lastSelect;

    //
    void Start()
    {
        lastSelect = new GameObject();
        TriggerDialogue();
    }

    public void TriggerDialogue()
    {
        DialogueManager.instance.EnqueueDialogue(dialogue);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            TriggerDialogue();
        }

        //reset selection to continue button for non-button clicks
        if (EventSystem.current.currentSelectedGameObject == null)
        {
            EventSystem.current.SetSelectedGameObject(lastSelect);
        }
        else
        {
            lastSelect = EventSystem.current.currentSelectedGameObject;
        }
    }


}
