using TkdCombate.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Crea por código, dentro de un contenedor, 3 columnas (J1, J2, J3) con los números 1 a 5.
/// Cada número se enciende cuando ese juez lo presiona para ese lado (lee PizarraJueces).
/// Se necesita un ContenedorJuecesRojo y un ContenedorJuecesAzul, uno de estos scripts en cada uno.
/// </summary>
public class ColumnasJueces : MonoBehaviour
{
    public PizarraJueces pizarra;
    public Lado lado;
    public int numeroDeJueces = 3;

    public Color colorApagado = new Color(1f, 1f, 1f, 0.25f);
    public Color colorEncendido = Color.yellow;

    private TextMeshProUGUI[,] celdas;   // [columna de juez, número 1..5]

    void Start()
    {
        var rt = GetComponent<RectTransform>();
        var grid = gameObject.AddComponent<GridLayoutGroup>();
        float ancho = rt.rect.width / numeroDeJueces;
        float alto = rt.rect.height / 5f;
        grid.cellSize = new Vector2(ancho, alto);
        grid.startAxis = GridLayoutGroup.Axis.Vertical;   // llena columna por columna
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 5;

        celdas = new TextMeshProUGUI[numeroDeJueces, 6];  // índice 1..5 usado
        for (int j = 0; j < numeroDeJueces; j++)
            for (int n = 1; n <= 5; n++)
                celdas[j, n] = CrearCelda(n.ToString());
    }

    void Update()
    {
        if (pizarra == null || celdas == null) return;
        for (int j = 0; j < numeroDeJueces; j++)
        {
            int iluminado = pizarra.NumeroIluminado(j + 1, lado);
            for (int n = 1; n <= 5; n++)
                celdas[j, n].color = (n == iluminado) ? colorEncendido : colorApagado;
        }
    }

    TextMeshProUGUI CrearCelda(string texto)
    {
        var go = new GameObject("Num " + texto, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(transform, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = texto;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 10;
        tmp.fontSizeMax = 28;
        tmp.color = colorApagado;
        tmp.raycastTarget = false;
        return tmp;
    }
}
