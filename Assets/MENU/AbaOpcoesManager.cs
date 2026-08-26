using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbaOpcoesManager : MonoBehaviour
{
    [Header("Textos das Abas")]
    [SerializeField] private TextMeshProUGUI textoVolume;
    [SerializeField] private TextMeshProUGUI textoMapeamento;

    [Header("Linhas indicadoras (embaixo do texto)")]
    [SerializeField] private GameObject linhaVolume;
    [SerializeField] private GameObject linhaMapeamento;

    [Header("Painéis de conteúdo")]
    [SerializeField] private GameObject painelVolume;
    [SerializeField] private GameObject painelMapeamento;

    [Header("Cores")]
    [SerializeField] private Color corAtiva = Color.red;
    [SerializeField] private Color corInativa = Color.white;

    private void OnEnable()
    {
        // Toda vez que o painel de Opções for reaberto, começa na aba Volume
        AbrirVolume();
    }

    public void AbrirVolume()
    {
        Debug.Log("Aba VOLUME selecionada");

        painelVolume.SetActive(true);
        painelMapeamento.SetActive(false);

        textoVolume.color = corAtiva;
        textoMapeamento.color = corInativa;

        linhaVolume.SetActive(true);
        linhaMapeamento.SetActive(false);
    }

    public void AbrirMapeamento()
    {
        Debug.Log("Aba MAPEAMENTO selecionada");

        painelVolume.SetActive(false);
        painelMapeamento.SetActive(true);

        textoVolume.color = corInativa;
        textoMapeamento.color = corAtiva;

        linhaVolume.SetActive(false);
        linhaMapeamento.SetActive(true);
    }
}