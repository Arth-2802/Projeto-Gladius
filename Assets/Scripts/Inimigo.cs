using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class Inimigo : MonoBehaviour
{
    [Header("Alvo (deixe vazio para achar o Player sozinho)")]
    [SerializeField] private Vida alvo;

    [Header("Movimento")]
    [SerializeField] private float velocidade = 2f;
    [Tooltip("Espaco livre entre o corpo dele e o do Player para comecar a atacar")]
    [SerializeField] private float alcanceAtaque = 0.6f;

    [Header("Ataque")]
    [SerializeField] private float dano = 10f;
    [SerializeField] private float intervaloAtaque = 1.2f;
    [SerializeField] private int frameDoGolpe = 3;
    [SerializeField] private bool mostrarDebug = true;

    [Header("Visual")]
    [SerializeField] private bool desenhoOlhaParaDireita = true;
    [SerializeField] private Sprite[] andar;
    [SerializeField] private Sprite[] atacar;
    [SerializeField] private float fpsAndar = 8f;
    [SerializeField] private float fpsAtaque = 10f;

    [Header("Efeito do golpe (opcional)")]
    [SerializeField] private Sprite[] efeitoGolpe;
    [SerializeField] private Vector2 posicaoEfeito = new Vector2(1.2f, 0.5f);

    private enum Estado { Andando, Atacando }

    private Estado estado = Estado.Andando;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Collider2D meuCol;
    private Collider2D colAlvo;
    private int direcao = 1;
    private float moverX;
    private float proximoAtaque;
    private float tempoAnim;
    private float tempoAtaque;
    private bool golpeDado;

    void Start()
{
    sr = GetComponent<SpriteRenderer>();
    rb = GetComponent<Rigidbody2D>();
    meuCol = GetComponent<Collider2D>();

    if (alvo == null) alvo = EncontrarAlvo();

    if (alvo == null)
    {
        Debug.LogWarning("Inimigo: nao achei o Player. Coloque a Tag 'Player' nele.");
        return;
    }

    Collider2D[] colsDoAlvo = alvo.GetComponentsInChildren<Collider2D>();

    if (colsDoAlvo.Length == 0)
    {
        Debug.LogWarning("Inimigo: o Player nao tem Collider2D, vou medir pelo centro.");
        return;
    }

    colAlvo = colsDoAlvo[0];

    // o inimigo e o Player nao colidem entre si (chao e paredes continuam valendo)
    foreach (Collider2D c in colsDoAlvo)
        Physics2D.IgnoreCollision(meuCol, c);
}

    private Vida EncontrarAlvo()
    {
        GameObject jogador = GameObject.FindGameObjectWithTag("Player");
        if (jogador != null)
        {
            Vida v = jogador.GetComponentInChildren<Vida>();
            if (v != null) return v;
        }

        foreach (Vida outro in FindObjectsByType<Vida>())
        {
            if (outro.gameObject != gameObject) return outro;
        }
        return null;
    }

    // folgaX: espaco livre entre os dois corpos (0 = encostados)
    // alinhadoY: os dois corpos estao na mesma altura
    // sinal: 1 se o Player esta a direita, -1 se esta a esquerda
    private void Medir(out float folgaX, out bool alinhadoY, out int sinal)
    {
        Bounds a = meuCol.bounds;
        Bounds b = colAlvo != null
            ? colAlvo.bounds
            : new Bounds(alvo.transform.position, Vector3.one * 0.5f);

        float dx = b.center.x - a.center.x;
        sinal = dx >= 0 ? 1 : -1;
        folgaX = Mathf.Max(0f, Mathf.Abs(dx) - (a.extents.x + b.extents.x));
        alinhadoY = a.min.y <= b.max.y && a.max.y >= b.min.y;
    }

    void Update()
    {
        moverX = 0f;

        if (alvo == null) return;

        if (alvo.Morto)
        {
            MostrarSprite(andar, 0);
            return;
        }

        Medir(out float folgaX, out bool alinhadoY, out int sinal);

        if (estado == Estado.Andando)
        {
            direcao = sinal;
            sr.flipX = desenhoOlhaParaDireita ? direcao < 0 : direcao > 0;

            if (folgaX > alcanceAtaque)
            {
                moverX = direcao;
                tempoAnim += Time.deltaTime;
                MostrarSprite(andar, (int)(tempoAnim * fpsAndar));
            }
            else
            {
                MostrarSprite(andar, 0);

                if (alinhadoY && Time.time >= proximoAtaque)
                {
                    estado = Estado.Atacando;
                    tempoAtaque = 0f;
                    golpeDado = false;
                    proximoAtaque = Time.time + intervaloAtaque;
                }
            }
        }
        else
        {
            tempoAtaque += Time.deltaTime;
            int frame = (int)(tempoAtaque * fpsAtaque);
            int total = (atacar != null && atacar.Length > 0) ? atacar.Length : frameDoGolpe + 2;

            if (frame >= total)
            {
                estado = Estado.Andando;
                return;
            }

            MostrarSprite(atacar, frame);

            if (frame >= frameDoGolpe && !golpeDado)
            {
                golpeDado = true;
                Golpear();
            }
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moverX * velocidade, rb.linearVelocity.y);
    }

    private void MostrarSprite(Sprite[] quadros, int indice)
    {
        if (quadros == null || quadros.Length == 0) return;
        sr.sprite = quadros[indice % quadros.Length];
    }

    private void Golpear()
    {
        Medir(out float folgaX, out bool alinhadoY, out int sinal);

        // margem extra: a espada alcanca um pouco alem do ponto onde ele parou
        bool acertou = folgaX <= alcanceAtaque + 0.5f && alinhadoY;

        if (acertou) alvo.LevarDano(dano);

        if (mostrarDebug)
            Debug.Log(acertou
                ? "Inimigo: ACERTOU"
                : $"Inimigo: ERROU (folga {folgaX:F2}, mesma altura: {alinhadoY})");

        if (efeitoGolpe == null || efeitoGolpe.Length == 0) return;

        Vector3 pos = transform.position + new Vector3(posicaoEfeito.x * direcao, posicaoEfeito.y, 0f);

        GameObject fx = new GameObject("EfeitoGolpe");
        fx.transform.position = pos;
        fx.transform.localScale = transform.localScale;
        fx.AddComponent<EfeitoEspada>().Iniciar(efeitoGolpe, direcao, sr.sortingLayerID, sr.sortingOrder + 1);
    }
}