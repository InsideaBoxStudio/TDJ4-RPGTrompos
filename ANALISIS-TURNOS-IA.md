# Análisis: turnos de la IA, daño y "la IA no hace nada"

Respuesta a los tres reclamos de Nehemías. Cada hallazgo está marcado como
**CONFIRMADO** (leído en el código, la causa es esa) o **SOSPECHA** (candidato
concreto, pero hace falta probarlo en Unity para descartar los otros).

> Análisis hecho con IA (Claude) leyendo el código y las escenas. **No se ejecutó
> el juego.** Lo que dice "confirmado" es confirmado *en el código*, no jugado.

> [!CAUTION]
> **Los cambios al sistema de turnos ROMPIERON el juego y fueron revertidos.**
> Lo de abajo sigue siendo un diagnóstico válido de por qué la IA ataca más, pero
> **la solución que propuse era incorrecta**. Ver "Qué se revirtió y por qué".
> Los otros arreglos (retroceso, daño, parry) siguen en pie.

---

## 1. "La IA no respeta los turnos igual que el jugador, ataca mucho más"

**CONFIRMADO.** No es una impresión: el humano y la IA usan **reglas distintas**
para pedir turno. Hay cuatro asimetrías, y se suman.

### 1A. La IA pide turno a mayor velocidad

| | Condición para pedir turno |
|---|---|
| Humano (`CheckPlayerTurn.Update`) | `velocidad < maxVelocityTurn` → por defecto **1.0** |
| IA (`AIBrain.Update`) | `velocidad < aiReadyVelocity` → **1.5** |

El humano tiene que frenar más que la IA. **Cada turno, la IA se habilita antes.**
Esto solo ya explica buena parte del "ataca mucho más".

### 1B. La IA se saltea la ventana de sincronización

- Humano: `NotifyReady()` → corrutina `CheckSync()` → espera `syncWindow` (0.2s) → recién ahí `Ready()`
- IA: `RequestTurnNow()` → `NotifyReadyImmediate()` → `Ready()` **al instante**

Matiz importante: en **Practica**, `RPGTurn` vive en un objeto inactivo (lo dice su
propio comentario), así que `NotifyReady` detecta `!isActiveAndEnabled` y manda a
**todos** por el camino inmediato. O sea que en Practica nadie tiene ventana de
sincronización. En **1VS1** el humano sí la tiene y la IA no aplica.

### 1C. Bug real de contabilidad en `RPGTurn`

```csharp
IEnumerator CheckSync()
{
    yield return new WaitForSeconds(syncWindow);
    activePlayers.Clear();        // ← borra a quien ya estaba
    foreach (var p in readyPlayers) { activePlayers.Add(p); p.Ready(); }
```

Si la IA ya tomó su turno por `NotifyReadyImmediate` (que la agregó a
`activePlayers`), la corrutina del humano **la borra de la lista**. Después, cuando
la IA termina, `PlayerFinished` no la encuentra → `everyoneFinished` sale mal →
`Time.timeScale` y el reseteo del contador quedan desfasados.

Sumado a que `NotifyReadyImmediate` hace `Time.timeScale = 0f` sin condiciones y
solo `PlayerFinished` lo devuelve a 1 cuando la lista queda vacía, **el tiempo se
puede quedar congelado o descongelar antes de tiempo.**

### 1D. `maxVelocityTurn` cambia con cada acción; `aiReadyVelocity` no

`BasicAttack` llama `PlayerChoseAnAction(moveDuration, 1000f, true)`. Ese `1000f`
le pisa al humano `maxVelocityTurn = 1000` → el turno siguiente lo toma
**a cualquier velocidad**. La IA ignora ese mecanismo y siempre usa su 1.5 fijo.

O sea: **no es que la IA tenga otro número, es que juega con otro sistema.**

### Qué haría

La solución de fondo es que la IA pida turno **por el mismo camino que el humano**:
que `AIBrain` no llame a `RequestTurnNow()` y deje que `CheckPlayerTurn.Update()`
decida, igual que para una persona. La IA seguiría decidiendo *qué* hacer, pero
*cuándo* le toca lo decidiría el mismo juez que para el jugador.

Eso toca el corazón del combate, así que **no lo cambié**: hay que decidirlo entre
ustedes y probarlo. Como paso intermedio y barato: poner `aiReadyVelocity` en 1.0
para igualar 1A.

---

## 2. "A veces el enemigo no recibe daño en los choques de ataque básico"

**SOSPECHA**, con un candidato principal bastante claro.

El daño lo aplica `AttackCollider`, que vive en un objeto aparte (`ColliderAttack`)
que `BasicAttack` prende con `SetActive(true)`. Pero el cuerpo del trompo tiene
**otro** collider, `BeybladeCollider`, y este hace:

```csharp
private void OnCollisionEnter2D(Collision2D collision)
{
    ...
    rb.linearVelocity = velocidadReflejada * rebound;   // los separa al instante
}
```

**Es una carrera entre dos colliders.** Si el cuerpo toca primero y el rebote los
separa, el `ColliderAttack` puede no llegar a solaparse nunca → no hay daño. Que
dependa de la geometría del choque explica que pase *a veces* y no siempre.

Otros dos factores que pueden sumarse:

- `AttackCollider` compara `collision.gameObject.layer != transform.parent.gameObject.layer`.
  Usa `transform.parent`: si alguna vez el objeto se reparenta, la comparación se
  hace contra el objeto equivocado. Y varios scripts reparentan hijos
  (`LaunchShuriken`, `Sustitution`, las magias).
- `BasicAttack` apaga el collider con `Invoke("EndAttack", moveDuration)`, que corre
  en **tiempo escalado**. Como durante los turnos `Time.timeScale = 0`, esa ventana
  se estira de forma impredecible: a veces el collider ya se apagó cuando llega el
  contacto.

### Cómo confirmarlo

Un `Debug.Log` en `AttackCollider.OnCollisionEnter2D` antes del `if` del layer, y
otro adentro. Si el primero no aparece en los choques sin daño → es la carrera de
colliders. Si aparece el primero pero no el segundo → es la comparación de layers.

---

## 3. "A veces la IA entra en su turno y no hace nada"

**CONFIRMADO. Y sí, David: está usando la opción de espera.** No está colgada.

Hay tres caminos que terminan en `PerformWait()`:

**1. Duda deliberada** (`AttackState`):
```csharp
if (brain.DudaYEspera()) { brain.PerformWait(); return; }
```

| Dificultad | Probabilidad de dudar |
|---|---|
| Fácil | **45%** |
| Normal | **15%** |
| Difícil | 0% |

En Normal, 15% coincide con el "pasó pocas veces" que reportó Nehemías.

**2. `RetreatState` no hace nada todavía.** `PerformRetreat()` es literalmente
`PerformWait()`:
```csharp
public void PerformRetreat()
{
    // El alejarse real (retroceder) se afinará al implementar las dificultades.
    PerformWait();
}
```
Así que **con la vida baja la IA se queda esperando, turno tras turno.** Esto
probablemente se nota más que la duda del 15%.

**3. `DefendState`** espera cuando se queda sin energía. Ese sí es correcto.

### Qué haría

- Lo de la duda es una decisión de diseño: si molesta, se baja `probabilidadDudar`.
- **Lo de `RetreatState` sí es una deuda real**: hay que implementar el retroceder
  de verdad (moverse en dirección contraria al rival) en vez de esperar.
- En los dos casos, que la espera se **vea** (una animación, un cartelito) evita que
  parezca que el juego se colgó.

---

## Qué se revirtió y por qué

Los tres cambios al sistema de turnos **rompieron el juego** y se dieron de baja.
`RPGTurn.cs` y `CheckPlayerTurn.cs` volvieron exactamente al estado anterior.

**El error de fondo fue mío:** propuse que la IA pidiera turno con "la misma regla
que el humano", leyendo `maxVelocityTurn`. Pero como está explicado más arriba en
1D, **`maxVelocityTurn` no es un umbral estable**: cada acción lo reescribe, y
`BasicAttack` lo deja en **1000**, que significa "tomá turno a cualquier
velocidad". Copiar ese número para la IA no la frena — le saca el freno que tenía
(su 1.5 fijo). O sea que hacía **lo contrario** de lo que buscaba.

Lo diagnostiqué bien y lo arreglé mal. Y lo peor: escribí el análisis que explicaba
exactamente por qué ese número no servía, y aun así lo usé.

Los otros dos cambios (`activePlayers.Clear()` y `WaitForSecondsRealtime`) también
se revirtieron. Puede que fueran correctos, pero al ir en el mismo commit no hay
forma de saber cuál rompió qué — y con el juego roto, lo primero es volver a lo
que funcionaba.

**Si se vuelve a intentar, hay que hacerlo de a un cambio por vez y probando cada
uno en Unity.** Sin correr el juego no alcanza para tocar el sistema de turnos.

## Qué se arregló (y sigue en pie)

| # | Arreglo | Archivo |
|---|---|---|
| 4 | `RetreatState` **retrocede de verdad**: se mueve en dirección contraria al rival en vez de esperar. Requirió un `DoMove(direccion)` nuevo, porque el prompter siempre apunta al enemigo. | `AIBrain`, `MovementOption` |
| 5 | El golpe también se intenta en `OnCollisionStay2D`, con una bandera que garantiza **un solo impacto por ataque**. Así el rebote del cuerpo ya no puede robarle el golpe al collider de ataque. | `AttackCollider` |
| 6 | La layer del dueño se **cachea al prender** en vez de leer `transform.parent` en cada choque (varios scripts reparentan hijos). Y si el objeto golpeado no tiene `Vida`, sale limpio en vez de tirar excepción. | `AttackCollider` |
| 7 | `Parry` toma el índice de `PlayerIdentity` en vez de `int.Parse(transform.name)`. | `Parry` |

### Lo que NO se tocó, a propósito

**La duda del `AttackState`** (`probabilidadDudar`: 45% en Fácil, 15% en Normal, 0%
en Difícil). Eso no es un bug: es la palanca deliberada que hace que la Fácil sea
fácil. Con el `RetreatState` ya arreglado, la mayor parte del "la IA no hace nada"
debería desaparecer sola. Si igual molesta, es un número y se baja — pero es una
decisión de **balance**, no de código, y la toman ustedes.

---

## Pendiente para ustedes

- **Solo hay un `Parry` en cada escena**, en el GameObject llamado `"0"` (jugador 1).
  El jugador 2 y la CPU **no pueden hacer parry**. Eso se arregla en la escena, no
  en el código.
- El conflicto de `dpad.down` en el Magus (`LaunchOrb` e `InvokeMagicTurret`
  comparten botón) sigue ahí: hay que decidir qué botón le toca a cada uno.
