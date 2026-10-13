using UnityEngine;
using UnityEngine.InputSystem;

// >>> TECLADO <<<
// Mapeo de teclado centralizado para los dos jugadores.
// Es FIEL al joystick: cada metodo equivale a un boton del gamepad.
// Jugador 0 (P1) = teclas izquierdas: WASD (apuntar) + flechas (menu de ataques) + Q E R F T (botones) + C V (menus).
// Jugador 1 (P2) = teclas derechas:  IJKL (apuntar y menu de ataques) + U O H N P (botones) + M , (menus).
//
// Este archivo es SOLO el mapa de teclas. Los scripts de pelea preguntan por aca.
// Un jugador usa el teclado cuando no tiene joystick propio (UsaTeclado): con dos
// joysticks no se usa; con uno, el P2 juega con teclado; sin ninguno, juegan los dos
// con teclas distintas.
public static class TeclasJugador
{
    private static Keyboard K => Keyboard.current;

    // Jugadores que maneja la CPU: el teclado no les llega aunque alguien toque sus teclas
    // (si no, el J2 humano le movia, apuntaba y atacaba a la CPU).
    private static readonly bool[] esIA = new bool[2];

    // Lo llama PresetCPU al nacer (true) y al destruirse (false). PlayerSettings es estatico
    // y sobrevive entre escenas, por eso se libera: en un 1 contra 1 el P2 vuelve a ser humano.
    public static void MarcarIA(int p, bool valor)
    {
        if (p >= 0 && p < esIA.Length) esIA[p] = valor;
    }

    private static bool Libre(int p) => p < 0 || p >= esIA.Length || !esIA[p];

    private static bool Press(int p, Key key) => K != null && Libre(p) && K[key].wasPressedThisFrame;
    private static bool Hold(int p, Key key)  => K != null && Libre(p) && K[key].isPressed;

    // El jugador p juega con teclado si no tiene joystick: los joysticks se reparten en orden
    // de conexion (el primero es el J1), asi que al J1 le toca teclado solo si no hay ninguno
    // y al J2 si hay menos de dos.
    public static bool UsaTeclado(int p) => K != null && Libre(p) && p >= Gamepad.all.Count;

    // --- Acciones (equivalen a un boton del joystick por jugador) ---

    // dpad.up  -> Atacar (ataque basico)
    public static bool Atacar(int p)   => Press(p, p == 0 ? Key.W : Key.I);
    // dpad.right -> Shuriken
    public static bool Shuriken(int p) => Press(p, p == 0 ? Key.D : Key.L);
    // dpad.left -> Pinchos
    public static bool Pinchos(int p)  => Press(p, p == 0 ? Key.A : Key.J);
    // dpad.down -> Cierra (Ninja) / Orbe (Magus)
    public static bool Abajo(int p)    => Press(p, p == 0 ? Key.S : Key.K);
    // buttonNorth -> Avanzar (P1=E, P2=O)
    public static bool Avanzar(int p)  => Press(p, p == 0 ? Key.E : Key.O);
    // buttonSouth -> Esperar + frenar barra (P1=Q, P2=U). El J2 tambien con Enter,
    // para que coincida con el que usa para elegir personaje (flechas + Enter).
    public static bool Esperar(int p)  => Press(p, p == 0 ? Key.Q : Key.U)
                                       || (p == 1 && (Press(p, Key.Enter) || Press(p, Key.NumpadEnter)));
    // leftShoulder (L1) -> P1=R, P2=H. Boton fisico compartido:
    //   en tu turno: Veneno (Ninja) / Fuego / Paralisis (Magus). Defendiendo: Parry.
    public static bool L1(int p)       => Press(p, p == 0 ? Key.R : Key.H);
    // rightShoulder (R1) -> P1=F, P2=N. Compartido: Clon (Ninja) / Hielo (Magus).
    public static bool R1(int p)       => Press(p, p == 0 ? Key.F : Key.N);
    // leftTrigger (L2) -> Sustitucion (Ninja). P1=T, P2=P.
    public static bool L2(int p)       => Press(p, p == 0 ? Key.T : Key.P);
    // Alias legibles (misma tecla que L1/R1):
    public static bool Veneno(int p)   => L1(p);
    public static bool Clon(int p)     => R1(p);
    // buttonWest (mantener) -> abrir menu/barra lateral 1 (P1=C, P2=M)
    public static bool Menu1(int p)    => Hold(p, p == 0 ? Key.C : Key.M);
    // buttonEast (mantener) -> abrir menu/barra lateral 2 (P1=V, P2=coma)
    public static bool Menu2(int p)    => Hold(p, p == 0 ? Key.V : Key.Comma);

    // dpad con el menu de ataques abierto, en el orden que usa AttacksMenuPreset:
    // 0 = arriba, 1 = derecha, 2 = izquierda, 3 = abajo, -1 = ninguno.
    // El P1 elige con las flechas (el WASD queda para apuntar); el P2 no tiene dos manos
    // libres, asi que elige con las mismas IJKL con las que apunta (ver Apuntar).
    public static int MenuDireccion(int p)
    {
        if (!UsaTeclado(p)) return -1;

        if (p == 0)
        {
            if (K.upArrowKey.isPressed)    return 0;
            if (K.rightArrowKey.isPressed) return 1;
            if (K.leftArrowKey.isPressed)  return 2;
            if (K.downArrowKey.isPressed)  return 3;
            return -1;
        }

        if (K.iKey.isPressed) return 0;
        if (K.lKey.isPressed) return 1;
        if (K.jKey.isPressed) return 2;
        if (K.kKey.isPressed) return 3;
        return -1;
    }

    // leftStick -> Apuntar. Devuelve la direccion WASD/IJKL como vector
    // (Vector2.zero si no se toca nada). El Prompter la usa para orientar el trompo.
    public static Vector2 Apuntar(int p)
    {
        if (K == null || !Libre(p)) return Vector2.zero;

        // El P2 elige ataque con las mismas teclas con las que apunta: mientras tiene un menu
        // abierto no apunta, si no el trompo giraria hacia donde eligio el ataque.
        if (p == 1 && (Menu1(p) || Menu2(p))) return Vector2.zero;

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
