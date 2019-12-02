using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreditsController : MonoBehaviour
{
    // Start is called before the first frame update
    public RectTransform creditsPanel;
    public float scrollSpeed = 2.0f;
    private float currentScrollSpeed;
    public RectTransform startPosition;
    public RectTransform endPosition;
    public Image background;
    public BGMController bgm;
    public AudioClip creditsBgm;

    private bool fastForward;
    public Text skipPrompt;

    public void StartCredits()
    {
        fastForward = false;
        currentScrollSpeed = scrollSpeed;
        background.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        StartCoroutine(RunCredits());
    }
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            fastForward = true;
        }
    }

    private IEnumerator RunCredits()
    {
        Debug.Log("start");
        bgm.FadeInMusic(creditsBgm, 0.0f);
        StartCoroutine(FadeBackground(1.0f, 2.0f));
        

        creditsPanel.localPosition = new Vector3(creditsPanel.localPosition.x, startPosition.localPosition.y, creditsPanel.localPosition.z);
        while (creditsPanel.localPosition.y < endPosition.localPosition.y + (creditsPanel.sizeDelta.y))
        {
            if (fastForward)
            {
                currentScrollSpeed = scrollSpeed * 10;
                skipPrompt.gameObject.SetActive(false);
            }
            else
            {
                skipPrompt.gameObject.SetActive(true);
            }
            creditsPanel.localPosition = new Vector3(creditsPanel.localPosition.x, creditsPanel.localPosition.y + currentScrollSpeed, creditsPanel.localPosition.z);
            yield return null;
        }
        skipPrompt.gameObject.SetActive(false);

        StartCoroutine(FadeBackground(0.0f, 2.0f));
        bgm.FadeOutMusic(3.0f);
        while (bgm.audioSource.volume > 0)
        {
            yield return null;
        }
        Debug.Log("end");
        CloseCredits();
    }

    private void CloseCredits()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator FadeBackground(float alphaTarget, float duration)
    {
        float alpha = background.GetComponent<CanvasRenderer>().GetAlpha();
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
        {
            background.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Lerp(alpha, alphaTarget, t));
            yield return null;
        }
        background.GetComponent<CanvasRenderer>().SetAlpha(alphaTarget);
    }

}
