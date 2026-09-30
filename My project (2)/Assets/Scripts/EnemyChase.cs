using UnityEngine;

/// <summary>
/// Enemy AI without NavMesh: walks straight toward the player using a CharacterController
/// (so it collides with walls and floors) and deals melee damage when close.
/// Works together with EnemyHealth (which handles the enemy's Vida and death).
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyChase : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 10f;
    public float gravity = -20f;

    [Tooltip("The enemy starts chasing when the player is closer than this distance.")]
    public float detectionRange = 15f;

    [Header("Attack")]
    [Tooltip("Distance at which the enemy can hit the player.")]
    public float attackRange = 1.8f;
    public int attackDamage = 10;
    [Tooltip("Seconds between attacks.")]
    public float attackCooldown = 1f;

    private CharacterController controller;
    private EnemyHealth health;
    private PlayerHealth playerHealth;
    private Transform player;
    private float verticalVelocity;
    private float nextAttackTime;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        // Find the player in the scene (the object that has PlayerHealth).
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
            player = playerHealth.transform;
        else
            Debug.LogWarning("EnemyChase: no PlayerHealth found in the scene.");
    }

    private void Update()
    {
        Vector3 horizontalVelocity = Vector3.zero;

        if (!health.IsDead && player != null && !playerHealth.IsDead)
        {
            // Direction to the player, ignoring height differences.
            Vector3 toPlayer = player.position - transform.position;
            toPlayer.y = 0f;
            float distance = toPlayer.magnitude;

            if (distance <= detectionRange && distance > 0.001f)
            {
                Vector3 direction = toPlayer / distance;
                FaceDirection(direction);

                // Keep walking until we are close enough to attack.
                if (distance > attackRange * 0.8f)
                    horizontalVelocity = direction * moveSpeed;

                if (distance <= attackRange)
                    TryAttack();
            }
        }

        ApplyMovement(horizontalVelocity);
    }

    private void ApplyMovement(Vector3 horizontalVelocity)
    {
        // Grounded state comes from the single Move() call of the previous frame.
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    private void FaceDirection(Vector3 direction)
    {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void TryAttack()
    {
        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;
        playerHealth.TakeDamage(attackDamage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
