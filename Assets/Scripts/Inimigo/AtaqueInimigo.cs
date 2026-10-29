using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AtaqueJogador : MonoBehaviour
{
    [Header("Dano")]
    [SerializeField] private float dano = 25f;
    [SerializeField] private float intervalo = 0.5f;        // tempo minimo entre ataques
    [SerializeField] private float atrasoDoGolpe = 0.2f;    // segundos ate a espada acertar

    [Header("Area do golpe (comeca na frente do corpo)")]
    [SerializeField] private Vector2 tamanho = new Vector2(1.8f, 1.8f);
    [SerializeField] private LayerMask camadaAlvos = ~0;

    [Header("Nomes das acoes no Input System")]
    [SerializeField] private string nomeAcaoAtaque = "Attack";
    [SerializeField] private string nomeAcaoMover = "Move";

    [Header("Diagnostico")]
    [SerializeField] private bool mostrarDebug = true;

    private PlayerInput entrada;
    private InputAction acaoAtaque;
    private InputAction acaoMover;
    private Collider2D corpo;
    private Vida vida;
    private int direcao = 1;
    private float proximoAtaque;

    void Awake()
    {
        entrada = GetComponent<PlayerInput>();
        corpo = GetComponent<Collider2D>();
        vida = GetComponentInChildren<Vida>();
    }

    void Start()
    {
        if (entrada == null)
        {
            Debug.LogWarning("[Ataque] Este objeto precisa ter o componente Player Input.");
            return;
        }

        acaoAtaque = entrada.actions.FindAction(nomeAcaoAtaque, false);
        acaoMover = entrada.actions.FindAction(nomeAcaoMover, false);

        // so le a direcao se a acao de mover for um Vector2
        if (acaoMover != null && acaoMover.expectedControlType != "Vector2")
            acaoMover = null;

        if (acaoAtaque == null)
        {
            Debug.LogWarning($"[Ataque] Nao achei a acao '{nomeAcaoAtaque}' no Input System.");
            return;
        }

        acaoAtaque.performed += AoAtacar;
    }

    void OnDestroy()
    {
        if (acaoAtaque != null) acaoAtaque.performed -= AoAtacar;
    }

    void Update()
    {
        // guarda para que lado o jogador andou por ultimo
        if (acaoMover == null) return;

        float x = acaoMover.ReadValue<Vector2>().x;
        if (x > 0.1f) direcao = 1;
        else if (x < -0.1f) direcao = -1;
    }

    private void AoAtacar(InputAction.CallbackContext contexto)
    {
        if (Time.timeScale == 0f) return;                 // pausado ou game over
        if (vida != null && vida.Morto) return;
        if (Time.time < proximoAtaque) return;

        proximoAtaque = Time.time + intervalo;

        if (mostrarDebug) Debug.Log("[Ataque] Player atacou");

        Invoke(nameof(Golpear), atrasoDoGolpe);
    }

    private Vector2 CentroDoGolpe()
    {
        if (corpo == null) corpo = GetComponent<Collider2D>();
        if (corpo == null) return transform.position;

        Bounds b = corpo.bounds;
        return new Vector2(
            b.center.x + direcao * (b.extents.x + tamanho.x * 0.5f),
            b.center.y);
    }

    private void Golpear()
    {
        if (vida != null && vida.Morto) return;

        Collider2D[] achados = Physics2D.OverlapBoxAll(CentroDoGolpe(), tamanho, 0f, camadaAlvos);
        HashSet<VidaInimigo> atingidos = new HashSet<VidaInimigo>();

        foreach (Collider2D c in achados)
        {
            VidaInimigo alvo = c.GetComponentInParent<VidaInimigo>();
            if (alvo != null && atingidos.Add(alvo))
                alvo.LevarDano(dano);
        }

        if (mostrarDebug)
            Debug.Log(atingidos.Count > 0
                ? $"[Ataque] Acertou {atingidos.Count} inimigo(s)"
                : "[Ataque] Ninguem na area do golpe");
    }

    // mostra a area do golpe em vermelho no Scene view quando o Player esta selecionado
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(CentroDoGolpe(), tamanho);
    }
}