using System.Collections.Generic;
using TkdCombate.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Lee los mandos (Xbox, PlayStation... por cable o Bluetooth, Unity los trata igual) y los
/// convierte en votos de juez. Cada mando es un juez; con "Un solo mando" activado un mando
/// puntúa directo para los dos lados.
///
/// Qué botón corresponde a cada técnica lo decide MapeoMandos (editable desde la pantalla
/// de "Configuración de mandos"), así que este script ya no tiene el mapeo fijo en el código.
/// </summary>
public class EntradaMandos : MonoBehaviour
{
    public MarcadorUI marcador;
    public MapeoMandos mapeo;
    [Tooltip("Opcional: si lo asignas, las columnas J1-J3 muestran los votos en pantalla.")]
    public PizarraJueces pizarra;

    [Tooltip("Escribe en la Console cada voto (útil para probar).")]
    public bool mostrarEnConsola = true;

    private static readonly TipoAtaque[] Tipos =
    {
        TipoAtaque.Punio, TipoAtaque.PatadaPeto, TipoAtaque.PatadaCasco,
        TipoAtaque.GiroPeto, TipoAtaque.GiroCasco
    };
    private static readonly Lado[] Lados = { Lado.Rojo, Lado.Azul };

    // Número de juez de cada mando, asignado en el orden en que se usan por primera vez.
    private readonly Dictionary<int, int> juezPorMando = new Dictionary<int, int>();

    /// <summary>Se pone en true mientras la pantalla de asignación está abierta, para no puntuar por error.</summary>
    public bool Pausado = false;

    void Update()
    {
        if (Pausado || mapeo == null) return;

        var mandos = Gamepad.all;
        for (int m = 0; m < mandos.Count; m++)
        {
            Gamepad g = mandos[m];
            int juez = NumeroDeJuez(g);

            foreach (Lado lado in Lados)
            {
                foreach (TipoAtaque tipo in Tipos)
                {
                    ButtonControl control = MapeoMandos.ControlPara(g, mapeo.RutaDe(lado, tipo));
                    if (control == null || !control.wasPressedThisFrame) continue;

                    ResultadoVoto resultado = marcador.Jueces.Votar(juez, lado, tipo);
                    if (pizarra != null) pizarra.Marcar(juez, lado, tipo);
                    if (mostrarEnConsola)
                        Debug.Log(string.Format("Juez {0} ({1}) -> {2} {3}: {4}",
                            juez, g.displayName, lado, tipo, resultado));
                }
            }
        }
    }

    int NumeroDeJuez(Gamepad g)
    {
        int numero;
        if (!juezPorMando.TryGetValue(g.deviceId, out numero))
        {
            numero = juezPorMando.Count + 1;
            juezPorMando[g.deviceId] = numero;
        }
        return numero;
    }
}
