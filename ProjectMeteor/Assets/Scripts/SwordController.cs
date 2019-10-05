using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    private BoxCollider bc;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        bc = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        //transform.position += Vector3.up * Physics2D.gravity.y * Time.deltaTime * fallSpeed;

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            Destroy(bc);
            Destroy(rb);
        }
    }

}
