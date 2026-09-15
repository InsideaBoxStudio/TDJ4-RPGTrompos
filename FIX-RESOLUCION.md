# La pantalla se parte en dos fuera de 16:9

Diagnóstico y arreglo del bug que aparece en 480x854 (y en cualquier resolución
que no sea 16:9).

> Análisis hecho con IA (Claude) leyendo el código y las escenas. **No pude
> ejecutar el juego a 480x854**, así que el diagnóstico está razonado sobre el
> código, no comprobado en pantalla. La parte de editor hay que hacerla y probarla
> en Unity.

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

### 2. La mitad de la UI no pasa por el CRT

En `InitMenu` hay:

| Canvas | Render Mode | ¿Lo afecta el CRT? |
|---|---|---|
| 2 canvas | **Screen Space - Overlay** | **No** |
| 1 canvas | Screen Space - Camera | Sí |

Los Canvas en **Overlay se dibujan DESPUÉS del post-procesado**. Nunca los toca
la lente: siguen ocupando la pantalla completa.

Entonces: la UI que va por cámara queda encerrada en el recuadro chico, y la UI
Overlay sigue a pantalla completa. **Esas son las dos copias.**

A 16:9 las dos coinciden bastante y no se nota. Cuanto más se aleja el aspecto
de 16:9, más se separan.

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

**Paso 1 — Poner `AspectoFijo` en la cámara principal de cada escena.**
No hay nada que configurar: 16:9 viene por defecto.

**Paso 2 — Pasar los Canvas de `Screen Space - Overlay` a `Screen Space - Camera`.**

Este es el paso que de verdad arregla las dos copias. Un Canvas en Overlay
**ignora la cámara**, así que las bandas negras del paso 1 no lo afectan: va a
seguir ocupando la pantalla entera.

Por cada Canvas en Overlay:
1. Render Mode → **Screen Space - Camera**
2. Render Camera → la cámara principal de esa escena
3. Plane Distance → algo como 10 (que quede delante de todo)

Empezar por `InitMenu`, que es donde está el problema de la captura. Después
revisar el resto de las escenas.

> [!WARNING]
> El paso 2 toca las escenas, y Nehemías estuvo trabajando ahí. **Coordinen antes**
> — los conflictos de merge en archivos `.unity` no se resuelven, se pierde una de
> las dos versiones.

**Paso 3 — Probar.** Abrir el juego a 480x854 y verificar que se vea una sola
imagen, centrada, con bandas negras arriba y abajo.

---

## Si aun así se sigue viendo raro

Quedaría una cosa por ajustar, y es decisión de diseño: el `CanvasScaler` de
`InitMenu` está en **Match = 0 (Match Width)**. Con un aspecto muy vertical, eso
escala la UI solo por el ancho y la deja desbordando a lo alto. Poniéndolo en
**0.5** reparte entre ancho y alto, que suele portarse mejor en pantallas raras.
No lo cambié porque afecta cómo se ve la UI en 16:9 también, y eso lo tienen que
mirar ustedes.
