using TkdCombate.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Crea por código los botones manuales del árbitro y los conecta con el núcleo de reglas.
///   Lados:  fila de arriba +1 +2 +3 +4 +6 y +GJ  /  fila de abajo -1 -2 -3 -4 -6 y -GJ (tabla WT 2026)
///   Centro: Ganador rojo/azul, Iniciar/Pausa, Tiempo -1/+1, Médico, Round -/+, Nuevo combate
/// Solo necesitas 3 contenedores vacíos en el Canvas y arrastrarlos a los campos de este script.
/// </summary>
public class BotonesMarcador : MonoBehaviour
{
    public MarcadorUI marcador;
    public RectTransform contenedorRojo;
    public RectTransform contenedorAzul;
    public RectTransform contenedorCentro;

    public Color colorBotonRojo = new Color(0.45f, 0f, 0f, 1f);
    public Color colorBotonAzul = new Color(0.03f, 0.15f, 0.40f, 1f);
    public Color colorBotonCentro = new Color(0.15f, 0.15f, 0.15f, 1f);

    private Combate C { get { return marcador.Combate; } }

    void Start()
    {
        PrepararGrid(contenedorRojo, 6, 8f);
        PrepararGrid(contenedorAzul, 6, 8f);
        PrepararGrid(contenedorCentro, 3, 10f);

        ConstruirLado(contenedorRojo, Lado.Rojo, colorBotonRojo, false);
        ConstruirLado(contenedorAzul, Lado.Azul, colorBotonAzul, true);
        ConstruirCentro(contenedorCentro);
    }

    // ---------- Construcción ----------

    void ConstruirLado(RectTransform cont, Lado lado, Color color, bool invertir)
    {
        // Fila de arriba: sumar.  Fila de abajo: restar.
        // Columnas: las 5 técnicas (con los puntos de la tabla de la configuración) + gam-jeom.
        ConfiguracionCombate config = marcador.Combate.Config;
        for (int fila = 0; fila < 2; fila++)
        {
            bool sumar = fila == 0;
            for (int i = 0; i < 6; i++)
            {
                int idx = invertir ? 5 - i : i;          // el azul es el espejo del rojo
                if (idx < 5)
                {
                    TipoAtaque tipo = (TipoAtaque)(idx + 1);
                    string texto = (sumar ? "+" : "-") + config.PuntosDe(tipo);
                    if (sumar) CrearBoton(cont, texto, color, () => C.SumarPunto(lado, tipo));
                    else CrearBoton(cont, texto, color, () => C.RestarPunto(lado, tipo));
                }
                else
                {
                    if (sumar) CrearBoton(cont, "+GJ", color, () => C.SumarGamJeom(lado));
                    else CrearBoton(cont, "-GJ", color, () => C.RestarGamJeom(lado));
                }
            }
        }
    }

    void ConstruirCentro(RectTransform cont)
    {
        Color c = colorBotonCentro;
        CrearBoton(cont, "GANADOR ROJO", c, () => C.DeclararGanador(Lado.Rojo));
        CrearBoton(cont, "INICIAR / PAUSA", c, AlternarInicio);
        CrearBoton(cont, "GANADOR AZUL", c, () => C.DeclararGanador(Lado.Azul));

        CrearBoton(cont, "TIEMPO -1", c, () => C.AjustarTiempo(-1));
        CrearBoton(cont, "MEDICO", c, AlternarMedico);
        CrearBoton(cont, "TIEMPO +1", c, () => C.AjustarTiempo(1));

        CrearBoton(cont, "ROUND -", c, () => C.AjustarRound(-1));
        CrearBoton(cont, "NUEVO COMBATE", c, () => marcador.NuevoCombate());
        CrearBoton(cont, "ROUND +", c, () => C.AjustarRound(1));
    }

    void AlternarInicio()
    {
        if (!C.IniciarRound())
            if (!C.Pausar())
                C.Reanudar();
    }

    void AlternarMedico()
    {
        if (!C.IniciarTiempoMedico())
            C.TerminarTiempoMedico();
    }

    // ---------- Utilidades de interfaz ----------

    /// <summary>Acomoda los hijos en una cuadrícula que llena el contenedor.</summary>
    void PrepararGrid(RectTransform cont, int columnas, float espacio)
    {
        var grid = cont.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = cont.gameObject.AddComponent<GridLayoutGroup>();

        int filas = columnas == 3 ? 3 : 2;   // centro: 3 filas; lados: 2 filas
        float ancho = (cont.rect.width - espacio * (columnas - 1)) / columnas;
        float alto = (cont.rect.height - espacio * (filas - 1)) / filas;

        grid.cellSize = new Vector2(ancho, alto);
        grid.spacing = new Vector2(espacio, espacio);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columnas;
    }

    Button CrearBoton(RectTransform padre, string texto, Color color, UnityAction accion)
    {
        var go = new GameObject("Boton " + texto, typeof(RectTransform), typeof(Image), typeof(Button));
        go.layer = padre.gameObject.layer;
        go.transform.SetParent(padre, false);

        var imagen = go.GetComponent<Image>();
        imagen.color = color;

        var boton = go.GetComponent<Button>();
        boton.targetGraphic = imagen;
        boton.onClick.AddListener(accion);

        var goTexto = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
        goTexto.layer = go.layer;
        goTexto.transform.SetParent(go.transform, false);

        var rt = goTexto.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = goTexto.GetComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 12;
        tmp.fontSizeMax = 34;
        tmp.raycastTarget = false;

        return boton;
    }
}
