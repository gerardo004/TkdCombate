namespace TkdCombate.Core
{
    public enum Lado { Rojo = 0, Azul = 1 }

    /// <summary>Identifica la técnica. Los puntos de cada una salen de ConfiguracionCombate.PuntosPorTipo.</summary>
    public enum TipoAtaque
    {
        Punio = 1,
        PatadaPeto = 2,
        PatadaCasco = 3,
        GiroPeto = 4,   // técnica de giro al peto
        GiroCasco = 5   // técnica de giro al casco
    }

    public enum EstadoCombate
    {
        Preparado,      // listo para iniciar el round RoundActual
        RoundEnCurso,
        RoundPausado,
        TiempoMedico,
        Descanso,
        PuntoDeOro,
        Finalizado
    }

    public enum MotivoVictoria
    {
        Rounds,
        DiferenciaDePuntos,
        GamJeoms,
        PuntoDeOro,
        Superioridad,
        Abandono,
        DecisionArbitral
    }

    public static class LadoExt
    {
        public static Lado Otro(this Lado l) { return l == Lado.Rojo ? Lado.Azul : Lado.Rojo; }
    }

    public sealed class ResultadoRound
    {
        public int Numero;
        public int PuntosRojo;
        public int PuntosAzul;
        /// <summary>null si el round terminó empatado.</summary>
        public Lado? Ganador;
        /// <summary>true si el round terminó porque un lado llegó al máximo de gam-jeoms.</summary>
        public bool PorGamJeoms;
    }

    public sealed class ResultadoCombate
    {
        public Lado Ganador;
        public MotivoVictoria Motivo;
    }
}
