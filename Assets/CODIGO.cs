using UnityEngine;
using UnityEngine.EventSystems;

public class CODIGO : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject caixaHover;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        caixaHover.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        caixaHover.SetActive(false);
    }
}