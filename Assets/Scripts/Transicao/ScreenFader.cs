using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Cria uma sobreposicao preta em tela cheia para transicoes entre mapas.
/// </summary>
public class ScreenFader : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        CreateOverlay();
    }

    /// <summary>
    /// Escurece a tela sem depender do timeScale.
    /// </summary>
    public IEnumerator FadeOut(float duration)
    {
        yield return Fade(0f, 1f, duration);
    }

    /// <summary>
    /// Revela a tela sem depender do timeScale.
    /// </summary>
    public IEnumerator FadeIn(float duration)
    {
        yield return Fade(1f, 0f, duration);
    }

    private void CreateOverlay()
    {
        GameObject canvasObject = new GameObject(
            "ScreenFaderCanvas",
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup));

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        canvasGroup = canvasObject.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        GameObject imageObject = new GameObject("BlackOverlay", typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform imageTransform = imageObject.GetComponent<RectTransform>();
        imageTransform.anchorMin = Vector2.zero;
        imageTransform.anchorMax = Vector2.one;
        imageTransform.offsetMin = Vector2.zero;
        imageTransform.offsetMax = Vector2.zero;

        Image image = imageObject.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (duration <= 0f)
        {
            canvasGroup.alpha = endAlpha;
            yield break;
        }

        float elapsed = 0f;
        canvasGroup.alpha = startAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
}
