using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public void UpdateDisplay(int newHealth)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < newHealth)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                StartCoroutine(AnimateHeartLoss(hearts[i], 0.25f));
                break;
            }
        }
    }

    private IEnumerator AnimateHeartLoss(Image affectedHeart, float halfDuration)
    {
        float counter = 0.0f;
        float imageWidth = affectedHeart.GetComponent<RectTransform>().rect.width;
        float imageHeight = affectedHeart.GetComponent<RectTransform>().rect.height;

        while (counter < halfDuration)
        {
            affectedHeart.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Lerp(imageWidth, 0.0f, counter / halfDuration), imageHeight);
            counter += Time.deltaTime;
            yield return null;
        }
        affectedHeart.sprite = emptyHeart;
        counter = 0.0f;
        while (counter < halfDuration)
        {
            affectedHeart.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Lerp(0.0f, imageWidth, counter / halfDuration), imageHeight);
            counter += Time.deltaTime;
            yield return null;
        }
        affectedHeart.GetComponent<RectTransform>().sizeDelta = new Vector2(imageWidth, imageHeight);
    }
}
