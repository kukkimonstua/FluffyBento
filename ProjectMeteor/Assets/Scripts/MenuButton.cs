using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuButton : MonoBehaviour
{
	[SerializeField] MenuButtonController menuButtonController;
	private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    void PlaySFX(AudioClip whichSound)
    {
        menuButtonController.audioSource.PlayOneShot(whichSound);
    }

    public void IsSelected(bool state)
    {
        anim.SetBool("selected", state);
    }

    public void IsPressed(bool state)
    {
        anim.SetBool("pressed", state);
    }
}
