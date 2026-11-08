using UnityEngine;
using UnityEngine.InputSystem;

public class ExpandMenu : MonoBehaviour
{
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private GameObject menuToExpand;
    [SerializeField] private int playerIndex;

    // El playerIndex sale del PlayerIdentity del trompo (ver PlayerIdentity.cs).
    // Si el trompo todavia no lo tiene, queda el valor serializado de siempre.
    private void Start()
    {
        playerIndex = PlayerIdentity.Resolve(this, playerIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (rpgTurn.isTurnActive && ((JoystickPorJugador.De(playerIndex) != null && JoystickPorJugador.De(playerIndex).buttonWest.isPressed)
            || (JoystickPorJugador.UsaTeclado(playerIndex) && Keyboard.current.cKey.isPressed && JoystickPorJugador.UsaTeclado(playerIndex)))) // >>> TECLADO <<<
        {
            menuToExpand.SetActive(true);
        }
        else
        {
            menuToExpand.SetActive(false);
        }
    }
}
