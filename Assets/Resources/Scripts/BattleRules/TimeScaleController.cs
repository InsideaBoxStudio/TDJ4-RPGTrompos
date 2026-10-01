using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    public static TimeScaleController Instance;

    public int currentTurn = 0;
    private int ticketTurn = 0;

    private bool[] turns = new bool[3];

    private void Awake()
    {
        Instance = this;

        // Empieza el script 1
        turns[currentTurn] = true;
        EndTurn(currentTurn);
    }

    public int GetMyNumberTurn()
    {
        ticketTurn ++;
        return ticketTurn;
    }

    public bool IsMyTurn(int turnID)
    {
        // Un hechizo recién lanzado puede sacar un número mayor que el tamaño de
        // la lista (que solo crece cuando termina un turno). Todavía no le toca:
        // antes esto tiraba IndexOutOfRange en cada frame hasta que la fila llegaba.
        if (turnID < 0 || turnID >= turns.Length) return false;

        if (turns[turnID])
        {
            Time.timeScale = 0f;
        }

        return turns[turnID];
    }

    public void EndTurn(int turnID)
    {
        Time.timeScale = 1f;

        Debug.Log("Turno terminado: " + turnID);

        // Evitar que un script que no tiene el turno lo termine (incluye el que
        // todavía ni entra en la lista, ver IsMyTurn)
        if (turnID < 0 || turnID >= turns.Length || !turns[turnID])
            return;

        turns[turnID] = false;

        // Siguiente turno
        currentTurn++;

        if (currentTurn >= turns.Length)
            turns = new bool[currentTurn + 1];

        turns[currentTurn] = true;
    }
}