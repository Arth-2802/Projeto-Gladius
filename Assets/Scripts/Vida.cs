using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Vida : MonoBehaviour
{
    // ---------- campos que aparecem no Inspector ----------
    [SerializeField] private Slider barraDeVida;        // a barra de vida na tela
    [SerializeField] private float vidaMaxima = 100f;   // quanta vida o personagem tem
    [SerializeField] private float danoTeste = 10f;     // dano da tecla de teste (H)

    // ---------- dados internos ----------
    private float vidaAtual;                            // a vida que ele tem agora
    public bool Morto { get; private set; }             // true quando a vida zerou

    // ---------- NOVO: deixa o sistema de save LER a vida ----------
    public float VidaAtual => vidaAtual;

    // ---------- NOVO: deixa o sistema de save COLOCAR a vida ----------
    public void DefinirVida(float valor)
    {
        vidaAtual = Mathf.Clamp(valor, 1f, vidaMaxima);
        barraDeVida.value = vidaAtual;
    }

    // ---------- ao iniciar a fase: vida cheia ----------
    void Start()
    {
        vidaAtual = vidaMaxima;
        barraDeVida.maxValue = vidaMaxima;
        barraDeVida.value = vidaAtual;
    }

    // ---------- a cada quadro: tecla H causa dano (so para teste) ----------
    void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            LevarDano(danoTeste);
        }
    }

    // ---------- tirar vida (o inimigo chama isto) ----------
    public void LevarDano(float dano)
    {
        if (Morto) return;

        vidaAtual = Mathf.Clamp(vidaAtual - dano, 0, vidaMaxima);
        barraDeVida.value = vidaAtual;

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    // ---------- quando a vida zera ----------
    private void Morrer()
    {
        Morto = true;
        Debug.Log("Personagem morreu");

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }
    }
}