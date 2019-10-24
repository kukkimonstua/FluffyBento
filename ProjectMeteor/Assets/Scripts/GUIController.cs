using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{
    public Sprite buttonX;
    public Sprite buttonsXA;
    // Start is called before the first frame update
    public Text promptText;
    public Text playerActionText;
    public Text scoreAdditionText;
    private IEnumerator animatedScore;
    public Transform actionTextClamp;
    public Transform scoreTextClamp;
    public Transform meteorTestClamp;
    public Image lowestMeteorMarker;

    public HealthDisplay healthDisplay;
    public Text scoreText;
    public Text meteorCounterText;
    public Text timerText;
    public Slider meteorLandingSlider;
    public Text meteorLandingDanger;
    public Text meteorLandingTimer;

    public GameObject fullScreenBlack;
    public GameObject fullScreenRed;
    public GameObject gameOverMenu;
    public GameObject victoryScreenMenu;
    public static bool menuUnlocked;

    public GameObject topBlackBar;
    public GameObject bottomBlackBar;

    public Text meteorDirectionMarker; //Text for now
    private Vector2 meteorDirectionMarkerOriginalPosition;

    public Text tempEquipText; //Replace with icon eventually

    float minutes;
    float seconds;
    float milliseconds;    

    void ResetTimer()
    {
        minutes = 0;
        seconds = 0;
        milliseconds = 0;
    }
    void Start()
    {
        meteorDirectionMarkerOriginalPosition = meteorDirectionMarker.GetComponent<RectTransform>().anchoredPosition;
        animatedScore = FadeUI(scoreAdditionText.gameObject, 0.0f, 0.0f);
        ResetTimer();
    }

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
    public void AnimateScore(Vector3 floatPosition, int scoreToAdd)
    {
        scoreTextClamp.position = floatPosition;
        scoreAdditionText.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        scoreAdditionText.text = "+" + scoreToAdd;

        //Vector2 originalPosition = scoreAdditionText.GetComponent<RectTransform>().position;
        //scoreAdditionText.GetComponent<RectTransform>().position -= new Vector3(0.0f, 1.0f, 0.0f);

        StopCoroutine(animatedScore);
        animatedScore = FadeUI(scoreAdditionText.gameObject, 0.0f, 1.0f); // create an IEnumerator object
        StartCoroutine(animatedScore);
    }

    public void TogglePrompt(bool toggle, string text)
    {
        promptText.gameObject.SetActive(toggle);
        promptText.text = text;
        if (promptText.gameObject.transform.GetChild(0).GetComponent<Image>() != null)
        {
            promptText.gameObject.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    public void TogglePrompt(bool toggle, string text, string buttonPrompt)
    {
        promptText.gameObject.SetActive(toggle);
        promptText.text = text;
        if (promptText.gameObject.transform.GetChild(0).GetComponent<Image>() != null)
        {
            promptText.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            if (buttonPrompt == "buttonX")
            {
                promptText.gameObject.transform.GetChild(0).GetComponent<Image>().sprite = buttonX;
            }
            if (buttonPrompt == "buttonsXA")
            {
                promptText.gameObject.transform.GetChild(0).GetComponent<Image>().sprite = buttonsXA;
            }
        }
    }

    public void TogglePlayerActionText(GameObject targetedSword, int holdingSword)
    {
        if (targetedSword != null)
        {
            if (holdingSword == 0)
            {
                playerActionText.text = "Equip";
            }
            else
            {
                playerActionText.text = "Swap";
            }
            actionTextClamp.position = targetedSword.transform.position;
            playerActionText.gameObject.SetActive(true);
        } 
        else
        {
            HidePlayerActionText();
        }
    }
    public void HidePlayerActionText()
    {
        playerActionText.gameObject.SetActive(false);
        playerActionText.text = "";
    }

    public void ResetGUI()
    {
        menuUnlocked = false;
        TogglePrompt(false, "");
        meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(0.0f);

        victoryScreenMenu.SetActive(true); //May be redundant in the future
        victoryScreenMenu.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        gameOverMenu.SetActive(true); //May be redundant in the future
        gameOverMenu.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        fullScreenRed.SetActive(true); //May be redundant in the future
        fullScreenRed.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        fullScreenBlack.SetActive(true); //May be redundant in the future
        fullScreenBlack.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        StartCoroutine(FadeUI(fullScreenBlack, 0.0f, 3.0f));

        tempEquipText.text = "EQUIP: -";
        ResetTimer();
    }
    public void FlashRed()
    {
        StartCoroutine(StartFlashRed());
    }
    private IEnumerator StartFlashRed()
    {
        yield return StartCoroutine(FadeUI(fullScreenRed, 1.0f, 0.15f));
        yield return StartCoroutine(FadeUI(fullScreenRed, 0.0f, 0.15f));
        fullScreenRed.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
    }

    public void UpdateHealthUI(int newValue)
    {
        healthDisplay.UpdateDisplay(newValue);
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
    public void UpdateMeteorDirectionUI(int direction, float distance, Vector3 meteorPosition)
    {
        if (direction == 0)
        {
            meteorDirectionMarker.text = "";
        }
        else if (direction < 0)
        {
            meteorDirectionMarker.text = ((int)distance / 2) + "\n<<<";
        }
        else
        {
            meteorDirectionMarker.text = ((int)distance / 2) + "\n>>>";
        }

        Vector2 drift = new Vector2(Mathf.Sin(Time.time * 5.0f) * 5.0f, 0.0f);
        meteorDirectionMarker.GetComponent<RectTransform>().anchoredPosition = meteorDirectionMarkerOriginalPosition + drift;

        meteorTestClamp.position = meteorPosition + new Vector3(0.0f, 75.0f, 0.0f); //75 is current radius of meteor
    }
    public void UpdateMeteorLandingUI(float lowestMeteorPosition, float meteorDeathThreshold, float sliderRange)
    {
        //Debug.Log(lowestMeteorPosition + " is higher than " + meteorDeathThreshold);
        meteorLandingSlider.value = (lowestMeteorPosition - meteorDeathThreshold) / sliderRange;
        if(lowestMeteorPosition < meteorDeathThreshold + (sliderRange / 3) && PlayerController.playerState == 1)
        {
            meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Sin(Time.time * 10.0f) * 0.5f + 0.5f);
            meteorLandingTimer.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
            meteorLandingTimer.text = Mathf.Round((lowestMeteorPosition - meteorDeathThreshold) / MeteorManager.fallSpeed) + "";
        }
        else
        {
            meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
            meteorLandingTimer.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
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
        uiElement.GetComponent<CanvasRenderer>().SetAlpha(alphaTarget);
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
    public void ShowVictoryUI(float duration, float delay)
    {
        StartCoroutine(FadeInVictoryScreen(duration, delay));
    }
    private IEnumerator FadeInVictoryScreen(float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(FadeUI(victoryScreenMenu, 1.0f, duration));
        StartCoroutine(FadeUI(fullScreenBlack, 0.5f, duration));
        yield return new WaitForSeconds(duration);
        menuUnlocked = true;
    }
}
