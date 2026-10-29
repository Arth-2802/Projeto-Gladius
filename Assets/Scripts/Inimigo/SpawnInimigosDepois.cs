using System.Collections;
using UnityEngine;

public class SpawnInimigosDepois : MonoBehaviour
{
    [SerializeField] private GameObject[] inimigos;
    [SerializeField] private float tempoEspera = 3f;
    [SerializeField] private float intervaloEntreInimigos = 0.5f;
    [Tooltip("Marque para este grupo comecar a spawnar ao iniciar o jogo (use no mapa 1).")]
    [SerializeField] private bool iniciarAutomaticamente = false;

    private bool jaIniciou;

    void Awake()
    {
        foreach (GameObject i in inimigos)
            if (i != null) i.SetActive(false);
    }

    void Start()
    {
        if (iniciarAutomaticamente) Iniciar();
    }

    // chamado pelo MapExit quando o player chega neste mapa
    public void Iniciar()
    {
        if (jaIniciou) return;
        jaIniciou = true;
        StartCoroutine(Rotina());
    }

    IEnumerator Rotina()
    {
        yield return new WaitForSeconds(tempoEspera);

        foreach (GameObject i in inimigos)
        {
            if (i != null) i.SetActive(true);
            yield return new WaitForSeconds(intervaloEntreInimigos);
        }
    }
}