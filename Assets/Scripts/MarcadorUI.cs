using TkdCombate.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Marcador de combate. Solo MUESTRA el estado del núcleo y traduce teclas en órdenes.
/// Teclas de prueba (los mandos vendrán después; las 5 teclas = puño, peto, casco, giro peto, giro casco):
///   Espacio = iniciar / pausar / reanudar
///   Rojo:  Q W E R T = +1 +2 +3 +4 +5      A = gam-jeom
///   Azul:  Y U I O P = +1 +2 +3 +4 +5      L = gam-jeom
///   M = tiempo médico (iniciar / terminar)
/// </summary>
public class MarcadorUI : MonoBehaviour
{
    [Header("Textos (arrastra aquí desde la Hierarchy)")]
    public TMP_Text textoReloj;
    public TMP_Text textoRound;
    public TMP_Text textoEstado;
    public TMP_Text textoNombreRojo;
    public TMP_Text textoNombreAzul;
    public TMP_Text textoRojo;
    public TMP_Text textoAzul;
    public TMP_Text textoGamJeomRojo;
    public TMP_Text textoGamJeomAzul;

    [Header("Nombres de atletas")]
    public string nombreRojo = "HONG";
    public string nombreAzul = "CHONG";

    [Header("Configuración (se puede cambiar en el Inspector)")]
    public int segundosRound = 60;
    public int segundosDescanso = 10;
    public int segundosMedico = 30;
    public int diferenciaPuntos = 15;
    public bool diferenciaPuntosHabilitadaEnConfig = true;
    public int maximoGamJeoms = 5;
    public bool ganadorPorGamJeomsHabilitado = true;

    [Header("Mandos")]
    [Tooltip("Activado: un solo mando puntúa directo para los dos lados. Desactivado: deben coincidir 2 jueces.")]
    public bool unSoloMando = true;

    private Combate combate;

    /// <summary>El combate actual (los botones lo usan para dar órdenes).</summary>
    public Combate Combate { get { return combate; } }

    /// <summary>Se dispara cada vez que un combate termina (la instancia interna cambia en cada NuevoCombate).</summary>
    public event System.Action<ResultadoCombate> CombateTerminado;

    /// <summary>Se dispara en cada cambio del combate (reloj, puntos, cambio de estado...). Para saber
    /// SOLO cuándo cambia el estado, compara Combate.Estado contra el valor anterior en tu propio script.</summary>
    public event System.Action CombateCambio;

    /// <summary>Validación de puntos por coincidencia de jueces (los mandos votan aquí).</summary>
    public PanelJueces Jueces { get; private set; }

    private static readonly TipoAtaque[] Tipos =
    {
        TipoAtaque.Punio, TipoAtaque.PatadaPeto, TipoAtaque.PatadaCasco,
        TipoAtaque.GiroPeto, TipoAtaque.GiroCasco
    };

    void Awake()
    {
        NuevoCombate();   // en Awake para que otros scripts ya lo encuentren creado en su Start
    }

    /// <summary>Empieza un combate desde cero con la configuración del Inspector.</summary>
    public void NuevoCombate()
    {
        if (combate != null) combate.Cambio -= Refrescar;

        var config = new ConfiguracionCombate();
        config.SegundosRound = segundosRound;
        config.SegundosDescanso = segundosDescanso;
        config.SegundosMedico = segundosMedico;
        config.DiferenciaPuntos = diferenciaPuntos;
        config.DiferenciaPuntosHabilitada = diferenciaPuntosHabilitadaEnConfig;
        config.MaximoGamJeoms = maximoGamJeoms;
        config.GanadorPorGamJeomsHabilitado = ganadorPorGamJeomsHabilitado;
        config.UnSoloMando = unSoloMando;

        combate = new Combate(config);
        combate.Cambio += Refrescar;
        combate.Cambio += () => { if (CombateCambio != null) CombateCambio(); };
        combate.CombateTerminado += r => { if (CombateTerminado != null) CombateTerminado(r); };
        Jueces = new PanelJueces(combate);
        Refrescar();
    }

    void Update()
    {
        combate.Tick(Time.deltaTime);
        Jueces.Tick(Time.deltaTime);

        var k = Keyboard.current;
        if (k == null) return;

        if (k.spaceKey.wasPressedThisFrame)
        {
            if (!combate.IniciarRound())
                if (!combate.Pausar())
                    combate.Reanudar();
        }
        if (k.mKey.wasPressedThisFrame)
        {
            if (!combate.IniciarTiempoMedico())
                combate.TerminarTiempoMedico();
        }

        Key[] teclasRojo = { Key.Q, Key.W, Key.E, Key.R, Key.T };
        Key[] teclasAzul = { Key.Y, Key.U, Key.I, Key.O, Key.P };
        for (int i = 0; i < 5; i++)
        {
            if (k[teclasRojo[i]].wasPressedThisFrame) combate.SumarPunto(Lado.Rojo, Tipos[i]);
            if (k[teclasAzul[i]].wasPressedThisFrame) combate.SumarPunto(Lado.Azul, Tipos[i]);
        }
        if (k.aKey.wasPressedThisFrame) combate.SumarGamJeom(Lado.Rojo);
        if (k.lKey.wasPressedThisFrame) combate.SumarGamJeom(Lado.Azul);
    }

    void Refrescar()
    {
        double segundos = combate.Estado == EstadoCombate.TiempoMedico
            ? combate.SegundosMedicoRestantes
            : combate.SegundosRestantes;
        int s = Mathf.CeilToInt((float)segundos);
        textoReloj.text = string.Format("{0:00}:{1:00}", s / 60, s % 60);

        textoRound.text = "ROUND " + combate.RoundActual;
        textoEstado.text = TextoDeEstado();

        textoNombreRojo.text = nombreRojo;
        textoNombreAzul.text = nombreAzul;
        textoRojo.text = combate.PuntosRound(Lado.Rojo).ToString();
        textoAzul.text = combate.PuntosRound(Lado.Azul).ToString();
        textoGamJeomRojo.text = "Gam-Jeom: " + combate.GamJeoms(Lado.Rojo);
        textoGamJeomAzul.text = "Gam-Jeom: " + combate.GamJeoms(Lado.Azul);
    }

    string TextoDeEstado()
    {
        switch (combate.Estado)
        {
            case EstadoCombate.Preparado: return "EN ESPERA";
            case EstadoCombate.RoundEnCurso: return "COMBATE";
            case EstadoCombate.RoundPausado: return "PAUSA";
            case EstadoCombate.TiempoMedico: return "TIEMPO MÉDICO";
            case EstadoCombate.Descanso: return "DESCANSO";
            case EstadoCombate.PuntoDeOro:
                return combate.TiempoPuntoDeOroAgotado ? "PUNTO DE ORO - TIEMPO AGOTADO" : "PUNTO DE ORO";
            case EstadoCombate.Finalizado:
                string ganador = combate.Resultado.Ganador == Lado.Rojo ? nombreRojo : nombreAzul;
                return "GANADOR: " + ganador;
        }
        return "";
    }
}
