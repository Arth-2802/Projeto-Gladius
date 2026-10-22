using System.Collections;
using UnityEngine;

public class BossGroundColumn : MonoBehaviour
{
    [SerializeField] private float riseHeight = 2f;
    [SerializeField] private float riseDuration = 0.3f;
    [SerializeField] private float columnWidth = 1.5f;
    [SerializeField] private float columnHeight = 4f;
    [SerializeField] private float damageInterval = 0.5f;

    private Rigidbody2D body;
    private BoxCollider2D columnCollider;
    private SpriteRenderer spriteRenderer;
    private LayerMask targetLayer;
    private int damage;
    private float warningDuration;
    private float activeDuration;
    private float damageTimer;
    private bool damageActive;
    private bool playerWasHit;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        columnCollider = GetComponent<BoxCollider2D>();
        if (columnCollider != null)
        {
            columnCollider.isTrigger = true;
            columnCollider.enabled = false;
            columnCollider.size = new Vector2(columnWidth, columnHeight);
        }

        body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }

    public void Initialize(LayerMask targetLayer, int damage, float warningDuration, float activeDuration)
    {
        this.targetLayer = targetLayer;
        this.damage = damage;
        this.warningDuration = warningDuration;
        this.activeDuration = activeDuration;
        StartCoroutine(RiseAndDamage());
    }

    private IEnumerator RiseAndDamage()
    {
        yield return new WaitForSeconds(warningDuration);

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        if (columnCollider != null)
        {
            columnCollider.enabled = true;
        }

        damageActive = true;
        damageTimer = 0f;
        playerWasHit = false;

        Debug.Log("Coluna do boss subiu do chão.");

        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.up * riseHeight;
        float elapsed = 0f;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / riseDuration);
            yield return null;
        }

        transform.position = endPosition;
        yield return new WaitForSeconds(activeDuration);

        damageActive = false;
        if (columnCollider != null)
        {
            columnCollider.enabled = false;
        }

        elapsed = 0f;
        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(endPosition, startPosition, elapsed / riseDuration);
            yield return null;
        }

        transform.position = startPosition;
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        Destroy(gameObject);
    }

    private void Update()
    {
        if (damageTimer > 0f)
        {
            damageTimer -= Time.deltaTime;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!damageActive || damageTimer > 0f || !IsTargetLayer(other.gameObject.layer))
        {
            return;
        }

        if (!playerWasHit)
        {
            Debug.Log("Coluna do boss atingiu o jogador.");
            playerWasHit = true;
        }

        other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        damageTimer = damageInterval;
    }

    private bool IsTargetLayer(int layer)
    {
        return (targetLayer.value & (1 << layer)) != 0;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(columnWidth, columnHeight, 0f));
    }
}