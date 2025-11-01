using UnityEngine;
using UnityEngine.InputSystem;

public class Tree : MonoBehaviour
{
    public float holdTimeToChop = 4f;
    private float holdTimer = 0f;
    private bool isChopping = false;
    private bool isChopped = false;

    public GameObject woodDropPrefab;
    public Transform dropPoint;

    [Header("Player Interaction")]
    public float interactionDistance = 2f;
    public string playerTag = "Survivor";
    private Transform player;
    private bool playerInRange = false;

    [Header("Input Settings")]
    public InputActionReference chopAction;
    private bool chopInputHeld = false;

    [Header("Chopping Progress")]
    [Range(0f,1f)]
    public float chopProgress = 0f; // <--- ADD THIS

    void Start()
    {
        Debug.Log("=== TREE SCRIPT START ===");
        
        player = GameObject.FindGameObjectWithTag(playerTag)?.transform;
        
        if (player == null)
        {
            Debug.LogError($"❌ PLAYER NOT FOUND with tag '{playerTag}'!");
            return;
        }
        else
        {
            Debug.Log("✅ Player found: " + player.name);
        }

        if (chopAction == null)
        {
            Debug.LogError("❌ CHOP ACTION NOT ASSIGNED in inspector!");
            return;
        }
        else
        {
            Debug.Log("✅ Chop action assigned: " + chopAction.action.name);
        }

        chopAction.action.started += OnChopStarted;
        chopAction.action.canceled += OnChopCanceled;
        chopAction.action.Enable();

        Debug.Log("✅ Tree setup complete - Ready for chopping");
        Debug.Log("=== TREE SCRIPT START FINISHED ===");
    }

    void Update()
    {
        if (player == null || isChopped) return; // NEW: Don't run if already chopped
        
        CheckPlayerDistance();
        
        if (playerInRange && chopInputHeld)
        {
            if (!isChopping)
            {
                isChopping = true;
                holdTimer = 0f;
                Debug.Log("🪓 START CHOPPING TREE - Hold timer started");
            }
            
            holdTimer += Time.deltaTime;
            Debug.Log("Chopping: " + holdTimer.ToString("F1") + "s / " + holdTimeToChop + "s");
            
            if (holdTimer >= holdTimeToChop)
            {
                ChopTree();
            }
        }
        else if (isChopping)
        {
            Debug.Log("⏹️ STOPPED CHOPPING - Progress lost");
            ResetChopping();
        }
    }

    void OnChopStarted(InputAction.CallbackContext context)
    {
        if (isChopped) return; // NEW: Don't process input if already chopped
        Debug.Log("🎮 CHOP BUTTON PRESSED - Player in range: " + playerInRange);
        chopInputHeld = true;
    }

    void OnChopCanceled(InputAction.CallbackContext context)
    {
        Debug.Log("🎮 CHOP BUTTON RELEASED");
        chopInputHeld = false;
    }

    void CheckPlayerDistance()
    {
        if (player != null)
        {
            bool wasInRange = playerInRange;
            float distance = Vector2.Distance(transform.position, player.position);
            playerInRange = distance <= interactionDistance;
            
            if (wasInRange != playerInRange)
            {
                if (playerInRange)
                {
                    Debug.Log("🎯 PLAYER ENTERED RANGE - Distance: " + distance.ToString("F2"));
                }
                else
                {
                    Debug.Log("🚶 PLAYER LEFT RANGE - Distance: " + distance.ToString("F2"));
                }
            }
        }
    }

    void ResetChopping()
    {
        isChopping = false;
        holdTimer = 0f;
    }

    private void ChopTree()
{
    if (isChopped) return;

    isChopped = true;
    isChopping = false;
    chopProgress = 1f;

    // Use existing child log
    Transform childLog = transform.Find("WoodLogs"); // the visual log
    if(childLog != null)
    {
        childLog.SetParent(null);
        if(childLog.TryGetComponent<Rigidbody2D>(out var rb)) rb.isKinematic = false;
        if(childLog.TryGetComponent<Collider2D>(out var col)) col.enabled = true;
    }
    else if(woodDropPrefab != null) // fallback
    {
        Transform drop = dropPoint != null ? dropPoint : transform;
        Instantiate(woodDropPrefab, drop.position, Quaternion.identity);
    }

    TreeSpawner spawner = FindObjectOfType<TreeSpawner>();
    if (spawner != null)
    {
        spawner.TreeChopped(transform.position);
        spawner.RemoveTree(gameObject);
    }

    Destroy(gameObject);
}


    void OnDestroy()
    {
        if (chopAction != null && chopAction.action != null)
        {
            chopAction.action.started -= OnChopStarted;
            chopAction.action.canceled -= OnChopCanceled;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
    }
}