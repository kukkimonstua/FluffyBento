using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformAttach : MonoBehaviour
{
    //public GameObject Player;

    private void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>() != null)
        {
            other.transform.parent = transform;
        }
    }

    private void OnTriggerExit (Collider other)
    {
        if (other.gameObject.GetComponent<PlayerController>() != null)
        {
            other.transform.parent = null;
        }
    }
}
