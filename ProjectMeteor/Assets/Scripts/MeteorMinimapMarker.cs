using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeteorMinimapMarker : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isLowest;
    public int type = 0;
    public Sprite[] sprites;
    
    void Start()
    {
        isLowest = false;
        GetComponent<Image>().sprite = sprites[type];
    }

    void Update()
    {
        if (isLowest)
        {
            GetComponent<CanvasRenderer>().SetAlpha(0.8f);
            GetComponent<RectTransform>().localScale = new Vector3((Mathf.Sin(Time.time * 8) / 5) + 0.9f, (Mathf.Sin(Time.time * 8) / 5) + 0.9f, 0.0f);
        }
        else
        {
            GetComponent<CanvasRenderer>().SetAlpha(0.4f);
            GetComponent<RectTransform>().localScale = new Vector3(0.75f, 0.75f, 0.0f);
        }
    }
}
