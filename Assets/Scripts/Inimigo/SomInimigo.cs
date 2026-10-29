using UnityEngine;

public class SomInimigo : MonoBehaviour
{
    [Header("Sons")]
    [SerializeField] private AudioClip espadaSwing;   // ao iniciar o golpe
    [SerializeField] private AudioClip espadaAcerto;  // quando acerta o player

    [Header("Pular silencio do inicio (segundos)")]
    [SerializeField] private float inicioSwing = 0f;
    [SerializeField] private float inicioAcerto = 0f;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.5f;

    private AudioSource srcSwing;
    private AudioSource srcAcerto;

    void Awake()
    {
        srcSwing = CriarFonte(espadaSwing);
        srcAcerto = CriarFonte(espadaAcerto);
    }

    private AudioSource CriarFonte(AudioClip clip)
    {
        AudioSource s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.clip = clip;
        return s;
    }

    private void Tocar(AudioSource s, float inicio)
    {
        if (s == null || s.clip == null) return;
        s.volume = volume;
        s.time = Mathf.Clamp(inicio, 0f, s.clip.length - 0.01f);
        s.Play();
    }

    public void Swing()  { Tocar(srcSwing, inicioSwing); }
    public void Acerto() { Tocar(srcAcerto, inicioAcerto); }
}