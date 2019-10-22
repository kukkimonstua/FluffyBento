using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIClamp : MonoBehaviour
{
    public GameObject guiToClamp;

    public bool floatAndFade = false;
    private bool stillFading;

    //private Vector3 startingPosition;

    void Start()
    {
        stillFading = false;
        if (floatAndFade)
        {
            guiToClamp.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        }
    }

    void LateUpdate()
    {
        Vector3 textPos = Camera.main.WorldToScreenPoint(transform.position);
        guiToClamp.transform.position = textPos;

        if (floatAndFade)
        {
            if (guiToClamp.GetComponent<CanvasRenderer>().GetAlpha() > 0.0f)
            {
                stillFading = true;
                transform.position += new Vector3(0.0f, guiToClamp.GetComponent<CanvasRenderer>().GetAlpha() / 10, 0.0f);
            }
            else
            {
                if (stillFading)
                {
                    stillFading = false;
                    //transform.localPosition = startingPosition;
                }
                //startingPosition = transform.localPosition;
            }
        }
    }
}
