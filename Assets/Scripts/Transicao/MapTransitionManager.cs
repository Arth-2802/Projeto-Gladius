using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// Coordena a troca de mapa dentro da mesma cena.
/// </summary>
public class MapTransitionManager : MonoBehaviour
{
    public static MapTransitionManager Instance { get; private set; }

    [Header("Referencias da transicao")]
    [SerializeField] private Player player;
    [SerializeField] private CinemachineCamera cinemachineCamera;
    [SerializeField] private CinemachineConfiner2D confiner;
    [SerializeField] private ScreenFader fader;
    [SerializeField] private Collider2D[] mapBounds;

    [Header("Pausa com a tela preta")]
    [Tooltip("Pequeno intervalo para garantir que o teleporte aconteca totalmente escondido.")]
    [SerializeField, Min(0f)] private float blackScreenPause = 0.1f;

    private bool isTransitioning;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("Ja existe um MapTransitionManager na cena.", this);
            enabled = false;
            return;
        }

        Instance = this;

        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        if (cinemachineCamera == null)
        {
            cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
        }

        if (confiner == null && cinemachineCamera != null)
        {
            confiner = cinemachineCamera.GetComponent<CinemachineConfiner2D>();
        }

        if (fader == null)
        {
            fader = GetComponent<ScreenFader>();
        }

        ValidateReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // O save restaura a posicao dentro da mesma cena; a camera precisa acompanhar o mapa.
    public void RestoreCameraAfterLoad(Vector3 previousPosition)
    {
        if (player == null || confiner == null || cinemachineCamera == null) return;

        Physics2D.SyncTransforms();
        if (mapBounds != null)
        {
            foreach (Collider2D bounds in mapBounds)
            {
                if (bounds == null || !bounds.OverlapPoint(player.transform.position)) continue;
                confiner.BoundingShape2D = bounds;
                confiner.InvalidateBoundingShapeCache();
                break;
            }
        }

        cinemachineCamera.OnTargetObjectWarped(player.transform, player.transform.position - previousPosition);
    }

    public void StartTransition(
        Transform destinationSpawn,
        Collider2D destinationBounds,
        float fadeOutTime,
        float fadeInTime)
    {
        if (isTransitioning)
        {
            return;
        }

        if (destinationSpawn == null || destinationBounds == null || !ValidateReferences())
        {
            Debug.LogError("Nao foi possivel iniciar a transicao: faltam referencias obrigatorias.", this);
            return;
        }

        isTransitioning = true;
        StartCoroutine(TransitionRoutine(destinationSpawn, destinationBounds, fadeOutTime, fadeInTime));
    }

    private IEnumerator TransitionRoutine(
        Transform destinationSpawn,
        Collider2D destinationBounds,
        float fadeOutTime,
        float fadeInTime)
    {
        // Impede movimento e novos triggers enquanto a troca ocorre.
        player.playerInput.DeactivateInput();
        player.moveInput = Vector2.zero;
        player.rb.linearVelocity = Vector2.zero;
        player.rb.simulated = false;

        // Esconde o mundo antes de mudar a posicao e os limites da camera.
        yield return fader.FadeOut(fadeOutTime);

        // Mantem a tela preta por um instante para a troca nao ficar perceptivel.
        yield return new WaitForSecondsRealtime(blackScreenPause);

        Vector3 currentPosition = player.transform.position;
        Vector3 destinationPosition = destinationSpawn.position;
        destinationPosition.z = currentPosition.z;
        Vector3 delta = destinationPosition - currentPosition;

        // Move o Transform e o Rigidbody para manter renderizacao e fisica alinhadas.
        player.transform.position = destinationPosition;
        player.rb.position = new Vector2(destinationPosition.x, destinationPosition.y);
        Physics2D.SyncTransforms();

        // Atualiza o limite antes de a Cinemachine calcular a proxima camera.
        confiner.BoundingShape2D = destinationBounds;
        confiner.InvalidateBoundingShapeCache();

        // Informa o salto ao Cinemachine para o damping nao atravessar os mapas.
        cinemachineCamera.OnTargetObjectWarped(player.transform, delta);

        // Da tempo para a Cinemachine aplicar a nova posicao ainda com a tela preta.
        yield return null;
        yield return null;

        // Devolve controle somente depois de player, camera e confiner estarem prontos.
        player.rb.linearVelocity = Vector2.zero;
        player.rb.simulated = true;
        player.playerInput.ActivateInput();

        // Revela o mapa novo apos toda a troca estar concluida.
        yield return fader.FadeIn(fadeInTime);
        isTransitioning = false;
    }

    private bool ValidateReferences()
    {
        bool hasAllReferences = true;

        if (player == null)
        {
            Debug.LogError("MapTransitionManager: Player nao foi encontrado.", this);
            hasAllReferences = false;
        }

        if (cinemachineCamera == null)
        {
            Debug.LogError("MapTransitionManager: CinemachineCamera nao foi encontrada.", this);
            hasAllReferences = false;
        }

        if (confiner == null)
        {
            Debug.LogError("MapTransitionManager: CinemachineConfiner2D nao foi encontrado na CinemachineCamera.", this);
            hasAllReferences = false;
        }

        if (fader == null)
        {
            Debug.LogError("MapTransitionManager: ScreenFader deve estar no mesmo GameObject.", this);
            hasAllReferences = false;
        }

        if (player != null && player.rb == null)
        {
            Debug.LogError("MapTransitionManager: o Rigidbody2D do Player nao esta atribuido.", player);
            hasAllReferences = false;
        }

        if (player != null && player.playerInput == null)
        {
            Debug.LogError("MapTransitionManager: o PlayerInput do Player nao esta atribuido.", player);
            hasAllReferences = false;
        }

        return hasAllReferences;
    }
}
