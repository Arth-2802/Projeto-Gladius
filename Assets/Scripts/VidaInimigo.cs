using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class VidaInimigo : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private float vidaMaxima = 50f;
    [SerializeField] private bool mostrarNoConsole = true;   // a vida nao aparece na tela, so no Console

    [Header("Teste")]
    [SerializeField] private bool usarTeclaDeTeste = true;
    [SerializeField] private Key teclaDeTeste = Key.J;
    [SerializeField] private float danoDeTeste = 10f;

    [Header("Morte")]
    [SerializeField] private float tempoAteSumir = 0.5f;     // 0 = some na hora

    private float vidaAtual;
    private bool morto;
    private SpriteRenderer sr;
    private Color corOriginal = Color.white;

    public bool Morto => morto;
    public float VidaAtual => vidaAtual;

    void Awake()
    {
        vidaAtual = vidaMaxima;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) corOriginal = sr.color;
    }

    void Update()
    {
        if (!usarTeclaDeTeste || morto) return;

        Keyboard teclado = Keyboard.current;
        if (teclado != null && teclado[teclaDeTeste].wasPressedThisFrame)
            LevarDano(danoDeTeste);
    }

    // o ataque do Player vai chamar este metodo
    public void LevarDano(float dano)
    {
        if (morto) return;

        vidaAtual = Mathf.Max(0f, vidaAtual - dano);

        if (mostrarNoConsole)
            Debug.Log($"{name}: vida {vidaAtual}/{vidaMaxima}");

        StopAllCoroutines();

        if (vidaAtual <= 0f)
            Morrer();
        else
            StartCoroutine(Piscar());
    }

    private IEnumerator Piscar()
    {
        if (sr != null) sr.color = new Color(1f, 0.4f, 0.4f, 1f);
        yield return new WaitForSeconds(0.1f);
        if (sr != null) sr.color = corOriginal;
    }

    private void Morrer()
    {
        morto = true;

        // para de andar e de atacar
        Inimigo comportamento = GetComponent<Inimigo>();
        if (comportamento != null) comportamento.enabled = false;

        // congela no lugar enquanto some
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (tempoAteSumir <= 0f)
            Destroy(gameObject);
        else
            StartCoroutine(Sumir());
    }

    private IEnumerator Sumir()
    {
        float t = 0f;
        while (t < tempoAteSumir)
        {
            t += Time.deltaTime;
            if (sr != null)
            {
                Color c = corOriginal;
                c.a = 1f - t / tempoAteSumir;
                sr.color = c;
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}