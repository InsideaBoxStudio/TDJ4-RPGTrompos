# La CPU con el trompo Base y los ataques nuevos

## Qué cambia

En `J1vsCPU` el jugador usa el trompo **Base** (sus estadísticas y sus ataques salen de
`PlayerSettings`), pero los tres personajes de la CPU (Ninja, Magus, Caballero) seguían armados
con los scripts viejos de ataque. Por eso la CPU no usaba los ataques nuevos. Además, el Magus
de la CPU tomaba la energía y el punto de disparo del Ninja de la CPU.

Ahora cada personaje de la CPU es **una copia del trompo Base**, con un `PresetCPU` que le da
aspecto, estadísticas y ataques, y un `AIBrain` que decide qué hacer.

## Cómo se arma (menú de Unity, con la escena `J1vsCPU` abierta)

1. `Trompos IA > CPU > Reconstruir CPU con trompo Base`: reemplaza los tres cuerpos viejos por
   copias del Base. Se puede deshacer con `Ctrl+Z` o con git.
2. `Trompos IA > CPU > Validar CPU (trompo Base)`: revisa que quedó bien, sin darle Play.
3. `Ctrl+S`.

## Qué hace cada pieza

| Pieza | Para qué |
|---|---|
| `PresetCPU` | Escribe en `PlayerSettings`, en la posición de la CPU (1), el aspecto, las estadísticas y las listas de ataques del personaje. Corre antes que el resto porque `EnergyCounter` lee la energía en su `Awake`. |
| `AIBrain` | Decide qué hace la CPU en su turno. Con un trompo Base crea dos menús de ataques invisibles (Ataques y Especiales) para que existan las opciones, porque el humano las abre con un botón y la CPU no. |
| `AttacksMenuPreset` | Tiene la marca `controladoPorIA`: arma sus opciones pero no lee joystick ni teclado. |
| `IAIAction` | Todos los ataques (menos `ParalysisOption`, que está sin terminar) la implementan, así la CPU descubre sola lo que puede usar. |

## Ajustar un personaje (level design)

Todo está en el Inspector del `PresetCPU` de cada personaje, sin tocar código: vida, energía
máxima e inicial, peso, fricción, tiempo de espera máximo y las dos listas de ataques.

Valores de partida:

| | Vida | Energía (ini/máx) | Peso | Fricción | Espera |
|---|---|---|---|---|---|
| Ninja | 100 | 4 / 8 | 9 | 3 | 2,5 |
| Magus | 90 | 10 / 12 | 8 | 2 | 3 |
| Caballero | 120 | 4 / 8 | 10 | 3 | 2,5 |

Ninja y Magus usan las listas y estadísticas que ya traen las escenas. El Caballero no tenía
ninguna: sus valores son un punto de partida para probar y ajustar.

## Dificultad

Es la misma lógica para los tres (`AIBrain.AplicarDificultad`): en Fácil solo usa los ataques
básicos y poco; en Normal usa todos, a veces; en Difícil usa todos, casi siempre. Cada ataque
declara si es "fuerte" con el campo `esEspecialFuerte`.

## Qué falta probar

Nada de esto se probó todavía dentro de Unity. Al jugar contra cada personaje, mirar:

- Que aparezca un solo trompo por lado y que la CPU pelee (se mueve, ataca, espera).
- Que use sus propios ataques y que la línea `AIBrain` de la Console los liste.
- Que la interfaz de energía y velocidad de la CPU quede a la derecha.
- Que la vida y la energía de la CPU sean las del preset, y no las del jugador.
- Que las flechas del jugador no disparen ataques de la CPU.
