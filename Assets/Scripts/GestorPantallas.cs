using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Controla el flujo de pantallas: Menú -> Configuración -> Comprobación -> Combate.
/// El marcador (fondos, textos, botones, columnas de jueces) ya existe en la escena bajo
/// un objeto llamado "PantallaCombate"; este script solo lo oculta hasta que el árbitro
/// termine de configurar y pulse "Continuar".
/// </summary>
public class GestorPantallas : MonoBehaviour
{
    public RectTransform canvas;
    public MarcadorUI marcador;
    [Tooltip("El objeto padre que agrupa todo el marcador de combate (fondos, textos, botones).")]
    public GameObject pantallaCombate;

    private GameObject panelMenu, panelConfig, panelComprobacion, panelNombres;
    private GameObject listaMandos;   // sub-panel dentro de Comprobación

    // Valores elegidos en el menú, aplicados al pulsar Continuar.
    private int round = 60, descanso = 10, medico = 30, diferencia = 15, maxGamJeoms = 5;
    private bool aplicaDiferencia = true, aplicaGamJeoms = true, unMando = true;

    private readonly List<TextMeshProUGUI> filasMandos = new List<TextMeshProUGUI>();

    void Start()
    {
        if (pantallaCombate != null) pantallaCombate.SetActive(false);
        CrearMenu();
        CrearConfig();
        CrearComprobacion();
        CrearNombres();
        Mostrar(panelMenu);
    }

    void Update()
    {
        // Mientras el sub-panel de mandos está visible, refresca el estado de conexión.
        if (listaMandos != null && listaMandos.activeSelf) ActualizarListaMandos();
    }

    void Mostrar(GameObject panel)
    {
        panelMenu.SetActive(panel == panelMenu);
        panelConfig.SetActive(panel == panelConfig);
        panelComprobacion.SetActive(panel == panelComprobacion);
        panelNombres.SetActive(panel == panelNombres);
        if (panel != panelComprobacion && listaMandos != null) listaMandos.SetActive(false);
    }

    void EmpezarCombate()
    {
        marcador.segundosRound = round;
        marcador.segundosDescanso = descanso;
        marcador.segundosMedico = medico;
        marcador.diferenciaPuntos = diferencia;
        marcador.diferenciaPuntosHabilitadaEnConfig = aplicaDiferencia;   // ver nota en MarcadorUI
        marcador.maximoGamJeoms = maxGamJeoms;
        marcador.ganadorPorGamJeomsHabilitado = aplicaGamJeoms;
        marcador.unSoloMando = unMando;
        marcador.NuevoCombate();

        Mostrar(null);
        if (pantallaCombate != null) pantallaCombate.SetActive(true);
    }

    // =====================================================================
    // MENÚ PRINCIPAL
    // =====================================================================
    void CrearMenu()
    {
        panelMenu = CrearPanelBase("PanelMenu", new Color(0.02f, 0.05f, 0.12f, 1f));
        CrearTitulo(panelMenu.transform, "COMBATE", new Vector2(0, 420), 64);

        var tarjeta = CrearTarjeta(panelMenu.transform, new Vector2(0, 0), new Vector2(700, 620));
        CrearBotonGrande(tarjeta.transform, "Iniciar", new Vector2(0, 195), () => Mostrar(panelComprobacion));
        CrearBotonGrande(tarjeta.transform, "Configuración", new Vector2(0, 65), () => Mostrar(panelConfig));
        CrearBotonGrande(tarjeta.transform, "Nombres", new Vector2(0, -65), () => Mostrar(panelNombres));
        CrearBotonGrande(tarjeta.transform, "Comprobación", new Vector2(0, -195), MostrarComprobacionDirecta);
    }

    void MostrarComprobacionDirecta()
    {
        Mostrar(panelComprobacion);
        listaMandos.SetActive(true);
    }

    // =====================================================================
    // CONFIGURACIÓN DE COMBATE
    // =====================================================================
    void CrearConfig()
    {
        panelConfig = CrearPanelBase("PanelConfig", new Color(0.02f, 0.05f, 0.12f, 1f));
        CrearTitulo(panelConfig.transform, "CONFIGURACIÓN DE COMBATE", new Vector2(0, 430), 40);

        TextMeshProUGUI etRound, etDescanso, etMedico;
        CrearSlider(panelConfig.transform, new Vector2(-420, 260), "Duración del round", 10, 180, round,
            v => { round = (int)v; }, out etRound);
        CrearSlider(panelConfig.transform, new Vector2(-420, 150), "Tiempo de descanso", 0, 60, descanso,
            v => { descanso = (int)v; }, out etDescanso);
        CrearSlider(panelConfig.transform, new Vector2(-420, 40), "Tiempo médico", 0, 120, medico,
            v => { medico = (int)v; }, out etMedico);

        CrearToggleConNumero(panelConfig.transform, new Vector2(420, 260), "Aplicar diferencia de puntos", aplicaDiferencia,
            diferencia, 1, 30, on => aplicaDiferencia = on, v => diferencia = v);
        CrearToggleConNumero(panelConfig.transform, new Vector2(420, 130), "Aplicar ganador por Gam-Jeoms", aplicaGamJeoms,
            maxGamJeoms, 1, 10, on => aplicaGamJeoms = on, v => maxGamJeoms = v);
        CrearToggleSimple(panelConfig.transform, new Vector2(420, 0), "Un mando", unMando, on => unMando = on);

        CrearBotonGrande(panelConfig.transform, "Regresar", new Vector2(-300, -420), () => Mostrar(panelMenu));
        CrearBotonGrande(panelConfig.transform, "Continuar", new Vector2(300, -420), () => Mostrar(panelMenu));
    }

    // =====================================================================
    // NOMBRES DE ATLETAS
    // =====================================================================
    void CrearNombres()
    {
        panelNombres = CrearPanelBase("PanelNombres", new Color(0.02f, 0.05f, 0.12f, 1f));
        CrearTitulo(panelNombres.transform, "NOMBRES DE ATLETAS", new Vector2(0, 420), 40);

        CrearTexto(panelNombres.transform, "Rojo", new Vector2(0, 140), 30);
        CrearCampoTexto(panelNombres.transform, new Vector2(0, 60), new Vector2(500, 70),
            marcador.nombreRojo, "Nombre del atleta rojo", v => marcador.nombreRojo = v);

        CrearTexto(panelNombres.transform, "Azul", new Vector2(0, -60), 30);
        CrearCampoTexto(panelNombres.transform, new Vector2(0, -140), new Vector2(500, 70),
            marcador.nombreAzul, "Nombre del atleta azul", v => marcador.nombreAzul = v);

        CrearBotonGrande(panelNombres.transform, "Regresar", new Vector2(0, -420), () => Mostrar(panelMenu));
    }

    TMP_InputField CrearCampoTexto(Transform padre, Vector2 pos, Vector2 tam, string valorInicial,
        string placeholder, System.Action<string> onChange)
    {
        var go = new GameObject("Campo", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = tam;
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);

        var area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        area.transform.SetParent(go.transform, false);
        var rtA = area.GetComponent<RectTransform>();
        rtA.anchorMin = Vector2.zero; rtA.anchorMax = Vector2.one;
        rtA.offsetMin = new Vector2(18, 6); rtA.offsetMax = new Vector2(-18, -6);

        var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        placeholderGo.transform.SetParent(area.transform, false);
        var rtP = placeholderGo.GetComponent<RectTransform>();
        rtP.anchorMin = Vector2.zero; rtP.anchorMax = Vector2.one;
        rtP.offsetMin = Vector2.zero; rtP.offsetMax = Vector2.zero;
        var tmpP = placeholderGo.GetComponent<TextMeshProUGUI>();
        tmpP.text = placeholder; tmpP.fontStyle = FontStyles.Italic;
        tmpP.color = new Color(1f, 1f, 1f, 0.4f); tmpP.fontSize = 28;
        tmpP.alignment = TextAlignmentOptions.MidlineLeft;

        var textoGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textoGo.transform.SetParent(area.transform, false);
        var rtT = textoGo.GetComponent<RectTransform>();
        rtT.anchorMin = Vector2.zero; rtT.anchorMax = Vector2.one;
        rtT.offsetMin = Vector2.zero; rtT.offsetMax = Vector2.zero;
        var tmpT = textoGo.GetComponent<TextMeshProUGUI>();
        tmpT.fontSize = 28; tmpT.alignment = TextAlignmentOptions.MidlineLeft; tmpT.color = Color.white;

        var campo = go.GetComponent<TMP_InputField>();
        campo.textViewport = rtA;
        campo.textComponent = tmpT;
        campo.placeholder = tmpP;
        campo.characterLimit = 16;
        campo.text = valorInicial;
        campo.onValueChanged.AddListener(v => onChange(v));
        return campo;
    }

    // =====================================================================
    // COMPROBACIÓN DE CONTROLES
    // =====================================================================
    void CrearComprobacion()
    {
        panelComprobacion = CrearPanelBase("PanelComprobacion", new Color(0.02f, 0.05f, 0.12f, 1f));
        CrearTitulo(panelComprobacion.transform, "COMBATE", new Vector2(0, 420), 48);

        var aviso = CrearTarjeta(panelComprobacion.transform, new Vector2(0, 60), new Vector2(700, 320));
        CrearTexto(aviso.transform, "Revise la conexión de mando(s)", new Vector2(0, 90), 30);
        CrearBotonGrande(aviso.transform, "Comprobar", new Vector2(0, 0), () => listaMandos.SetActive(true));
        CrearBotonGrande(aviso.transform, "Continuar sin comprobar", new Vector2(0, -100), EmpezarCombate);

        CrearBotonGrande(panelComprobacion.transform, "Regresar", new Vector2(0, -420), () => Mostrar(panelMenu));

        // Sub-panel con la cuadrícula de mandos, oculto hasta pulsar "Comprobar".
        listaMandos = CrearPanelBase("ListaMandos", new Color(0.02f, 0.05f, 0.12f, 1f));
        listaMandos.transform.SetParent(panelComprobacion.transform, false);
        listaMandos.SetActive(false);
        CrearTitulo(listaMandos.transform, "COMPROBACIÓN DE CONTROLES", new Vector2(0, 420), 40);
        for (int i = 0; i < 4; i++)
        {
            var fila = CrearTexto(listaMandos.transform, (i + 1) + ": Sin conexión", new Vector2(0, 260 - i * 90), 30);
            filasMandos.Add(fila);
        }
        CrearBotonGrande(listaMandos.transform, "Continuar", new Vector2(0, -420), EmpezarCombate);
    }

    void ActualizarListaMandos()
    {
        var mandos = Gamepad.all;
        for (int i = 0; i < filasMandos.Count; i++)
        {
            string estado = i < mandos.Count ? mandos[i].displayName : "Sin conexión";
            filasMandos[i].text = (i + 1) + ": " + estado;
        }
    }

    // =====================================================================
    // Utilidades de interfaz (mismo estilo que ConfiguracionMandosUI)
    // =====================================================================

    GameObject CrearPanelBase(string nombre, Color color)
    {
        var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(canvas, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return go;
    }

    GameObject CrearTarjeta(Transform padre, Vector2 pos, Vector2 tam)
    {
        var go = new GameObject("Tarjeta", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = tam;
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.06f);
        return go;
    }

    TextMeshProUGUI CrearTitulo(Transform padre, string texto, Vector2 pos, float tamano)
    {
        var t = CrearTexto(padre, texto, pos, tamano);
        t.fontStyle = FontStyles.Bold;
        return t;
    }

    TextMeshProUGUI CrearTexto(Transform padre, string texto, Vector2 pos, float tamano)
    {
        var go = new GameObject("Texto " + texto, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(900, 80);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = texto; tmp.fontSize = tamano; tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;   // una etiqueta nunca debe tapar el clic de un botón o casilla vecina
        return tmp;
    }

    void CrearBotonGrande(Transform padre, string texto, Vector2 pos, UnityEngine.Events.UnityAction accion)
    {
        var go = new GameObject("Boton " + texto, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(420, 90);
        go.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);
        go.GetComponent<Button>().onClick.AddListener(accion);

        var goTexto = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        goTexto.transform.SetParent(go.transform, false);
        var rtT = goTexto.GetComponent<RectTransform>();
        rtT.anchorMin = Vector2.zero; rtT.anchorMax = Vector2.one;
        rtT.offsetMin = Vector2.zero; rtT.offsetMax = Vector2.zero;
        var tmp = goTexto.GetComponent<TextMeshProUGUI>();
        tmp.text = texto; tmp.fontSize = 30; tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    void CrearSlider(Transform padre, Vector2 pos, string etiqueta, float min, float max, float valor,
        System.Action<float> onChange, out TextMeshProUGUI etiquetaValor)
    {
        CrearTexto(padre, etiqueta, pos + new Vector2(0, 45), 26);
        var valorTxt = CrearTexto(padre, FormatoTiempo(valor), pos + new Vector2(250, 0), 26);
        valorTxt.color = Color.yellow;
        etiquetaValor = valorTxt;

        var go = new GameObject("Slider " + etiqueta, typeof(RectTransform), typeof(Slider));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(400, 20);

        var bg = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(go.transform, false);
        var bgRt = bg.GetComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 0.3f); bgRt.anchorMax = new Vector2(1, 0.7f);
        bgRt.offsetMin = Vector2.zero; bgRt.offsetMax = Vector2.zero;
        bg.GetComponent<Image>().color = new Color(1, 1, 1, 0.15f);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        var faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = new Vector2(0, 0.3f); faRt.anchorMax = new Vector2(1, 0.7f);
        faRt.offsetMin = new Vector2(5, 0); faRt.offsetMax = new Vector2(-5, 0);

        var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        var fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero; fillRt.anchorMax = new Vector2(0, 1);
        fillRt.offsetMin = Vector2.zero; fillRt.offsetMax = Vector2.zero;
        fill.GetComponent<Image>().color = new Color(1f, 0.8f, 0f, 1f);

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        var haRt = handleArea.GetComponent<RectTransform>();
        haRt.anchorMin = Vector2.zero; haRt.anchorMax = Vector2.one;
        haRt.offsetMin = new Vector2(10, 0); haRt.offsetMax = new Vector2(-10, 0);

        var handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(handleArea.transform, false);
        var haRt2 = handle.GetComponent<RectTransform>();
        haRt2.sizeDelta = new Vector2(24, 24);
        handle.GetComponent<Image>().color = Color.white;

        var slider = go.GetComponent<Slider>();
        slider.fillRect = fillRt;
        slider.handleRect = haRt2;
        slider.targetGraphic = handle.GetComponent<Image>();
        slider.minValue = min; slider.maxValue = max; slider.wholeNumbers = true;
        slider.value = valor;
        slider.onValueChanged.AddListener(v =>
        {
            onChange(v);
            valorTxt.text = FormatoTiempo(v);
        });
    }

    void CrearToggleSimple(Transform padre, Vector2 pos, string etiqueta, bool valor, System.Action<bool> onChange)
    {
        var estado = CrearTexto(padre, valor ? "ACTIVADO" : "DESACTIVADO", pos + new Vector2(320, 0), 20);
        estado.color = valor ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.5f, 0.5f);
        estado.alignment = TextAlignmentOptions.MidlineLeft;

        CrearToggleBase(padre, pos, valor, on =>
        {
            onChange(on);
            estado.text = on ? "ACTIVADO" : "DESACTIVADO";
            estado.color = on ? new Color(0.4f, 1f, 0.4f) : new Color(1f, 0.5f, 0.5f);
        });
        CrearTexto(padre, etiqueta, pos + new Vector2(160, 0), 24).alignment = TextAlignmentOptions.MidlineLeft;
    }

    void CrearToggleConNumero(Transform padre, Vector2 pos, string etiqueta, bool valor, int numero, int min, int max,
        System.Action<bool> onChangeBool, System.Action<int> onChangeNumero)
    {
        CrearToggleBase(padre, pos, valor, onChangeBool);
        CrearTexto(padre, etiqueta, pos + new Vector2(160, 15), 22).alignment = TextAlignmentOptions.MidlineLeft;

        // Botones - / + para el número, ya que no hay teclado numérico dentro del panel.
        var etiquetaNum = CrearTexto(padre, "(" + numero + ")", pos + new Vector2(160, -25), 22);
        etiquetaNum.alignment = TextAlignmentOptions.MidlineLeft;
        etiquetaNum.color = Color.yellow;

        CrearBotonPequeno(padre, pos + new Vector2(330, -25), "-", () =>
        {
            numero = Mathf.Max(min, numero - 1);
            etiquetaNum.text = "(" + numero + ")";
            onChangeNumero(numero);
        });
        CrearBotonPequeno(padre, pos + new Vector2(380, -25), "+", () =>
        {
            numero = Mathf.Min(max, numero + 1);
            etiquetaNum.text = "(" + numero + ")";
            onChangeNumero(numero);
        });
    }

    void CrearBotonPequeno(Transform padre, Vector2 pos, string texto, UnityEngine.Events.UnityAction accion)
    {
        var go = new GameObject("Boton " + texto, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(36, 36);
        go.GetComponent<Image>().color = new Color(1, 1, 1, 0.15f);
        go.GetComponent<Button>().onClick.AddListener(accion);

        var goTexto = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        goTexto.transform.SetParent(go.transform, false);
        var rtT = goTexto.GetComponent<RectTransform>();
        rtT.anchorMin = Vector2.zero; rtT.anchorMax = Vector2.one;
        rtT.offsetMin = Vector2.zero; rtT.offsetMax = Vector2.zero;
        var tmp = goTexto.GetComponent<TextMeshProUGUI>();
        tmp.text = texto; tmp.fontSize = 22; tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    Toggle CrearToggleBase(Transform padre, Vector2 pos, bool valor, System.Action<bool> onChange)
    {
        // Imagen y Toggle en el MISMO objeto (igual que los botones), para que el clic no falle.
        var go = new GameObject("Toggle", typeof(RectTransform), typeof(Image), typeof(Toggle));
        go.transform.SetParent(padre, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = new Vector2(40, 40);
        var fondoImg = go.GetComponent<Image>();
        fondoImg.color = new Color(1, 1, 1, 0.15f);

        var marca = new GameObject("Marca", typeof(RectTransform), typeof(Image));
        marca.transform.SetParent(go.transform, false);
        var rtM = marca.GetComponent<RectTransform>();
        rtM.anchorMin = new Vector2(0.2f, 0.2f); rtM.anchorMax = new Vector2(0.8f, 0.8f);
        rtM.offsetMin = Vector2.zero; rtM.offsetMax = Vector2.zero;
        var marcaImg = marca.GetComponent<Image>();
        marcaImg.color = new Color(1f, 0.8f, 0f, 1f);
        marcaImg.raycastTarget = false;   // el clic lo recibe el fondo, no la marca

        var toggle = go.GetComponent<Toggle>();
        toggle.targetGraphic = fondoImg;
        toggle.graphic = marcaImg;
        toggle.isOn = valor;
        marcaImg.enabled = valor;   // estado inicial visible correcto (isOn=true crossfade puede tardar)
        toggle.onValueChanged.AddListener(on => onChange(on));
        return toggle;
    }

    string FormatoTiempo(float segundos)
    {
        int s = (int)segundos;
        return string.Format("{0:0}:{1:00}", s / 60, s % 60);
    }
}
