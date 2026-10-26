using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Painéis")]
    [SerializeField] private GameObject painelSlot;
    [SerializeField] private GameObject painelLoading;

    [Header("Textos dos perfis (2 itens: perfil 1 e perfil 2)")]
    [SerializeField] private TextMeshProUGUI[] textosSlot;

    [Header("Loading")]
    [SerializeField] private Slider barra;
    [SerializeField] private TextMeshProUGUI textoPorcentagem;
    [SerializeField] private float tempoMinimo = 2f;

    [Header("Jogo")]
    [SerializeField] private string cenaDoJogo = "GameScene (2)";

    private void Start()
    {
        painelSlot.SetActive(false);
        painelLoading.SetActive(false);
    }

    // Botão PLAY do menu
    public void AbrirSlot()
    {
        AtualizarSlots();
        painelSlot.SetActive(true);
    }

    // Botão Voltar
    public void FecharSlot()
    {
        painelSlot.SetActive(false);
    }

    // Botões dos perfis: passe 0 para o perfil 1 e 1 para o perfil 2
    public void SelecionarSlot(int slot)
    {
        string cena = GerenciadorSave.PrepararContinuar(slot, cenaDoJogo);
        StartCoroutine(CarregarJogo(cena));
    }

    // Botões Apagar (opcional): passe 0 ou 1
    public void ApagarSlot(int slot)
    {
        GerenciadorSave.ApagarSlot(slot);
        AtualizarSlots();
    }

    private void AtualizarSlots()
    {
        for (int i = 0; i < textosSlot.Length; i++)
            textosSlot[i].text = (i + 1) + ".   " + GerenciadorSave.ResumoSlot(i);
    }

    private IEnumerator CarregarJogo(string cena)
    {
        painelSlot.SetActive(false);
        painelLoading.SetActive(true);
        Time.timeScale = 1f;

        AsyncOperation op = SceneManager.LoadSceneAsync(cena);
        op.allowSceneActivation = false;

        float tempo = 0f;
        while (op.progress < 0.9f || tempo < tempoMinimo)
        {
            tempo += Time.unscaledDeltaTime;
            float real = Mathf.Clamp01(op.progress / 0.9f);
            float porTempo = Mathf.Clamp01(tempo / tempoMinimo);
            float valor = Mathf.Min(real, porTempo);

            barra.value = valor;
            textoPorcentagem.text = (valor * 100f).ToString("0") + "%";
            yield return null;
        }

        barra.value = 1f;
        textoPorcentagem.text = "100%";
        yield return new WaitForSecondsRealtime(0.3f);
        op.allowSceneActivation = true;
    }
}
