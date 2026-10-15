using UnityEngine;

public class MinionProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float homingDuration = 1.5f;

    private Transform target;
    private Vector2 travelDirection;
    private bool isHoming;
    private int damage;
    private LayerMask targetLayer;
    private float launchTime;

    public void Initialize(Transform newTarget, int newDamage, LayerMask newTargetLayer)
    {
        target = newTarget;
        travelDirection = GetDirectionToTarget();
        isHoming = true;
        damage = newDamage;
        targetLayer = newTargetLayer;
        launchTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    public void InitializeAtPosition(Transform targetForHit, Vector2 targetPosition, int newDamage, LayerMask newTargetLayer)
    {
        target = targetForHit;
        travelDirection = targetPosition - (Vector2)transform.position;
        if (travelDirection.sqrMagnitude > 0.001f)
        {
            travelDirection.Normalize();
        }
        isHoming = false;
        damage = newDamage;
        targetLayer = newTargetLayer;
        launchTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (isHoming && Time.time - launchTime >= homingDuration)
        {
            isHoming = false;
        }

        if (isHoming && target != null)
        {
            Vector2 directionToTarget = GetDirectionToTarget();
            if (directionToTarget.sqrMagnitude > 0.001f)
            {
                travelDirection = directionToTarget;
            }
        }

        transform.position += (Vector3)(travelDirection * speed * Time.deltaTime);

        if (travelDirection.sqrMagnitude > 0.001f)
        {
            transform.right = travelDirection;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - launchTime < 0.15f)
        {
            return;
        }

        if (target != null && IsTargetCollider(other))
        {
            Debug.Log("MinionProjectile atingiu o alvo.");
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        Destroy(gameObject);
    }

    private Vector2 GetDirectionToTarget()
    {
        if (target == null)
        {
            return travelDirection;
        }

        return ((Vector2)target.position - (Vector2)transform.position).normalized;
    }

    private bool IsTargetCollider(Collider2D other)
    {
        Transform otherTransform = other.transform;
        return otherTransform == target || otherTransform.IsChildOf(target);
    }
}