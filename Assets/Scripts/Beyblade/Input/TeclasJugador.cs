using UnityEngine;
using UnityEngine.InputSystem;

// >>> TECLADO <<<
// Mapeo de teclado centralizado para los dos jugadores.
// Es FIEL al joystick: cada metodo equivale a un boton del gamepad.
// Jugador 0 (P1) = teclas izquierdas: WASD (apuntar+dir) + Q E R F (botones) + C V (menus).
// Jugador 1 (P2) = teclas derechas:  IJKL (apuntar+dir) + U O H N (botones) + M , (menus).
//
// Para QUITAR el teclado: borrar este archivo y las lineas "|| TeclasJugador..."
// (y el bloque de Prompter) marcadas con ">>> TECLADO <<<" en los scripts.
public static class TeclasJugador
{
    private static Keyboard K => Keyboard.current;

    private static bool Press(Key key) => K != null && K[key].wasPressedThisFrame;
    private static bool Hold(Key key)  => K != null && K[key].isPressed;

    // --- Acciones (equivalen a un boton del joystick por jugador) ---

    // dpad.up  -> Atacar (ataque basico)
    public static bool Atacar(int p)   => Press(p == 0 ? Key.W : Key.I);
    // dpad.right -> Shuriken
    public static bool Shuriken(int p) => Press(p == 0 ? Key.D : Key.L);
    // dpad.left -> Pinchos
    public static bool Pinchos(int p)  => Press(p == 0 ? Key.A : Key.J);
    // dpad.down -> Cierra (Ninja) / Orbe (Magus)
    public static bool Abajo(int p)    => Press(p == 0 ? Key.S : Key.K);
    // buttonNorth -> Avanzar (P1=E, P2=O)
    public static bool Avanzar(int p)  => Press(p == 0 ? Key.E : Key.O);
    // buttonSouth -> Esperar + frenar barra (P1=Q, P2=U)
    public static bool Esperar(int p)  => Press(p == 0 ? Key.Q : Key.U);
    // leftShoulder (L1) -> P1=R, P2=H. Boton fisico compartido:
    //   en tu turno: Veneno (Ninja) / Fuego / Paralisis (Magus). Defendiendo: Parry.
    public static bool L1(int p)       => Press(p == 0 ? Key.R : Key.H);
    // rightShoulder (R1) -> P1=F, P2=N. Compartido: Clon (Ninja) / Hielo (Magus).
    public static bool R1(int p)       => Press(p == 0 ? Key.F : Key.N);
    // Alias legibles (misma tecla que L1/R1):
    public static bool Veneno(int p)   => L1(p);
    public static bool Clon(int p)     => R1(p);
    // buttonWest (mantener) -> abrir menu/barra lateral 1 (P1=C, P2=M)
    public static bool Menu1(int p)    => Hold(p == 0 ? Key.C : Key.M);
    // buttonEast (mantener) -> abrir menu/barra lateral 2 (P1=V, P2=coma)
    public static bool Menu2(int p)    => Hold(p == 0 ? Key.V : Key.Comma);

    // leftStick -> Apuntar. Devuelve la direccion WASD/IJKL como vector
    // (Vector2.zero si no se toca nada). El Prompter la usa para orientar el trompo.
    public static Vector2 Apuntar(int p)
    {
        if (K == null) return Vector2.zero;
        Vector2 v = Vector2.zero;
        if (p == 0)
        {
            if (K.wKey.isPressed) v.y += 1f;
            if (K.sKey.isPressed) v.y -= 1f;
            if (K.dKey.isPressed) v.x += 1f;
            if (K.aKey.isPressed) v.x -= 1f;
        }
        else
        {
            if (K.iKey.isPressed) v.y += 1f;
            if (K.kKey.isPressed) v.y -= 1f;
            if (K.lKey.isPressed) v.x += 1f;
            if (K.jKey.isPressed) v.x -= 1f;
        }
        return v;
    }
}
// <<< FIN TECLADO <<<
