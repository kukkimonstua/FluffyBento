using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{

    private float fallSpeed;
    // Start is called before the first frame update
    void Start()
    {
        fallSpeed = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position += Vector3.up * Physics2D.gravity.y * Time.deltaTime * fallSpeed;

    }

}
