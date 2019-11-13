using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableController : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isBroken = false;
    private BoxCollider myCollider;
    
    [Header("ANIMATION")]
    public Animator crateAnim;

    [Header("SOUND")]
    public AudioClip breakingSound;
    private AudioSource audioSource;

    void Start()
    {
        myCollider = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();
    }

    public void GetBroken()
    {
        if (!isBroken)
        {
            isBroken = true;
            crateAnim.SetBool("isBroken", isBroken);
            myCollider.enabled = false;
            audioSource.PlayOneShot(breakingSound);
        }
    }
    public void GetRestored()
    {
        if (isBroken)
        {
            isBroken = false;
            crateAnim.SetBool("isBroken", isBroken);
            myCollider.enabled = true;
        }
    }
}
