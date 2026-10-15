using TkdCombate.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Se muestra automáticamente cuando el combate termina (escucha MarcadorUI.CombateTerminado).
/// Panel dividido rojo/azul con "GANADOR" y el nombre, botones Estadísticas / Combate nuevo / Salir.
/// </summary>
public class PantallaFinCombate : MonoBehaviour
{
    public RectTransform canvas;
    public MarcadorUI marcador;

    private static readonly TipoAtaque[] Tipos =
    {
        TipoAtaque.Punio, TipoAtaque.PatadaPeto, TipoAtaque.PatadaCasco,
        TipoAtaque.GiroPeto, TipoAtaque.GiroCasco
    };

    private GameObject panelGanador, panelEstadisticas;
    private TextMeshProUGUI nombreGanador;
    private Image franjaGanador;
    private TextMeshProUGUI[] filasRojo = new TextMeshProUGUI[6];
    private TextMeshProUGUI[] filasAzul = new TextMeshProUGUI[6];

    void Start()
    {
        CrearPanelGanador();
        CrearPanelEstadisticas();
        panelGanador.SetActive(false);
        panelEstadisticas.SetActive(false);
        marcador.CombateTerminado += Mostrar;
    }

    void Mostrar(ResultadoCombate r)
    {
        bool ganaRojo = r.Ganador == Lado.Rojo;
        nombreGanador.text = ganaRojo ? marcador.nombreRojo : marcador.nombreAzul;
        franjaGanador.color = ganaRojo ? new Color(0.45f, 0f, 0f) : new Color(0.03f, 0.15f, 0.40f);

        LlenarEstadisticas();
        panelGanador.SetActive(true);
        panelEstadisticas.SetActive(false);
    }

    void LlenarEstadisticas()
    {
        var c = marcador.Combate;
        for (int i = 0; i < 5; i++)
        {
            filasRojo[i].text = NombreTecnica(i) + ": " + c.ConteoTotal(Lado.Rojo, Tipos[i]);
            filasAzul[i].text = NombreTecnica(i) + ": " + c.ConteoTotal(Lado.Azul, Tipos[i]);
        }
        filasRojo[5].text = "Gam-Jeom: " + c.GamJeomsTotal(Lado.Rojo) + "   Rounds: " + c.RoundsGanados(Lado.Rojo);
        filasAzul[5].text = "Gam-Jeom: " + c.GamJeomsTotal(Lado.Azul) + "   Rounds: " + c.RoundsGanados(Lado.Azul);
    }

    static string NombreTecnica(int i)
    {
        switch (i)
        {
            case 0: return "Puño";
            case 1: return "Patada peto";
            case 2: return "Patada casco";
            case 3: return "Giro peto";
            default: return "Giro casco";
        }
    }

    void NuevoCombate()
    {
        panelGanador.SetActive(false);
        panelEstadisticas.SetActive(false);
        marcador.NuevoCombate();
    }

    void Salir()
    {
        Application.Quit();   // en el editor no cierra nada; sí funciona en el .exe compilado
    }

    // ---------- Construcción ----------

    void CrearPanelGanador()
    {
        panelGanador = CrearPanelBase("PanelGanador");
        franjaGanador = panelGanador.GetComponent<Image>();

        // Fondo del lado que no ganó, para el efecto mitad/mitad como en la app original.
        var fondoContrario = new GameObject("FondoContrario", typeof(RectTransform), typeof(Image));
        fondoContrario.transform.SetParent(panelGanador.transform, false);
        var rt = fondoContrario.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f); rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        fondoContrario.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f);

        CrearTexto(panelGanador.transform, "GANADOR", new Vector2(-480, 260), 40);
        nombreGanador = CrearTexto(panelGanador.transform, "", new Vector2(-480, 160), 60);

        CrearBotonGrande(panelGanador.transform, "Estadísticas", new Vector2(-480, -300),
            () => panelEstadisticas.SetActive(!panelEstadisticas.activeSelf));
        CrearBotonGrande(panelGanador.transform, "Combate nuevo", new Vector2(300, -420), NuevoCombate);
        CrearBotonGrande(panelGanador.transform, "Salir", new Vector2(650, -420), Salir);
    }

    void CrearPanelEstadisticas()
    {
        panelEstadisticas = CrearPanelBase("PanelEstadisticas");
        panelEstadisticas.GetComponent<Image>().color = new Color(0.02f, 0.05f, 0.12f, 0.97f);
        panelEstadisticas.transform.SetParent(panelGanador.transform, false);   // por encima del ganador

        CrearTexto(panelEstadisticas.transform, "ESTADÍSTICAS", new Vector2(0, 420), 44);
        CrearTexto(panelEstadisticas.transform, "ROJO", new Vector2(-420, 340), 32);
        CrearTexto(panelEstadisticas.transform, "AZUL", new Vector2(420, 340), 32);

        for (int i = 0; i < 6; i++)
        {
            float y = 260 - i * 70;
            filasRojo[i] = CrearTexto(panelEstadisticas.transform, "", new Vector2(-420, y), 26);
            filasAzul[i] = CrearTexto(panelEstadisticas.transform, "", new Vector2(420, y), 26);
        }

        CrearBotonGrande(panelEstadisticas.transform, "Cerrar", new Vector2(0, -430),
            () => panelEstadisticas.SetActive(false));
    }

    GameObject CrearPanelBase(string nombre)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvas, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    TextMeshProUGUI CrearTexto(Transform padre, string texto, Vector2 pos, float tamano)
    {
        var go = new GameObject("Texto " + texto, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(700, 70);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = texto; tmp.fontSize = tamano; tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    void CrearBotonGrande(Transform padre, string texto, Vector2 pos, UnityEngine.Events.UnityAction accion)
    {
        var go = new GameObject("Boton " + texto, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(380, 90);
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
        go.GetComponent<Button>().onClick.AddListener(accion);

        var goTexto = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        goTexto.transform.SetParent(go.transform, false);
        var rtT = goTexto.GetComponent<RectTransform>();
        rtT.anchorMin = Vector2.zero; rtT.anchorMax = Vector2.one;
        rtT.offsetMin = Vector2.zero; rtT.offsetMax = Vector2.zero;
        var tmp = goTexto.GetComponent<TextMeshProUGUI>();
        tmp.text = texto; tmp.fontSize = 28; tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }
}
