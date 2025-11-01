using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickUp : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference pickUpAction;

    [Header("Carry Settings")]
    public Transform carryPoint;
    public float pickUpRadius = 1.5f;
    public LayerMask logLayer;

    [Header("Dock & Bridge Settings")]
    public float dockRepairRadius = 2f;
    public LayerMask dockLayer; // assign the layer your dock is on
    [Tooltip("How much to increase bridge scale per log (e.g., 0.1 = 10% larger per log)")]
    public float bridgeScalePerLog = 0.1f;
    [Tooltip("Initial bridge scale")]
    public float initialBridgeScale = 1f;
    [Tooltip("Tag name for bridge GameObject (will create if not found)")]
    public string bridgeTag = "Bridge";

    private int logsPlacedOnBridge = 0;
    private GameObject carriedLog = null;
    private GameObject bridgeObject = null;

    private void OnEnable()
    {
        pickUpAction.action.performed += OnPickUp;
        pickUpAction.action.Enable();
    }

    private void OnDisable()
    {
        pickUpAction.action.performed -= OnPickUp;
        pickUpAction.action.Disable();
    }

    private void OnPickUp(InputAction.CallbackContext context)
    {
        // ✅ Check for dock and place log on bridge first
        Collider2D dock = Physics2D.OverlapCircle(transform.position, dockRepairRadius, dockLayer);
        if (dock != null && carriedLog != null)
        {
            PlaceLogOnBridge(dock.gameObject);
            return;
        }

        // ✅ Pick up nearest log if not holding one
        if (carriedLog == null)
        {
            // Only search if logLayer is properly set (not 0)
            if (logLayer.value == 0)
            {
                Debug.LogWarning("LogLayer is not set! Please assign a layer mask in the inspector.");
                return;
            }

            Collider2D[] logs = Physics2D.OverlapCircleAll(transform.position, pickUpRadius, logLayer);
            
            // Find the closest log within range
            GameObject closestLog = null;
            float closestDistance = pickUpRadius + 1f; // Start with distance beyond range

            foreach (Collider2D log in logs)
            {
                if (log.CompareTag("Log"))
                {
                    float distance = Vector2.Distance(transform.position, log.transform.position);
                    
                    // Only consider logs within the pickup radius
                    if (distance <= pickUpRadius && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestLog = log.gameObject;
                    }
                }
            }

            // Pick up the closest log if found
            if (closestLog != null)
            {
                carriedLog = closestLog;
                carriedLog.transform.SetParent(carryPoint);
                carriedLog.transform.localPosition = Vector3.zero;
                carriedLog.transform.localRotation = Quaternion.identity;

                if (carriedLog.TryGetComponent<Rigidbody2D>(out var rb)) rb.isKinematic = true;
                if (carriedLog.TryGetComponent<Collider2D>(out var col)) col.enabled = false;
            }
        }
        else
        {
            // ✅ Drop carried log
            carriedLog.transform.SetParent(null);

            if (carriedLog.TryGetComponent<Rigidbody2D>(out var rb)) rb.isKinematic = false;
            if (carriedLog.TryGetComponent<Collider2D>(out var col)) col.enabled = true;

            carriedLog = null;
        }
    }

    private void PlaceLogOnBridge(GameObject dock)
    {
        if (carriedLog == null) return;

        // Get or create bridge object
        if (bridgeObject == null)
        {
            bridgeObject = GameObject.FindGameObjectWithTag(bridgeTag);
            
            // If no bridge found, create one as a child of the dock or at dock position
            if (bridgeObject == null)
            {
                bridgeObject = new GameObject("Bridge");
                bridgeObject.tag = bridgeTag;
                bridgeObject.transform.position = dock.transform.position;
                bridgeObject.transform.parent = dock.transform;
                
                // Add a sprite renderer for visual representation
                SpriteRenderer sr = bridgeObject.AddComponent<SpriteRenderer>();
                
                // Try to load bridge sprite from Resources (optional)
                Sprite bridgeSprite = Resources.Load<Sprite>("Wood_log");
                if (bridgeSprite == null)
                {
                    // Try alternative names
                    bridgeSprite = Resources.Load<Sprite>("WoodLogs");
                }
                
                sr.sprite = bridgeSprite;
                if (bridgeSprite == null)
                {
                    // No sprite found, use a colored rectangle as fallback
                    sr.color = new Color(0.5f, 0.3f, 0.1f, 0.8f); // Brown-ish bridge color
                }
                else
                {
                    sr.color = Color.white;
                }
                
                sr.sortingOrder = -1; // Behind dock
                
                // Set initial scale
                bridgeObject.transform.localScale = Vector3.one * initialBridgeScale;
            }
            else
            {
                // Bridge already exists, preserve its current scale as initial
                initialBridgeScale = bridgeObject.transform.localScale.x;
            }
        }

        // Increment logs placed
        logsPlacedOnBridge++;
        
        // Scale up the bridge (grows with each log)
        float newScale = initialBridgeScale + (logsPlacedOnBridge * bridgeScalePerLog);
        bridgeObject.transform.localScale = new Vector3(newScale, newScale, 1f);
        
        // Update dock animator if available (using dock animations from Resources)
        Animator dockAnimator = dock.GetComponent<Animator>();
        if (dockAnimator != null)
        {
            // Clamp to available dock animations (0-10 based on Resources)
            int animIndex = Mathf.Clamp(logsPlacedOnBridge - 1, 0, 10);
            dockAnimator.SetInteger("PlanksPlaced", animIndex);
        }

        Debug.Log($"Log placed on bridge! Total logs: {logsPlacedOnBridge}. Bridge scale: {newScale}");

        // Destroy the carried log
        Destroy(carriedLog);
        carriedLog = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dockRepairRadius);
    }
}
