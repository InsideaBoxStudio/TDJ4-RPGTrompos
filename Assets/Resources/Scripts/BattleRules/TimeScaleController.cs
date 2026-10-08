using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    public static TimeScaleController Instance;

    public int currentTurn = 0;

    private int ticketTurn = 0;

    private bool[] turns = new bool[1];

    private void Awake()
    {
        Instance = this;

        // El primer turno es el 0
        turns[0] = true;
    }

    public int GetMyNumberTurn()
    {
        int turnID = ticketTurn;

        ticketTurn++;

        // Asegurarse de que exista ese índice
        ExpandTurnsIfNeeded(turnID);

        return turnID;
    }

    public bool IsMyTurn(int turnID)
    {
        // Si el ID todavía no existe, no es su turno
        if (turnID < 0 || turnID >= turns.Length)
            return false;

        if (turns[turnID])
        {
            Time.timeScale = 0f;
        }

        return turns[turnID];
    }

    public void EndTurn(int turnID)
    {
        // Seguridad
        if (turnID < 0 || turnID >= turns.Length)
            return;

        // Evitar que un script que no tiene el turno lo termine
        if (!turns[turnID])
            return;

        turns[turnID] = false;

        // Siguiente turno
        currentTurn++;

        // Crear espacio para el siguiente turno
        ExpandTurnsIfNeeded(currentTurn);

        turns[currentTurn] = true;

        Time.timeScale = 1f;
    }

    private void ExpandTurnsIfNeeded(int index)
    {
        if (index < turns.Length)
            return;

        bool[] newTurns = new bool[index + 1];

        // Copiar los turnos anteriores
        for (int i = 0; i < turns.Length; i++)
        {
            newTurns[i] = turns[i];
        }

        turns = newTurns;
    }
}