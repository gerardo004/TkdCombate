using TkdCombate.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Prueba mínima: conecta el núcleo de reglas con 3 textos en pantalla.
/// Espacio = iniciar / pausar / reanudar.  Q = punto rojo.  P = punto azul.
/// </summary>
public class PruebaCombate : MonoBehaviour
{
    // Estos 3 campos aparecen en el Inspector: arrastra ahí los textos.
    public TMP_Text textoReloj;
    public TMP_Text textoRojo;
    public TMP_Text textoAzul;

    private Combate combate;

    void Start()
    {
        var config = new ConfiguracionCombate();
        config.SegundosRound = 20;      // round corto para probar rápido
        config.SegundosDescanso = 5;

        combate = new Combate(config);
        combate.Cambio += Refrescar;    // cada vez que el núcleo cambia, actualizamos la pantalla
        Refrescar();
    }

    void Update()
    {
        combate.Tick(Time.deltaTime);   // hace avanzar el reloj del combate

        var teclado = Keyboard.current;
        if (teclado == null) return;

        if (teclado.spaceKey.wasPressedThisFrame)
        {
            if (!combate.IniciarRound())
                if (!combate.Pausar())
                    combate.Reanudar();
        }
        if (teclado.qKey.wasPressedThisFrame) combate.SumarPunto(Lado.Rojo, TipoAtaque.Punio);
        if (teclado.pKey.wasPressedThisFrame) combate.SumarPunto(Lado.Azul, TipoAtaque.Punio);
    }

    void Refrescar()
    {
        int s = Mathf.CeilToInt((float)combate.SegundosRestantes);
        textoReloj.text = string.Format("{0:00}:{1:00}   ROUND {2}   {3}",
            s / 60, s % 60, combate.RoundActual, combate.Estado);
        textoRojo.text = combate.PuntosRound(Lado.Rojo).ToString();
        textoAzul.text = combate.PuntosRound(Lado.Azul).ToString();
    }
}
