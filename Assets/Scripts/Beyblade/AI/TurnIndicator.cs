using UnityEngine;

// ============================================================================
//  INDICADOR VISUAL DE TURNO
//  ----------------------------------------------------------------------------
//  Pinta el trompo de un color brillante cuando es SU turno (isTurnActive),
//  para que el jugador sepa exactamente CUÁNDO puede elegir una acción.
//
//  Se pega al mismo objeto que tiene CheckPlayerTurn (ej: el Ninja del Jugador 1).
//  Es auto-contenido: encuentra el CheckPlayerTurn y el SpriteRenderer solos.
// ============================================================================
public class TurnIndicator : MonoBehaviour
{
    [SerializeField] private CheckPlayerTurn check;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Color turnColor = Color.green; // color cuando es tu turno

    private Color originalColor;
    private bool wasActive = false;

    void Start()
    {
        if (check == null) check = GetComponentInChildren<CheckPlayerTurn>(true);
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>(true);

        if (sprite != null) originalColor = sprite.color;
    }

    void Update()
    {
        if (check == null || sprite == null) return;

        // Cambiar el color solo cuando cambia el estado del turno (eficiente).
        if (check.isTurnActive && !wasActive)
        {
            wasActive = true;
            sprite.color = turnColor; // ¡es tu turno!
        }
        else if (!check.isTurnActive && wasActive)
        {
            wasActive = false;
            sprite.color = originalColor; // turno terminado
        }
    }
}
