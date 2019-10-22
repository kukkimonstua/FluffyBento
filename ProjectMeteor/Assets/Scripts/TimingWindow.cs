using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimingWindow : MonoBehaviour
{
    public PlayerController player;
    public Image button;
    public RectTransform shrinker;
    private float shrinkerDiameter;
    private float minShrinkerDiameter;
    private float maxShrinkerDiameter;
    private float shrinkerTrueValue; //current image means 96% of the diameter

    public float goodThreshold = 200;
    public float sweetThreshold = 150;
    public float deadThreshold = 125;

    public Text displayText;
    private static string feedbackText;

    public static bool gotPressed;
    private bool pressable;

    public Sprite buttonsNone;
    public Sprite buttonX;
    public Sprite buttonA;
    public Sprite buttonB;
    public Sprite buttonY;

    void Start()
    {
        gotPressed = false;
        pressable = false;
        minShrinkerDiameter = 50.0f;
        maxShrinkerDiameter = 500.0f;
        shrinkerDiameter = maxShrinkerDiameter;
        feedbackText = "";
        FadeTimingWindowUI(0.0f);
        displayText.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
    }

    // Update is called once per frame
    void Update()
    {
        displayText.text = feedbackText;
        shrinker.sizeDelta = new Vector2(shrinkerDiameter, shrinkerDiameter);
    }

    private IEnumerator TimingWindowCoroutine(float duration) //Currently, 3.0 secs for whole sequence
    {
        float counter = 0.0f;
        float counterOffset = Random.Range(duration * 0.33f, duration * 0.66f);
        shrinkerDiameter = maxShrinkerDiameter;
        button.sprite = buttonsNone;
        feedbackText = "Ready...";

        float alphaValue = 0.2f;
        FadeTimingWindowUI(alphaValue);
        StartCoroutine(ShowFeedbackText(true, 1.0f));

        //int buttonRouletteIndex = 0;
        //float buttonRouletteTimer = 0.0f;

        pressable = false;
        while (counter < duration)
        {
            if (counter > counterOffset && !pressable)
            {
                gotPressed = false;
                pressable = true;
                button.sprite = buttonX;
            }
            if (pressable)
            {
                if (!gotPressed)
                {
                    if (alphaValue < 1.0f) alphaValue += 0.05f;
                    FadeTimingWindowUI(alphaValue);
                    feedbackText = "";
                    shrinkerDiameter = Mathf.Lerp(maxShrinkerDiameter, minShrinkerDiameter, (counter - counterOffset) / (duration - counterOffset));
                    shrinkerTrueValue = shrinkerDiameter * 0.96f;

                    if (shrinkerTrueValue < deadThreshold * 0.75) gotPressed = true;
                }
                else
                {
                    if (alphaValue > 0.0f) alphaValue -= 0.05f;
                    FadeTimingWindowUI(alphaValue);
                    if (shrinkerTrueValue < sweetThreshold && shrinkerTrueValue > deadThreshold)
                    {
                        player.timingGrade = 2;
                        feedbackText = "Excellent!";
                    }
                    else if (shrinkerTrueValue < goodThreshold && shrinkerTrueValue > deadThreshold)
                    {
                        player.timingGrade = 1;
                        feedbackText = "Good!";
                    }
                    else
                    {
                        player.timingGrade = 0;
                        feedbackText = "Miss...";
                    }                    
                }
            }
            else
            {
                /*
                buttonRouletteTimer += Time.deltaTime;
                if (buttonRouletteTimer > 0.1f) {
                    buttonRouletteTimer = 0.0f;
                    buttonRouletteIndex++;
                }
                if (buttonRouletteIndex > 3)
                {
                    buttonRouletteIndex = 0;
                }
                switch(buttonRouletteIndex)
                {
                    case 0:
                        button.sprite = buttonX;
                        break;
                    case 1:
                        button.sprite = buttonA;
                        break;
                    case 2:
                        button.sprite = buttonB;
                        break;
                    case 3:
                        button.sprite = buttonY;
                        break;
                }
                */
            }
            counter += Time.deltaTime;
            yield return null;
        }
        //Debug.Log(feedbackText + ": " + shrinkerDiameter);

        FadeTimingWindowUI(0.0f);
        StartCoroutine(ShowFeedbackText(false, 1.0f));
    }

    public void StartTimingWindow(float duration)
    {
        FadeTimingWindowUI(1.0f);
        StartCoroutine(TimingWindowCoroutine(duration));
    }

    private IEnumerator ShowFeedbackText(bool state, float duration)
    {
        Vector3 startingPosition = displayText.transform.localPosition;
        if (state)
        {
            float startAlpha = 0.0f;
            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
            {
                float alphaValue = Mathf.Lerp(startAlpha, 1.0f, t);
                displayText.GetComponent<CanvasRenderer>().SetAlpha(alphaValue);
                displayText.transform.localPosition = startingPosition + new Vector3(0.0f, (1.0f - alphaValue) * 20, 0.0f);
                //displayText.rectTransform.localScale = new Vector3(1.0f + alphaValue / 2, 1.0f + alphaValue / 2, 1.0f + alphaValue / 2);
                yield return null;
            }
            displayText.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        }
        else
        {
            float startAlpha = 1.0f;
            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
            {
                float alphaValue = Mathf.Lerp(startAlpha, 0.0f, t);
                displayText.GetComponent<CanvasRenderer>().SetAlpha(alphaValue);
                displayText.transform.localPosition = startingPosition + new Vector3(0.0f, (alphaValue - 1.0f) * 20, 0.0f);
                //displayText.rectTransform.localScale = new Vector3(1.0f + alphaValue / 2, 1.0f + alphaValue / 2, 1.0f + alphaValue / 2);
                yield return null;
            }
            displayText.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        }
        displayText.transform.localPosition = startingPosition;
    }
    private IEnumerator FadeUI(GameObject uiElement, float alphaTarget, float duration)
    {
        float alpha = uiElement.GetComponent<CanvasRenderer>().GetAlpha();
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
        {
            uiElement.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Lerp(alpha, alphaTarget, t));
            yield return null;
        }
    }

    private void FadeTimingWindowUI(float alphaValue)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).gameObject;
            if (child != null)
                child.GetComponent<CanvasRenderer>().SetAlpha(alphaValue);
        }
    }
}
