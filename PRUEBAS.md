# Pruebas — `main` después del merge

Hola Nehe 👋 Ya está todo mergeado en `main`: tu `arte_callejero` primero, y
arriba nuestra rama `feature/player-identity`. Esto es lo que hay que probar.

> [!IMPORTANT]
> **Estos cambios los hice con IA (Claude).** Lo aclaro para que los mires con ojo
> crítico y no des nada por sentado. Compila sin errores, pero **el juego no se
> ejecutó ni una vez** — la parte de "compila" está verificada, la parte de "anda"
> es justamente lo que te pido.
>
> Se tocaron 23 scripts de combate. Si algo se rompió, se va a notar acá.

> [!TIP]
> **El merge respetó tu reorganización.** Moviste los 113 scripts a
> `Assets/Resources/Scripts/`; nuestros cambios aterrizaron ahí y los 4 archivos
> nuevos también. Donde tu trabajo y el nuestro se pisaban, ganó el tuyo — el
> detalle está en el mensaje del commit de merge.

---

## Qué cambió y por qué

Son dos cosas, en dos commits separados.

### 1. La IA ya no conoce personajes

Antes el `AIBrain` tenía hardcodeados los 3 especiales del Ninja como campos
serializados. Por eso **la CPU con Magus o Caballero no podía usar ni uno solo de
sus especiales** — hacía embestida, moverse y esperar, nada más.

Ahora hay una interfaz `IAIAction` (`Assets/Scripts/Beyblade/AI/IAIAction.cs`) que
implementan los 16 especiales de los 3 personajes. El cerebro los descubre solos
con `GetComponentsInChildren<IAIAction>()`.

**Lo importante:** la interfaz la implementa *el mismo script que usa el jugador*,
no una copia. La IA dispara exactamente el mismo método que vos cuando apretás la
tecla, así que hace el mismo daño y el mismo efecto. No hay dos caminos de código
para mantener sincronizados.

Agregar un personaje nuevo ya no requiere tocar la IA.

### 2. Un solo `playerIndex` por trompo

Antes cada script tenía su propio `[SerializeField] int playerIndex`: **26 campos
a configurar a mano por cada copia del personaje.** Con 2 escenas × 2 jugadores ×
2 personajes son 8 armados manuales del mismo trompo.

Ahora hay un componente `PlayerIdentity` que va una vez en la raíz del trompo, y
los 23 scripts lo leen desde ahí.

Esto es lo que **destraba hacer un prefab del trompo** (que es lo que sigue): con
el índice horneado en cada componente, las instancias del jugador 1 y del jugador
2 no podían salir del mismo prefab.

La migración **no rompe nada**: si un trompo no tiene `PlayerIdentity`, cada script
usa el valor serializado de siempre. Las escenas actuales funcionan igual.

### De yapa

- Se fue el `reflection` del `AIBrain`. Antes recorría todos los `MonoBehaviour`
  buscando campos por nombre en string (`"playerIndex"`, `"jugador"`, `"playerID"`)
  y se rompía en silencio si alguien renombraba uno.
- **Bug latente arreglado:** 6 especiales resolvían `playerIndex` en su `Awake`,
  pero el `AIBrain` lo reescribe *también* en `Awake` y Unity no garantiza el orden.
  Apuntaban al `CheckPlayerTurn` equivocado. Pasaron a `Start`.
- `CheckPlayerTurn` ahora resuelve solo sus referencias de escena (`RPGTurn` y
  `CountDownRPG`). Un prefab no puede guardarlas, así que era el último bloqueante.
- `TryPerformSpecial()` ya no genera basura para el GC en cada ataque de la IA.
- `Cierra` y `TurnIndicator` se ordenaron (ver commits).

---

## Cómo probarlo

```bash
git fetch origin
git checkout feature/player-identity
```

Abrí el proyecto con **Unity 6000.3.14f1** (no la 6000.0.40f1). Si el Hub te ofrece
actualizar la versión, decile que **no**. Dejá que termine de compilar y mirá la
consola: no tiene que haber nada rojo.

### Teclas

| | Jugador 1 | Jugador 2 |
|---|---|---|
| Apuntar | `W` `A` `S` `D` | `I` `J` `K` `L` |
| Atacar | `W` | `I` |
| Avanzar | `E` | `O` |
| Esperar | `Q` | `U` |
| Especiales | `A` `S` `D` `R` `F` | `J` `K` `L` `H` `N` |
| Sustitución (nueva) | `T` | `P` |

**Menús** (los maneja cualquiera de los dos, con teclado o joystick):

| Acción | Tecla |
|---|---|
| Pausar / cerrar opciones | `Esc` |
| Confirmar | `Enter` o `Espacio` |
| Volver | `Backspace` o `Esc` |
| Navegar | Flechas |

### Prueba 1 — escena `1VS1` (la más importante)

Dos humanos, sin IA de por medio. Es la que aísla mejor si rompí algo.

- [ ] Cada jugador mueve **su** trompo (apretás `W` y responde el del J1, no el del J2)
- [ ] El turno pasa de uno al otro con normalidad
- [ ] Los especiales de cada personaje salen con las teclas de siempre
- [ ] Probalo con Ninja **y** con Magus

> Si los controles están cruzados o un jugador no responde, el problema está en el
> cambio de `PlayerIdentity`. Avisá y lo revertimos.

### Prueba 2 — escena `Practica` (contra la CPU)

- [ ] La CPU se mueve sola y ataca
- [ ] Tus teclas (J1) siguen andando mientras la CPU juega
- [ ] El turno alterna entre vos y la máquina, sin quedarse trabado
- [ ] Probá las 3 dificultades

### Prueba 3 — lo nuevo de verdad

Acá es donde se ve si el refactor sirvió de algo:

- [ ] **Elegí Magus para la CPU.** Antes solo embestía. Ahora tiene que tirar
      disparo mágico, orbe, torreta, orbes protectores.
- [ ] En **Difícil** la CPU tiene que usar los especiales fuertes bastante seguido;
      en **Fácil**, casi nunca.

### Prueba 4 — teclado sin joystick

**Desconectá todos los joysticks** y probá con teclado solo.

Antes, los 4 especiales del Caballero, 2 del Magus y la sustitución del Ninja
**solo leían joystick**: sin un control conectado esos personajes quedaban a medio
jugar. Ahora todo pasa por una sola clase (`Controles`) que une joystick y teclado.

- [ ] El Caballero tira sus 4 especiales con teclado (`A` `D` `R` `F` para el J1)
- [ ] La torreta y los orbes protectores del Magus salen con teclado
- [ ] La sustitución del Ninja sale con la tecla nueva (`T` para el J1, `P` para el J2)
- [ ] Con el joystick conectado **sigue andando igual que antes**

> [!WARNING]
> **Conflicto de botones que ya existía:** en el Magus, `LaunchOrb` y
> `InvokeMagicTurret` están los dos atados a `dpad.down` (tecla `S`/`K`). Con
> energía suficiente, apretarlo dispara **los dos a la vez**. Pasaba igual con
> joystick, no es nuevo. Hay que decidir qué botón le toca a cada uno — es
> decisión de diseño, por eso no lo toqué.

### Prueba 5 — menús con teclado

Sin joystick, antes el juego era casi injugable: no se podía pausar, ni abrir o
cerrar opciones, ni cambiar el volumen, ni volver atrás, ni salir de la pantalla
de fin de partida.

Y había **dos crasheos**: `CloseOptionsMenu` y `PressButtomTime2` hacían
`Gamepad.all[i]` sin verificar que hubiera alguno conectado, así que con cero
joysticks tiraban excepción **en cada frame**.

> [!NOTE]
> **Al mergear, el de `CloseOptionsMenu` quedó resuelto por otro lado.** Nehemías
> reescribió todo el menú de opciones y ese script ya no existe; sus versiones
> nuevas no usan `Gamepad` (van por UI de Unity), así que el crasheo se fue igual.
> Lo mismo con los 3 de volumen. El arreglo de `PressButtomTime2` sí es nuestro.

- [ ] `Esc` pausa la partida y `Esc` de nuevo la despausa
- [ ] Se abre y se cierra el menú de opciones (menú nuevo de Nehemías)
- [ ] Se puede cambiar el volumen sin joystick
- [ ] `Backspace` vuelve atrás en el selector de personaje
- [ ] Al terminar una partida, `Enter` sale de la pantalla de fin
- [ ] La consola **no escupe excepciones** en el menú de configuración

> [!CAUTION]
> **`PauseMenu` cambió de comportamiento, revisalo.** El código anterior tenía dos
> problemas: el bucle que detectaba la pausa cortaba el método entero con el
> primer joystick que no estuviera apretando el botón, y al pausar ponía
> `Time.timeScale = 0` pero el camino de "despausar" nunca lo restauraba — la
> pausa era un camino de ida.
>
> Ahora es un toggle real que devuelve el tiempo a 1. **Es un arreglo, pero cambia
> cómo se comporta el juego**, así que confirmá que la pausa haga lo que ustedes
> esperan.

---

## Qué avisar

Si algo falla, contame:

1. **En qué escena** y con qué personaje
2. **Qué esperabas** que pasara y qué pasó
3. Si hay algo rojo en la consola, **copiá el error completo**

---

## No toques las escenas de combate 🙏

Estoy por convertir los trompos en prefabs, y eso reescribe medio
`Practica.unity` y `1VS1.unity`. Son 2,5 MB de YAML cada una: **un conflicto de
merge en un archivo de escena no se resuelve, se elige una versión y se pierde la
otra.**

Si necesitás tocar alguna, avisame antes y coordinamos.

---

## Tres cosas para decidir entre los dos

No las toqué porque no son decisión mía:

1. **`ActiveBurn`, `ActiveFreeze` y `ActiveParalysis`** gastan energía *sin
   verificar que alcance* y no llaman a `PlayerChoseAnAction`, así que no cierran
   el turno. ¿Es a propósito (un buff que no consume turno) o es un bug? Los dejé
   afuera de `IAIAction` porque si la IA los usa podría quedarse colgada en su
   turno — el mismo bug del Magus congelado que ya peleamos.

2. **`Shield.cs` está vacío** — `Start` y `Update` sin cuerpo. ¿Se implementa o se
   borra? (Un `Update()` vacío igual cuesta una llamada por frame.)

3. **`Assets/_Recovery/`** tiene ahora **6** archivos de recuperación de Unity
   versionados en el repo. Es basura que Unity genera cuando se cierra mal.
   Conviene sacarlos y agregar la carpeta al `.gitignore`.

4. **El conflicto `dpad.down` del Magus** (ver Prueba 4): `LaunchOrb` y
   `InvokeMagicTurret` comparten botón.

5. **Quedan ~13 scripts de combate con el patrón viejo.** Cosas como
   `BasicAttack` o `LaunchShuriken` todavía preguntan
   `Gamepad.all[...] || TeclasJugador.X(...)` en línea, en vez de usar `Controles`.
   **Funcionan bien** — no son un bug, es inconsistencia nomás. Los dejé sin tocar
   a propósito para que este cambio quedara acotado a lo que estaba roto. Se pasan
   cuando quieran.

6. **`TurnIndicator` quedó en `Resources/Scripts/Beyblade/RPG/`**, no en `AI/`
   donde vos lo pusiste. Lo sacamos de `AI/` porque no es código de IA (pinta el
   trompo en su turno, sirve para humanos también). Es cosmético: si preferís tu
   ubicación, se mueve sin romper nada.

7. **Los scripts viven bajo `Assets/Resources/`.** Ojo con esto: todo lo que está
   en una carpeta `Resources` Unity lo mete en el build sí o sí, aunque no se use.
   Para código no suele hacer falta (Unity compila los `.cs` estén donde estén).
   No lo cambié porque es tu decisión y tocar 113 archivos ahora sería un merge
   infernal, pero vale la pena que lo charlen.

---

## Si algo sale muy mal

Hay un tag apuntando al estado anterior a todo esto:

```bash
git checkout backup-pre-playeridentity
```

Y una copia física del proyecto fuera del repo, por las dudas.
