using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorController : MonoBehaviour
{
    public float fallSpeed = 5.0f;
    public Material testColour;
    private bool withinAttackRange;

    void Start()
    {
        withinAttackRange = false;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.up * -fallSpeed * Time.deltaTime);
        if(transform.position.y < 200 && !withinAttackRange)
        {
            withinAttackRange = true;
            GetComponent<MeshRenderer>().material = testColour;
        }
    }
}
