using System;
using System.Collections.Generic;

namespace TkdCombate.Core
{
    public enum ResultadoVoto
    {
        Pendiente,       // falta que otro juez coincida
        PuntoValidado,   // se alcanzó la coincidencia y se sumó el punto
        Absorbido,       // voto tardío del juez restante de un golpe ya validado
        Duplicado,       // el mismo juez ya había votado ese golpe
        Rechazado        // el combate no está en un estado que admita puntos
    }

    /// <summary>
    /// Convierte los votos de cada juez (uno por mando) en puntos validados.
    /// Un punto cuenta cuando <see cref="ConfiguracionCombate.JuecesRequeridos"/> jueces DISTINTOS
    /// votan el mismo lado y tipo de ataque dentro de una ventana corta (2 jueces; 1 en modo "Un mando").
    /// La ventana es interna (no se expone en la pantalla de configuración).
    /// </summary>
    public sealed class PanelJueces
    {
        public const double VentanaPorDefecto = 3.0;

        private struct Voto { public int Juez; public double Tiempo; }

        private const int Tipos = 6;
        private readonly Combate _combate;
        private readonly double _ventana;
        private double _reloj;

        private readonly List<Voto>[] _pendientes = new List<Voto>[2 * Tipos];
        private readonly double[] _ultimaValidacion = new double[2 * Tipos];
        private readonly HashSet<int>[] _juecesUsados = new HashSet<int>[2 * Tipos];

        public PanelJueces(Combate combate, double ventanaSegundos = VentanaPorDefecto)
        {
            if (combate == null) throw new ArgumentNullException("combate");
            _combate = combate;
            _ventana = ventanaSegundos;
            for (int i = 0; i < _pendientes.Length; i++)
            {
                _pendientes[i] = new List<Voto>();
                _juecesUsados[i] = new HashSet<int>();
                _ultimaValidacion[i] = double.NegativeInfinity;
            }
        }

        /// <summary>Avanza el reloj interno (en Unity: Time.deltaTime, junto con Combate.Tick).</summary>
        public void Tick(double dt) { if (dt > 0) _reloj += dt; }

        public void Reiniciar()
        {
            for (int i = 0; i < _pendientes.Length; i++)
            {
                _pendientes[i].Clear();
                _juecesUsados[i].Clear();
                _ultimaValidacion[i] = double.NegativeInfinity;
            }
        }

        public ResultadoVoto Votar(int juez, Lado lado, TipoAtaque tipo)
        {
            if (!_combate.PuedePuntuar)
            {
                Reiniciar();
                return ResultadoVoto.Rechazado;
            }

            int requeridos = _combate.Config.JuecesRequeridos;
            if (requeridos <= 1)
                return _combate.SumarPunto(lado, tipo) ? ResultadoVoto.PuntoValidado : ResultadoVoto.Rechazado;

            int k = (int)lado * Tipos + (int)tipo;
            var pend = _pendientes[k];
            pend.RemoveAll(v => _reloj - v.Tiempo > _ventana);

            // Voto tardío de un juez que no participó en un golpe recién validado: se absorbe.
            if (_reloj - _ultimaValidacion[k] <= _ventana && !_juecesUsados[k].Contains(juez))
            {
                _juecesUsados[k].Add(juez);
                return ResultadoVoto.Absorbido;
            }

            foreach (var v in pend)
                if (v.Juez == juez) return ResultadoVoto.Duplicado;

            pend.Add(new Voto { Juez = juez, Tiempo = _reloj });
            if (pend.Count < requeridos) return ResultadoVoto.Pendiente;

            _juecesUsados[k].Clear();
            foreach (var v in pend) _juecesUsados[k].Add(v.Juez);
            _ultimaValidacion[k] = _reloj;
            pend.Clear();
            return _combate.SumarPunto(lado, tipo) ? ResultadoVoto.PuntoValidado : ResultadoVoto.Rechazado;
        }
    }
}
