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

    [Header("Condicao para passar (opcional)")]
    [Tooltip("Deixe vazio para a saida ficar sempre liberada.")]
    [SerializeField] private VidaInimigo[] inimigosQueTemQueMorrer;

    [Tooltip("Spawner do mapa de destino (opcional). Inicia quando o player chega la.")]
    [SerializeField] private SpawnInimigosDepois spawnerDoDestino;

    [Header("Recompensa ao passar de mapa")]
    [Tooltip("Vida que o Player ganha ao passar por esta saida. 0 = nao cura.")]
    [SerializeField, Min(0f)] private float curaAoPassar = 0f;

    [Tooltip("Marcado: cura so na primeira vez (evita ficar indo e voltando para encher a vida).")]
    [SerializeField] private bool curarSoUmaVez = true;

    private bool jaCurou;
    private float proximaCuraPermitida;

    private void Reset()
    {
        SetColliderAsTrigger();
    }

    private void OnValidate()
    {
        SetColliderAsTrigger();
    }

    // OnTriggerStay garante que, se o player ja estiver na saida quando o
    // ultimo inimigo morrer, a passagem abre sem ele precisar sair e voltar.
    private void OnTriggerEnter2D(Collider2D other) { TentarPassar(other); }
    private void OnTriggerStay2D(Collider2D other) { TentarPassar(other); }

    private void TentarPassar(Collider2D other)
    {
        if (other.GetComponentInParent<Player>() == null)
        {
            return;
        }

        if (!InimigosMortos())
        {
            return;
        }

        if (destinationSpawn == null || destinationBounds == null || MapTransitionManager.Instance == null)
        {
            Debug.LogError($"MapExit no objeto '{name}' esta sem referencias para iniciar a transicao.", this);
            return;
        }

        Curar(other);

        if (spawnerDoDestino != null) spawnerDoDestino.Iniciar();

        MapTransitionManager.Instance.StartTransition(
            destinationSpawn,
            destinationBounds,
            fadeOutTime,
            fadeInTime);
    }

    private void Curar(Collider2D other)
    {
        if (curaAoPassar <= 0f) return;
        if (curarSoUmaVez && jaCurou) return;

        // o trigger dispara varias vezes seguidas; esta pausa garante uma cura so por passagem
        if (Time.time < proximaCuraPermitida) return;

        Vida vida = other.GetComponentInParent<Vida>();
        if (vida == null || vida.Morto) return;

        vida.DefinirVida(vida.VidaAtual + curaAoPassar);

        jaCurou = true;
        proximaCuraPermitida = Time.time + 2f;
        Debug.Log($"MapExit: curou {curaAoPassar} de vida.");
    }

    private bool InimigosMortos()
    {
        if (inimigosQueTemQueMorrer == null) return true;

        foreach (VidaInimigo v in inimigosQueTemQueMorrer)
        {
            // inimigo destruido (null) ou morto = ok
            if (v != null && !v.Morto) return false;
        }
        return true;
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