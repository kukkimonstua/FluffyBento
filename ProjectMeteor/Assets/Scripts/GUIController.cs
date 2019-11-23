using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{
    [Header("LINKS TO 3D GUI")]
    public Transform actionTextClamp;
    public Transform scoreTextClamp;
    public Transform meteorReticleClamp;

    [Header("CLAMPS TO 3D GUI")]
    public Text playerActionText;
    public Text scoreAdditionText;
    private IEnumerator animatedScore;
    public Image lowestMeteorMarker;

    [Header("MINIMAP")]
    public GameObject minimap;
    public PlayerMinimapMarker playerMarker;
    public List<GameObject> currentMeteors;
    private List<GameObject> minimapMeteors;
    public GameObject minimapMeteorMarker;
    public Text meteorLandingDanger;
    public Text meteorLandingTimer;

    [Header("OTHER GUI")]
    public HealthDisplay healthDisplay;
    public Text scoreText;
    public SwordEquipIcon swordEquipIcon;
    public MeteorDirectionMarker meteorDirectionMarker;
    public Text meteorHeightMarker;
    public Text timerText;
    public Text meteorCounterText;

    [Header("SCREEN EFFECTS")]
    public GameObject fullScreenBlack;
    public GameObject fullScreenRed;
    public GameObject fullScreenWhite;
    private IEnumerator fadeCoroutine;
    public Image blaze;
    public GameObject topBlackBar;
    public GameObject bottomBlackBar;

    [Header("ALERTS")]
    public Text promptText;
    public Text subtitles;
    public ResultsMenu resultsMenu;

    [Header("SPRITES")]
    public Sprite buttonX;
    public Sprite buttonsXA;

    float minutes;
    float seconds;
    float milliseconds;

    private float minFogValue;
    private float maxFogValue;

    void ResetTimer()
    {
        minutes = 0;
        seconds = 0;
        milliseconds = 0;
    }
    private void UpdateTimer()
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

    void Start()
    {
        RenderSettings.fog = true;
        minFogValue = RenderSettings.fogDensity;
        maxFogValue = minFogValue + 0.01f;
    }
    void Update()
    {
        if (PlayerController.playerState == PlayerController.ACTIVELY_PLAYING)
        {
            UpdateMinimap();
            //Track time if the timer is even visible (AKA Survival Mode)
            if (timerText.gameObject.activeSelf)
            {
                UpdateTimer();
            }
        }
    }
    #region MINIMAP
    private void UpdateMinimap()
    {
        if (currentMeteors.Count > 0)
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
    }
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
        playerMarker.GetComponent<RectTransform>().anchoredPosition = new Vector2(playerPosition.x / 4.0f, playerPosition.z / 4.0f);
        playerMarker.GetComponent<RectTransform>().localScale = new Vector3((Mathf.Sin(Time.time * 8) / 5) + 0.9f, 1.0f, 1.0f);
    }
    public void AddMinimapMeteor(GameObject meteor, int type)
    {
        GameObject newMarker = Instantiate(minimapMeteorMarker);
        if (newMarker.GetComponent<MeteorMinimapMarker>() != null)
        {
            newMarker.GetComponent<MeteorMinimapMarker>().type = type;
        }
        newMarker.transform.SetParent(minimap.transform, false);
        //Debug.Log(meteor.transform.position.x + " and " + meteor.transform.position.z);
        newMarker.GetComponent<RectTransform>().anchoredPosition = new Vector2(meteor.transform.position.x / 4.0f, meteor.transform.position.z / 4.0f);

        minimapMeteors.Add(newMarker);
        currentMeteors.Add(meteor);
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
    #endregion

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

    #region CLAMPED UI FUNCTIONS
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
    #endregion

    public void ResetGUI()
    {
        TogglePrompt(false, "");
        meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(0.0f);

        resultsMenu.HideResultsMenu();
        fullScreenWhite.SetActive(true); //May be redundant in the future
        fullScreenWhite.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        fullScreenRed.SetActive(true); //May be redundant in the future
        fullScreenRed.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        fullScreenBlack.SetActive(true); //May be redundant in the future
        fullScreenBlack.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        FadeIntoBlack(0.0f, 3.0f);

        swordEquipIcon.UpdateCurrentlyEquipped(0);
        ResetTimer();
        ResetMinimap();

        animatedScore = FadeUI(scoreAdditionText.gameObject, 0.0f, 0.0f);
        if (PlayerController.currentLevel != 0) //i.e. NOT in survival mode
        {
            meteorCounterText.gameObject.SetActive(false);
            timerText.gameObject.SetActive(false);
        }
        else
        {
            blaze.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
            meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
            meteorLandingTimer.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        }
    }

    #region FULLSCREEN EFFECTS
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
    public void FadeIntoBlack(float target, float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = FadeUI(fullScreenBlack, target, duration);
        StartCoroutine(fadeCoroutine);
    }
    private IEnumerator StartFlashWhiteToDeath(float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(FadeUI(fullScreenWhite, 1.0f, 0.2f));
        fullScreenBlack.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        yield return StartCoroutine(FadeUI(fullScreenWhite, 0.0f, 0.8f));
        fullScreenWhite.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
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
        topBlackBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f, heightTarget);
        bottomBlackBar.GetComponent<RectTransform>().sizeDelta = new Vector2(0.0f, heightTarget);
    }
    public void ToggleNonTimingWindowGUI(bool state)
    {
        subtitles.gameObject.SetActive(state);
        minimap.SetActive(state);
        meteorDirectionMarker.gameObject.SetActive(state);
        healthDisplay.gameObject.SetActive(state);
        scoreText.gameObject.SetActive(state);

        if (PlayerController.currentLevel == 0) //i.e. only in survival mode
        {
            meteorCounterText.gameObject.SetActive(state);
            timerText.gameObject.SetActive(state);
        }
    }
    #endregion

    #region GUI UPDATE ON EVENT
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
    public void UpdateSubtitles(string dialogue)
    {
        subtitles.text = dialogue;
    }
    #endregion

    #region GUI UPDATE PER FRAME
    public void UpdateMeteorDirectionUI(int direction, float distance, Vector3 meteorPosition)
    {
        meteorDirectionMarker.direction = direction;
        meteorDirectionMarker.distance = distance;

        meteorReticleClamp.position = meteorPosition + new Vector3(0.0f, 50.0f, 0.0f); //50 is current radius of meteor
        lowestMeteorMarker.GetComponent<RectTransform>().Rotate(new Vector3(0, 0, 60.0f * Time.deltaTime));
    }
    public void UpdateMeteorHeightUI(float distance, int holdingSword)
    {
        if (distance <= 0.0f)
        {
            meteorHeightMarker.gameObject.SetActive(false);
        }
        else
        {
            meteorHeightMarker.gameObject.SetActive(true);
            meteorHeightMarker.text = (int) distance + "m too far";
            if (holdingSword == 0)
            {
                meteorHeightMarker.text += "\nYou need a sword!";
            }
        }
    }
    public void UpdateMeteorLandingUI(float lowestMeteorPosition, float meteorDeathThreshold)
    {
        //Debug.Log(lowestMeteorPosition + " is higher than " + meteorDeathThreshold);
        float lerpValue = 1.0f - (lowestMeteorPosition - meteorDeathThreshold) / ((PlayerController.worldHeight - meteorDeathThreshold) / 3);

        if (PlayerController.playerState == PlayerController.ACTIVELY_PLAYING)
        {
            if (lowestMeteorPosition < meteorDeathThreshold + ((PlayerController.worldHeight - meteorDeathThreshold) / 3))
            {
                meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(Mathf.Sin(Time.time * 10.0f) * 0.5f + 0.5f);
                meteorLandingTimer.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
                meteorLandingTimer.text = Mathf.Round((lowestMeteorPosition - meteorDeathThreshold) / MeteorManager.fallSpeed) + "";

                blaze.GetComponent<CanvasRenderer>().SetAlpha(lerpValue/2 + Mathf.Sin(Time.time * 4.0f) * (lerpValue/4));
                RenderSettings.fogDensity = Mathf.Lerp(minFogValue, maxFogValue, lerpValue);
                //audio source volume uses lerpValue too
            }
            else
            {
                meteorLandingDanger.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
                meteorLandingTimer.GetComponent<CanvasRenderer>().SetAlpha(0.0f);

                blaze.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
                RenderSettings.fogDensity = minFogValue;

            }
        }
    }
    #endregion

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

    public void ShowResults(int currentScore, int resultType, float duration, float delay)
    {
        blaze.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        if (resultType == ResultsMenu.METEOR_DEATH)
        {
            StartCoroutine(StartFlashWhiteToDeath(delay));
        }
        resultsMenu.ShowResultsMenu(currentScore, resultType, duration, delay);
    }
}
