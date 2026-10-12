using UnityEngine;
using UnityEngine.InputSystem;

// Lista de consejos de la pantalla de selección. Cambia sola cada nextTipTime
// segundos y el jugador la puede mover a mano:
//   - L1 / R1 de CUALQUIER joystick conectado,
//   - Q / E del teclado,
//   - los botones L y R de la pantalla (que llaman a PreviousTip / NextTip).
//
// Antes solo respondía al L1/R1 del PRIMER joystick y solo si había DOS o más
// conectados (Gamepad.all.Count > 1): con un solo joystick, o solo con teclado, los
// botones L y R no hacían nada.
public class ListOfTips : MonoBehaviour
{
    [SerializeField] private GameObject[] tips;
    [SerializeField] private float nextTipTime = 5f;
    [SerializeField] private int currentTip = 0;

    // Segundos desde el último cambio. Se reinicia al cambiar a mano, así el consejo
    // que el jugador acaba de elegir no desaparece enseguida por el cambio automático.
    private float tiempoDesdeElCambio;

    private void Start()
    {
        Mostrar();
    }

    private void Update()
    {
        if (tips == null || tips.Length == 0) return;

        if (PidioAnterior()) PreviousTip();
        else if (PidioSiguiente()) NextTip();

        // Tiempo sin escalar: en un menú el timeScale puede haber quedado en 0 de la
        // pelea anterior y el cambio automático no correría nunca.
        tiempoDesdeElCambio += Time.unscaledDeltaTime;
        if (tiempoDesdeElCambio >= nextTipTime) NextTip();
    }

    // Públicos: los botones L y R de la pantalla los llaman desde su onClick.
    public void PreviousTip()
    {
        IrA(currentTip - 1);
    }

    public void NextTip()
    {
        IrA(currentTip + 1);
    }

    private void IrA(int indice)
    {
        if (tips == null || tips.Length == 0) return;

        // Vuelta circular, también hacia atrás desde el primero.
        currentTip = ((indice % tips.Length) + tips.Length) % tips.Length;
        tiempoDesdeElCambio = 0f;
        Mostrar();
    }

    private void Mostrar()
    {
        if (tips == null) return;

        for (int i = 0; i < tips.Length; i++)
        {
            if (tips[i] != null) tips[i].SetActive(i == currentTip);
        }
    }

    // ---- Entrada ----
    private static bool PidioAnterior()
    {
        for (int i = 0; i < Gamepad.all.Count; i++)
            if (Gamepad.all[i].leftShoulder.wasPressedThisFrame) return true;

        Keyboard teclado = Keyboard.current;
        return teclado != null && teclado.qKey.wasPressedThisFrame;
    }

    private static bool PidioSiguiente()
    {
        for (int i = 0; i < Gamepad.all.Count; i++)
            if (Gamepad.all[i].rightShoulder.wasPressedThisFrame) return true;

        Keyboard teclado = Keyboard.current;
        return teclado != null && teclado.eKey.wasPressedThisFrame;
    }
}
