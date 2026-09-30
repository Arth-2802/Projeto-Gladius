using UnityEngine;

public class minion : MonoBehaviour
{
    [Header("Target and movement")]
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.7f;
    [SerializeField] private string targetTag = "Player";

    [Header("Melee attack")]
    [SerializeField] private Transform meleePoint;
    [SerializeField] private float meleeRange = 0.8f;
    [SerializeField] private int meleeDamage = 10;
    [SerializeField] private float meleeCooldown = 1f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Ranged attack")]
    [SerializeField] private Transform projectilePoint;
    [SerializeField] private MinionProjectile projectilePrefab;
    [SerializeField] private float rangedRange = 7f;
    [SerializeField] private int rangedDamage = 5;
    [SerializeField] private float rangedCooldown = 2f;

    private float meleeTimer;
    private float rangedTimer;

    private void Start()
    {
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
    }

    private void Update()
    {
        if (target == null)
        {
            return;
        }

        meleeTimer -= Time.deltaTime;
        rangedTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, target.position);
        FaceTarget();

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

    private void TryMeleeAttack()
    {
        if (meleeTimer > 0f)
        {
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(meleePoint.position, meleeRange, targetLayer);
        if (hit != null)
        {
            Debug.Log("Jogador esfaqueado pelo ataque corpo a corpo.");
            hit.SendMessage("TakeDamage", meleeDamage, SendMessageOptions.DontRequireReceiver);
            meleeTimer = meleeCooldown;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 meleePosition = meleePoint == null ? transform.position : meleePoint.position;
        Gizmos.DrawWireSphere(meleePosition, meleeRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
    }
}
