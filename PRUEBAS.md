# Pruebas — rama `feature/player-identity`

Hola Nehe 👋 Acá está lo que hay que probar antes de mergear esta rama.

> [!IMPORTANT]
> **Estos cambios los hice con IA (Claude).** Lo aclaro para que los mires con ojo
> crítico y no des nada por sentado. Todo compila sin errores y sin advertencias
> nuevas, pero **el juego no se ejecutó ni una vez** — la parte de "compila" está
> verificada, la parte de "anda" es justamente lo que te pido.
>
> Se tocaron 23 scripts de combate. Si algo se rompió, se va a notar acá.

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

3. **`Assets/_Recovery/`** tiene 4 archivos de recuperación de Unity versionados en
   el repo. Es basura que Unity genera cuando se cierra mal. Conviene sacarlos y
   agregar la carpeta al `.gitignore`.

---

## Si algo sale muy mal

Hay un tag apuntando al estado anterior a todo esto:

```bash
git checkout backup-pre-playeridentity
```

Y una copia física del proyecto fuera del repo, por las dudas.
