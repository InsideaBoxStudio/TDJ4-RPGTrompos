using UnityEngine;
using UnityEngine.InputSystem;

// ============================================================================
//  QUÉ JOYSTICK ES DE CADA JUGADOR (un solo dueño de esa decisión)
//  ----------------------------------------------------------------------------
//  PROBLEMA QUE RESUELVE
//  Antes cada script decidía por su cuenta con Gamepad.all[playerIndex]: "el
//  primer joystick de la lista es del J1, el segundo del J2". En el navegador esa
//  lista es estable, pero en Windows no: si un joystick Bluetooth se duerme y se
//  reconecta vuelve al FINAL de la lista, y programas como Steam o DS4Windows
//  pueden agregar un joystick virtual. Cada vez que la lista cambiaba, el J2 pasaba
//  a manejar el trompo del J1 y los íconos de botones saltaban entre PS y Xbox.
//
//  CÓMO FUNCIONA
//  Cada jugador tiene un "lugar". Al arrancar, los joysticks conectados ocupan los
//  lugares en orden, y desde ahí el lugar no cambia:
//   - si un joystick se desconecta, su lugar queda vacío y RESERVADO para él;
//     cuando vuelve, recupera el mismo lugar (y el mismo trompo).
//   - un joystick nuevo ocupa el primer lugar vacío.
//  Todos (combate, lanzador, menús, selector e íconos) preguntan acá.
// ============================================================================
public static class JoystickPorJugador
{
    public const int Jugadores = 2;

    private static readonly Gamepad[] lugar = new Gamepad[Jugadores];
    // El último joystick que tuvo cada lugar, para devolvérselo si se reconecta.
    private static readonly Gamepad[] reservado = new Gamepad[Jugadores];
    private static bool iniciado;

    // Con "Enter Play Mode" sin recarga de dominio los estáticos sobreviven entre
    // corridas del Editor: se limpian al arrancar cada partida.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reiniciar()
    {
        if (iniciado) InputSystem.onDeviceChange -= AlCambiarDispositivo;
        iniciado = false;
        for (int i = 0; i < Jugadores; i++) { lugar[i] = null; reservado[i] = null; }
    }

    // Joystick del jugador p (0 = J1, 1 = J2), o null si no tiene: juega con teclado.
    public static Gamepad De(int p)
    {
        Iniciar();
        return (p >= 0 && p < Jugadores) ? lugar[p] : null;
    }

    // Mismo criterio que usaba el juego para el teclado ("Gamepad.all.Count == p"):
    // el J1 usa teclado si no tiene joystick; el J2, si el J1 tiene y él no.
    public static bool UsaTeclado(int p)
    {
        if (De(p) != null) return false;
        return p == 0 || De(p - 1) != null;
    }

    private static void Iniciar()
    {
        if (iniciado) return;
        iniciado = true;
        InputSystem.onDeviceChange += AlCambiarDispositivo;
        Ubicar();
    }

    private static void AlCambiarDispositivo(InputDevice dispositivo, InputDeviceChange cambio)
    {
        if (!(dispositivo is Gamepad pad)) return;

        if (cambio == InputDeviceChange.Removed || cambio == InputDeviceChange.Disconnected)
        {
            for (int i = 0; i < Jugadores; i++)
            {
                if (lugar[i] != pad) continue;
                lugar[i] = null;
                reservado[i] = pad;
            }
        }
        else if (cambio == InputDeviceChange.Added || cambio == InputDeviceChange.Reconnected)
        {
            Ubicar();
        }
    }

    // Pone cada joystick conectado que todavía no tiene lugar: primero devuelve los
    // reservados a su dueño, después llena los lugares vacíos en orden.
    private static void Ubicar()
    {
        var conectados = Gamepad.all;

        for (int g = 0; g < conectados.Count; g++)
        {
            Gamepad pad = conectados[g];
            for (int i = 0; i < Jugadores; i++)
            {
                if (lugar[i] == null && reservado[i] == pad)
                {
                    lugar[i] = pad;
                    reservado[i] = null;
                }
            }
        }

        for (int g = 0; g < conectados.Count; g++)
        {
            Gamepad pad = conectados[g];
            if (TieneLugar(pad)) continue;

            int libre = PrimerLugarLibre();
            if (libre < 0) return; // ya hay un joystick por jugador

            lugar[libre] = pad;
            reservado[libre] = null;
        }
    }

    private static bool TieneLugar(Gamepad pad)
    {
        for (int i = 0; i < Jugadores; i++)
            if (lugar[i] == pad) return true;
        return false;
    }

    // Prefiere un lugar sin reserva; si todos los vacíos están reservados, usa el
    // primero igual (el joystick original no volvió y alguien tiene que jugar).
    private static int PrimerLugarLibre()
    {
        for (int i = 0; i < Jugadores; i++)
            if (lugar[i] == null && reservado[i] == null) return i;
        for (int i = 0; i < Jugadores; i++)
            if (lugar[i] == null) return i;
        return -1;
    }
}
