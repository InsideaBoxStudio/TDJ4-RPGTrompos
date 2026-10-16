# Tutorial del juego

Es una pelea **coreografiada** contra la computadora, en una **escena aparte** (`Assets/Scenes/Tutorial.unity`). Está pensada como una escena independiente: no hay ningún botón ni enganche en otras escenas, y no se tocó nada de lo que ya existía. Cómo se llega a ella (menú, botón, etc.) queda para decidir después.

## Cómo probarlo

Abrir `Assets/Scenes/Tutorial.unity` y darle Play. Para sumarla a un build hay que agregarla a los Build Settings (a propósito no se hizo).

## Cómo funciona la coreografía

El juego se pausa cada vez que el jugador tiene que hacer algo, y se le enseña **una mecánica a la vez**:

1. **Lanzamiento:** el cordón entra solo en el lanzador y todo queda quieto hasta que el jugador aprieta el botón.
2. **Cada turno:** el juego ya se congela solo cuando llega el turno. Ahí aparece el cartel de la mecánica y se espera a que el jugador la haga de verdad (y, si corresponde, que pegue) antes de pasar a la siguiente.

Cada cartel explica **por qué** existe la mecánica, no solo qué tecla apretar, y un marco amarillo resalta la parte de la pantalla de la que se habla.

| Acción | Teclado | Joystick |
|---|---|---|
| Continuar un cartel | ENTER o ESPACIO | START |
| Saltar el paso actual | TAB | R3 |

## Qué enseña (en este orden)

Lanzamiento, turnos, energía, vida, apuntar, menú de ataques, ataque básico, moverse, esperar, shuriken, pinchos, clon, veneno (y cómo contagiarlo), sustitución, sierra, parry (solo se explica) y por último ganar la pelea.

## Qué hace el tutorial con la partida

La escena es una copia de `Practica` con un componente nuevo (`TutorialDirector`, pegado al objeto `Configs`). Todo se hace desde afuera, leyendo datos que el juego ya expone:

- Deja solo **Ninja contra Ninja** (el Ninja es el que tiene más ataques distintos).
- La CPU se queda quieta: se apaga su `AIBrain` y en su turno solo espera.
- Se le devuelve a la CPU su vida entera: en esta escena el juego la deja con el 10% al frenar su barra de lanzamiento.
- El rival no baja de cierta vida hasta el último paso; ahí se le deja poca vida para que se pueda ganar. El jugador tampoco puede morir.
- Se regala la energía que pide cada paso, porque sin eso no se llegaría a los ataques caros.
- El reloj de 10 segundos del turno y la cuenta regresiva se congelan mientras se lee.

Al terminar, `EndGame` muestra su pantalla de siempre y vuelve a `InitMenu`.

## Archivos

| Archivo | Para qué |
|---|---|
| `Assets/Scenes/Tutorial.unity` | La escena: copia de `Practica` + el director en `Configs`. |
| `Scripts/Tutorial/TutorialDirector` | La coreografía: los pasos y la lógica para saber si el jugador los cumplió. |
| `Scripts/Tutorial/TutorialPanel` | El cartel y el marco que resalta. |
| `Scripts/Tutorial/TutorialControles` | Los nombres de teclas y botones que se muestran (según haya joystick o no). |

Para quitar el tutorial: borrar la carpeta `Scripts/Tutorial` y la escena `Tutorial.unity`.

## Cómo agregar o cambiar un paso

Los pasos están en `TutorialDirector.ConstruirPasos()`: cada uno es un título, un texto y una condición. Para el ataque que se usa, `ataque` es el nombre con el que el juego lo anota en `EnergyCounter.ataquesUsados`.

## Qué falta probar

Se compiló sin errores, pero todavía no se jugó. Revisar que el cordón entre y se frene, que cada paso se complete, que el marco amarillo caiga sobre lo que se explica y que al final se pueda ganar. La posición del cordón "completamente adentro" es la constante `PosicionCordonAdentro` del director, por si hay que ajustarla.
