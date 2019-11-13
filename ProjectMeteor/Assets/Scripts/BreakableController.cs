using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableController : MonoBehaviour
{
    ////explosion
    //public float minForce;
    //public float maxForce;
    //public float radius;

    public Animator crateAnim;

    public bool isBroken = false;
    private BoxCollider myCollider;
    public Material testmat;
    private Material originalmat;

    void Start()
    {
        //originalmat = GetComponent<MeshRenderer>().material;
        myCollider = GetComponent<BoxCollider>();

    }

    public void GetBroken()
    {
        if (!isBroken)
        {
            isBroken = true;
            //GetComponent<MeshRenderer>().material = testmat;
            myCollider.enabled = false;
            crateAnim.SetBool("isBroken", isBroken);
            //Explode();
            //myCollider.center = new Vector3(myCollider.center.x, myCollider.size.y * -0.4f, myCollider.center.z);
            //myCollider.size = new Vector3(myCollider.size.x, myCollider.size.y * 0.1f, myCollider.size.z);
        }
    }
    public void GetRestored()
    {
        if (isBroken)
        {
            isBroken = false;
            //GetComponent<MeshRenderer>().material = originalmat;
            myCollider.enabled = true;
            //myCollider.center = new Vector3(myCollider.center.x, 0.0f, myCollider.center.z);
            //myCollider.size = new Vector3(myCollider.size.x, myCollider.size.y * 10.0f, myCollider.size.z);
        }
    }

    //public void Explode()
    //{
    //    {
    //        foreach (Transform t in transform)
    //        {
    //            var rb = t.GetComponent<Rigidbody>();

    //            if (rb != null)
    //            {
    //                rb.AddExplosionForce(Random.Range(minForce, maxForce), transform.position, radius);
    //            }
    //        }
    //    }
    //}
}
