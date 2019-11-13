using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableController : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isBroken = false;
    private BoxCollider myCollider;
    public Material testmat;
    private Material originalmat;

    [Header("SOUND")]
    public AudioClip breakingSound;
    private AudioSource audioSource;

    void Start()
    {
        originalmat = GetComponent<MeshRenderer>().material;
        myCollider = GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();
    }

    public void GetBroken()
    {
        if (!isBroken)
        {
            isBroken = true;
            GetComponent<MeshRenderer>().material = testmat;
            myCollider.enabled = false;
            //myCollider.center = new Vector3(myCollider.center.x, myCollider.size.y * -0.4f, myCollider.center.z);
            //myCollider.size = new Vector3(myCollider.size.x, myCollider.size.y * 0.1f, myCollider.size.z);
            audioSource.PlayOneShot(breakingSound);
        }
    }
    public void GetRestored()
    {
        if (isBroken)
        {
            isBroken = false;
            GetComponent<MeshRenderer>().material = originalmat;
            myCollider.enabled = true;
            //myCollider.center = new Vector3(myCollider.center.x, 0.0f, myCollider.center.z);
            //myCollider.size = new Vector3(myCollider.size.x, myCollider.size.y * 10.0f, myCollider.size.z);
        }
    }
}
