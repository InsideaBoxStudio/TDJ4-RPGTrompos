using UnityEngine;
using UnityEngine.InputSystem;

public class ExpandMenu2 : MonoBehaviour
{
    [SerializeField] private CheckPlayerTurn rpgTurn;
    [SerializeField] private GameObject menuToExpand;
    [SerializeField] private int playerIndex;

    // Update is called once per frame
    void Update()
    {
        if (rpgTurn.isTurnActive && Gamepad.all.Count > playerIndex && Gamepad.all[playerIndex].buttonEast.isPressed)
        {
            menuToExpand.SetActive(true);
        }
        else
        {
            menuToExpand.SetActive(false);
        }
    }
}
