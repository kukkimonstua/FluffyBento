using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorController : MonoBehaviour
{
    public Material testColour;
    private Material originalColour;

    public bool withinAttackRange;
    public bool isLowest;

    void Start()
    {
        originalColour = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material;
        isLowest = false;
        withinAttackRange = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!MeteorManager.meteorsPaused)
        {
            transform.Translate(Vector3.up * -1 * MeteorManager.fallSpeed * Time.deltaTime);
        }
        if(isLowest)
        {
            transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = testColour;
        }
        else
        {
            transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = originalColour;
        }

    }
}
