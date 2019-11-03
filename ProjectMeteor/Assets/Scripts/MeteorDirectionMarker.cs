using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeteorDirectionMarker : MonoBehaviour
{
    // Start is called before the first frame update
    private Image[] arrows;
    public Sprite leftArrow;
    public Sprite rightArrow;
    public float flashLoopDuration = 1.0f;
    private float flashLoopTimer;
    public int direction;
    public float distance;

    void Start()
    {
        flashLoopTimer = 0.0f;
        arrows = GetComponentsInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (distance > 60.0f)
        {
            arrows[0].gameObject.SetActive(true);
            arrows[1].gameObject.SetActive(true);
            arrows[2].gameObject.SetActive(true);
            //arrows[0].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-20.0f, 15.0f, 0.0f);
            //arrows[1].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 15.0f, 0.0f);
            //arrows[2].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(20.0f, 15.0f, 0.0f);
        }
        else if (distance > 30.0f)
        {
            arrows[0].gameObject.SetActive(true);
            arrows[1].gameObject.SetActive(true);
            arrows[2].gameObject.SetActive(false);
            //arrows[0].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(-10.0f, 15.0f, 0.0f);
            //arrows[1].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(10.0f, 15.0f, 0.0f);
        }
        else if (distance > 10.0f)
        {
            arrows[0].gameObject.SetActive(true);
            arrows[1].gameObject.SetActive(false);
            arrows[2].gameObject.SetActive(false);
            //arrows[1].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 15.0f, 0.0f);
        }
        else
        {
            arrows[0].gameObject.SetActive(false);
            arrows[1].gameObject.SetActive(false);
            arrows[2].gameObject.SetActive(false);
        }

        flashLoopTimer += Time.deltaTime;
        if (flashLoopTimer > flashLoopDuration * 0.75f)
        {
            arrows[0].GetComponent<CanvasRenderer>().SetAlpha(1.0f);
            arrows[1].GetComponent<CanvasRenderer>().SetAlpha(0.5f);
            arrows[2].GetComponent<CanvasRenderer>().SetAlpha(0.5f);
        }
        else if (flashLoopTimer > flashLoopDuration * 0.5f)
        {
            arrows[0].GetComponent<CanvasRenderer>().SetAlpha(0.5f);
            arrows[1].GetComponent<CanvasRenderer>().SetAlpha(1.0f);
            arrows[2].GetComponent<CanvasRenderer>().SetAlpha(0.5f);
        }
        else if (flashLoopTimer > flashLoopDuration * 0.25f)
        {
            arrows[0].GetComponent<CanvasRenderer>().SetAlpha(0.5f);
            arrows[1].GetComponent<CanvasRenderer>().SetAlpha(0.5f);
            arrows[2].GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        }
        else
        {
            arrows[0].GetComponent<CanvasRenderer>().SetAlpha(0.5f);
            arrows[1].GetComponent<CanvasRenderer>().SetAlpha(0.5f);
            arrows[2].GetComponent<CanvasRenderer>().SetAlpha(0.5f);
        }
        if (direction > 0)
        {
            GetComponent<RectTransform>().localScale = new Vector3(1.0f, 1.0f, 1.0f);
            foreach (Image arrow in arrows)
            {
                //arrow.sprite = rightArrow;
            }            
        }
        else
        {
            GetComponent<RectTransform>().localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }
        GetComponent<Text>().text = "";
        /*
        if (direction == 0)
        {
            GetComponent<Text>().text = "";
        }
        else
        {
            if (distance < 30)
            {
                GetComponent<Text>().text = "";
            }
            else
            {
                GetComponent<Text>().text = (int)(distance * 1.0f) + "m";
            }
        }
        */

        if (flashLoopTimer >= flashLoopDuration)
        {
            flashLoopTimer = 0.0f;
        }
    }
}
