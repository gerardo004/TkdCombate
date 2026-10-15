using TkdCombate.Core;
using UnityEngine;

/// <summary>
/// Guarda, para cada juez y cada lado, qué número presionó por última vez y desde cuándo.
/// ColumnasJueces lo lee cada frame para saber qué casilla iluminar.
/// No decide nada de las reglas: es solo un tablero de luces.
/// </summary>
public class PizarraJueces : MonoBehaviour
{
    [Tooltip("Cuánto tiempo queda iluminado un voto en pantalla.")]
    public float duracionLuz = 1.2f;

    private const int MaxJueces = 4;   // columnas J1..J4 por si algún día hay 4 mandos
    private const int Tipos = 6;       // 0 sin usar, 1..5 técnicas

    // [juez, lado] = (tipo presionado, momento)
    private readonly int[,] tipoActual = new int[MaxJueces, 2];
    private readonly float[,] momento = new float[MaxJueces, 2];

    /// <summary>Llamar cuando un juez presiona un botón, sin importar si el voto se validó o no.</summary>
    public void Marcar(int juez, Lado lado, TipoAtaque tipo)
    {
        int j = juez - 1;
        if (j < 0 || j >= MaxJueces) return;
        tipoActual[j, (int)lado] = (int)tipo;
        momento[j, (int)lado] = Time.time;
    }

    /// <summary>El número (1..5) que ese juez tiene iluminado ahora mismo en ese lado, o 0 si ninguno.</summary>
    public int NumeroIluminado(int juez, Lado lado)
    {
        int j = juez - 1;
        if (j < 0 || j >= MaxJueces) return 0;
        if (Time.time - momento[j, (int)lado] > duracionLuz) return 0;
        return tipoActual[j, (int)lado];
    }
}
