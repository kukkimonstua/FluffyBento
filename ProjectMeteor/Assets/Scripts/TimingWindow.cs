using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimingWindow : MonoBehaviour
{
    public PlayerController player;
    public GUIController gui;
    public Image button;
    public RectTransform shrinker;
    private float shrinkerDiameter;
    private float minShrinkerDiameter;
    private float maxShrinkerDiameter;
    private float shrinkerTrueValue; //current image means 96% of the diameter

    public float goodThreshold = 250;
    public float greatThreshold = 200;
    public float perfectThreshold = 150;
    public float deadThreshold = 125;

    public Text displayText;
    private static string feedbackText;

    public bool gotPressed;
    private bool pressable;
    public bool eventOver;

    public Sprite buttonsNone;
    public Sprite buttonX;
    public Sprite buttonA;
    public Sprite buttonB;
    public Sprite buttonY;

    private bool coloursMatching;
    public Animator eventCutin;

    private AudioSource audioSource;

    /*
    private IEnumerator AnimateCutin(float duration)
    {
        int index;
        float counter = 0.0f;
        while (counter < duration)
        {
            index = Mathf.RoundToInt(counter / duration * cutinFrames.Length);
            if (index >= cutinFrames.Length) index = cutinFrames.Length - 1;

            Debug.Log("show image at index " + index);

            cutinWindow.sprite = cutinFrames[index];
            counter += Time.deltaTime;
            yield return null;
        }
    }
    */

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
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
        eventOver = false;
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

                    /*
                    if (shrinkerTrueValue < perfectThreshold && shrinkerTrueValue > deadThreshold)
                    {
                        Debug.Log("Perfect!");
                    }
                    else if (shrinkerTrueValue < greatThreshold && shrinkerTrueValue > deadThreshold)
                    {
                        Debug.Log("Great!");
                    }
                    else if (shrinkerTrueValue < goodThreshold && shrinkerTrueValue > deadThreshold)
                    {
                        Debug.Log("Good!");
                    }
                    else
                    {
                        Debug.Log("Miss...");
                    }
                    */

                    if (shrinkerTrueValue < deadThreshold * 0.75) TimedPress();
                }
                else
                {
                    if (alphaValue > 0.0f) alphaValue -= 0.05f;
                    FadeTimingWindowUI(alphaValue);
                    
                }
            }
            counter += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.1f); //Delays the one frame setting to ensure the animation starts before it
        while (eventCutin.GetCurrentAnimatorStateInfo(0).IsName("Time_Strike"))
        {
            yield return null;
        }
        gui.FlashWhite();
        yield return new WaitForSeconds(0.2f);

        FadeTimingWindowUI(0.0f);
        StartCoroutine(ShowFeedbackText(false, 1.0f));
        eventOver = true;
    }
    public void TimedPress()
    {
        gotPressed = true;
        if (shrinkerTrueValue < perfectThreshold && shrinkerTrueValue > deadThreshold)
        {
            player.timingGrade = 3;
            feedbackText = "Perfect!";
        }
        else if (shrinkerTrueValue < greatThreshold && shrinkerTrueValue > deadThreshold)
        {
            player.timingGrade = 2;
            feedbackText = "Great!";
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
        if (player.timingGrade > 0 && coloursMatching)
        {
            eventCutin.SetTrigger("cutin");
        }
    }

    public void StartTimingWindow(float duration, int swordID, int meteorID)
    {
        switch (swordID)
        {
            default:
                shrinker.gameObject.GetComponent<RawImage>().color = new Color(255.0f / 255.0f, 145.0f / 255.0f, 177.0f / 255.0f);
                break;
            case 2:
                shrinker.gameObject.GetComponent<RawImage>().color = new Color(255.0f / 255.0f, 255.0f / 255.0f, 145.0f / 255.0f);
                break;
            case 3:
                shrinker.gameObject.GetComponent<RawImage>().color = new Color(145.0f / 255.0f, 242.0f / 255.0f, 255.0f / 255.0f);
                break;
        }
        if (meteorID == 0 || swordID == meteorID)
        {
            coloursMatching = true;
        }
        else
        {
            coloursMatching = false;
        }
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
            yield return new WaitForSeconds(0.5f);
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
