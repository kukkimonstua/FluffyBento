using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextClamp : MonoBehaviour
{
    public Text textToClamp;

    public bool floatAndFade = false;
    private bool fading;

    private Vector3 startingPosition;

    void Start()
    {
        fading = false;
        if (floatAndFade)
        {
            textToClamp.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        }
    }

    void LateUpdate()
    {
        Vector3 textPos = Camera.main.WorldToScreenPoint(transform.position);
        textToClamp.transform.position = textPos;

        if (floatAndFade)
        {
            if (textToClamp.GetComponent<CanvasRenderer>().GetAlpha() > 0.0f)
            {
                fading = true;
                transform.localPosition = startingPosition + new Vector3(0.0f, 1.0f - textToClamp.GetComponent<CanvasRenderer>().GetAlpha(), 0.0f);
            }
            else
            {
                if (fading)
                {
                    fading = false;
                    transform.localPosition = startingPosition;
                }
                startingPosition = transform.localPosition;
            }
        }
    }
}
