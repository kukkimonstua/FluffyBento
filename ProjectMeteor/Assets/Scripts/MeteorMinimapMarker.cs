using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeteorMinimapMarker : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isLowest;
    public int type = 0;

    void Start()
    {
        isLowest = false;
        switch (type)
        {
            case 3:
                GetComponent<Image>().color = new Color(145.0f / 255.0f, 242.0f / 255.0f, 255.0f / 255.0f);
                break;
            case 2:
                GetComponent<Image>().color = new Color(255.0f / 255.0f, 255.0f / 255.0f, 145.0f / 255.0f);
                break;
            case 1:
                GetComponent<Image>().color = new Color(255.0f / 255.0f, 145.0f / 255.0f, 177.0f / 255.0f);
                break;
            default:
                GetComponent<Image>().color = new Color(230.0f / 255.0f, 160.0f / 255.0f, 0.0f / 255.0f);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLowest)
        {
            GetComponent<CanvasRenderer>().SetAlpha(0.9f);
            GetComponent<RectTransform>().localScale = new Vector3((Mathf.Sin(Time.time * 8) / 5) + 0.9f, (Mathf.Sin(Time.time * 8) / 5) + 0.9f, 0.0f);
        }
        else
        {
            GetComponent<CanvasRenderer>().SetAlpha(0.5f);
            GetComponent<RectTransform>().localScale = new Vector3(0.75f, 0.75f, 0.0f);
        }
    }
}
