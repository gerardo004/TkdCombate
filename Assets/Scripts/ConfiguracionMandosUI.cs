using TkdCombate.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Crea por código el panel "Configuración de mandos": una fila por técnica y lado, cada una
/// con el botón asignado y un botón "Asignar" que espera la siguiente pulsación de cualquier mando.
/// Se abre y cierra con un botón propio que este script coloca en una esquina del Canvas.
/// </summary>
public class ConfiguracionMandosUI : MonoBehaviour
{
    public RectTransform canvas;
    public MapeoMandos mapeo;
    public EntradaMandos entradaMandos;   // se pausa mientras el panel está abierto

    private static readonly TipoAtaque[] Tipos =
    {
        TipoAtaque.Punio, TipoAtaque.PatadaPeto, TipoAtaque.PatadaCasco,
        TipoAtaque.GiroPeto, TipoAtaque.GiroCasco
    };
    private static readonly string[] NombreTipo = { "Puño", "Patada peto", "Patada casco", "Giro peto", "Giro casco" };

    private GameObject panel;
    private TextMeshProUGUI[] etiquetasRojo = new TextMeshProUGUI[5];
    private TextMeshProUGUI[] etiquetasAzul = new TextMeshProUGUI[5];
    private Button[] botonesRojo = new Button[5];
    private Button[] botonesAzul = new Button[5];

    // Mientras no sea null, esperamos la próxima pulsación de un mando para esa casilla.
    private Lado? esperandoLado;
    private TipoAtaque? esperandoTipo;
    private TextMeshProUGUI etiquetaEnEspera;

    void Start()
    {
        CrearBotonAbrir();
        CrearPanel();
        panel.SetActive(false);
    }

    void Update()
    {
        if (esperandoLado == null) return;

        foreach (var g in Gamepad.all)
        {
            string ruta;
            UnityEngine.InputSystem.Controls.ButtonControl control;
            if (MapeoMandos.BuscarBotonPresionado(g, out ruta, out control))
            {
                mapeo.Asignar(esperandoLado.Value, esperandoTipo.Value, ruta);
                etiquetaEnEspera.text = MapeoMandos.NombreLegible(ruta);
                esperandoLado = null;
                esperandoTipo = null;
                etiquetaEnEspera = null;
                return;
            }
        }
    }

    // ---------- Construcción ----------

    void CrearBotonAbrir()
    {
        var go = new GameObject("BotonAbrirConfigMandos", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(canvas, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-20, -20);
        rt.sizeDelta = new Vector2(180, 50);
        go.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 1f);
        go.GetComponent<Button>().onClick.AddListener(AbrirPanel);
        CrearTextoEn(go.transform, "MANDOS");
    }

    void CrearPanel()
    {
        panel = new GameObject("PanelConfigMandos", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas, false);
        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.92f);

        CrearTitulo(panel.transform, "CONFIGURACIÓN DE MANDOS", new Vector2(0, 480));

        // Encabezados de cada lado.
        CrearTitulo(panel.transform, "ROJO", new Vector2(-420, 400));
        CrearTitulo(panel.transform, "AZUL", new Vector2(420, 400));

        for (int i = 0; i < 5; i++)
        {
            int fila = i;   // copia local: cada botón debe recordar SU fila, no la última del bucle
            float y = 300 - fila * 110;
            etiquetasRojo[fila] = CrearFila(panel.transform, new Vector2(-420, y), NombreTipo[fila],
                mapeo.RutaDe(Lado.Rojo, Tipos[fila]), () => EmpezarEspera(Lado.Rojo, Tipos[fila], fila, true));
            etiquetasAzul[fila] = CrearFila(panel.transform, new Vector2(420, y), NombreTipo[fila],
                mapeo.RutaDe(Lado.Azul, Tipos[fila]), () => EmpezarEspera(Lado.Azul, Tipos[fila], fila, false));
        }

        // Botón "Restaurar valores por defecto".
        var goRestaurar = new GameObject("BotonRestaurar", typeof(RectTransform), typeof(Image), typeof(Button));
        goRestaurar.transform.SetParent(panel.transform, false);
        var rtR = goRestaurar.GetComponent<RectTransform>();
        rtR.anchoredPosition = new Vector2(0, -400);
        rtR.sizeDelta = new Vector2(320, 60);
        goRestaurar.GetComponent<Image>().color = new Color(0.35f, 0.1f, 0.1f, 1f);
        goRestaurar.GetComponent<Button>().onClick.AddListener(() =>
        {
            mapeo.RestaurarValoresPorDefecto();
            for (int i = 0; i < 5; i++)
            {
                etiquetasRojo[i].text = MapeoMandos.NombreLegible(mapeo.RutaDe(Lado.Rojo, Tipos[i]));
                etiquetasAzul[i].text = MapeoMandos.NombreLegible(mapeo.RutaDe(Lado.Azul, Tipos[i]));
            }
        });
        CrearTextoEn(goRestaurar.transform, "RESTAURAR VALORES POR DEFECTO");

        // Botón "Cerrar".
        var goCerrar = new GameObject("BotonCerrar", typeof(RectTransform), typeof(Image), typeof(Button));
        goCerrar.transform.SetParent(panel.transform, false);
        var rtC = goCerrar.GetComponent<RectTransform>();
        rtC.anchoredPosition = new Vector2(0, -480);
        rtC.sizeDelta = new Vector2(320, 60);
        goCerrar.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.15f, 1f);
        goCerrar.GetComponent<Button>().onClick.AddListener(CerrarPanel);
        CrearTextoEn(goCerrar.transform, "CERRAR");
    }

    TextMeshProUGUI CrearFila(Transform padre, Vector2 pos, string nombreTecnica, string rutaActual, System.Action alPulsar)
    {
        var contenedor = new GameObject("Fila " + nombreTecnica, typeof(RectTransform));
        contenedor.transform.SetParent(padre, false);
        var rt = contenedor.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(720, 90);

        var nombre = new GameObject("Nombre", typeof(RectTransform), typeof(TextMeshProUGUI));
        nombre.transform.SetParent(contenedor.transform, false);
        var rtN = nombre.GetComponent<RectTransform>();
        rtN.anchoredPosition = new Vector2(-260, 0); rtN.sizeDelta = new Vector2(220, 60);
        var tmpN = nombre.GetComponent<TextMeshProUGUI>();
        tmpN.text = nombreTecnica; tmpN.fontSize = 28; tmpN.alignment = TextAlignmentOptions.MidlineLeft;

        var etiqueta = new GameObject("Actual", typeof(RectTransform), typeof(TextMeshProUGUI));
        etiqueta.transform.SetParent(contenedor.transform, false);
        var rtE = etiqueta.GetComponent<RectTransform>();
        rtE.anchoredPosition = new Vector2(-20, 0); rtE.sizeDelta = new Vector2(260, 60);
        var tmpE = etiqueta.GetComponent<TextMeshProUGUI>();
        tmpE.text = MapeoMandos.NombreLegible(rutaActual); tmpE.fontSize = 24; tmpE.color = Color.yellow;
        tmpE.alignment = TextAlignmentOptions.Midline;

        var goBoton = new GameObject("Asignar", typeof(RectTransform), typeof(Image), typeof(Button));
        goBoton.transform.SetParent(contenedor.transform, false);
        var rtB = goBoton.GetComponent<RectTransform>();
        rtB.anchoredPosition = new Vector2(260, 0); rtB.sizeDelta = new Vector2(160, 60);
        goBoton.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 1f);
        goBoton.GetComponent<Button>().onClick.AddListener(() => alPulsar());
        CrearTextoEn(goBoton.transform, "Asignar");

        return tmpE;
    }

    void AbrirPanel()
    {
        panel.SetActive(true);
        if (entradaMandos != null) entradaMandos.Pausado = true;
    }

    void CerrarPanel()
    {
        CancelarEspera();
        panel.SetActive(false);
        if (entradaMandos != null) entradaMandos.Pausado = false;
    }

    void EmpezarEspera(Lado lado, TipoAtaque tipo, int fila, bool esRojo)
    {
        esperandoLado = lado;
        esperandoTipo = tipo;
        etiquetaEnEspera = esRojo ? etiquetasRojo[fila] : etiquetasAzul[fila];
        etiquetaEnEspera.text = "Presiona un botón...";
    }

    void CancelarEspera()
    {
        if (esperandoLado == null) return;
        int i = System.Array.IndexOf(Tipos, esperandoTipo.Value);
        etiquetaEnEspera.text = MapeoMandos.NombreLegible(mapeo.RutaDe(esperandoLado.Value, esperandoTipo.Value));
        esperandoLado = null; esperandoTipo = null; etiquetaEnEspera = null;
    }

    void CrearTitulo(Transform padre, string texto, Vector2 pos)
    {
        var go = new GameObject("Titulo " + texto, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(500, 70);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = texto; tmp.fontSize = 36; tmp.alignment = TextAlignmentOptions.Center;
    }

    void CrearTextoEn(Transform padre, string texto)
    {
        var go = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = texto; tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true; tmp.fontSizeMin = 12; tmp.fontSizeMax = 26;
        tmp.color = Color.white; tmp.raycastTarget = false;
    }
}
