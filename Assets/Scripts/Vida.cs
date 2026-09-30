using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class Vida : MonoBehaviour
{
    [SerializeField] private Slider barraDeVida;
    [SerializeField] private float vidaMaxima = 100f;
    [SerializeField] private float danoTeste = 10f;

    private float vidaAtual;
    public bool Morto { get; private set; }

    void Start()
    {
        vidaAtual = vidaMaxima;
        barraDeVida.maxValue = vidaMaxima;
        barraDeVida.value = vidaAtual;
    }

    void Update()
    {
        // Tecla de teste: H causa dano
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            LevarDano(danoTeste);
        }
    }

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