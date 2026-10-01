# La CPU con cualquier personaje (Ninja, Magus, Caballero)

> Esto lo armé con IA (Claude). **No se ejecutó el juego**: el script compila pero
> hay que correrlo y probarlo en Unity.

## Qué pasaba

Contra la CPU (escena `Practica`) solo andaba el Ninja. El código de la IA **no era
el problema**: el `AIBrain` descubre solo los poderes de cualquier personaje
(interfaz `IAIAction`) y los 12 poderes de los tres personajes ya la implementan.

El problema era la escena:

- **`CPU > Magus` era una copia del Ninja**: traía los botones del Ninja (Pinchos,
  Shuriken, Clon) y solo un poder de Magus mezclado. Peor: tenía las referencias
  **cruzadas** con el Magus del Jugador 1 (su "Esperar" movía el trompo del
  Jugador 1) y con el Ninja de la CPU.
- **`CPU` no tenía Caballero**: elegirlo dejaba la arena sin trompo.

## Cómo se arregla

Menú de Unity, con la escena `Practica` abierta:

1. `Trompos IA > CPU > Validar personajes` — muestra qué poderes tiene cada
   personaje de la CPU y avisa de referencias cruzadas. Corrélo antes para ver el
   estado actual y después para confirmar.
2. `Trompos IA > CPU > Construir Magus y Caballero` — duplica
   `Jugador1 > Magus` y `Jugador1 > Knight` con la duplicación de Unity (las
   referencias internas quedan bien) y los convierte en CPU:
   - el trompo `0` pasa a llamarse `1` (varios poderes ordenan los `Player` por nombre),
   - capa de colisión del Jugador 1 → capa de la CPU,
   - misma posición inicial y mismo estado de la UI que el Ninja de la CPU,
   - un `AIBrain` con los valores por defecto (igual que el del Ninja),
   - se anota en `EndGame.Players` y en `MusicManager`.
   El Ninja de la CPU no se toca.
3. `Ctrl+S`.

Se puede deshacer con `Ctrl+Z` o con git.

## Dificultad

Es la misma lógica para los tres personajes (`AIBrain.AplicarDificultad`):

| | Fácil | Normal | Difícil |
|---|---|---|---|
| Poderes que usa | solo los básicos | todos | todos |
| Probabilidad de usar uno al atacar | 15 % | 50 % | 90 % |

Poderes por personaje (costo, básico o fuerte):

| Ninja | Magus | Caballero |
|---|---|---|
| Shuriken (2) básico | Disparo mágico (2) básico | Estocada (1) básico |
| Pinchos (2) fuerte | Orbe (4) fuerte | Espada (1) básico |
| Clon (2) fuerte | Torreta (4) fuerte | Onda expansiva (1) básico |
| Sustitución (2) fuerte | Orbes protectores (4) fuerte | Gravedad (2) fuerte |
| Cierra (4) fuerte | | |

> **Ojo con el Caballero:** 3 de sus 4 poderes son básicos y baratos, así que en
> Fácil usa casi todo su repertorio, a diferencia del Ninja o del Magus. Si en las
> pruebas se siente más difícil en Fácil, es por eso (se resuelve cambiando
> `esEspecialFuerte` de alguno).

## Qué sigue afuera de la IA

`ActiveBurn`, `ActiveFreeze`, `ActiveParalysis` y `ActivePoison` no implementan
`IAIAction` a propósito: gastan energía sin cerrar el turno y la IA podría
quedarse colgada (es el mismo bug del Magus congelado). Quedó como estaba.

## Para ver que la IA agarró bien sus poderes

Al arrancar, el `AIBrain` escribe en la Console una línea con los poderes que
descubrió. Un Magus que liste `LaunchPinchos` o `LaunchShuriken` sigue siendo un
Ninja disfrazado.
