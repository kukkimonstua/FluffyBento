using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorController : MonoBehaviour
{
    public float fallSpeed = 5.0f;
    public Material testColour;
    private Material originalColour;

    private bool withinAttackRange;

    void Start()
    {
        originalColour = GetComponent<MeshRenderer>().material;

        withinAttackRange = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!MeteorManager.MeteorsPaused())
        {
            transform.Translate(Vector3.up * -fallSpeed * Time.deltaTime);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (!withinAttackRange)
        {
            withinAttackRange = true;
            GetComponent<MeshRenderer>().material = testColour;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (withinAttackRange)
        {
            withinAttackRange = false;
            GetComponent<MeshRenderer>().material = originalColour;
        }
    }

}
