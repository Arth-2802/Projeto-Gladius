using UnityEngine;

public class MinionProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 4f;

    private Transform target;
    private int damage;
    private LayerMask targetLayer;
    private float launchTime;

    public void Initialize(Transform newTarget, int newDamage, LayerMask newTargetLayer)
    {
        target = newTarget;
        damage = newDamage;
        targetLayer = newTargetLayer;
        launchTime = Time.time;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime);

        Vector2 direction = target.position - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.right = direction;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (target == null || Time.time - launchTime < 0.15f || !IsTargetCollider(other))
        {
            return;
        }

        Debug.Log("MinionProjectile atingiu o alvo.");
        other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        Destroy(gameObject);
    }

    private bool IsTargetCollider(Collider2D other)
    {
        Transform otherTransform = other.transform;
        return otherTransform == target || otherTransform.IsChildOf(target);
    }
}