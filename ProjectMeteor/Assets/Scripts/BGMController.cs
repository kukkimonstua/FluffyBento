using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMController : MonoBehaviour
{

    public AudioSource audioSource;
    private IEnumerator fadeCoroutine;

    public void FadeInMusic(AudioClip clip, float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            audioSource.volume = 0.0f;
        }
        if (clip != null)
        {
            audioSource.Stop();
            audioSource.clip = clip;
            audioSource.Play();
        }
        fadeCoroutine = FadeVolume(0.3f, duration);
        StartCoroutine(fadeCoroutine);
    }
    public void FadeOutMusic(float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = FadeVolume(0.0f, duration);
        StartCoroutine(fadeCoroutine);
    }

    public void SwitchMusic(AudioClip clip, float duration)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = SwitchClipAfterFade(clip, duration);
        StartCoroutine(fadeCoroutine);
    }

    private IEnumerator FadeVolume(float targetVolume, float duration)
    {
        float currentVolume = audioSource.volume;
        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration)
        {
            audioSource.volume = Mathf.Lerp(currentVolume, targetVolume, t);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    private IEnumerator SwitchClipAfterFade(AudioClip clip, float duration)
    {
        yield return FadeVolume(0.0f, duration);
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.volume = 1.0f;
        audioSource.Play();
    }
}
