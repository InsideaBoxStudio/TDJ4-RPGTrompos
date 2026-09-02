using UnityEngine;
using UnityEngine.InputSystem;

// ============================================================================
//  PUERTA ÚNICA DE ENTRADA: JOYSTICK + TECLADO EN UN SOLO LUGAR
//  ----------------------------------------------------------------------------
//  PROBLEMA QUE RESUELVE
//  Cada script preguntaba por el input a su manera, repitiendo esta línea:
//
//      if (Gamepad.all.Count > playerIndex
//          && Gamepad.all[playerIndex].dpad.up.wasPressedThisFrame
//          || TeclasJugador.Atacar(playerIndex))
//
//  Y a varios se les olvidó la mitad del teclado: los 4 especiales del Caballero,
//  2 del Magus y la sustitución del Ninja SOLO leían joystick. Sin un control
//  conectado, esos personajes quedaban a medio jugar.
//
//  Ahora los scripts preguntan una sola cosa:
//
//      if (Controles.Atacar(playerIndex))
//
//  CÓMO ESTÁ ARMADO
//   - Controles (este archivo): la única API pública. Une joystick + teclado.
//   - TeclasJugador: solo el mapa de teclas. Es la mitad "teclado" de acá.
//
//  SI ALGÚN DÍA MIGRAN AL ASSET .inputactions
//  Este archivo es la costura: se reescribe SOLO acá adentro y los ~25 scripts
//  que lo usan no se enteran. Ese es el motivo de que exista esta capa.
//
//  Nota sobre el índice: 0 = Jugador 1, 1 = Jugador 2. Sale del PlayerIdentity
//  del trompo (ver PlayerIdentity.cs).
// ============================================================================
public static class Controles
{
    // Devuelve el gamepad de ese jugador, o null si no hay ninguno conectado en
    // esa posición. Chequear null es lo que permite jugar solo con teclado.
    private static Gamepad Pad(int p)
    {
        return (p >= 0 && Gamepad.all.Count > p) ? Gamepad.all[p] : null;
    }

    // ---- ACCIONES DE COMBATE ----
    // Cada una equivale a UN botón del joystick, y a su tecla equivalente.

    // dpad.up -> ataque básico (embestida)
    public static bool Atacar(int p)
    {
        Gamepad g = Pad(p);
        return (g != null && g.dpad.up.wasPressedThisFrame) || TeclasJugador.Atacar(p);
    }

    // buttonNorth -> avanzar
    public static bool Avanzar(int p)
    {
        Gamepad g = Pad(p);
        return (g != null && g.buttonNorth.wasPressedThisFrame) || TeclasJugador.Avanzar(p);
    }

    // buttonSouth -> esperar (y frenar la barra de lanzamiento)
    public static bool Esperar(int p)
    {
        Gamepad g = Pad(p);
        return (g != null && g.buttonSouth.wasPressedThisFrame) || TeclasJugador.Esperar(p);
    }

    // dpad.right -> shuriken (Ninja) / disparo mágico (Magus) / gravedad (Caballero)
    public static bool Derecha(int p)
    {
        Gamepad g = Pad(p);
        return (g != null && g.dpad.right.wasPressedThisFrame) || TeclasJugador.Shuriken(p);
    }

    // dpad.left -> pinchos (Ninja) / orbes protectores (Magus) / estocada (Caballero)
    public static bool Izquierda(int p)
    {
        Gamepad g = Pad(p);
        return (g != null && g.dpad.left.wasPressedThisFrame) || TeclasJugador.Pinchos(p);
    }

    // dpad.down -> cierra (Ninja) / orbe y torreta (Magus)
    public static bool Abajo(int p)
    {
        Gamepad g = Pad(p);
        return (g != null && g.dpad.down.wasPressedThisFrame) || TeclasJugador.Abajo(p);
    }

    // leftShoulder (L1) -> veneno/fuego/parálisis, espada (Caballero), parry
    public static bool L1(int p)
    {
        Gamepad g = Pad(p);
        return (g != null && g.leftShoulder.wasPressedThisFrame) || TeclasJugador.L1(p);
    }

    // rightShoulder (R1) -> clon (Ninja) / hielo (Magus) / onda expansiva (Caballero)
    public static bool R1(int p)
    {
        Gamepad g = Pad(p);
        return (g != null && g.rightShoulder.wasPressedThisFrame) || TeclasJugador.R1(p);
    }

    // leftTrigger (L2) -> sustitución (Ninja)
    public static bool L2(int p)
    {
        Gamepad g = Pad(p);
        return (g != null && g.leftTrigger.wasPressedThisFrame) || TeclasJugador.L2(p);
    }

    // ---- MENÚS RADIALES (se mantienen apretados, no son un toque) ----

    // buttonWest mantenido -> abre la barra lateral 1
    public static bool Menu1(int p)
    {
        Gamepad g = Pad(p);
        return (g != null && g.buttonWest.isPressed) || TeclasJugador.Menu1(p);
    }

    // buttonEast mantenido -> abre la barra lateral 2
    public static bool Menu2(int p)
    {
        Gamepad g = Pad(p);
        return (g != null && g.buttonEast.isPressed) || TeclasJugador.Menu2(p);
    }

    // ---- APUNTAR ----
    // Devuelve la dirección del stick derecho; si no se toca, la del teclado.
    public static Vector2 Apuntar(int p)
    {
        Gamepad g = Pad(p);
        Vector2 stick = g != null ? g.rightStick.ReadValue() : Vector2.zero;

        // El teclado tiene prioridad solo si el stick está en reposo: así, con un
        // joystick conectado, apoyar el pulgar no anula las teclas.
        Vector2 teclas = TeclasJugador.Apuntar(p);
        return teclas != Vector2.zero ? teclas : stick;
    }
}
