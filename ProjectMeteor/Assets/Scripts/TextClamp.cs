using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextClamp : MonoBehaviour
{
    // Start is called before the first frame update
    public Text textToClamp;

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 textPos = Camera.main.WorldToScreenPoint(transform.position);
        textToClamp.transform.position = textPos;
    }
}
