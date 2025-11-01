using UnityEngine;
using UnityEngine.InputSystem;

public class LogCollector : MonoBehaviour
{
    [Header("Carry Settings")]
    public Transform carryPoint;
    public float pickupRange = 5f;

    [Header("Dock Settings")]
    public Transform dock;
    public float dockRange = 3f;

    [Header("Dock Tracking")]
    public int logsPlaced = 0;

    private GameObject carriedLog = null;
    private PlayerControls controls;
    private Animator dockAnimator;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.PickUp.performed += ctx => HandleInteract();

        // Try to get animator from dock (optional)
        if (dock != null)
        {
            dockAnimator = dock.GetComponent<Animator>();
            if (dockAnimator == null)
                dockAnimator = dock.GetComponentInChildren<Animator>();
        }
    }

    private void OnEnable() => controls.Gameplay.Enable();
    private void OnDisable() => controls.Gameplay.Disable();

    void HandleInteract()
    {
        if (carriedLog == null)
        {
            TryPickupLog();
        }
        else
        {
            float distanceToDock = Vector2.Distance(transform.position, dock.position);
            if (distanceToDock <= dockRange)
                PlaceLogAtDock();
            else
                DropLog();
        }
    }

    void TryPickupLog()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRange);

        // Find the closest log
        GameObject closestLog = null;
        float closestDistance = Mathf.Infinity;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Log"))
            {
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestLog = hit.gameObject;
                }
            }
        }

        if (closestLog != null && closestDistance <= pickupRange)
        {
            PickUp(closestLog);
        }
        else
        {
            Debug.Log("[LogCollector] No logs in pickup range.");
        }
    }

    void PickUp(GameObject log)
    {
        carriedLog = log;
        carriedLog.transform.SetParent(carryPoint);
        carriedLog.transform.localPosition = Vector3.zero;

        Rigidbody2D rb = carriedLog.GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;

        Collider2D col = carriedLog.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    void PlaceLogAtDock()
    {
        if (carriedLog == null) return;

        Rigidbody2D rb = carriedLog.GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;

        Collider2D col = carriedLog.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Destroy(carriedLog);
        carriedLog = null;

        logsPlaced++;

        if (dockAnimator != null)
        {
            dockAnimator.SetInteger("PlanksPlaced", logsPlaced);
        }
    }

    void DropLog()
    {
        if (carriedLog == null) return;

        carriedLog.transform.SetParent(null);
        carriedLog.transform.position = transform.position;

        Rigidbody2D rb = carriedLog.GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = false;

        Collider2D col = carriedLog.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        carriedLog = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);

        if (dock != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(dock.position, dockRange);
        }
    }
}
