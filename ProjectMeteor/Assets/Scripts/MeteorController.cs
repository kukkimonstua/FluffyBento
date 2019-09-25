using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorController : MonoBehaviour
{
    public Material testColour;
    private Material originalColour;

    public bool withinAttackRange;

    void Start()
    {
        originalColour = GetComponent<MeshRenderer>().material;

        withinAttackRange = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!MeteorManager.meteorsPaused)
        {
            transform.Translate(Vector3.up * -1 * MeteorManager.fallSpeed * Time.deltaTime);
        }

        //TEMP, REMOVE WHEN DONE
        if(withinAttackRange)
        {
            GetComponent<MeshRenderer>().material = testColour;
        }
        else
        {
            GetComponent<MeshRenderer>().material = originalColour;
        }

    }
}
