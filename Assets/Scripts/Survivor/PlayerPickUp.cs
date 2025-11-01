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

    [Header("Dock Repair Settings")]
    public float dockRepairRadius = 2f;
    public int planksNeeded = 10; 
    private int currentPlanks = 0;
    public LayerMask dockLayer; // assign the layer your dock is on

    private GameObject carriedLog = null;

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
        // ✅ Check for dock repair first
        Collider2D dock = Physics2D.OverlapCircle(transform.position, dockRepairRadius, dockLayer);
        if (dock != null && carriedLog != null && currentPlanks < planksNeeded)
        {
            currentPlanks++;
            Animator dockAnimator = dock.GetComponent<Animator>();
            if (dockAnimator != null)
                dockAnimator.SetInteger("PlanksPlaced", currentPlanks);

            Debug.Log($"Dock repaired: {currentPlanks}/{planksNeeded}");

            Destroy(carriedLog);
            carriedLog = null;
            return;
        }

        // ✅ Pick up nearest log if not holding one
        if (carriedLog == null)
        {
            Collider2D[] logs = Physics2D.OverlapCircleAll(transform.position, pickUpRadius, logLayer);
            foreach (Collider2D log in logs)
            {
                if (log.CompareTag("Log"))
                {
                    carriedLog = log.gameObject;
                    carriedLog.transform.SetParent(carryPoint);
                    carriedLog.transform.localPosition = Vector3.zero;
                    carriedLog.transform.localRotation = Quaternion.identity;

                    if (carriedLog.TryGetComponent<Rigidbody2D>(out var rb)) rb.isKinematic = true;
                    if (carriedLog.TryGetComponent<Collider2D>(out var col)) col.enabled = false;
                    break;
                }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dockRepairRadius);
    }
}
