# La pantalla se parte en dos fuera de 16:9

Diagnóstico y arreglo del bug que aparece en 480x854 (y en cualquier resolución
que no sea 16:9).

> Análisis hecho con IA (Claude) leyendo el código y las escenas.

> [!TIP]
> **Probado y funcionando en `InitMenu`.** David agregó `AspectoFijo` a la cámara
> y a 480x854 ya se ve una sola imagen. Falta repetirlo en el resto de las
> escenas.

---

## Qué se ve

Dos imágenes a la vez: un recuadro chico y curvo con el juego adentro, y detrás
una copia del menú ocupando la pantalla entera, más oscura y corrida.

## Por qué pasa

Son **dos causas que se suman**.

### 1. El efecto CRT depende del aspecto de la pantalla

El CRT no es un shader propio: es post-procesado de URP. En
`Assets/Scenes/Practica/Global Volume Profile.asset`:

```
LensDistortion
  intensity: 0.3
  center: (0.5, 0.5)   scale: 1
```

Esa deformación se aplica sobre lo que renderiza **la cámara**, y su resultado
depende del aspecto: está calibrada para 16:9. En 9:16 (480x854 es vertical)
aprieta la imagen hacia el centro y la deja en un recuadro chico. El borde curvo
que se ve es eso.

### 2. La copia de afuera es el "estirado" de la lente

> [!NOTE]
> **Corrección.** La primera versión de este documento decía que `InitMenu` tenía
> 2 Canvas en `Screen Space - Overlay` y que esa era la segunda causa. **Era
> falso**: salió de buscar `m_RenderMode` en todo el archivo de escena, y ese
> campo también aparece en otros componentes. Mirando los componentes Canvas de
> verdad, `InitMenu` tiene **uno solo, y ya está en Screen Space - Camera**.

La imagen grande y borrosa de atrás no es un segundo canvas: es **la misma
imagen, estirada hacia afuera por la lente**.

Cuando `LensDistortion` comprime el centro, los píxeles del borde se estiran
radialmente hacia los costados. Por eso afuera se ve el logo y los botones más
grandes, más oscuros y desenfocados: es el borde del recuadro repetido hacia
afuera.

O sea: **una sola causa, no dos.** El aspecto que no es 16:9.

### 3. De yapa: la resolución se pisaba sola

`GraphicsManager` está puesto en **las 7 escenas** y hacía
`Screen.SetResolution(1920, 1080)` en cada `Start()`. O sea: si jugabas a
480x854, al cambiar de escena te la devolvía a 1920x1080.

---

## Qué se arregló por código (ya está en esta rama)

**1. `GraphicsManager` aplica la resolución una sola vez por sesión.** Una bandera
`static` evita que cada cambio de escena vuelva a pisarla. Y se agregó el tilde
`forzarAlArrancar` por si quieren que respete la resolución con la que se abrió la
ventana.

**2. Nuevo script `AspectoFijo.cs`** (en `Scripts/Effects/`). Recorta el viewport
de la cámara para que el área dibujada **siempre tenga 16:9**, con bandas negras
arriba y abajo (o a los costados). Así el CRT recibe siempre el aspecto para el
que fue calibrado.

---

## Lo que falta hacer en Unity (no lo puedo hacer yo)

**Paso único — Poner `AspectoFijo` en la cámara principal de cada escena.**

1. Abrir la escena (empezar por `InitMenu`).
2. En la Hierarchy, seleccionar la cámara principal.
3. **Add Component** → buscar `AspectoFijo`.
4. No tocar nada más: 16:9 viene por defecto.
5. Guardar la escena (`Ctrl+S`).

Repetir en las otras escenas: `Practica`, `1VS1`, `ConfigMenu`,
`SeleccionDePersonaje`, `SeleccionDificultad`.

**Probar:** abrir el juego a 480x854. Tiene que verse **una sola imagen**,
centrada, con bandas negras arriba y abajo — y el CRT con la misma curva de
siempre.

---

## Aparte: 2 Canvas en Overlay en `Practica`

Nada que ver con el bug de la captura, pero lo encontré de paso y es una
inconsistencia real:

| Escena | PlayerCanvas en Camera | PlayerCanvas en Overlay |
|---|---|---|
| `1VS1` | 6 | 0 |
| `Practica` | 3 | **2** |

En `1VS1` **todos** los PlayerCanvas están en `Screen Space - Camera`; en
`Practica` hay dos que quedaron en `Overlay`. Esos dos no reciben el efecto CRT,
así que el HUD de esos jugadores se ve "limpio" mientras el resto tiene el filtro.

Se arregla igual: Render Mode → `Screen Space - Camera`, asignar la cámara,
Plane Distance ~10.

> [!WARNING]
> Eso toca `Practica.unity`, y Nehemías estuvo trabajando ahí. **Coordinen antes**
> — los conflictos de merge en archivos `.unity` no se resuelven, se pierde una de
> las dos versiones.

---

## Si aun así se sigue viendo raro

Quedaría una cosa por ajustar, y es decisión de diseño: el `CanvasScaler` de
`InitMenu` está en **Match = 0 (Match Width)**. Con un aspecto muy vertical, eso
escala la UI solo por el ancho y la deja desbordando a lo alto. Poniéndolo en
**0.5** reparte entre ancho y alto, que suele portarse mejor en pantallas raras.
No lo cambié porque afecta cómo se ve la UI en 16:9 también, y eso lo tienen que
mirar ustedes.
