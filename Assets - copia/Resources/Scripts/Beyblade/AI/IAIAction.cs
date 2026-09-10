// ============================================================================
//  CONTRATO ÚNICO ENTRE LA IA Y LAS ACCIONES DE LOS TROMPOS
//  ----------------------------------------------------------------------------
//  Antes, el AIBrain tenía hardcodeados los 3 especiales del Ninja (shuriken,
//  pinchos, clon) como campos serializados. Consecuencia: la IA Magus y la IA
//  Caballero NO podían usar ninguno de sus especiales, y cada personaje nuevo
//  obligaba a editar el AIBrain.
//
//  Con esta interfaz el AIBrain ya no conoce personajes: en Awake pide
//  GetComponentsInChildren<IAIAction>() y usa lo que encuentre. Agregar un
//  personaje o un especial nuevo = implementar esta interfaz, cero cambios
//  en la IA.
//
//  IMPORTANTE: la implementa el MISMO script que ya usa el jugador. Por eso la
//  IA produce exactamente el mismo efecto y daño que cuando la acción la
//  dispara un humano (no hay dos caminos de código que mantener sincronizados).
// ============================================================================
public interface IAIAction
{
    // Cuánta energía cuesta. La IA lo usa para decidir y para depurar.
    int EnergyCost { get; }

    // ¿Es un especial "fuerte"? Solo la dificultad Difícil los usa siempre;
    // en Fácil quedan bloqueados para que la CPU no aplaste al jugador.
    bool IsStrong { get; }

    // ¿Se puede usar AHORA? Cada acción valida lo suyo (energía y lo que haga
    // falta: prefabs asignados, referencias resueltas, etc.).
    bool CanExecute();

    // Ejecuta la acción. Es el mismo método que dispara el input del jugador.
    void Execute();
}
