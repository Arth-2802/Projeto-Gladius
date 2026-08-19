using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string nomeDoLevelDeJogo;
    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;

    public void Jogar()
    {
        Debug.Log("CLIQUEI NO PLAY");
        SceneManager.LoadScene(nomeDoLevelDeJogo);
    }

    public void AbrirOpcoes()
    {
        Debug.Log("CLIQUEI EM OPÇÕES");
        painelMenuInicial.SetActive(false);
        painelOpcoes.SetActive(true);
        Debug.Log("Painel opções ativo: " + painelOpcoes.activeSelf);
    }

    public void FecharOpcoes()
    {
        Debug.Log("CLIQUEI EM FECHAR OPÇÕES");
        painelOpcoes.SetActive(false);
        painelMenuInicial.SetActive(true);
    }

    public void SairJogo()
    {
        Debug.Log("CLIQUEI EM SAIR");
        Application.Quit();
    }

    // ✅ Métodos obrigatórios das interfaces
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"[Foco] Mouse entrou em: {gameObject.name}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"[Foco] Mouse saiu de: {gameObject.name}");
    }
}