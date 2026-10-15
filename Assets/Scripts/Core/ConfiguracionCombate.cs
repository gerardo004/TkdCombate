namespace TkdCombate.Core
{
    /// <summary>
    /// Todo lo que el reglamento o la federación pueden cambiar vive aquí, nunca "quemado" en la lógica.
    /// </summary>
    public sealed class ConfiguracionCombate
    {
        /// <summary>
        /// Puntos por técnica (índice = valor del enum TipoAtaque; el 0 no se usa).
        /// Tabla WT 2026: puño 1, patada al peto 2, patada a la cabeza 3, giro al peto 4, giro a la cabeza 6.
        /// </summary>
        public int[] PuntosPorTipo = { 0, 1, 2, 3, 4, 6 };

        public int PuntosDe(TipoAtaque tipo) { return PuntosPorTipo[(int)tipo]; }

        public int RoundsTotales = 3;
        public int SegundosRound = 120;
        public int SegundosDescanso = 60;
        public int SegundosMedico = 60;
        public int SegundosPuntoDeOro = 60;

        /// <summary>Si un lado saca esta ventaja DENTRO DEL ROUND, el round termina de inmediato.</summary>
        public bool DiferenciaPuntosHabilitada = true;
        public int DiferenciaPuntos = 15;

        /// <summary>Si está activo, llegar a MaximoGamJeoms termina el round y lo gana el rival.</summary>
        public bool GanadorPorGamJeomsHabilitado = true;
        public int MaximoGamJeoms = 5;
        /// <summary>Si es true, el conteo de gam-jeoms empieza en 0 en cada round (llegar al máximo cierra ese round).</summary>
        public bool GamJeomsSeReinicianPorRound = true;
        /// <summary>Si es true, cada gam-jeom suma 1 punto al rival.</summary>
        public bool GamJeomDaPunto = true;

        /// <summary>Modo "Un mando": un solo juez puntúa para ambos lados.</summary>
        public bool UnSoloMando = false;

        /// <summary>Jueces que deben coincidir para validar un punto.</summary>
        public int JuecesRequeridos { get { return UnSoloMando ? 1 : 2; } }
    }
}
