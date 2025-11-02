using UnityEngine;

public class NPCController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float patrolMinSpeed = 1f;
    public float patrolMaxSpeed = 2f;
    public float chaseSpeed = 4f;
    public float changePatrolDirectionTime = 3f;
    public float obstacleAvoidDistance = 0.5f;

    [Header("Detection Settings")]
    public float detectionRadius = 5f;
    public LayerMask obstacleMask; // walls, etc.

    [Header("Map Bounds")]
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer sr;

    private Vector2 moveDirection;
    private float speed;
    private float patrolTimer;

    private Transform player;

    private enum NPCState { Patrolling, Chasing }
    private NPCState currentState = NPCState.Patrolling;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        player = GameObject.FindGameObjectWithTag("Survivor")?.transform;
        if (player == null) Debug.LogError("[NPCController] Survivor not found!");

        PickNewPatrolDirection();
    }

    void Update()
    {
        switch (currentState)
        {
            case NPCState.Patrolling:
                PatrolUpdate();
                DetectPlayer();
                break;
            case NPCState.Chasing:
                ChaseUpdate();
                break;
        }
    }

    void FixedUpdate()
    {
        // Move NPC
        Vector2 newPos = rb.position + moveDirection * speed * Time.fixedDeltaTime;
        newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
        newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);
        rb.MovePosition(newPos);

        // Animate
        animator.SetBool("isMoving", moveDirection != Vector2.zero);
        if (moveDirection.x != 0) sr.flipX = moveDirection.x < 0;
    }

    // ---------------------- Patrol ----------------------
    void PatrolUpdate()
    {
        patrolTimer += Time.deltaTime;
        if (patrolTimer >= changePatrolDirectionTime || IsObstacleAhead(moveDirection))
        {
            patrolTimer = 0f;
            PickNewPatrolDirection();
        }
    }

    void PickNewPatrolDirection()
    {
        Vector2 dir;
        int attempts = 0;
        do
        {
            dir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
            attempts++;
        } while (IsObstacleAhead(dir) && attempts < 10);

        moveDirection = dir;
        speed = Random.Range(patrolMinSpeed, patrolMaxSpeed);
    }

    bool IsObstacleAhead(Vector2 dir)
    {
        return Physics2D.CircleCast(rb.position, 0.2f, dir, obstacleAvoidDistance, obstacleMask);
    }

    // ---------------------- Detection & Chasing ----------------------
    void DetectPlayer()
    {
        if (player == null) return;

        // First, check if player is within radius
        if (Vector2.Distance(transform.position, player.position) <= detectionRadius)
        {
            // Check line-of-sight (ignore player layer)
            Vector2 dir = player.position - transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir.normalized, detectionRadius, obstacleMask);
            if (hit.collider == null) // no obstacle in the way
            {
                currentState = NPCState.Chasing;
            }
        }
    }

    void ChaseUpdate()
    {
        if (player == null)
        {
            currentState = NPCState.Patrolling;
            PickNewPatrolDirection();
            return;
        }

        // Move toward player
        moveDirection = ((Vector2)player.position - rb.position).normalized;
        speed = chaseSpeed;

        // Stop chasing if player escapes too far
        if (Vector2.Distance(rb.position, player.position) > detectionRadius * 2f)
        {
            currentState = NPCState.Patrolling;
            PickNewPatrolDirection();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
