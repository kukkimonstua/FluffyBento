using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{
    // Start is called before the first frame update
    public Text promptText;

    public Text healthText;
    public Text scoreText;
    public Text meteorCounterText;
    public Text timerText;
    public Slider meteorLandingSlider;
    public Text meteorLandingDanger;

    public GameObject fullScreenBlack;
    public GameObject gameOverMenu;
    public static bool menuUnlocked;

    public GameObject topBlackBar;
    public GameObject bottomBlackBar;

    public Text tempEquipText; //Replace with icon eventually

    float minutes = 0;
    float seconds = 0;
    float milliseconds = 0;    

    void Update()
    {
        if(PlayerController.playerState == 1)
        {
            milliseconds += Time.deltaTime * 100;
            if (milliseconds >= 100)
            {
                seconds++;
                if (seconds >= 60)
                {
                    minutes++;
                    seconds = 0;
                }
                milliseconds = 0;
            }
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, (int)milliseconds);
        }        
    }

    public void TogglePrompt(bool toggle, string text)
    {
        promptText.gameObject.SetActive(toggle);
        promptText.text = text;
    }

    public void ResetGUI()
    {
        menuUnlocked = false;
        
        meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(0.0f);

        gameOverMenu.SetActive(true); //May be redundant in the future
        gameOverMenu.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        fullScreenBlack.SetActive(true); //May be redundant in the future
        fullScreenBlack.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        StartCoroutine(FadeUI(fullScreenBlack, 0.0f, 3.0f));

        tempEquipText.text = "Equipped: None";

    }

    public void UpdateHealthUI(int newValue)
    {
        healthText.text = "Health: " + newValue;
    }
    public void UpdateScoreUI(int newValue)
    {
        scoreText.text = "Score: " + newValue;
    }
    public void UpdateEquipmentUI(string tempString)
    {
        tempEquipText.text = tempString;
    }

    public void UpdateMeteorsDestroyed(int newValue)
    {
        meteorCounterText.text = "Destroyed: " + newValue;
    }

    public void UpdateMeteorLandingUI(float lowestMeteorPosition, float meteorDeathThreshold, float sliderRange)
    {
        //Debug.Log(lowestMeteorPosition + " is higher than " + meteorDeathThreshold);
        meteorLandingSlider.value = (lowestMeteorPosition - meteorDeathThreshold) / sliderRange;
        if(lowestMeteorPosition < meteorDeathThreshold + (sliderRange / 3) && PlayerController.playerState == 1)
        {
            meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Sin(Time.time * 10.0f) * 0.5f + 0.5f);
        }
        else
        {
            meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        }
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

    public void ScaleBlackBars(float heightTarget, float duration)
    {
        StartCoroutine(BlackBarsCoroutine(heightTarget, duration));
    }
    private IEnumerator BlackBarsCoroutine(float heightTarget, float duration)
    {
        float tHeight = topBlackBar.GetComponent<RectTransform>().sizeDelta.y;
        float bHeight = bottomBlackBar.GetComponent<RectTransform>().sizeDelta.y;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
        {
            topBlackBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f, Mathf.Lerp(tHeight, heightTarget, t));
            bottomBlackBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f, Mathf.Lerp(bHeight, heightTarget, t));
            yield return null;
        }
    }
    public void ShowGameOverUI(float duration, float delay)
    {
        StartCoroutine(FadeInGameOver(duration, delay));
    }
    private IEnumerator FadeInGameOver(float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(FadeUI(gameOverMenu, 1.0f, duration));
        StartCoroutine(FadeUI(fullScreenBlack, 1.0f, duration));
        yield return new WaitForSeconds(duration);
        menuUnlocked = true;
    }
}
