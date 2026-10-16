# Tutorial del juego

Es una pelea **coreografiada** contra la computadora, en una escena propia (`Tutorial`). La escena `Practica` y todo lo que ya existía quedan como estaban: el tutorial son archivos nuevos.

## Cómo se entra

- **Desde el juego:** en la pantalla de dificultad (`SeleccionDificultad`, la entrada a Práctica) aparece arriba a la derecha el botón **TUTORIAL: ¿Cómo se juega?**. Se activa con el mouse, la tecla **T** o el botón superior del joystick (triángulo / Y).
- **Para probarlo:** abrir la escena `Assets/Scenes/Tutorial.unity` y darle Play.

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

La pelea está armada para que el jugador gane. Todo se hace desde afuera, leyendo datos que el juego ya expone:

- Deja solo **Ninja contra Ninja** (el Ninja es el que tiene más ataques distintos).
- La CPU se queda quieta: se apaga su `AIBrain` y en su turno solo espera.
- Se le devuelve a la CPU su vida entera: en esta escena el juego la deja con el 10% al frenar su barra de lanzamiento.
- El rival no baja de cierta vida hasta el último paso; ahí se le deja poca vida para que se pueda ganar. El jugador tampoco puede morir.
- Se regala la energía que pide cada paso, porque sin eso no se llegaría a los ataques caros.
- El reloj de 10 segundos del turno y la cuenta regresiva se congelan mientras se lee.

Al terminar, `EndGame` muestra su pantalla de siempre y vuelve al menú. Al salir, la dificultad y los personajes que el jugador tenía elegidos se restauran.

## Archivos

| Archivo | Para qué |
|---|---|
| `Assets/Scenes/Tutorial.unity` | La escena: una copia de `Practica`. |
| `Assets/Editor/TutorialEnBuild.cs` | Agrega la escena a los Build Settings al abrir el proyecto (es lo único que se toca fuera de la carpeta del tutorial). |
| `Scripts/Tutorial/TutorialArranque` | Se engancha solo a la carga de escenas: agrega el botón y crea el director. |
| `Scripts/Tutorial/TutorialBoton` | El botón de la entrada a Práctica. |
| `Scripts/Tutorial/TutorialModo` | Guarda si el tutorial está activo y restaura lo elegido al salir. |
| `Scripts/Tutorial/TutorialDirector` | La coreografía: los pasos y la lógica para saber si el jugador los cumplió. |
| `Scripts/Tutorial/TutorialPanel` | El cartel y el marco que resalta. |
| `Scripts/Tutorial/TutorialControles` | Los nombres de teclas y botones que se muestran (según haya joystick o no). |

Para quitar el tutorial: borrar la carpeta `Scripts/Tutorial`, `TutorialEnBuild.cs`, la escena `Tutorial.unity` y sacarla de los Build Settings.

## Cómo agregar o cambiar un paso

Los pasos están en `TutorialDirector.ConstruirPasos()`: cada uno es un título, un texto y una condición. Para el ataque que se usa, `ataque` es el nombre con el que el juego lo anota en `EnergyCounter.ataquesUsados`.

## Qué falta probar

Se compiló sin errores, pero todavía no se jugó. Para probarlo: abrir `Tutorial` y darle Play. Revisar que el cordón entre y se frene, que cada paso se complete, que el marco amarillo caiga sobre lo que se explica y que al final se pueda ganar. La posición del cordón "completamente adentro" es la constante `PosicionCordonAdentro` del director, por si hay que ajustarla.
