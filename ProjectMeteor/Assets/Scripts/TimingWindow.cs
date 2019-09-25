using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimingWindow : MonoBehaviour
{
    // Start is called before the first frame update
    public RectTransform shrinker;
    private static float shrinkerDiameter;
    private static float minShrinkerDiameter;
    private static float maxShrinkerDiameter;

    public Text displayText;
    private static string feedbackText;

    public static bool gotPressed;

    void Start()
    {
        shrinkerDiameter = 50.0f;
        gotPressed = false;
        minShrinkerDiameter = 50.0f;
        maxShrinkerDiameter = 500.0f;
        feedbackText = "";
    }

    // Update is called once per frame
    void Update()
    {
        displayText.text = feedbackText;
        shrinker.sizeDelta = new Vector2(shrinkerDiameter, shrinkerDiameter);
    }

    private IEnumerator TimingWindowCoroutine()
    {
        gotPressed = false;

        float counter = 0.0f;
        float counterOffset = Random.Range(0.0f, 1.5f);
        float duration = 3.0f;

        feedbackText = "Ready...";

        while (counter < duration)
        {
            if (!gotPressed)
            {
                if (counter > counterOffset)
                {
                    feedbackText = "";
                    shrinkerDiameter = Mathf.Lerp(maxShrinkerDiameter, minShrinkerDiameter, (counter - counterOffset) / (duration - counterOffset));
                }
            }
            else
            {
                feedbackText = "Good!"; //no matter what lol
            }
            counter += Time.deltaTime;
            yield return null;
        }
        ToggleTimingWindowUI(false);
    }

    public void StartTimingWindow()
    {
        ToggleTimingWindowUI(true);
        StartCoroutine(TimingWindowCoroutine());
    }

    private void ToggleTimingWindowUI(bool setActiveState)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).gameObject;
            if (child != null)
                child.SetActive(setActiveState);
        }
    }
}
