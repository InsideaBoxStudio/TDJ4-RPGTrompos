using UnityEngine;
using UnityEngine.InputSystem;

public class ExpandMenu2 : MonoBehaviour
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
        if (rpgTurn.isTurnActive && ((JoystickPorJugador.De(playerIndex) != null && JoystickPorJugador.De(playerIndex).buttonEast.isPressed)
            || (JoystickPorJugador.UsaTeclado(playerIndex) && Keyboard.current.vKey.isPressed))) // >>> TECLADO <<<
        {
            menuToExpand.SetActive(true);
        }
        else
        {
            menuToExpand.SetActive(false);
        }
    }
}
