using UnityEngine.InputSystem;

// ============================================================================
//  NOMBRES DE LOS CONTROLES PARA LOS CARTELES
//  ----------------------------------------------------------------------------
//  Los carteles tienen que nombrar el boton que el jugador tiene en la mano: si hay un
//  joystick conectado se habla de la cruceta y los botones, si no, de las teclas. Los
//  nombres salen del mismo mapa que usa el juego (Controles / TeclasJugador, jugador 0),
//  asi que si alguien cambia una tecla alla, solo hay que actualizar esta tabla.
//
//  Para los botones de forma se dan los dos nombres (PlayStation / Xbox) porque desde
//  el codigo no se puede saber de que marca es el joystick con certeza.
// ============================================================================
public static class TutorialControles
{
    public static bool ConJoystick => Gamepad.all.Count > 0;

    public static string Atacar => ConJoystick ? "cruceta ARRIBA" : "W";
    public static string Derecha => ConJoystick ? "cruceta DERECHA" : "D";
    public static string Izquierda => ConJoystick ? "cruceta IZQUIERDA" : "A";
    public static string Abajo => ConJoystick ? "cruceta ABAJO" : "S";

    public static string Avanzar => ConJoystick ? "triángulo / Y" : "E";
    public static string Esperar => ConJoystick ? "cruz / A" : "Q";

    public static string L1 => ConJoystick ? "L1 / LB" : "R";
    public static string R1 => ConJoystick ? "R1 / RB" : "F";
    public static string L2 => ConJoystick ? "L2 / LT" : "T";

    public static string Menu1 => ConJoystick ? "cuadrado / X" : "C";
    public static string Menu2 => ConJoystick ? "círculo / B" : "V";

    public static string Apuntar => ConJoystick ? "el STICK DERECHO" : "las teclas W A S D";

    public const string Continuar = "ENTER o START";
    public const string Saltar = "TAB o R3";
}
