using System;
using System.Collections.Generic;
using TkdCombate.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Traduce "lado + técnica" al botón físico del mando que le corresponde, y permite reasignarlo.
/// El mapeo se guarda en PlayerPrefs, así que sobrevive a cerrar y abrir la app.
/// </summary>
public class MapeoMandos : MonoBehaviour
{
    private const string ClavePlayerPrefs = "TkdCombate.MapeoMandos";

    // Claves internas: "Rojo_Punio", "Azul_GiroCasco", etc.
    public static string Clave(Lado lado, TipoAtaque tipo) { return lado + "_" + tipo; }

    // Rutas por defecto, relativas al mando (las mismas que usábamos fijas).
    private static readonly Dictionary<string, string> Defecto = new Dictionary<string, string>
    {
        { "Rojo_Punio",      "leftShoulder" },
        { "Rojo_PatadaPeto", "leftStick/down" },
        { "Rojo_PatadaCasco","leftStick/up" },
        { "Rojo_GiroPeto",   "dpad/down" },
        { "Rojo_GiroCasco",  "leftTrigger" },
        { "Azul_Punio",      "rightShoulder" },
        { "Azul_PatadaPeto", "rightStick/down" },
        { "Azul_PatadaCasco","rightStick/up" },
        { "Azul_GiroPeto",   "buttonSouth" },
        { "Azul_GiroCasco",  "rightTrigger" },
    };

    private Dictionary<string, string> rutas;

    void Awake()
    {
        Cargar();
    }

    public string RutaDe(Lado lado, TipoAtaque tipo) { return rutas[Clave(lado, tipo)]; }

    /// <summary>Nombre corto para mostrar en pantalla, p. ej. "leftStick/up" -> "L Stick Arriba".</summary>
    public static string NombreLegible(string ruta)
    {
        if (string.IsNullOrEmpty(ruta)) return "(sin asignar)";
        switch (ruta)
        {
            case "leftShoulder": return "LB";
            case "rightShoulder": return "RB";
            case "leftTrigger": return "LT";
            case "rightTrigger": return "RT";
            case "leftStick/up": return "Stick Izq. Arriba";
            case "leftStick/down": return "Stick Izq. Abajo";
            case "leftStick/left": return "Stick Izq. Izquierda";
            case "leftStick/right": return "Stick Izq. Derecha";
            case "rightStick/up": return "Stick Der. Arriba";
            case "rightStick/down": return "Stick Der. Abajo";
            case "rightStick/left": return "Stick Der. Izquierda";
            case "rightStick/right": return "Stick Der. Derecha";
            case "dpad/up": return "Cruceta Arriba";
            case "dpad/down": return "Cruceta Abajo";
            case "dpad/left": return "Cruceta Izquierda";
            case "dpad/right": return "Cruceta Derecha";
            case "buttonSouth": return "A / Cruz";
            case "buttonNorth": return "Y / Triángulo";
            case "buttonEast": return "B / Círculo";
            case "buttonWest": return "X / Cuadrado";
            case "leftStickPress": return "Click Stick Izq.";
            case "rightStickPress": return "Click Stick Der.";
            default: return ruta;
        }
    }

    /// <summary>El control de un mando concreto para esa ruta, o null si ese mando no lo tiene.</summary>
    public static ButtonControl ControlPara(Gamepad g, string ruta)
    {
        if (g == null || string.IsNullOrEmpty(ruta)) return null;
        return g.TryGetChildControl<ButtonControl>(ruta);
    }

    /// <summary>Cambia la asignación de "lado + técnica" a una ruta nueva y guarda.</summary>
    public void Asignar(Lado lado, TipoAtaque tipo, string ruta)
    {
        rutas[Clave(lado, tipo)] = ruta;
        Guardar();
    }

    public void RestaurarValoresPorDefecto()
    {
        rutas = new Dictionary<string, string>(Defecto);
        Guardar();
    }

    // ---------- Botones "libres" que se pueden asignar (para el modo de escucha) ----------

    /// <summary>Devuelve, de un mando, el botón que se presionó recién, junto con su ruta relativa.</summary>
    public static bool BuscarBotonPresionado(Gamepad g, out string ruta, out ButtonControl control)
    {
        foreach (var candidato in Candidatos(g))
        {
            if (candidato.Value.wasPressedThisFrame)
            {
                ruta = candidato.Key;
                control = candidato.Value;
                return true;
            }
        }
        ruta = null; control = null;
        return false;
    }

    private static IEnumerable<KeyValuePair<string, ButtonControl>> Candidatos(Gamepad g)
    {
        yield return new KeyValuePair<string, ButtonControl>("leftShoulder", g.leftShoulder);
        yield return new KeyValuePair<string, ButtonControl>("rightShoulder", g.rightShoulder);
        yield return new KeyValuePair<string, ButtonControl>("leftTrigger", g.leftTrigger);
        yield return new KeyValuePair<string, ButtonControl>("rightTrigger", g.rightTrigger);
        yield return new KeyValuePair<string, ButtonControl>("leftStick/up", g.leftStick.up);
        yield return new KeyValuePair<string, ButtonControl>("leftStick/down", g.leftStick.down);
        yield return new KeyValuePair<string, ButtonControl>("leftStick/left", g.leftStick.left);
        yield return new KeyValuePair<string, ButtonControl>("leftStick/right", g.leftStick.right);
        yield return new KeyValuePair<string, ButtonControl>("rightStick/up", g.rightStick.up);
        yield return new KeyValuePair<string, ButtonControl>("rightStick/down", g.rightStick.down);
        yield return new KeyValuePair<string, ButtonControl>("rightStick/left", g.rightStick.left);
        yield return new KeyValuePair<string, ButtonControl>("rightStick/right", g.rightStick.right);
        yield return new KeyValuePair<string, ButtonControl>("dpad/up", g.dpad.up);
        yield return new KeyValuePair<string, ButtonControl>("dpad/down", g.dpad.down);
        yield return new KeyValuePair<string, ButtonControl>("dpad/left", g.dpad.left);
        yield return new KeyValuePair<string, ButtonControl>("dpad/right", g.dpad.right);
        yield return new KeyValuePair<string, ButtonControl>("buttonSouth", g.buttonSouth);
        yield return new KeyValuePair<string, ButtonControl>("buttonNorth", g.buttonNorth);
        yield return new KeyValuePair<string, ButtonControl>("buttonEast", g.buttonEast);
        yield return new KeyValuePair<string, ButtonControl>("buttonWest", g.buttonWest);
        yield return new KeyValuePair<string, ButtonControl>("leftStickPress", g.leftStickButton);
        yield return new KeyValuePair<string, ButtonControl>("rightStickPress", g.rightStickButton);
    }

    // ---------- Guardado ----------

    [Serializable]
    private class Datos { public string[] claves; public string[] valores; }

    private void Guardar()
    {
        var d = new Datos { claves = new string[rutas.Count], valores = new string[rutas.Count] };
        int i = 0;
        foreach (var kv in rutas) { d.claves[i] = kv.Key; d.valores[i] = kv.Value; i++; }
        PlayerPrefs.SetString(ClavePlayerPrefs, JsonUtility.ToJson(d));
        PlayerPrefs.Save();
    }

    private void Cargar()
    {
        rutas = new Dictionary<string, string>(Defecto);
        string json = PlayerPrefs.GetString(ClavePlayerPrefs, "");
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            var d = JsonUtility.FromJson<Datos>(json);
            for (int i = 0; i < d.claves.Length; i++)
                if (rutas.ContainsKey(d.claves[i])) rutas[d.claves[i]] = d.valores[i];
        }
        catch { /* si el guardado está corrupto, se queda con los valores por defecto */ }
    }
}
