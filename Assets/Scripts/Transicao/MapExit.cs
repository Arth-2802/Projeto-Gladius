using UnityEngine;

/// <summary>
/// Inicia a transicao quando o Player alcanca a saida de um mapa.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MapExit : MonoBehaviour
{
    [Header("Destino")]
    [Tooltip("SpawnPoint do mapa para o qual o Player sera levado.")]
    [SerializeField] private Transform destinationSpawn;

    [Tooltip("Collider 2D usado pelo Confiner do mapa de destino.")]
    [SerializeField] private Collider2D destinationBounds;

    [Header("Duracao do fade")]
    [Tooltip("Tempo para a tela ficar preta.")]
    [SerializeField, Min(0f)] private float fadeOutTime = 0.3f;

    [Tooltip("Tempo para revelar o mapa novo.")]
    [SerializeField, Min(0f)] private float fadeInTime = 0.3f;

    private void Reset()
    {
        SetColliderAsTrigger();
    }

    private void OnValidate()
    {
        SetColliderAsTrigger();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() == null)
        {
            return;
        }

        if (destinationSpawn == null || destinationBounds == null || MapTransitionManager.Instance == null)
        {
            Debug.LogError($"MapExit no objeto '{name}' esta sem referencias para iniciar a transicao.", this);
            return;
        }

        MapTransitionManager.Instance.StartTransition(
            destinationSpawn,
            destinationBounds,
            fadeOutTime,
            fadeInTime);
    }

    private void OnDrawGizmos()
    {
        if (destinationSpawn == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, destinationSpawn.position);
        Gizmos.DrawSphere(destinationSpawn.position, 0.2f);
    }

    private void SetColliderAsTrigger()
    {
        Collider2D exitCollider = GetComponent<Collider2D>();
        if (exitCollider != null)
        {
            exitCollider.isTrigger = true;
        }
    }
}
