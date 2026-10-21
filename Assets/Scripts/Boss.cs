using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Target and movement")]
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stopDistance = 1.2f;
    [SerializeField] private string targetTag = "Player";

    [Header("Melee attack")]
    [SerializeField] private Transform meleePoint;
    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private int meleeDamage = 25;
    [SerializeField] private float meleeCooldown = 1.2f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Health")]
    [SerializeField] private int maxHealth = 500;
    [SerializeField] private float criticalKnockback = 1.5f;
    private int currentHealth;

    [Header("Ranged attack")]
    [SerializeField] private Transform projectilePoint;
    [SerializeField] private MinionProjectile projectilePrefab;
    [SerializeField] private float rangedRange = 9f;
    [SerializeField] private int rangedDamage = 15;
    [SerializeField] private float rangedCooldown = 2f;

    [Header("Ground column attack")]
    [SerializeField] private BossGroundColumn groundColumnPrefab;
    [SerializeField] private int groundColumnDamage = 45;
    [SerializeField] private int groundColumnCount = 15;
    [SerializeField] private float groundColumnSpacing = 1.8f;
    [SerializeField] private float groundColumnWidth = 1.5f;
    [SerializeField] private float groundColumnWarning = 0.75f;
    [SerializeField] private float groundColumnDuration = 3f;
    [SerializeField] private float groundColumnCooldown = 12f;
    [SerializeField] private float groundColumnRange = 10f;

    private float meleeTimer;
    private float rangedTimer;
    private float groundColumnTimer;
    private bool groundColumnAttackActive;
    private bool groundColumnWarningShown;

    private void Start()
    {
        currentHealth = maxHealth;

        if (target == null && !string.IsNullOrEmpty(targetTag))
        {
            GameObject targetObject = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObject != null)
            {
                target = targetObject.transform;
            }
        }

        if (target == null)
        {
            Player player = FindFirstObjectByType<Player>();
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (meleePoint == null)
        {
            meleePoint = transform;
        }

        if (projectilePoint == null)
        {
            projectilePoint = transform;
        }

        if (targetLayer.value == 0 && target != null)
        {
            targetLayer = 1 << target.gameObject.layer;
        }

        if (groundColumnPrefab == null && !groundColumnWarningShown)
        {
            Debug.LogWarning("Boss sem Ground Column Prefab: arraste BossGroundColumn.prefab para o campo Ground Column Prefab.", this);
            groundColumnWarningShown = true;
        }

        IgnorePhysicalCollisionWithTarget();
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        meleeTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;
        groundColumnTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, target.position);
        FaceTarget();

        if (groundColumnAttackActive)
        {
            return;
        }

        TryGroundColumnAttack(distance);

        if (distance > stopDistance)
        {
            MoveTowardsTarget();
        }

        if (distance <= meleeRange)
        {
            TryMeleeAttack();
        }
        else if (distance <= rangedRange)
        {
            TryRangedAttack();
        }
    }

    private void MoveTowardsTarget()
    {
        Vector2 nextPosition = Vector2.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime);

        transform.position = nextPosition;
    }

    private void FaceTarget()
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(target.position.x - transform.position.x);
        transform.localScale = scale;
    }

    private void IgnorePhysicalCollisionWithTarget()
    {
        if (target == null)
        {
            return;
        }

        Collider2D[] bossColliders = GetComponentsInChildren<Collider2D>();
        Collider2D[] targetColliders = target.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D bossCollider in bossColliders)
        {
            foreach (Collider2D targetCollider in targetColliders)
            {
                Physics2D.IgnoreCollision(bossCollider, targetCollider, true);
            }
        }
    }

    private void TryMeleeAttack()
    {
        if (meleeTimer > 0f)
        {
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(meleePoint.position, meleeRange, targetLayer);
        if (hit != null)
        {
            Debug.Log("Jogador atingido pelo boss por " + meleeDamage + " de dano.");
            hit.SendMessage("TakeDamage", meleeDamage, SendMessageOptions.DontRequireReceiver);
            meleeTimer = meleeCooldown;
        }
    }

    public void ReceiveAttack(int damage, bool isCritical, Vector2 attackerPosition)
    {
        currentHealth = Mathf.Max(currentHealth - damage, 0);

        if (isCritical)
        {
            Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;
            if (knockbackDirection.sqrMagnitude < 0.001f)
            {
                knockbackDirection = Vector2.right;
            }

            transform.position = (Vector2)transform.position + knockbackDirection * criticalKnockback;
        }

        Debug.Log("Boss recebeu " + damage + " de dano. Vida: " + currentHealth + "/" + maxHealth);

        if (currentHealth == 0)
        {
            Debug.Log("Boss derrotado.");
            Destroy(gameObject);
        }
    }

    private void TryRangedAttack()
    {
        if (rangedTimer > 0f || projectilePrefab == null)
        {
            return;
        }

        MinionProjectile projectile = Instantiate(
            projectilePrefab,
            projectilePoint.position,
            Quaternion.identity);

        projectile.Initialize(target, rangedDamage, targetLayer);
        rangedTimer = rangedCooldown;
    }

    private void TryGroundColumnAttack(float distance)
    {
        if (groundColumnPrefab == null || groundColumnTimer > 0f || distance > groundColumnRange)
        {
            return;
        }

        int columnCount = Mathf.Max(1, groundColumnCount);
        float firstOffset = (columnCount - 1) * groundColumnSpacing * -0.5f;

        for (int index = 0; index < columnCount; index++)
        {
            Vector3 spawnPosition = target.position;
            spawnPosition.x += firstOffset + index * groundColumnSpacing;

            BossGroundColumn column = Instantiate(
                groundColumnPrefab,
                spawnPosition,
                Quaternion.identity);

            column.transform.localScale = new Vector3(
                groundColumnWidth,
                column.transform.localScale.y,
                column.transform.localScale.z);
            column.Initialize(targetLayer, groundColumnDamage, groundColumnWarning, groundColumnDuration);
        }

        groundColumnAttackActive = true;
        groundColumnTimer = groundColumnCooldown;
        FireGroundColumnProjectiles();
        StartCoroutine(ReleaseGroundColumnAttack());
        Debug.Log("Boss iniciou o ataque de colunas do chão.");
    }

    private void FireGroundColumnProjectiles()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        for (int index = 0; index < 2; index++)
        {
            Vector3 spawnPosition = projectilePoint.position;
            spawnPosition.x += index == 0 ? -0.3f : 0.3f;

            MinionProjectile projectile = Instantiate(
                projectilePrefab,
                spawnPosition,
                Quaternion.identity);

            projectile.Initialize(target, rangedDamage, targetLayer);
        }
    }

    private System.Collections.IEnumerator ReleaseGroundColumnAttack()
    {
        yield return new WaitForSeconds(groundColumnWarning + 0.3f + groundColumnDuration + 0.3f);
        groundColumnAttackActive = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 meleePosition = meleePoint == null ? transform.position : meleePoint.position;
        Gizmos.DrawWireSphere(meleePosition, meleeRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
    }
}