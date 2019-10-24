using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialObject : MonoBehaviour
{
    public GameObject myObject;

    public void SpawnMyObject()
    {
        Instantiate(myObject, transform.position, transform.rotation);
    }
}
