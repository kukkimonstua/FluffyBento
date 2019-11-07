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

    public ResultsMenu resultsMenu;

    public GameObject topBlackBar;
    public GameObject bottomBlackBar;

    public MeteorDirectionMarker meteorDirectionMarker;
    public Text meteorHeightMarker; //Text for now
    private Vector2 meteorDirectionMarkerOriginalPosition;

    public Text subtitles;
    public SwordEquipIcon swordEquipIcon; //Replace with icon eventually

    float minutes;
    float seconds;
    float milliseconds;

    public List<GameObject> currentMeteors;
    private List<GameObject> minimapMeteors;
    public GameObject minimap;
    public PlayerMinimapMarker playerMarker;
    public GameObject minimapMeteorMarker;

    public void ResetMinimap()
    {
        minimapMeteors = new List<GameObject>();
        currentMeteors = new List<GameObject>();
        foreach (Transform marker in minimap.gameObject.transform)
        {
            if (marker.GetComponent<MeteorMinimapMarker>() != null)
            {
                Destroy(marker.gameObject);
            }
        }
    }
    public void UpdatePlayerMarker(Vector3 playerPosition)
    {
        playerMarker.GetComponent<RectTransform>().anchoredPosition = new Vector2(playerPosition.x / 3.0f, playerPosition.z / 3.0f);
    }
    public void AddMinimapMeteor(GameObject meteor)
    {
        GameObject newMarker = Instantiate(minimapMeteorMarker);
        newMarker.transform.SetParent(minimap.transform, false);
        Debug.Log(meteor.transform.position.x + " and " + meteor.transform.position.z);
        newMarker.GetComponent<RectTransform>().anchoredPosition = new Vector2(meteor.transform.position.x / 3.0f, meteor.transform.position.z / 3.0f);

        minimapMeteors.Add(newMarker);
        currentMeteors.Add(meteor);
        //Debug.Log(minimapMeteors.IndexOf(newMarker));
        //Debug.Log(currentMeteors.IndexOf(meteor));
        //Debug.Log(minimapMeteors.Count);
        //Debug.Log(currentMeteors.Count);
    }
    public void RemoveMinimapMeteor(GameObject meteor)
    {
        int meteorIndex = currentMeteors.IndexOf(meteor);
        //Debug.Log(minimapMeteors.Count);
        //Debug.Log(currentMeteors.Count);

        currentMeteors.Remove(meteor);
        GameObject relatedMarker = minimapMeteors[meteorIndex];
        Destroy(relatedMarker);
        minimapMeteors.Remove(minimapMeteors[meteorIndex]);
    }

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
        
    }

    void Update()
    {
        if(PlayerController.playerState == 1)
        {
            if (currentMeteors.Count != 0)
            {
                foreach (GameObject m in currentMeteors)
                {
                    GameObject relatedMarker = minimapMeteors[currentMeteors.IndexOf(m)];
                    if (m.GetComponent<MeteorController>() != null && relatedMarker.GetComponent<MeteorMinimapMarker>() != null)
                    {
                        relatedMarker.GetComponent<MeteorMinimapMarker>().isLowest = m.GetComponent<MeteorController>().isLowest;
                    }
                    //Debug.Log(m.transform.position.x + ", " + m.transform.position.z);
                }
                //Debug.Log("There are " + currentMeteors.Count);
            }
            else
            {
                Debug.Log("ALL CLEAR" + currentMeteors.Count);
            }

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
            if (targetedSword.GetComponent<SwordController>() != null)
            {
                swordEquipIcon.ShowTargetedSwordType(targetedSword.GetComponent<SwordController>().swordID);
            }
            playerActionText.gameObject.SetActive(true);
        } 
        else
        {
            HidePlayerActionText();
        }
    }
    public void HidePlayerActionText()
    {
        swordEquipIcon.ShowTargetedSwordType(0);
        playerActionText.gameObject.SetActive(false);
        playerActionText.text = "";
    }

    public void ResetGUI()
    {
        TogglePrompt(false, "");
        meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(0.0f);

        resultsMenu.HideResultsMenu();
        fullScreenRed.SetActive(true); //May be redundant in the future
        fullScreenRed.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        fullScreenBlack.SetActive(true); //May be redundant in the future
        fullScreenBlack.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        StartCoroutine(FadeUI(fullScreenBlack, 0.0f, 3.0f));

        swordEquipIcon.UpdateCurrentlyEquipped(0);
        ResetTimer();
        ResetMinimap();
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
        scoreText.text = newValue.ToString("000000");
    }
    public void UpdateEquipmentUI(int swordType)
    {
        swordEquipIcon.UpdateCurrentlyEquipped(swordType);
    }

    public void UpdateMeteorsDestroyed(int newValue)
    {
        meteorCounterText.text = "×" + newValue;
    }
    public void UpdateMeteorDirectionUI(int direction, float distance, Vector3 meteorPosition)
    {
        meteorDirectionMarker.direction = direction;
        meteorDirectionMarker.distance = distance;

        meteorTestClamp.position = meteorPosition + new Vector3(0.0f, 75.0f, 0.0f); //75 is current radius of meteor

        //Vector2 drift = new Vector2(Mathf.Sin(Time.time * 5.0f) * 5.0f, 0.0f);
        //meteorDirectionMarker.GetComponent<RectTransform>().anchoredPosition = meteorDirectionMarkerOriginalPosition + drift;

    }
    public void UpdateMeteorHeightUI(float distance)
    {
        if (distance <= 0.0f)
        {
            meteorHeightMarker.gameObject.SetActive(false);
        }
        else
        {
            meteorHeightMarker.gameObject.SetActive(true);
            meteorHeightMarker.text = (int) distance + "m too far";
        }
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
    public void ShowResults(int resultType, float duration, float delay)
    {
        resultsMenu.ShowResultsMenu(resultType, duration, delay);
        //StartCoroutine(FadeInGameOver(duration, delay));
    }

    public void UpdateSubtitles(string dialogue)
    {
        subtitles.text = dialogue;
    }
}
