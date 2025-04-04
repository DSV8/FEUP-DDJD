using UnityEngine;

public class PlayerInteractionController : MonoBehaviour
{
    private GameObject currentInteractable;

    [Header("Throwable Settings")]
    [SerializeField] private MovementController movementController;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float maxThrowForce = 20f;
    [SerializeField] private float throwForceMultiplier = 1f;
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private float minLineWidth = 0.05f;
    [SerializeField] private float maxLineWidth = 0.2f;

    private bool isAiming = false;
    private float currentThrowForce = 0f;
    private Throwable heldThrowable = null;

    public bool isHidden = false;
    private int originalSortingOrder;
    private SpriteRenderer playerSprite;

    public bool isOnRope = false;
    private bool ropeIgnoreW = true;
    private Vector3 ropeExitPosition;
    private Transform ropeHoleTransform;
    private Quaternion originalRotation;
    
    [Header("Rope Settings")]
    public float ropeDefaultFallSpeed = 10f;
    public float ropeClimbSpeed = 20f;
    public float maxRopeDepth = 5f;
    
    [SerializeField] private LineRenderer ropeLine;

    private Rigidbody2D rb;

    private void Awake()
    {
        if (throwPoint == null)
        {
            GameObject throwPointObj = new GameObject("ThrowPoint");
            throwPointObj.transform.SetParent(transform);
            throwPointObj.transform.localPosition = new Vector3(0.5f, 0.2f, 0f);
            throwPoint = throwPointObj.transform;
        }

        if (aimLine == null)
        {
            aimLine = gameObject.AddComponent<LineRenderer>();
            aimLine.positionCount = 2;
            aimLine.enabled = false;

            aimLine.material = new Material(Shader.Find("Sprites/Default"));
            aimLine.startColor = new Color(1f, 0f, 0f, 0.5f);
            aimLine.endColor = new Color(1f, 0f, 0f, 0.5f);
        }

        if (ropeLine == null)
        {
            GameObject ropeLineObj = new GameObject("RopeLine");
            ropeLineObj.transform.SetParent(transform);
            ropeLine = ropeLineObj.AddComponent<LineRenderer>();
            ropeLine.positionCount = 2;
            ropeLine.enabled = false;
            ropeLine.material = new Material(Shader.Find("Sprites/Default"));
            ropeLine.startColor = new Color(0.54f, 0.27f, 0.07f, 1.0f);
            ropeLine.endColor = new Color(0.72f, 0.52f, 0.04f, 1.0f);
            ropeLine.startWidth = 0.1f;
            ropeLine.endWidth = 0.1f;
        }

        if (movementController == null)
        {
            movementController = GetComponent<MovementController>();
        }

        playerSprite = GetComponentInChildren<SpriteRenderer>();
        if (playerSprite != null)
        {
            originalSortingOrder = playerSprite.sortingOrder;
        }
        else
        {
            Debug.LogWarning("PlayerInteractionController: No SpriteRenderer found on the player.");
        }

        rb = GetComponent<Rigidbody2D>();
    }

    public void SetCurrentInteractable(GameObject interactable)
    {
        currentInteractable = interactable;
    }

    public GameObject GetCurrentInteractable()
    {
        return currentInteractable;
    }

    private void Update()
    {
        if (isOnRope)
        {
            ProcessRopeMovement();
            return;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            HandleInteraction();
        }

        if (heldThrowable != null)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                ToggleAiming();
            }

            if (isAiming)
            {
                HandleAiming();

                if (Input.GetMouseButtonDown(0))
                {
                    ThrowHeldThrowable();
                }
            }
        }
    }

    private void HandleInteraction()
    {
        if (currentInteractable == null)
            return;

        if (currentInteractable.CompareTag("Portal"))
        {
            Portal portal = currentInteractable.GetComponent<Portal>();
            if (portal != null && portal.exitPoint != null)
            {
                var indicator = portal.GetComponent<InteractableIndicator>();
                if (indicator != null)
                {
                    indicator.HideIndicator();
                }

                transform.position = portal.exitPoint.position;
            }
        }
        else if (currentInteractable.CompareTag("HidingSpot"))
        {
            ToggleHiding();
        }
        else if (currentInteractable.GetComponent<Throwable>() != null)
        {
            if (heldThrowable == null)
            {
                PickupThrowable(currentInteractable);
            }
        }
        else if (currentInteractable.CompareTag("RopeHole"))
        {
            EnterRopeMode(currentInteractable);
        }
    }

    private void PickupThrowable(GameObject throwableObject)
    {
        Throwable throwable = throwableObject.GetComponent<Throwable>();
        if (throwable != null)
        {
            heldThrowable = throwable;
            throwable.transform.SetParent(throwPoint);
            throwable.transform.localPosition = Vector3.zero;
            throwable.PickUp();
        }
    }

    private void ToggleAiming()
    {
        isAiming = !isAiming;
        if (isAiming)
        {
            aimLine.enabled = true;
            
            if (movementController != null)
                movementController.DisableMovement();
        }
        else
        {
            aimLine.enabled = false;
            
            if (movementController != null)
                movementController.EnableMovement();
        }
    }

    private void ToggleHiding()
    {
        SpriteRenderer hidingSpotSprite = currentInteractable.GetComponentInChildren<SpriteRenderer>();
        if (hidingSpotSprite == null)
        {
            Debug.LogWarning("PlayerInteractionController: HidingSpot does not have a SpriteRenderer.");
            return;
        }

        if (!isHidden)
        {
            isHidden = true;
            if (movementController != null)
                movementController.DisableMovement();

            if (playerSprite != null)
                playerSprite.sortingOrder = hidingSpotSprite.sortingOrder - 1;
        }
        else
        {
            isHidden = false;
            if (movementController != null)
                movementController.EnableMovement();

            if (playerSprite != null)
                playerSprite.sortingOrder = originalSortingOrder;
        }
    }

    private void HandleAiming()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        
        Vector2 direction = (mousePos - throwPoint.position).normalized;
        float distance = Vector2.Distance(mousePos, throwPoint.position);
        
        currentThrowForce = Mathf.Clamp(distance * throwForceMultiplier, 0, maxThrowForce);
        
        float normalizedForce = currentThrowForce / maxThrowForce;
        float lineWidth = Mathf.Lerp(minLineWidth, maxLineWidth, normalizedForce);
        
        aimLine.startWidth = lineWidth;
        aimLine.endWidth = lineWidth;
        aimLine.SetPosition(0, throwPoint.position);
        aimLine.SetPosition(1, mousePos);
    }

    private void ThrowHeldThrowable()
    {
        if (heldThrowable != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 direction = (mousePos - throwPoint.position).normalized;
            
            heldThrowable.Launch(direction, currentThrowForce);
            heldThrowable = null;
            
            isAiming = false;
            aimLine.enabled = false;

            if (movementController != null)
                movementController.EnableMovement();
        }
    }

    private void EnterRopeMode(GameObject ropeHole)
    {
        isOnRope = true;
        ropeIgnoreW = true;
        ropeExitPosition = transform.position;
        ropeHoleTransform = ropeHole.transform;
        originalRotation = transform.rotation;

        transform.position = ropeHoleTransform.position;

        if (transform.localScale.x > 0)
            transform.rotation = Quaternion.Euler(0, 0, -90);
        else
            transform.rotation = Quaternion.Euler(0, 0, 90);

        if (movementController != null)
            movementController.DisableMovement();

        int playerLayer = gameObject.layer;
        int groundLayer = LayerMask.NameToLayer("Ground");
        Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, true);

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        ropeLine.enabled = true;
        ropeLine.positionCount = 2;
        ropeLine.SetPosition(0, ropeHoleTransform.position);
        ropeLine.SetPosition(1, transform.position);

        currentInteractable = null;
    }

    private void ProcessRopeMovement()
    {
        if (ropeIgnoreW && !Input.GetKey(KeyCode.W))
        {
            ropeIgnoreW = false;
        }

        float verticalSpeed = 0f;
        if (!ropeIgnoreW && Input.GetKey(KeyCode.W))
        {
            verticalSpeed = ropeClimbSpeed;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            verticalSpeed = -ropeClimbSpeed;
        }
        else
        {
            verticalSpeed = -ropeDefaultFallSpeed;
        }

        Vector2 pos = rb.position;
        pos.y += verticalSpeed * Time.deltaTime;
        float upperBound = ropeHoleTransform.position.y;
        float lowerBound = ropeHoleTransform.position.y - maxRopeDepth;
        pos.y = Mathf.Clamp(pos.y, lowerBound, upperBound);
        rb.MovePosition(pos);

        ropeLine.SetPosition(1, pos);

        if (Mathf.Abs(pos.y - upperBound) < 0.01f && Input.GetKey(KeyCode.W) && !ropeIgnoreW)
        {
            ExitRopeMode();
        }
    }

    public void ExitRopeMode()
    {
        isOnRope = false;

        ropeLine.enabled = false;

        int playerLayer = gameObject.layer;
        int groundLayer = LayerMask.NameToLayer("Ground");
        Physics2D.IgnoreLayerCollision(playerLayer, groundLayer, false);

        transform.rotation = originalRotation;

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = Vector2.zero;

        transform.position = ropeExitPosition;

        if (movementController != null)
            movementController.EnableMovement();
    }
}