using System.Collections.Generic;
using UnityEngine;
using System.Reflection; // Adicionado para permitir a busca segura de funções

// Fica no objeto "Sprite" (o filho do Player que tem o Animator).
public class PlayerAnimEvents : MonoBehaviour
{
    [Header("Dano")]
    [SerializeField] private float dano = 10f;

    [Header("Hitbox (relativa ao Player, vira com a direcao)")]
    [SerializeField] private Vector2 offsetHitbox = new Vector2(1.2f, 1f);
    [SerializeField] private Vector2 tamanhoHitbox = new Vector2(1.6f, 1.6f);

    [Header("Empurrao")]
    [SerializeField] private float forcaEmpurrao = 6f;
    [SerializeField] private float duracaoEmpurrao = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool mostrarDebug = true;

    private Player player;

    void Awake()
    {
        player = GetComponentInParent<Player>();
    }

    // Animation Event no ultimo frame do Attack
    public void FinishAttack()
    {
        if (player != null) player.FinishAttack();
    }

    // Animation Event no frame em que a espada acerta (espada esticada)
    public void HitEnemies()
    {
        if (player == null) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(CentroHitbox(), tamanhoHitbox, 0f);
        HashSet<VidaInimigo> jaAtingidos = new HashSet<VidaInimigo>();

        foreach (Collider2D c in hits)
        {
            VidaInimigo vida = c.GetComponentInParent<VidaInimigo>();
            if (vida == null || !jaAtingidos.Add(vida)) continue;

            vida.LevarDano(dano);

            SomPlayer som = player.GetComponent<SomPlayer>();
            if (som != null) som.Acerto();

            // Busca o componente Inimigo de forma padrão
            Inimigo inimigo = vida.GetComponent<Inimigo>();
            if (inimigo != null)
            {
                float dx = vida.transform.position.x - player.transform.position.x;
                int lado = Mathf.Abs(dx) > 0.01f ? (dx > 0 ? 1 : -1) : player.facingDirection;
                
                // Procura com segurança se a função 'Empurrar' existe dentro do script Inimigo.cs
                MethodInfo metodoEmpurrar = inimigo.GetType().GetMethod("Empurrar");

                if (metodoEmpurrar != null)
                {
                    // Se a função existir no Inimigo.cs, ela é executada aqui
                    object[] parametros = { new Vector2(lado, 0f), forcaEmpurrao, duracaoEmpurrao };
                    metodoEmpurrar.Invoke(inimigo, parametros);
                }
                else if (mostrarDebug)
                {
                    Debug.LogWarning("Inimigo atingido! (Nota: O empurrão visual só vai funcionar quando você criar a função 'public void Empurrar...' dentro do script Inimigo.cs)");
                }
            }

            if (mostrarDebug) Debug.Log("Player: acertou " + vida.name);
        }

        if (mostrarDebug && jaAtingidos.Count == 0) Debug.Log("Player: golpe no ar");
    }

    private Vector2 CentroHitbox()
    {
        Transform p = player.transform;
        return (Vector2)p.position + new Vector2(offsetHitbox.x * player.facingDirection, offsetHitbox.y);
    }

    void OnDrawGizmosSelected()
    {
        Player pl = player != null ? player : GetComponentInParent<Player>();
        if (pl == null) return;

        Gizmos.color = Color.red;
        Vector2 centro = (Vector2)pl.transform.position + new Vector2(offsetHitbox.x * pl.facingDirection, offsetHitbox.y);
        Gizmos.DrawWireCube(centro, tamanhoHitbox);
    }
}