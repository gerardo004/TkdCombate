using TkdCombate.Core;
using UnityEngine;

/// <summary>
/// Reproduce un sonido en 3 momentos exactos:
///   - Al empezar un round (transición a RoundEnCurso).
///   - Al empezar el descanso (transición a Descanso).
///   - Al mostrarse el ganador (evento MarcadorUI.CombateTerminado).
/// No suena en pausas, reanudaciones, tiempo médico ni nada más: solo esos 3 casos.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SonidosCombate : MonoBehaviour
{
    public MarcadorUI marcador;

    [Header("Arrastra aquí los archivos de audio")]
    public AudioClip sonidoInicioRound;
    public AudioClip sonidoDescanso;
    public AudioClip sonidoGanador;

    [Range(0f, 1f)] public float volumen = 1f;

    private AudioSource audioSource;
    private EstadoCombate estadoAnterior;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;

        estadoAnterior = marcador.Combate.Estado;   // normalmente "Preparado" al arrancar
        marcador.CombateCambio += RevisarCambioDeEstado;
        marcador.CombateTerminado += _ => Reproducir(sonidoGanador);
    }

    void RevisarCambioDeEstado()
    {
        EstadoCombate actual = marcador.Combate.Estado;
        if (actual == estadoAnterior) return;   // Cambio se dispara muy seguido (cada tick); solo nos
                                                 // interesa el instante en que el estado realmente cambia.

        if (actual == EstadoCombate.RoundEnCurso) Reproducir(sonidoInicioRound);
        else if (actual == EstadoCombate.Descanso) Reproducir(sonidoDescanso);

        estadoAnterior = actual;
    }

    void Reproducir(AudioClip clip)
    {
        if (clip != null) audioSource.PlayOneShot(clip, volumen);
    }
}
