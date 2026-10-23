using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// configuracoes (valem para todos os slots)
[System.Serializable]
public class Configuracoes
{
    public string teclas = "";
}

// progresso de UM slot
[System.Serializable]
public class DadosSave
{
    public bool criado;          // o perfil foi iniciado (Novo Jogo)
    public bool temProgresso;    // ja existe posicao/vida salvas
    public string cena;
    public float posX;
    public float posY;
    public float vida;
}

public class GerenciadorSave : MonoBehaviour
{
    public const int TotalSlots = 2;

    [Header("Teclas (preencha em todas as cenas)")]
    [SerializeField] private InputActionAsset acoes;

    [Header("So na cena do JOGO (vazio = acha o Player pela Tag)")]
    [SerializeField] private Transform jogador;
    [SerializeField] private Vida vida;
    [SerializeField] private float salvarACadaSegundos = 30f;

    [Header("So para teste no editor (carrega o save so na 1a vez que der Play)")]
    [SerializeField] private bool carregarSempreAoDarPlay = false;

    // ---------- estado compartilhado entre as cenas ----------
    private static Configuracoes config;
    private static readonly DadosSave[] slots = new DadosSave[TotalSlots];
    private static int slotAtual;          // slot que esta sendo jogado (0 = perfil 1)
    private static bool carregarAoEntrar;
    private static bool editorJaCarregou;
    private static bool saindo;            // true quando o jogo/editor esta fechando

    // NOVO: o menu escuta este evento para atualizar os textos dos slots
    public static event System.Action SlotsAlterados;

    // NOVO: o gerenciador da cena atual (para salvar de fora, ex.: botao "Voltar ao menu")
    public static GerenciadorSave Instancia { get; private set; }

    // ---------- estado desta cena ----------
    private bool pronto;
    private float proximoSave;
    private string nomeCena;

    // ultimo estado valido do jogador (usado para salvar ao sair da cena)
    private Vector2 ultimaPos;
    private float ultimaVida;
    private bool temUltimo;
    private bool ultimoMorto;

    private static string CaminhoConfig => Path.Combine(Application.persistentDataPath, "config.json");
    private static string CaminhoSlot(int i) => Path.Combine(Application.persistentDataPath, "save_" + (i + 1) + ".json");

    // evita dados velhos quando o editor nao recarrega o dominio ao dar Play
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ReiniciarEstatico()
    {
        config = null;
        System.Array.Clear(slots, 0, slots.Length);
        slotAtual = 0;
        carregarAoEntrar = false;
        editorJaCarregou = false;
        saindo = false;
        SlotsAlterados = null;
        Instancia = null;
    }

    private static void AvisarMudanca()
    {
        if (!saindo) SlotsAlterados?.Invoke();
    }

    // ---------- configuracoes (teclas) ----------
    private static Configuracoes Config
    {
        get
        {
            if (config == null) LerConfig();
            return config;
        }
    }

    private static void LerConfig()
    {
        config = new Configuracoes();
        if (!File.Exists(CaminhoConfig)) return;

        try
        {
            Configuracoes lido = JsonUtility.FromJson<Configuracoes>(File.ReadAllText(CaminhoConfig));
            if (lido != null) config = lido;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao ler as configuracoes: " + e.Message);
        }
    }

    private static void GravarConfig()
    {
        try { File.WriteAllText(CaminhoConfig, JsonUtility.ToJson(Config, true)); }
        catch (System.Exception e) { Debug.LogError("Erro ao gravar as configuracoes: " + e.Message); }
    }

    // chame estes dois no seu script de rebind
    public static void SalvarTeclas(InputActionAsset a)
    {
        Config.teclas = a.SaveBindingOverridesAsJson();
        GravarConfig();
        Debug.Log("[Save] Teclas salvas.");
    }

    public static void ResetarTeclas(InputActionAsset a)
    {
        a.RemoveAllBindingOverrides();
        Config.teclas = "";
        GravarConfig();
    }

    // ---------- slots ----------
    private static DadosSave Slot(int i)
    {
        if (slots[i] == null) LerSlot(i);
        return slots[i];
    }

    private static void LerSlot(int i)
    {
        slots[i] = new DadosSave();
        if (!File.Exists(CaminhoSlot(i))) return;

        try
        {
            DadosSave lido = JsonUtility.FromJson<DadosSave>(File.ReadAllText(CaminhoSlot(i)));
            if (lido != null)
            {
                if (lido.temProgresso) lido.criado = true; // saves antigos
                slots[i] = lido;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao ler o slot " + (i + 1) + ": " + e.Message);
        }
    }

    private static void GravarSlot(int i)
    {
        try { File.WriteAllText(CaminhoSlot(i), JsonUtility.ToJson(Slot(i), true)); }
        catch (System.Exception e) { Debug.LogError("Erro ao gravar o slot " + (i + 1) + ": " + e.Message); }

        AvisarMudanca();
    }

    // ---------- usado pelo MenuController ----------
    public static bool SlotCriado(int slot) => Slot(slot).criado;

    public static string ResumoSlot(int slot)
    {
        DadosSave d = Slot(slot);
        if (!d.criado) return "Novo Jogo";
        return d.temProgresso ? "Vida: " + d.vida.ToString("0") : "Jogo iniciado";
    }

    // cria o perfil do zero, marca como o slot em uso e devolve a cena que deve ser carregada
    public static string PrepararNovoJogo(int slot, string cenaPadrao)
    {
        slotAtual = slot;
        slots[slot] = new DadosSave { criado = true };
        GravarSlot(slot);
        carregarAoEntrar = false;
        Debug.Log("[Save] Novo jogo no slot " + (slot + 1));
        return cenaPadrao;
    }

    // marca o slot em uso e devolve a cena que deve ser carregada (se o slot esta vazio, vira jogo novo)
    public static string PrepararContinuar(int slot, string cenaPadrao)
    {
        DadosSave d = Slot(slot);
        if (!d.criado) return PrepararNovoJogo(slot, cenaPadrao);

        slotAtual = slot;
        carregarAoEntrar = d.temProgresso;
        Debug.Log("[Save] Continuar no slot " + (slot + 1));
        return string.IsNullOrEmpty(d.cena) ? cenaPadrao : d.cena;
    }

    public static void ApagarSlot(int slot)
    {
        slots[slot] = new DadosSave();
        if (File.Exists(CaminhoSlot(slot))) File.Delete(CaminhoSlot(slot));
        Debug.Log("[Save] Slot " + (slot + 1) + " apagado.");
        AvisarMudanca();
    }

    // NOVO: salva agora, de qualquer lugar. Chame antes de carregar o menu
    // (ex.: no botao "Voltar ao menu" do pause), assim o menu ja abre atualizado.
    public static void SalvarAgora()
    {
        if (Instancia != null) Instancia.SalvarJogo();
    }

    // ---------- botao "Tentar novamente" da tela de game over ----------
    // recomeca a cena sem aplicar o save (vida volta cheia)
    public static void TentarNovamente()
    {
        carregarAoEntrar = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ---------- inicio da cena ----------
    void Awake()
    {
        Instancia = this;

        Debug.Log("[Save] Pasta: " + Application.persistentDataPath);

        if (acoes != null && !string.IsNullOrEmpty(Config.teclas))
            acoes.LoadBindingOverridesFromJson(Config.teclas);
    }

    IEnumerator Start()
    {
        nomeCena = gameObject.scene.name;

        // se nao existe Player na cena (menu), nao ha o que salvar
        if (jogador == null)
        {
            GameObject go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) jogador = go.transform;
        }
        if (jogador == null) yield break;

        if (vida == null) vida = jogador.GetComponentInChildren<Vida>();

        // NOVO: mostra qual slot esta valendo nesta cena
        Debug.Log("[Save] Cena de jogo usando o SLOT " + (slotAtual + 1));

        // espera um quadro para o Start do Vida rodar antes
        yield return null;

        // o teste do editor so vale na primeira vez; depois de morrer nao reaplica o save
        bool aplicar = carregarAoEntrar || (carregarSempreAoDarPlay && !editorJaCarregou);
        editorJaCarregou = true;
        carregarAoEntrar = false;
        if (aplicar) AplicarProgresso();

        pronto = true;
        proximoSave = Time.time + salvarACadaSegundos;
    }

    private void AplicarProgresso()
    {
        DadosSave d = Slot(slotAtual);

        if (!d.temProgresso)
        {
            Debug.Log("[Save] Nao ha progresso salvo para carregar.");
            return;
        }
        if (d.cena != nomeCena)
        {
            Debug.Log("[Save] O save e de outra cena: " + d.cena);
            return;
        }

        Vector3 posicaoAnterior = jogador.position;
        jogador.position = new Vector3(d.posX, d.posY, jogador.position.z);

        Rigidbody2D rb = jogador.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = jogador.position;
            rb.linearVelocity = Vector2.zero;
        }

        if (MapTransitionManager.Instance != null)
            MapTransitionManager.Instance.RestoreCameraAfterLoad(posicaoAnterior);

        if (vida != null) vida.DefinirVida(d.vida);

        Debug.Log("[Save] Progresso carregado (slot " + (slotAtual + 1) + ").");
    }

    // ---------- salvar o progresso ----------
    private void GravarProgresso(Vector2 pos, float vidaAtual)
    {
        DadosSave d = Slot(slotAtual);
        d.criado = true;
        d.temProgresso = true;
        d.cena = nomeCena;
        d.posX = pos.x;
        d.posY = pos.y;
        d.vida = vidaAtual;
        GravarSlot(slotAtual);

        Debug.Log($"[Save] Slot {slotAtual + 1} salvo: vida {vidaAtual}");
    }

    // pode ser chamado tambem por um botao (ex.: "Voltar ao menu" do pause)
    public void SalvarJogo()
    {
        // nao salva antes de carregar, nem depois de morrer (senao salvaria vida 0)
        if (!pronto || jogador == null || vida == null || vida.Morto) return;

        GravarProgresso(jogador.position, vida.VidaAtual);
    }

    void Update()
    {
        if (!pronto) return;

        // guarda o ultimo estado valido (para salvar ao sair da cena)
        if (jogador != null && vida != null)
        {
            ultimoMorto = vida.Morto;
            if (!ultimoMorto)
            {
                ultimaPos = jogador.position;
                ultimaVida = vida.VidaAtual;
                temUltimo = true;
            }
        }

        if (Time.time >= proximoSave)
        {
            SalvarJogo();
            proximoSave = Time.time + salvarACadaSegundos;
        }
    }

    // salva ao sair da cena (ex.: voltar ao menu), a menos que o jogador tenha morrido
    void OnDestroy()
    {
        if (pronto && temUltimo && !ultimoMorto)
            GravarProgresso(ultimaPos, ultimaVida);

        if (Instancia == this) Instancia = null;
    }

    void OnApplicationQuit()
    {
        saindo = true; // nao avisa o menu ao fechar o Play (os objetos dele ja estao sendo destruidos)
        SalvarJogo();
    }
}
