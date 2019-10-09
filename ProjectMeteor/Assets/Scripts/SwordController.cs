using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    private BoxCollider bc;
    private Rigidbody rb;

    public int swordID;
    public GameObject ps1;
    public GameObject ps2;
    public GameObject ps3;
    // Start is called before the first frame update
    void Start()
    {
        bc = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        ps1.SetActive(false);
        ps2.SetActive(false);
        ps3.SetActive(false);

        switch (swordID)
        {
            default:
                ps1.SetActive(true);
                break;
            case 2:
                ps2.SetActive(true);
                break;
            case 3:
                ps3.SetActive(true);
                break;
        }

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
