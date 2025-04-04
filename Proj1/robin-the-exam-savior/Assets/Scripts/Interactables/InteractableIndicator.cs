using UnityEngine;
using System.Collections;

public class InteractableIndicator : MonoBehaviour
{
    public SpriteRenderer indicatorSprite;
    public float fadeDuration = 0.1f;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (indicatorSprite != null)
        {
            Color color = indicatorSprite.color;
            color.a = 0;
            indicatorSprite.color = color;
        }
    }

    public void ShowIndicator()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTo(1f, fadeDuration));
    }

    public void HideIndicator()
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTo(0f, fadeDuration));
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = indicatorSprite.color.a;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            Color color = indicatorSprite.color;
            color.a = newAlpha;
            indicatorSprite.color = color;
            yield return null;
        }
        Color finalColor = indicatorSprite.color;
        finalColor.a = targetAlpha;
        indicatorSprite.color = finalColor;
    }
}
