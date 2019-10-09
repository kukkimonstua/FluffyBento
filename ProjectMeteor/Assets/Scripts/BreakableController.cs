using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableController : MonoBehaviour
{
    // Start is called before the first frame update
    private bool isBroken;
    private BoxCollider myCollider;
    public Material testmat;

    void Start()
    {
        myCollider = GetComponent<BoxCollider>();
        isBroken = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetBroken()
    {
        if (!isBroken)
        {
            isBroken = true;
            GetComponent<MeshRenderer>().material = testmat;
            myCollider.center = new Vector3(myCollider.center.x, myCollider.size.y * -0.4f, myCollider.center.z);
            myCollider.size = new Vector3(myCollider.size.x, myCollider.size.y * 0.1f, myCollider.size.z);
        }
    }
    public void GetRestored()
    {
        if (isBroken)
        {
            isBroken = false;
            GetComponent<MeshRenderer>().material = testmat;
            myCollider.center = new Vector3(myCollider.center.x, 0.0f, myCollider.center.z);
            myCollider.size = new Vector3(myCollider.size.x, myCollider.size.y * 10.0f, myCollider.size.z);
        }
    }
}
