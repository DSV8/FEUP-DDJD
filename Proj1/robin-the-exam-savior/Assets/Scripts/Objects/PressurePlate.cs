using System.Collections;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer plateRenderer;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color correctColor = Color.white;
    [SerializeField] private Color incorrectColor = Color.black;
    [SerializeField] private float feedbackDuration = 0.5f;
    [SerializeField] private float pressedYOffset = -0.1f;
    
    private VendingMachinePuzzleManager puzzleManager;
    private VendingMachinePuzzleManager.PressurePlateColor plateColor;
    private bool isPressed = false;
    private Vector3 originalPosition;
    private Coroutine feedbackCoroutine;
    
    private void Awake()
    {
        originalPosition = transform.position;
        
        if (plateRenderer == null)
        {
            plateRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (plateRenderer != null)
        {
            plateRenderer.color = defaultColor;
        }
    }
    
    public void Initialize(VendingMachinePuzzleManager.PressurePlateColor color, VendingMachinePuzzleManager manager)
    {
        plateColor = color;
        puzzleManager = manager;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPressed)
        {
            isPressed = true;
            transform.position = new Vector3(transform.position.x, originalPosition.y + pressedYOffset, transform.position.z);
            puzzleManager.PlatePressed(plateColor);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isPressed)
        {
            isPressed = false;
            transform.position = originalPosition;
        }
    }
    
    public void ResetPlatePosition()
    {
        isPressed = false;
        transform.position = originalPosition;
    }
    
    public void ShowFeedback(bool isCorrect)
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(FeedbackCoroutine(isCorrect));
    }
    
    private IEnumerator FeedbackCoroutine(bool isCorrect)
    {
        if (plateRenderer != null)
        {
            plateRenderer.color = isCorrect ? correctColor : incorrectColor;
            
            yield return new WaitForSeconds(feedbackDuration);
            
            plateRenderer.color = defaultColor;
        }
    }
}