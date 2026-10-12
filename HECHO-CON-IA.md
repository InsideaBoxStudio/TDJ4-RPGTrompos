# Qué se hizo con IA en este proyecto

Parte del trabajo de este repositorio lo hizo **David Boxler con la ayuda de un asistente
de IA: Claude (de Anthropic)**, usado desde Claude Code. Este archivo existe para que
cualquiera que lea el repo sepa **qué es de la IA, cómo reconocerlo y qué no se probó**.

## Cómo reconocer los cambios hechos con IA

Todos esos commits llevan esta firma al final del mensaje (con la versión del modelo que
se usó, por ejemplo `Claude Opus 4.8`, `Claude Opus 5.5` o `Claude Sonnet 5.5`):

```
Co-Authored-By: Claude <versión> <noreply@anthropic.com>
```

Para verlos todos:

```bash
git log --grep="Co-Authored-By: Claude"
```

Los commits de Nehemías (GigaPikachu / NehemiasLugo) **no llevan esa firma**: su trabajo
(selector de piezas, nuevas opciones de ataque, escenas) es suyo.

## Qué se hizo con IA

| Tema | Ramas | Resumen |
|---|---|---|
| IA del rival (CPU) | `feature/ia-unificada`, `feature/combate-ia`, `feature/ia-poderes` (y la IA original, ya en `main`) | La IA de combate con máquina de estados del modo Práctica, sus tres dificultades y la pantalla para elegirlas. Una sola clase y una interfaz (`IAIAction`) para los poderes de cualquier personaje, así la CPU descubre sola lo que puede usar. Arreglos de combate. Magus y Caballero de la CPU. |
| Identidad e input | `feature/player-identity` | `PlayerIdentity` y la clase `Controles`, para no repetir el índice del jugador en 26 scripts y soportar teclado y joystick desde un solo lugar. |
| Menús con teclado | `feature/player-identity` (ya en `main`) | Los menús se pueden usar sin joystick y se corrigieron dos cierres del juego que ocurrían sin joystick. |
| Pantalla | `fix/resolucion-aspecto` | Arregla la pantalla partida en resoluciones que no son 16:9 (`AspectoFijo`). |
| Escena 1VS1 | `fix/1vs1` | Quita ~688 errores por referencias sin asignar. |
| Selección de personaje | `feature/seleccion-simultanea` | Cada jugador elige con su propio cursor; el joystick de cada lado se asigna con L1 / R1; vibración al conectar. |
| Beta (sobre la rama de Nehemías) | `beta/*` | Ver la tabla de abajo. |

### Ramas de la beta

Todas salen de `beta/base`, que es **idéntica a `Parts_Selector`** de Nehemías (no se mezcló
nada). Cada arreglo está en su propia rama, y `beta/pruebas` los junta para probarlos
todos de una.

| Rama | Qué hace |
|---|---|
| `beta/fix-pantalla-completa-web` | `GraphicsManager` ya no llama a `SetResolution` en cada frame; en la web la pantalla completa espera un clic o una tecla (el navegador no acepta el botón de un joystick). |
| `beta/fix-consejos-lr` | Los botones L y R de los consejos funcionan con cualquier joystick, con Q / E y con los botones de pantalla. |
| `beta/fix-salir-config-b` | Con B se sale del menú de Configuración. |
| `beta/fix-color-sin-energia` | Las opciones de ataque que no se pueden pagar con la energía actual se ven apagadas. |
| `beta/fix-objeto-curar` | El objeto de curar no pasa de la vida máxima ni dispara los efectos de golpe. |
| `beta/doc-ia` | Este archivo. |
| `beta/pruebas` | `beta/base` + todos los arreglos anteriores, para probar juntos. |

## Qué NO se probó

> **El asistente no ejecuta el juego.** Todo lo de arriba se verificó leyendo el código y
> las escenas, y compilando los scripts (0 errores, sin advertencias nuevas). **Nada de la
> beta se probó dentro de Unity ni en un navegador todavía.**
>
> Antes de darlo por bueno hay que jugarlo. Cada commit dice en su mensaje qué quedó sin
> probar.

Cosas concretas a mirar al probar:

- **Pantalla completa en la web:** que el casillero funcione con mouse, teclado y joystick.
- **Salir de Configuración con B:** el componente `SalirConfigConB` se agregó al objeto
  `Opciones` de `ConfigMenu` **editando el archivo de la escena a mano** (con Unity
  cerrado). Hay que abrir la escena y confirmar que el componente está.
- **Ataques apagados por falta de energía:** que se apaguen y vuelvan a la normalidad.

## Lo que sigue pendiente

- Que la IA use los ataques nuevos (`*Option`): hoy solo 8 de los 21 implementan `IAIAction`.
- Jugar solo con teclado, sin gamepad.
- Generador de objetos del escenario, escenarios nuevos y tutorial interactivo.
- Ilustraciones animadas de los controles (es trabajo de arte, no de código).

## Cosas que la IA no hace

- **No decide el diseño del juego.** Propone y explica; las decisiones las toma el equipo.
- **No sube nada a `main`** sin que David lo pida.
- **Puede equivocarse.** En esta misma beta se corrigió a sí mismo dos veces (un objeto que
  dijo que dañaba y en realidad curaba, y una edición de una escena que tuvo que rehacer).
  Por eso se pide jugar antes de confiar.
