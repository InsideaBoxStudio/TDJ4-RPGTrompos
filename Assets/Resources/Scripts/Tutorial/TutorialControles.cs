using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

// ============================================================================
//  NOMBRES DE LOS CONTROLES PARA LOS CARTELES
//  ----------------------------------------------------------------------------
//  Los carteles tienen que nombrar el boton que el jugador tiene en la mano. Si hay un
//  joystick conectado se habla de SUS botones, con los nombres de su marca (PlayStation:
//  cruz, circulo, cuadrado, triangulo, L1, R1, L2; Xbox: A, B, X, Y, LB, RB, LT); si no hay
//  ninguno, de las teclas. Las teclas salen de los scripts del juego (AttacksMenuPreset: flechas
//  para elegir el golpe; ExpandMenu / ExpandMenu2: C y V para los menus; TimingBar y Wait: Q):
//  si alguien cambia una tecla alla, hay que actualizar esta tabla.
//
//  La marca se deduce del tipo de dispositivo que reporta el Input System. Un joystick que
//  llega por un programa que lo disfraza (por ejemplo un DualShock emulado como Xbox) se ve
//  como Xbox: por eso el cartel muestra al pie QUE CONTROL detecto, asi se nota enseguida
//  si no coincide con el que se tiene en la mano.
//
//  Todo se calcula al momento de preguntar (no se guarda): si se conecta o desconecta un
//  joystick con el tutorial abierto, el director vuelve a armar los textos (ver Firma).
// ============================================================================
public static class TutorialControles
{
    private enum Marca { Teclado, PlayStation, Xbox, Generico }

    private static Gamepad Pad => Gamepad.all.Count > 0 ? Gamepad.all[0] : null;

    private static Marca Actual
    {
        get
        {
            Gamepad pad = Pad;
            if (pad == null) return Marca.Teclado;
            if (pad is DualShockGamepad) return Marca.PlayStation;
            if (pad is XInputController) return Marca.Xbox;
            return Marca.Generico;
        }
    }

    public static bool ConJoystick => Pad != null;

    // Cambia cuando cambia el tipo de control o cuantos hay: el director la compara para saber
    // cuando rehacer los textos.
    public static string Firma => Actual + "/" + Gamepad.all.Count;

    // Para mostrar al pie del cartel: lo que el juego detecto.
    public static string Detectado
    {
        get
        {
            Gamepad pad = Pad;
            if (pad == null) return "Teclado (no hay joystick conectado)";

            string nombre = string.IsNullOrEmpty(pad.displayName) ? pad.name : pad.displayName;
            switch (Actual)
            {
                case Marca.PlayStation: return "Joystick PlayStation (" + nombre + ")";
                case Marca.Xbox: return "Joystick Xbox (" + nombre + ")";
                default: return "Joystick (" + nombre + ")";
            }
        }
    }

    // ---- Botones de forma ----

    private static string Forma(string playStation, string xbox, string generico, string teclado)
    {
        switch (Actual)
        {
            case Marca.PlayStation: return playStation;
            case Marca.Xbox: return xbox;
            case Marca.Generico: return generico;
            default: return teclado;
        }
    }

    // Los golpes se eligen por direccion con el menu abierto (AttacksMenuPreset): cruceta en el
    // joystick, flechas en el teclado. El orden de las direcciones es el de los prefabs de opciones
    // que el DefaultPlayerSettings de la escena le da al jugador.
    public static string Atacar => ConJoystick ? "cruceta ARRIBA" : "flecha ARRIBA";
    public static string Derecha => ConJoystick ? "cruceta DERECHA" : "flecha DERECHA";
    public static string Izquierda => ConJoystick ? "cruceta IZQUIERDA" : "flecha IZQUIERDA";
    public static string Abajo => ConJoystick ? "cruceta ABAJO" : "flecha ABAJO";

    // El veneno es la segunda opcion del menu de especiales: con ese menu abierto, direccion derecha.
    public static string Veneno => Derecha;

    // buttonNorth / buttonSouth / buttonWest / buttonEast
    public static string Avanzar => Forma("triángulo", "Y", "botón de arriba", "E");
    public static string Esperar => Forma("X", "A", "botón de abajo", "Q");
    public static string Menu1 => Forma("cuadrado", "X", "botón izquierdo", "C");
    public static string Menu2 => Forma("círculo", "B", "botón derecho", "V");

    // gatillos y hombros
    public static string L1 => Forma("L1", "LB", "gatillo izquierdo (superior)", "R");
    public static string R1 => Forma("R1", "RB", "gatillo derecho (superior)", "F");
    public static string L2 => Forma("L2", "LT", "gatillo izquierdo (inferior)", "T");

    public static string Apuntar => ConJoystick ? "el STICK DERECHO" : "las teclas W A S D";

    // Para volver al menu al terminar: el boton de abajo lo atiende EndGame (Controles.Confirmar) y R3 el
    // director; con teclado, ENTER o ESPACIO.
    public static string ConfirmarFinal => ConJoystick ? Esperar + " o R3" : "ENTER o ESPACIO";
}
