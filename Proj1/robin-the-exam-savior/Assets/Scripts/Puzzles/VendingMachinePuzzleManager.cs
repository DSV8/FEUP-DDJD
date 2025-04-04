using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VendingMachinePuzzleManager : MonoBehaviour
{
    [Header("Puzzle Configuration")]
    [SerializeField] private List<PressurePlateColor> correctSequence = new List<PressurePlateColor>();
    [SerializeField] private float timeoutDuration = 5f;

    [Header("References")]
    [SerializeField] private PressurePlate greenPlate;
    [SerializeField] private PressurePlate yellowPlate;
    [SerializeField] private PressurePlate redPlate;
    [SerializeField] private Slider progressSlider;

    [Header("Vending Machine")]
    [SerializeField] private SpriteRenderer vendingMachineRenderer;
    [SerializeField] private GameObject bubbleGumObject;
    
    [Header("Bubble Gum Animation")]
    [SerializeField] private float inflationScale = 1.3f;
    [SerializeField] private float inflationDuration = 0.5f;
    [SerializeField] private float deflationDuration = 0.3f;
    [SerializeField] private int pulseCount = 2;

    private List<PressurePlateColor> currentSequence = new List<PressurePlateColor>();
    private Coroutine timeoutCoroutine;
    private bool puzzleSolved = false;
    private Vector3 originalBubbleGumScale;

    public enum PressurePlateColor
    {
        Green,
        Yellow,
        Red
    }

    private void Start()
    {
        greenPlate.Initialize(PressurePlateColor.Green, this);
        yellowPlate.Initialize(PressurePlateColor.Yellow, this);
        redPlate.Initialize(PressurePlateColor.Red, this);

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            UpdateProgressSliderColor(0f);
        }

        if (bubbleGumObject != null)
        {
            originalBubbleGumScale = bubbleGumObject.transform.localScale;
            bubbleGumObject.SetActive(false);
        }
    }

    public void PlatePressed(PressurePlateColor color)
    {
        if (puzzleSolved)
            return;

        if (currentSequence.Count == 0)
        {
            if (timeoutCoroutine != null)
                StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = StartCoroutine(SequenceTimeoutCoroutine());
        }

        currentSequence.Add(color);

        int currentIndex = currentSequence.Count - 1;
        if (currentIndex < correctSequence.Count && correctSequence[currentIndex] == color)
        {
            GetPlateByColor(color).ShowFeedback(true);

            float progress = (float)currentSequence.Count / correctSequence.Count;
            UpdateProgressSlider(progress);

            if (currentSequence.Count == correctSequence.Count)
            {
                StopAllCoroutines();
                StartCoroutine(PuzzleCompleteCoroutine());
            }
            else
            {
                if (timeoutCoroutine != null)
                    StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = StartCoroutine(SequenceTimeoutCoroutine());
            }
        }
        else
        {
            GetPlateByColor(color).ShowFeedback(false);
            ResetSequence();
        }
    }

    private void ResetSequence()
    {
        currentSequence.Clear();
        UpdateProgressSlider(0);
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
    }

    private IEnumerator SequenceTimeoutCoroutine()
    {
        yield return new WaitForSeconds(timeoutDuration);
        ResetSequence();
    }

    private IEnumerator PuzzleCompleteCoroutine()
    {
        puzzleSolved = true;

        UpdateProgressSlider(1.0f);

        yield return new WaitForSeconds(0.2f);

        if (bubbleGumObject != null)
        {
            bubbleGumObject.SetActive(true);
            
            StartCoroutine(AnimateBubbleGum());
        }

        greenPlate.ResetPlatePosition();
        yellowPlate.ResetPlatePosition();
        redPlate.ResetPlatePosition();
    }
    
    private IEnumerator AnimateBubbleGum()
    {
        bubbleGumObject.transform.localScale = originalBubbleGumScale;
        
        Vector3 inflatedScale = originalBubbleGumScale * inflationScale;
        
        for (int i = 0; i < pulseCount; i++)
        {
            float inflationTimer = 0f;
            while (inflationTimer < inflationDuration)
            {
                inflationTimer += Time.deltaTime;
                float progress = inflationTimer / inflationDuration;
                bubbleGumObject.transform.localScale = Vector3.Lerp(originalBubbleGumScale, inflatedScale, progress);
                yield return null;
            }
            
            bubbleGumObject.transform.localScale = inflatedScale;
            
            float deflationTimer = 0f;
            while (deflationTimer < deflationDuration)
            {
                deflationTimer += Time.deltaTime;
                float progress = deflationTimer / deflationDuration;
                bubbleGumObject.transform.localScale = Vector3.Lerp(inflatedScale, originalBubbleGumScale, progress);
                yield return null;
            }
            
            bubbleGumObject.transform.localScale = originalBubbleGumScale;
            
            if (i < pulseCount - 1)
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    private void UpdateProgressSlider(float value)
    {
        if (progressSlider != null)
        {
            progressSlider.value = value;
            UpdateProgressSliderColor(value);
        }
    }

    private void UpdateProgressSliderColor(float value)
    {
        Image fillImage = progressSlider.fillRect.GetComponent<Image>();
        if (fillImage != null)
        {
            fillImage.color = Color.Lerp(Color.gray, Color.green, value);
        }
    }

    private PressurePlate GetPlateByColor(PressurePlateColor color)
    {
        switch(color)
        {
            case PressurePlateColor.Green: return greenPlate;
            case PressurePlateColor.Yellow: return yellowPlate;
            case PressurePlateColor.Red: return redPlate;
            default: return null;
        }
    }
}