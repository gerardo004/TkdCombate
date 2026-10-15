using System;
using System.Collections.Generic;

namespace TkdCombate.Core
{
    /// <summary>
    /// Núcleo de reglas de un combate (kyorugi). No sabe nada de Unity, pantallas ni gamepads:
    /// recibe acciones (SumarPunto, Tick...) y expone estado y eventos.
    /// </summary>
    public sealed class Combate
    {
        private const int NumTipos = 6; // índices 1..5 usados (TipoAtaque)

        public ConfiguracionCombate Config { get; private set; }
        public EstadoCombate Estado { get; private set; }
        public int RoundActual { get; private set; }

        /// <summary>Segundos que quedan en el reloj activo (round, descanso o punto de oro).</summary>
        public double SegundosRestantes { get; private set; }
        public double SegundosMedicoRestantes { get; private set; }
        public bool TiempoPuntoDeOroAgotado { get; private set; }

        public ResultadoCombate Resultado { get; private set; }
        public IReadOnlyList<ResultadoRound> Rounds { get { return _rounds; } }

        public event Action Cambio;
        public event Action<ResultadoRound> RoundTerminado;
        public event Action<ResultadoCombate> CombateTerminado;

        private readonly int[] _puntosRound = new int[2];
        private readonly int[] _puntosTotales = new int[2];
        private readonly int[] _gamJeoms = new int[2];        // los del round (o acumulados, según la configuración)
        private readonly int[] _gamJeomsTotal = new int[2];   // todos los del combate
        private readonly int[] _roundsGanados = new int[2];
        private readonly int[,] _conteoRound = new int[2, NumTipos];
        private readonly int[,] _conteoTotal = new int[2, NumTipos];
        private readonly List<ResultadoRound> _rounds = new List<ResultadoRound>();
        private EstadoCombate _estadoAntesDeMedico;

        public Combate(ConfiguracionCombate config)
        {
            if (config == null) throw new ArgumentNullException("config");
            Config = config;
            Estado = EstadoCombate.Preparado;
            RoundActual = 1;
            SegundosRestantes = config.SegundosRound;
            SegundosMedicoRestantes = config.SegundosMedico;
        }

        // ---------- Consultas ----------
        public int PuntosRound(Lado l) { return _puntosRound[(int)l]; }
        public int PuntosTotales(Lado l) { return _puntosTotales[(int)l]; }
        public int GamJeoms(Lado l) { return _gamJeoms[(int)l]; }
        public int GamJeomsTotal(Lado l) { return _gamJeomsTotal[(int)l]; }
        public int RoundsGanados(Lado l) { return _roundsGanados[(int)l]; }
        public int ConteoRound(Lado l, TipoAtaque t) { return _conteoRound[(int)l, (int)t]; }
        public int ConteoTotal(Lado l, TipoAtaque t) { return _conteoTotal[(int)l, (int)t]; }

        public bool PuedePuntuar
        {
            get
            {
                return Estado == EstadoCombate.RoundEnCurso
                    || Estado == EstadoCombate.RoundPausado
                    || Estado == EstadoCombate.PuntoDeOro;
            }
        }

        // ---------- Control del round ----------
        public bool IniciarRound()
        {
            if (Estado != EstadoCombate.Preparado) return false;
            Array.Clear(_puntosRound, 0, 2);
            Array.Clear(_conteoRound, 0, _conteoRound.Length);
            if (Config.GamJeomsSeReinicianPorRound) Array.Clear(_gamJeoms, 0, 2);
            SegundosRestantes = Config.SegundosRound;
            SegundosMedicoRestantes = Config.SegundosMedico;
            Estado = EstadoCombate.RoundEnCurso;
            Notificar();
            return true;
        }

        public bool Pausar()
        {
            if (Estado != EstadoCombate.RoundEnCurso) return false;
            Estado = EstadoCombate.RoundPausado;
            Notificar();
            return true;
        }

        public bool Reanudar()
        {
            if (Estado != EstadoCombate.RoundPausado) return false;
            Estado = EstadoCombate.RoundEnCurso;
            Notificar();
            return true;
        }

        /// <summary>Se llama cada frame con el tiempo transcurrido (en Unity: Time.deltaTime).</summary>
        public void Tick(double dt)
        {
            if (dt <= 0) return;
            switch (Estado)
            {
                case EstadoCombate.RoundEnCurso:
                    SegundosRestantes -= dt;
                    if (SegundosRestantes <= 0)
                    {
                        SegundosRestantes = 0;
                        FinalizarRound();
                    }
                    else Notificar();
                    break;

                case EstadoCombate.PuntoDeOro:
                    if (SegundosRestantes > 0)
                    {
                        SegundosRestantes -= dt;
                        if (SegundosRestantes <= 0)
                        {
                            SegundosRestantes = 0;
                            TiempoPuntoDeOroAgotado = true; // decide el árbitro: RegistrarSuperioridad
                        }
                        Notificar();
                    }
                    break;

                case EstadoCombate.Descanso:
                    SegundosRestantes -= dt;
                    if (SegundosRestantes <= 0)
                    {
                        SegundosRestantes = Config.SegundosRound;
                        Estado = EstadoCombate.Preparado;
                    }
                    Notificar();
                    break;

                case EstadoCombate.TiempoMedico:
                    if (SegundosMedicoRestantes > 0)
                    {
                        SegundosMedicoRestantes = Math.Max(0, SegundosMedicoRestantes - dt);
                        Notificar();
                    }
                    break;
            }
        }

        /// <summary>Fin manual del round (p. ej. el árbitro lo da por terminado).</summary>
        public bool TerminarRoundAhora()
        {
            if (Estado != EstadoCombate.RoundEnCurso && Estado != EstadoCombate.RoundPausado) return false;
            FinalizarRound();
            return true;
        }

        // ---------- Tiempo médico ----------
        public bool IniciarTiempoMedico()
        {
            if (Estado != EstadoCombate.RoundEnCurso && Estado != EstadoCombate.RoundPausado
                && Estado != EstadoCombate.PuntoDeOro) return false;
            _estadoAntesDeMedico = Estado == EstadoCombate.PuntoDeOro
                ? EstadoCombate.PuntoDeOro : EstadoCombate.RoundPausado;
            Estado = EstadoCombate.TiempoMedico;
            Notificar();
            return true;
        }

        public bool TerminarTiempoMedico()
        {
            if (Estado != EstadoCombate.TiempoMedico) return false;
            Estado = _estadoAntesDeMedico;
            Notificar();
            return true;
        }

        /// <summary>Un atleta no puede continuar: gana el rival.</summary>
        public bool DeclararAbandono(Lado quienAbandona)
        {
            if (Estado == EstadoCombate.Finalizado) return false;
            Terminar(quienAbandona.Otro(), MotivoVictoria.Abandono);
            return true;
        }

        // ---------- Puntuación ----------
        public bool SumarPunto(Lado lado, TipoAtaque tipo)
        {
            if (!PuedePuntuar) return false;
            int v = (int)tipo;
            int pts = Config.PuntosDe(tipo);
            _puntosRound[(int)lado] += pts;
            _puntosTotales[(int)lado] += pts;
            _conteoRound[(int)lado, v]++;
            _conteoTotal[(int)lado, v]++;

            if (Estado == EstadoCombate.PuntoDeOro)
            {
                Terminar(lado, MotivoVictoria.PuntoDeOro);
                return true;
            }
            if (!RevisarDiferencia()) Notificar();
            return true;
        }

        /// <summary>Corrección (p. ej. tras revisión de video). Solo si ese punto fue registrado.</summary>
        public bool RestarPunto(Lado lado, TipoAtaque tipo)
        {
            if (!PuedePuntuar) return false;
            int v = (int)tipo;
            if (_conteoRound[(int)lado, v] <= 0) return false;
            int pts = Config.PuntosDe(tipo);
            _puntosRound[(int)lado] -= pts;
            _puntosTotales[(int)lado] -= pts;
            _conteoRound[(int)lado, v]--;
            _conteoTotal[(int)lado, v]--;
            Notificar();
            return true;
        }

        public bool SumarGamJeom(Lado lado)
        {
            if (!PuedePuntuar) return false;
            int i = (int)lado, r = (int)lado.Otro();
            _gamJeoms[i]++;
            _gamJeomsTotal[i]++;
            if (Config.GamJeomDaPunto)
            {
                _puntosRound[r]++;
                _puntosTotales[r]++;
            }

            // En punto de oro, un gam-jeom le da la victoria al rival.
            if (Estado == EstadoCombate.PuntoDeOro)
            {
                Terminar(lado.Otro(), MotivoVictoria.PuntoDeOro);
                return true;
            }
            // Llegar al máximo cierra el ROUND (lo gana el rival); el combate lo decide el mejor de N.
            if (Config.GanadorPorGamJeomsHabilitado && _gamJeoms[i] >= Config.MaximoGamJeoms)
            {
                FinalizarRound(lado.Otro());
                return true;
            }
            if (!RevisarDiferencia()) Notificar();
            return true;
        }

        public bool RestarGamJeom(Lado lado)
        {
            if (!PuedePuntuar) return false;
            int i = (int)lado, r = (int)lado.Otro();
            if (_gamJeoms[i] <= 0) return false;
            _gamJeoms[i]--;
            if (_gamJeomsTotal[i] > 0) _gamJeomsTotal[i]--;
            if (Config.GamJeomDaPunto && _puntosRound[r] > 0)
            {
                _puntosRound[r]--;
                _puntosTotales[r]--;
            }
            Notificar();
            return true;
        }

        /// <summary>Solo tras agotarse el punto de oro sin puntos: decide el árbitro.</summary>
        public bool RegistrarSuperioridad(Lado lado)
        {
            if (Estado != EstadoCombate.PuntoDeOro || !TiempoPuntoDeOroAgotado) return false;
            Terminar(lado, MotivoVictoria.Superioridad);
            return true;
        }

        // ---------- Ajustes manuales del árbitro ----------
        public bool AjustarTiempo(double deltaSegundos)
        {
            if (Estado == EstadoCombate.Finalizado || Estado == EstadoCombate.TiempoMedico) return false;
            SegundosRestantes = Math.Max(0, SegundosRestantes + deltaSegundos);
            Notificar();
            return true;
        }

        /// <summary>Solo con el round sin iniciar (estado Preparado).</summary>
        public bool AjustarRound(int delta)
        {
            if (Estado != EstadoCombate.Preparado) return false;
            RoundActual = Math.Max(1, Math.Min(Config.RoundsTotales, RoundActual + delta));
            Notificar();
            return true;
        }

        /// <summary>Botón "Ganador": el árbitro declara ganador directo.</summary>
        public bool DeclararGanador(Lado lado)
        {
            if (Estado == EstadoCombate.Finalizado) return false;
            Terminar(lado, MotivoVictoria.DecisionArbitral);
            return true;
        }

        // ---------- Reglas internas ----------
        /// <summary>Ventaja dentro del round: si se alcanza, el round termina y gana quien lidera.</summary>
        private bool RevisarDiferencia()
        {
            if (!Config.DiferenciaPuntosHabilitada) return false;
            if (Estado != EstadoCombate.RoundEnCurso && Estado != EstadoCombate.RoundPausado) return false;
            int dif = Math.Abs(_puntosRound[0] - _puntosRound[1]);
            if (dif >= Config.DiferenciaPuntos)
            {
                FinalizarRound();
                return true;
            }
            return false;
        }

        /// <param name="ganadorForzado">Si no es null, ese lado gana el round sin importar los puntos (5 gam-jeoms).</param>
        private void FinalizarRound(Lado? ganadorForzado = null)
        {
            var res = new ResultadoRound
            {
                Numero = RoundActual,
                PuntosRojo = _puntosRound[0],
                PuntosAzul = _puntosRound[1],
                Ganador = null
            };
            if (res.PuntosRojo > res.PuntosAzul) res.Ganador = Lado.Rojo;
            else if (res.PuntosAzul > res.PuntosRojo) res.Ganador = Lado.Azul;
            if (ganadorForzado.HasValue)
            {
                res.Ganador = ganadorForzado;
                res.PorGamJeoms = true;
            }

            if (res.Ganador.HasValue) _roundsGanados[(int)res.Ganador.Value]++;
            _rounds.Add(res);
            if (RoundTerminado != null) RoundTerminado(res);

            // 1) Mayoría de rounds (mejor de N).
            int mayoria = Config.RoundsTotales / 2 + 1;
            for (int i = 0; i < 2; i++)
            {
                if (_roundsGanados[i] >= mayoria)
                {
                    Terminar((Lado)i, MotivoVictoria.Rounds);
                    return;
                }
            }

            // 2) Último round: por rounds ganados o punto de oro.
            if (RoundActual >= Config.RoundsTotales)
            {
                if (_roundsGanados[0] != _roundsGanados[1])
                {
                    Terminar(_roundsGanados[0] > _roundsGanados[1] ? Lado.Rojo : Lado.Azul, MotivoVictoria.Rounds);
                }
                else
                {
                    TiempoPuntoDeOroAgotado = false;
                    SegundosRestantes = Config.SegundosPuntoDeOro;
                    Estado = EstadoCombate.PuntoDeOro;
                    Notificar();
                }
                return;
            }

            // 3) Sigue el combate: descanso. El marcador ya muestra el round siguiente.
            RoundActual++;
            SegundosRestantes = Config.SegundosDescanso;
            Estado = EstadoCombate.Descanso;
            Notificar();
        }

        private void Terminar(Lado ganador, MotivoVictoria motivo)
        {
            Estado = EstadoCombate.Finalizado;
            Resultado = new ResultadoCombate { Ganador = ganador, Motivo = motivo };
            if (CombateTerminado != null) CombateTerminado(Resultado);
            Notificar();
        }

        private void Notificar()
        {
            if (Cambio != null) Cambio();
        }
    }
}
