# Tutorial del juego

Es una pelea **coreografiada** contra la computadora, en una **escena aparte** (`Assets/Scenes/Tutorial.unity`, copia de `J1vsCPU`). Está pensada como una escena independiente: no hay ningún botón ni enganche en otras escenas, y no se tocó nada de lo que ya existía. Cómo se llega a ella (menú, botón, etc.) queda para decidir después.

El jugador usa el trompo **Base** del juego, armado por partes: el `DefaultPlayerSettings` de la escena (activo, así la escena funciona también abriéndola directo) le da ataque básico, shuriken, pinchos y sierra, y veneno, clon y sustitución como especiales. El rival se **sortea** entre los personajes de la CPU que haya en la escena (Ninja, Magus, Knight).

## Cómo probarlo

Abrir `Assets/Scenes/Tutorial.unity` y darle Play. Para sumarla a un build hay que agregarla a los Build Settings (a propósito no se hizo).

## Cómo funciona la coreografía

El juego se pausa cada vez que el jugador tiene que hacer algo, y se le enseña **una mecánica a la vez**:

1. **Lanzamiento:** el cordón entra y sale del lanzador (como en el juego, pero más lento y sin cuenta regresiva) y todo queda quieto hasta que el jugador aprieta el botón **con el cordón adentro** (umbral `CordonAdentro`); si lo aprieta afuera, se le avisa y sigue. La vida inicial sale de la posición real de la barra.
2. **Choque y primer turno:** los trompos chocan sin cartel. Como siguen rebotando y cada golpe frena el turno, el del jugador casi nunca llega solo. Por eso, 0,7 s después del primer golpe entre los trompos (para que se vea el choque) el director **le abre el turno** por la misma puerta que usa la CPU (`RPGTurn.NotifyReadyImmediate`): el juego suma la energía, muestra el menú de golpes y se congela. Recién ahí aparece el cartel con el «ENTER: saltar / R3: saltar». Si no hubiera golpe, lo abre igual a los 2,6 s.

   Para que ese turno sobreviva hubo que reparar el **reloj de turno** (`CountDownRPG`): en esta escena nace en 0 porque el director prende al Ninja desde su `Awake`, y el `Awake` del trompo reinicia el reloj antes de que el reloj haya guardado su duración. Con el reloj en 0 el juego cierra cualquier turno en el mismo cuadro en que empieza. El director lee la duración antes de prender al Ninja y la repone al arrancar.
3. **Cada turno siguiente:** el cartel de la mecánica y la espera a que el jugador la haga de verdad (y, si corresponde, que pegue). Si el turno tarda, se pide igual.
4. **Contraataque:** el director también le da el turno a la CPU, que hace su ataque básico con los métodos del juego, para que el jugador vea cómo se recibe un golpe. Fuera de ese paso la CPU no tiene turno.

Ojo al probar: las **paredes del estadio hacen 2 de daño** (`StadiumCollider`). Después de embestir, el trompo del jugador suele rebotar contra la pared y perder vida varias veces seguidas: no es la CPU atacando.

En la Console quedan líneas que empiezan con «Tutorial:» (paso actual, cuándo empieza y termina cada turno, golpes recibidos). Sirven para diagnosticar si algo no avanza.

Cada cartel explica **por qué** existe la mecánica, no solo qué tecla apretar, y un marco amarillo resalta la parte de la pantalla de la que se habla.

| Acción | Teclado | Joystick |
|---|---|---|
| Pasar un cartel / saltar el paso actual | ENTER | R3 (apretar el stick derecho) |
| Abrir el menú de golpes / de especiales | C / V | cuadrado / círculo |
| Elegir un golpe con el menú abierto | flechas | cruceta |
| Soltar el lanzador, esperar, acertar el timing del veneno | Q | X (cruz) |

Los carteles nombran los botones del joystick conectado (PlayStation: X, círculo, cuadrado, triángulo, L1/R1/L2; Xbox: A, B, X, Y, LB/RB/LT) y usan teclas solo si no hay joystick. En la Console queda una línea «Tutorial: control detectado» por si los nombres no coinciden con el control que se tiene en la mano.

## Qué enseña (en este orden)

Son 11 pasos, siguiendo el hilo de una pelea real:

1. Bienvenida y objetivo.
2. Lanzamiento (el cordón entra al lanzador).
3. Choque y menú de golpes: aparece al llegar el turno; al apretar el botón del menú (cuadrado / X / C) el director **deja el menú abierto** y lo resalta (en el juego solo se ve mientras se mantiene). Ningún otro botón responde.
4. El primer golpe: el ataque básico, con impacto (único golpe habilitado); el menú se cierra con el golpe, como en el juego.
5. La barra de energía: mientras se lee, la barra baja y sube sola en ciclo (llena → gasta 4 → recupera 2) para que se vea cómo funciona; al salir se deja llena.
6. Cómo impacta: la vida del rival.
7. El menú de especiales: el otro botón (círculo / B / V); también queda abierto.
8. El veneno (L1 / LB / R): abre el minijuego de timing del juego; el cartel resalta el indicador y dice **¡AHORA!** cuando está en la zona. Si sale bien queda cargado, si sale mal el jugador se envenena solo (el tutorial lo dice y sigue).
9. Contagiarlo: un golpe (básico o shuriken) pasa el veneno al rival, que pierde vida cada segundo. Si el veneno se perdió en el paso anterior, alcanza con pegarle.
10. El rival ataca: le tira un shuriken al jugador y se ve cómo baja su vida. Hasta acá la CPU **no toma turnos**: el director le mantiene apagada la bandera de turno, así el trompo rival no actúa ni congela el juego mientras se practica.
11. La pelea de verdad: se prende el `AIBrain` de la CPU, se sacan los pisos de vida y el reloj de turno vuelve a correr; todas las acciones quedan habilitadas. El rival arranca con poca vida para que el cierre sea corto. El cartel recuerda cómo abrir los menús; si el jugador se queda 6 s en su turno sin hacer nada, el cartel vuelve con una ayuda. Si los trompos quedan rebotando y nadie consigue turno en 4 s, el director le abre uno al jugador. Al caer el rival aparece «¡Ganaste!» (sin pedir botón) y se vuelve al menú con X / R3.

**Carteles:** ninguno se va solo. Los de lectura avanzan con ENTER / R3; los de acción quedan en «¡Bien hecho!» hasta ENTER / R3, salvo el lanzamiento (arranca al soltar) y los dos de abrir menú (siguen directo al golpe). El del último paso se cierra con ENTER / R3 y la pelea sigue; al caer el rival aparece «¡Ganaste!» y se vuelve al menú con X / R3 (o ENTER / ESPACIO).

**Botones por paso:** cada paso dice qué scripts del jugador responden (`permitidos`), menús incluidos; el director apaga los demás con `enabled = false` (sin tocar la escena). En el paso del menú de golpes solo abre cuadrado; en el de especiales, solo círculo.

Quedaron afuera (para sumar después si hace falta): esperar, moverse, pinchos, clon, sierra, sustitución y parry.

## Qué hace el tutorial con la partida

La escena es una copia de `J1vsCPU` con un componente nuevo (`TutorialDirector`, pegado al objeto `Configs`) y el `PlayerSettings` de la escena activo. Todo se hace desde afuera, leyendo datos que el juego ya expone:

- Deja **un trompo por lado**: el Base del jugador y un rival al azar (`ActiveCharacter` solo prende, nunca apaga; el director apaga el resto y deja los nombres en `CharacterData`).
- Apaga lo que distrae en una clase: el spawner de objetos del estadio y la caída fuera del estadio (vuelve en la pelea final).
- La CPU se queda quieta: se apaga su `AIBrain` y en su turno solo espera. Cuando le toca atacar usa el golpe a distancia de su personaje (shuriken, disparo mágico o estocada).
- Los golpes del jugador son prefabs que el menú instancia al abrirse por primera vez: el director rehace su lista de permisos cuando a los menús les aparecen hijos.
- Se le devuelve a la CPU su vida entera: en esta escena el juego la deja con el 10% al frenar su barra de lanzamiento.
- El rival no baja de cierta vida hasta el último paso; ahí se le deja poca vida para que se pueda ganar. El jugador tampoco puede morir.
- Se regala la energía que pide cada paso, porque sin eso no se llegaría a los ataques caros.
- El reloj de 10 segundos del turno y la cuenta regresiva se congelan mientras se lee.

Al terminar, `EndGame` muestra su pantalla de siempre y vuelve a `InitMenu`.

## Archivos

| Archivo | Para qué |
|---|---|
| `Assets/Scenes/Tutorial.unity` | La escena: copia de `J1vsCPU` + el director en `Configs` + `PlayerSettings` activo. |
| `Scripts/Tutorial/TutorialDirector` | La coreografía: los pasos y la lógica para saber si el jugador los cumplió. |
| `Scripts/Tutorial/TutorialPanel` | El cartel y el marco que resalta. |
| `Scripts/Tutorial/TutorialControles` | Los nombres de teclas y botones que se muestran (según haya joystick o no). |

Para quitar el tutorial: borrar la carpeta `Scripts/Tutorial` y la escena `Tutorial.unity`.

## Nota sobre la vuelta al menú

Al terminar cualquier pelea, `EndGame` deja `Time.timeScale = 0` y así se carga `InitMenu`: los trompos del fondo del menú quedan quietos en el centro, y `ChangeMap` vuelve a poner el tiempo en 1 al elegir un modo. El tutorial hace exactamente lo mismo (también al salir con R3). Si el menú llegara con el tiempo corriendo, esos trompos (físicos: `GravityToPoint` + `BeybladeCollider` con `reboundForceBeybladeStay = -10`) se disparan entre sí a los pocos segundos y dejan estelas largas; pasa igual dando Play directo en `InitMenu` y esperando. Es un tema del menú, no del tutorial.

## Cómo agregar o cambiar un paso

Los pasos están en `TutorialDirector.ConstruirPasos()`: cada uno es un título, un texto y una condición. Para el ataque que se usa, `ataque` es el nombre con el que el juego lo anota en `EnergyCounter.ataquesUsados`.

## Qué falta probar

Revisar que cada paso se complete, que el marco amarillo caiga sobre lo que se explica y que al final se pueda ganar. La velocidad del cordón y desde dónde cuenta como "adentro" son las constantes `VelocidadCordon` y `CordonAdentro` del director, por si hay que ajustarlas. Pendiente: confirmar cómo se ve `InitMenu` al volver (comparar con un Play directo en esa escena).
