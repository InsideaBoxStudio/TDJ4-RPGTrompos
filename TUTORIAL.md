# Tutorial del juego

Es una pelea **coreografiada** contra la computadora, en una **escena aparte** (`Assets/Scenes/Tutorial.unity`). Está pensada como una escena independiente: no hay ningún botón ni enganche en otras escenas, y no se tocó nada de lo que ya existía. Cómo se llega a ella (menú, botón, etc.) queda para decidir después.

## Cómo probarlo

Abrir `Assets/Scenes/Tutorial.unity` y darle Play. Para sumarla a un build hay que agregarla a los Build Settings (a propósito no se hizo).

## Cómo funciona la coreografía

El juego se pausa cada vez que el jugador tiene que hacer algo, y se le enseña **una mecánica a la vez**:

1. **Lanzamiento:** el cordón entra solo en el lanzador y todo queda quieto hasta que el jugador aprieta el botón.
2. **Choque y primer turno:** los trompos chocan sin cartel. Como siguen rebotando y cada golpe frena el turno, el del jugador casi nunca llega solo. Por eso, 0,7 s después del primer golpe entre los trompos (para que se vea el choque) el director **abre el turno directamente**: llama a `CheckPlayerTurn.Ready()` (lo que hace el juego al empezar un turno: suma la energía y muestra el menú de golpes) y congela el juego. Recién ahí aparece el cartel con el «ENTER: saltar / R3: saltar». Si no hubiera golpe, lo abre igual a los 2,6 s.
3. **Cada turno siguiente:** el cartel de la mecánica y la espera a que el jugador la haga de verdad (y, si corresponde, que pegue). Si el turno tarda, se pide igual.
4. **Contraataque:** el director también le da el turno a la CPU, que hace su ataque básico con los métodos del juego, para que el jugador vea cómo se recibe un golpe.

En la Console quedan líneas que empiezan con «Tutorial:» (paso actual, cuándo empieza y termina cada turno, golpes recibidos). Sirven para diagnosticar si algo no avanza.

Cada cartel explica **por qué** existe la mecánica, no solo qué tecla apretar, y un marco amarillo resalta la parte de la pantalla de la que se habla.

| Acción | Teclado | Joystick |
|---|---|---|
| Pasar un cartel / saltar el paso actual | ENTER | R3 (apretar el stick derecho) |

Los carteles nombran los botones del joystick conectado (PlayStation: X, círculo, cuadrado, triángulo, L1/R1/L2; Xbox: A, B, X, Y, LB/RB/LT) y usan teclas solo si no hay joystick. En la Console queda una línea «Tutorial: control detectado» por si los nombres no coinciden con el control que se tiene en la mano.

## Qué enseña (en este orden)

Son 8 pasos, siguiendo el hilo de una pelea real:

1. Bienvenida y objetivo.
2. Lanzamiento (el cordón entra al lanzador).
3. Choque y menú de golpes: aparece al llegar el turno.
4. La barra de energía.
5. Un golpe (ataque básico, con impacto).
6. Cómo impacta: la vida del rival.
7. El rival ataca: se ve cómo baja la vida del jugador.
8. Devolver el golpe y ganar.

Quedaron afuera (para sumar después si hace falta): esperar, moverse, shuriken, pinchos, clon, sierra, veneno, sustitución y parry.

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
