using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class Mapeamento : MonoBehaviour
{
    [Header("Action")]
    [SerializeField] private InputActionReference actionReference;

    [Header("Se for parte de um composite (ex: Move Left/Right)")]
    [SerializeField] private bool ehParteDeComposite = false;
    [SerializeField] private string nomeDaParte = ""; // "Left" ou "Right"

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoTecla;
    [SerializeField] private Button botaoTecla;

    private int bindingIndex;
    private InputActionRebindingExtensions.RebindingOperation rebindOperation;

    private void OnEnable()
    {
        bindingIndex = EncontrarBindingIndex();
        AtualizarTexto();
        botaoTecla.onClick.AddListener(IniciarRebind);
    }

    private void OnDisable()
    {
        botaoTecla.onClick.RemoveListener(IniciarRebind);
        rebindOperation?.Dispose();
    }

    private int EncontrarBindingIndex()
    {
        var bindings = actionReference.action.bindings;

        if (!ehParteDeComposite)
        {
            // Action simples: pega o primeiro binding de teclado
            for (int i = 0; i < bindings.Count; i++)
                if (bindings[i].effectivePath.Contains("Keyboard"))
                    return i;
            return 0;
        }
        else
        {
            // Composite: pega o PRIMEIRO binding cujo nome da parte bate
            // (no seu caso, o primeiro "Left"/"Right" é sempre o de letra, não o de seta)
            for (int i = 0; i < bindings.Count; i++)
                if (bindings[i].isPartOfComposite &&
                    bindings[i].name.ToLower() == nomeDaParte.ToLower())
                    return i;
            return -1;
        }
    }

    private void AtualizarTexto()
    {
        if (bindingIndex < 0) { textoTecla.text = "?"; return; }
        textoTecla.text = actionReference.action
            .GetBindingDisplayString(bindingIndex)
            .ToUpper();
    }

    private void IniciarRebind()
    {
        if (bindingIndex < 0) return;

        textoTecla.text = "...";
        actionReference.action.Disable();

        rebindOperation = actionReference.action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(operation =>
            {
                AtualizarTexto();
                actionReference.action.Enable();
                operation.Dispose();
                SalvarBindings();
            })
            .OnCancel(operation =>
            {
                AtualizarTexto();
                actionReference.action.Enable();
                operation.Dispose();
            })
            .Start();
    }

    private void SalvarBindings()
    {
        string json = actionReference.action.actionMap.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("bindings_" + actionReference.action.actionMap.name, json);
        PlayerPrefs.Save();
    }
}
