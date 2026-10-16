# Tutorial del juego

Es una pelea guiada contra la computadora, dentro de la escena `Practica`. No se modificó ningún script ni escena que ya existía: todo está en archivos nuevos de `Assets/Resources/Scripts/Tutorial/`.

## Cómo se entra

En la pantalla de dificultad (`SeleccionDificultad`, la entrada a Práctica) aparece arriba a la derecha el botón **TUTORIAL: ¿Cómo se juega?**. Se activa con el mouse, la tecla **T** o el botón superior del joystick (triángulo / Y).

## Cómo se maneja

| Acción | Teclado | Joystick |
|---|---|---|
| Continuar un cartel | ENTER o ESPACIO | START |
| Saltar el paso actual | TAB | R3 |

Los carteles piden una acción y esperan que el jugador la haga de verdad antes de pasar al siguiente. Cada uno explica **por qué** existe esa mecánica, no solo qué tecla apretar.

## Qué enseña (en este orden)

Lanzamiento, turnos, energía, vida, apuntar, menú de ataques, ataque básico, moverse, esperar, shuriken, pinchos, clon, veneno (y cómo contagiarlo), sustitución, sierra, parry (solo se explica) y por último ganar la pelea.

## Qué hace el tutorial con la partida

La pelea está armada para que el jugador gane. Todo se hace desde afuera, leyendo datos que el juego ya expone:

- Deja solo **Ninja contra Ninja** (el Ninja es el que tiene más ataques distintos).
- La CPU se queda quieta: se apaga su `AIBrain` y en su turno solo espera.
- El rival no baja de cierta vida hasta el último paso; ahí se le deja poca vida para que se pueda ganar. El jugador tampoco puede morir.
- Se regala la energía que pide cada paso, porque sin eso no se llegaría a los ataques caros.
- El reloj de 10 segundos del turno y la cuenta regresiva del lanzamiento se congelan mientras se lee.

Al terminar, `EndGame` muestra su pantalla de siempre y vuelve al menú. Al salir, la dificultad y los personajes que el jugador tenía elegidos se restauran.

## Archivos

| Archivo | Para qué |
|---|---|
| `TutorialArranque` | Se engancha solo a la carga de escenas: agrega el botón y crea el director. |
| `TutorialBoton` | El botón de la entrada a Práctica. |
| `TutorialModo` | Guarda si el tutorial está activo y restaura lo elegido al salir. |
| `TutorialDirector` | Los pasos y la lógica para saber si el jugador los cumplió. |
| `TutorialPanel` | El cartel y el marco que resalta la parte de la pantalla de la que se habla. |
| `TutorialControles` | Los nombres de teclas y botones que se muestran (según haya joystick o no). |

Para quitar el tutorial alcanza con borrar la carpeta `Tutorial` (y su `.meta`).

## Cómo agregar o cambiar un paso

Los pasos están en `TutorialDirector.ConstruirPasos()`: cada uno es un título, un texto y una condición. Para el ataque que se usa, `ataque` es el nombre con el que el juego lo anota en `EnergyCounter.ataquesUsados`.

## Qué falta probar

Se compiló sin errores, pero todavía no se jugó. Para probarlo: abrir `SeleccionDificultad`, apretar T y revisar que cada paso se complete, que el marco amarillo caiga sobre lo que se explica y que al final se pueda ganar.
