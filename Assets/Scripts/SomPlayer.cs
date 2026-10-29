using System.Collections.Generic;
using UnityEngine;

public class SomPlayer : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private AudioClip andar;
    [SerializeField] private AudioClip corrida;
    [SerializeField] private AudioClip dash;
    [SerializeField] private AudioClip doubleJump;

    [Header("Combate")]
    [SerializeField] private AudioClip ataque;
    [SerializeField] private AudioClip acerto;

    [Header("Ainda sem mecanica (pode deixar vazio)")]
    [SerializeField] private AudioClip ataqueApolo;
    [SerializeField] private AudioClip estalarDedos;
    [SerializeField] private AudioClip caindo;

    [Header("Pular silencio do inicio (segundos)")]
    [SerializeField] private float inicioAndar = 0f;
    [SerializeField] private float inicioDash = 0f;
    [SerializeField] private float inicioDoubleJump = 0f;
    [SerializeField] private float inicioAtaque = 0f;
    [SerializeField] private float inicioAcerto = 0f;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

    private AudioSource passos;
    private readonly Dictionary<AudioClip, AudioSource> fontes = new Dictionary<AudioClip, AudioSource>();

    void Awake()
    {
        passos = gameObject.AddComponent<AudioSource>();
        passos.playOnAwake = false;
        passos.loop = true;
        passos.clip = andar;
    }

    // uma fonte por clipe, para poder comecar do meio do arquivo
    private void Tocar(AudioClip clip, float inicio)
    {
        if (clip == null) return;

        if (!fontes.TryGetValue(clip, out AudioSource src))
        {
            src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.clip = clip;
            fontes[clip] = src;
        }

        src.volume = volume;
        src.time = Mathf.Clamp(inicio, 0f, clip.length - 0.01f);
        src.Play();
    }

    public void Passos(bool andando)
    {
        if (passos.clip == null) return;

        if (andando && !passos.isPlaying)
        {
            passos.volume = volume;
            passos.time = Mathf.Clamp(inicioAndar, 0f, passos.clip.length - 0.01f);
            passos.Play();
        }
        else if (!andando && passos.isPlaying)
        {
            passos.Stop();
        }
    }

    public void Dash()        { Tocar(dash, inicioDash); }
    public void DoubleJump()  { Tocar(doubleJump, inicioDoubleJump); }
    public void Ataque()      { Tocar(ataque, inicioAtaque); }
    public void Acerto()      { Tocar(acerto, inicioAcerto); }

    public void AtaqueApolo()  { Tocar(ataqueApolo, 0f); }
    public void EstalarDedos() { Tocar(estalarDedos, 0f); }
    public void Caindo()       { Tocar(caindo, 0f); }
}