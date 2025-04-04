using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CircuitPuzzleManager : MonoBehaviour
{
    [Header("Connections")]
    [SerializeField] private CircuitConnection[] connections;
    [SerializeField] private int[] correctSequence;
    
    [Header("Lines")]
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private Color defaultLineColor = Color.white;
    [SerializeField] private Color completedLineColor = Color.yellow;
    
    [Header("Settings")]
    [SerializeField] private float timeoutDuration = 5f;
    [SerializeField] private Transform objectToRotate;
    [SerializeField] private float rotationAmount = -90f;
    [SerializeField] private float rotationDuration = 1f;
    
    [Header("2D Light Settings")]
    [SerializeField] private Light2D puzzleLight2D;
    
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private int currentConnectionIndex = 0;
    private bool isPuzzleActive = false;
    private bool isPuzzleCompleted = false;
    private Coroutine timeoutCoroutine;
    
    private void Start()
    {
        if (connections.Length == 0)
        {
            Debug.LogError("No connections assigned to Circuit Puzzle Manager!");
            return;
        }
        
        for (int i = 0; i < connections.Length; i++)
        {
            connections[i].Initialize(this, i);
            connections[i].SetActive(false);
        }
        
        CreateLineRenderers();
        
        SetPuzzleActive(false);
    }
    
    private void CreateLineRenderers()
    {
        if (correctSequence.Length < 2)
        {
            Debug.LogWarning("Sequence needs at least two connections to create lines!");
            return;
        }
        
        for (int i = 0; i < correctSequence.Length - 1; i++)
        {
            if (correctSequence[i] >= connections.Length || correctSequence[i + 1] >= connections.Length)
            {
                Debug.LogError("Sequence index out of range!");
                continue;
            }
            
            LineRenderer lineRenderer = Instantiate(linePrefab, transform);
            lineRenderer.positionCount = 2;
            
            Vector3 startPos = connections[correctSequence[i]].transform.position;
            Vector3 endPos = connections[correctSequence[i + 1]].transform.position;
            
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            
            lineRenderer.startColor = defaultLineColor;
            lineRenderer.endColor = defaultLineColor;
            
            lineRenderers.Add(lineRenderer);
        }
    }
    
    public void SetPuzzleActive(bool active)
    {
        isPuzzleActive = active;
        
        if (isPuzzleCompleted)
            return;
            
        foreach (CircuitConnection connection in connections)
        {
            connection.SetActive(active);
        }
        
        foreach (LineRenderer line in lineRenderers)
        {
            line.gameObject.SetActive(active);
        }
        
        Cursor.visible = active;
        
        if (active)
        {
            ResetPuzzle();
        }
        else
        {
            if (timeoutCoroutine != null)
            {
                StopCoroutine(timeoutCoroutine);
                timeoutCoroutine = null;
            }
        }
    }
    
    public void ConnectionClicked(CircuitConnection connection)
    {
        if (!isPuzzleActive || isPuzzleCompleted)
            return;
            
        if (connection.connectionIndex == correctSequence[currentConnectionIndex])
        {
            connection.SetCorrect();
            
            if (currentConnectionIndex > 0)
            {
                int lineIndex = currentConnectionIndex - 1;
                if (lineIndex < lineRenderers.Count)
                {
                    lineRenderers[lineIndex].startColor = completedLineColor;
                    lineRenderers[lineIndex].endColor = completedLineColor;
                }
            }
            
            if (timeoutCoroutine != null)
                StopCoroutine(timeoutCoroutine);
                
            timeoutCoroutine = StartCoroutine(TimeoutCoroutine());
            
            currentConnectionIndex++;
            
            if (currentConnectionIndex >= correctSequence.Length)
            {
                CompletePuzzle();
            }
        }
        else
        {
            connection.FlashIncorrect(() => ResetPuzzle());
        }
    }
    
    public void OnClickedOutside()
    {
        if (!isPuzzleActive || isPuzzleCompleted)
            return;
            
        ResetPuzzle();
    }
    
    private void ResetPuzzle()
    {
        currentConnectionIndex = 0;
        
        foreach (CircuitConnection connection in connections)
        {
            connection.ResetColor();
        }
        
        foreach (LineRenderer line in lineRenderers)
        {
            line.startColor = defaultLineColor;
            line.endColor = defaultLineColor;
        }
        
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
    }
    
    private void CompletePuzzle()
    {
        isPuzzleCompleted = true;
        
        if (timeoutCoroutine != null)
        {
            StopCoroutine(timeoutCoroutine);
            timeoutCoroutine = null;
        }
        
        if (objectToRotate != null)
        {
            LeanTween.rotateZ(objectToRotate.gameObject, objectToRotate.eulerAngles.z + rotationAmount, rotationDuration)
                .setEase(LeanTweenType.easeOutQuad);
        }
        
        if (puzzleLight2D != null)
        {
            puzzleLight2D.gameObject.SetActive(true);
        }
    }
    
    private IEnumerator TimeoutCoroutine()
    {
        yield return new WaitForSeconds(timeoutDuration);
        
        ResetPuzzle();
    }
}
