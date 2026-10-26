using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance;

    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private CanvasGroup painelCanvasGroup;
    [SerializeField] private float duracaoFade = 1.5f;
    [SerializeField] private string menuSceneName = "MENU";

    private void Awake()
    {
        Instance = this;
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        painelCanvasGroup.alpha = 0f;

        float tempo = 0f;
        while (tempo < duracaoFade)
        {
            tempo += Time.unscaledDeltaTime;
            painelCanvasGroup.alpha = tempo / duracaoFade;
            yield return null;
        }

        painelCanvasGroup.alpha = 1f;
        Time.timeScale = 0f;
    }

    public void TryAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}