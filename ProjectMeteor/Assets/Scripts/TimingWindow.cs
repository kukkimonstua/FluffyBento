using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimingWindow : MonoBehaviour
{
    // Start is called before the first frame update
    public PlayerController player;

    public RectTransform shrinker;
    private static float shrinkerDiameter;
    private static float minShrinkerDiameter;
    private static float maxShrinkerDiameter;

    public Text displayText;
    private static string feedbackText;

    public static bool gotPressed;
    private bool pressable;

    void Start()
    {
        shrinkerDiameter = 50.0f;
        gotPressed = false;
        pressable = false;
        minShrinkerDiameter = 50.0f;
        maxShrinkerDiameter = 500.0f;
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

    private IEnumerator TimingWindowCoroutine(float duration)
    {
        float counter = 0.0f;
        float counterOffset = Random.Range(1.0f, 2.2f);
        shrinkerDiameter = 50.0f;
        feedbackText = "Ready...";

        float alphaValue = 0.0f;
        FadeTimingWindowUI(alphaValue);
        StartCoroutine(ShowFeedbackText(true));

        pressable = false;
        while (counter < duration)
        {
            if (counter > counterOffset && !pressable)
            {
                gotPressed = false;
                pressable = true;
            }
            if (pressable)
            {
                if (!gotPressed)
                {
                    if (alphaValue < 1.0f) alphaValue += 0.05f;
                    FadeTimingWindowUI(alphaValue);
                    feedbackText = "";
                    shrinkerDiameter = Mathf.Lerp(maxShrinkerDiameter, minShrinkerDiameter, (counter - counterOffset) / (duration - counterOffset));
                    if (shrinkerDiameter < 125) gotPressed = true;
                }
                else
                {
                    if (alphaValue > 0.0f) alphaValue -= 0.05f;
                    FadeTimingWindowUI(alphaValue);
                    if (shrinkerDiameter < 150 && shrinkerDiameter > 125)
                    {
                        player.timingGrade = 2;
                        feedbackText = "Excellent!";
                    }
                    else if (shrinkerDiameter < 200 && shrinkerDiameter > 125)
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
            counter += Time.deltaTime;
            yield return null;
        }
        Debug.Log(feedbackText + ": " + shrinkerDiameter);
        player.AddScore(player.timingGrade * 100);

        FadeTimingWindowUI(0.0f);
        StartCoroutine(ShowFeedbackText(false));

    }



    public void StartTimingWindow(float duration)
    {
        FadeTimingWindowUI(1.0f);
        StartCoroutine(TimingWindowCoroutine(duration));
    }

    private IEnumerator ShowFeedbackText(bool state)
    {
        if (state)
        {
            float alphaValue = 0.0f;
            while (alphaValue < 1.0f)
            {
                alphaValue += 0.02f;
                displayText.GetComponent<CanvasRenderer>().SetAlpha(alphaValue);
                yield return null;
            }
        }
        else
        {
            float alphaValue = 1.0f;
            while (alphaValue > 0.0f)
            {
                alphaValue -= 0.02f;
                displayText.GetComponent<CanvasRenderer>().SetAlpha(alphaValue);
                yield return null;
            }
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
