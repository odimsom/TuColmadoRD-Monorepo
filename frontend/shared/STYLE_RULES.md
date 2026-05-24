# Style Rules — cómo se comporta cada cosa

Reglas de comportamiento (motion, modales, formularios, hover/press, layout, jerarquía). Más estrictas y operativas que `README.md`. Cuando algo no esté aquí, pregunta antes de inventar.

> **Regla cero.** Si una decisión tiene 2 opciones plausibles, elige la **más silenciosa**. La marca confía en su voz tipográfica y en su color de bandera; la UI no compite con ellos.

---

## 1. Color — cuándo usar qué

| Token | Cuándo SÍ | Cuándo NO |
|---|---|---|
| `--color-primary` (#0047ab) | Acción principal · CTAs · enlaces · estado seleccionado · acentos en oscuro · valores financieros positivos | Texto largo · fondos extensos · iconos decorativos |
| `--color-secondary` (#e41e26) | Urgencia · CTA final ("sección roja") · destrucción · fiados vencidos · alertas críticas · saldos por cobrar | Acción primaria del día a día · botones repetidos |
| `--color-accent` (#facc15) | Reservado, **no usar** sin confirmar | Cualquier uso casual |
| `--color-neutral` (#1a1a1a) | Stage oscuro: nav, sidebar, footer, panel izquierdo de auth | Texto sobre fondo claro (usa `fg-1` en su lugar) |
| `fg-1 → fg-5` | Texto y elementos sobre superficies claras. Nunca interpolar opacidades a mano | Decoración |
| Verde / amarillo / azul cielo / rojo | **Solo** estado/feedback (success, warning, info, error). Nunca como brand | Como variante de marca |

**Mezclas válidas:**
- Primario sobre neutro: ✅ (hero secundario, CTA en footer)
- Secundario sobre neutro: ✅ pero raro (urgencia máxima)
- Primario sobre secundario: ✅ solo en CTA roja final
- Secundario sobre primario: ❌ nunca
- Acento sobre nada: ❌

**Combinaciones prohibidas:**
- Gradientes morado→rosa, azul→cian, mesh blobs, gradients radiales decorativos.
- Sombras de color (`box-shadow: 0 0 20px rgb(0 71 171 / 0.5)`). Las sombras son neutras.
- Bordes de colores aleatorios. Solo `base-300` (división), `primary` (foco/activo), `secondary` (error/destructivo), `error` (form error).

---

## 2. Tipografía — qué cuándo

| Estilo | Tamaño | Familia / weight | Uso |
|---|---|---|---|
| **Display** | 48–112 px | Playfair 900 **italic uppercase** + tracking −0.04em + leading 0.85 | Marketing headlines, *solo* la landing. Nunca dentro del admin. |
| **H1** (admin) | 28–32 px | Playfair 900, no italic | Title de página dentro del portal. Sin italic. |
| **H2** | 22–24 px | Playfair 900, no italic | Bloques mayores dentro de página. |
| **H3** | 18–20 px | Playfair 900, no italic | Section header de card. |
| **H4** | 14–16 px | Inter **700** (no Playfair) | Labels de card, fila de tabla con énfasis. |
| **Body** | 14–18 px | Inter 400 / 500 | Párrafo largo. 14 = default, 16+ = lead. |
| **Eyebrow** | 10–12 px | Inter **900 UPPERCASE** + tracking 0.18–0.30em | Etiquetas, micro-labels, indicadores de zona. |
| **Mono** | 11–14 px | system mono | Códigos: SKU, RNC, e-CF, recibos, timestamps. |

**Reglas duras:**
- Display **siempre** italic en la landing. Nunca italic dentro del admin.
- Eyebrows **siempre** terminan en ítem cerrado (sin coma final, sin elipsis): "PROGRAMA PIONERO" no "PROGRAMA PIONERO,".
- Nunca usar Playfair para body. Nunca Inter para headlines de marketing.
- Mínimos absolutos: 11 px para meta/eyebrow, 12 px para body inferior, 24 px para cualquier número grande de dashboard.
- **No usar guiones largos (—) en copy.** Sustituir por coma, punto o `·`. Excepción única: como placeholder de celda vacía en tablas (ese sí).

---

## 3. Espaciado — escala y aplicación

Base = **4 px**. Tokens en `colors_and_type.css`.

| Contexto | Gap interno | Gap entre elementos | Padding externo |
|---|---|---|---|
| **Botón** | 6–8 px (icon → label) | — | sm 8/14, md 10/18, lg 14/24 (vert/horiz) |
| **Form field** | 6 px (label → input) | 16–20 px entre fields | — |
| **Card content** | 12 px sección → sección | 8 px ítem → ítem | compact 16, tight 12, default 24 |
| **Sidebar nav item** | 12 px icon → label | 0 (rows tocan) | 10/14 |
| **Toolbar / topbar** | 8 px botón → botón | 16 px grupo → grupo | 0/24 |
| **Section spacing (landing)** | — | — | `py-32` (128 px) entre secciones |
| **Section spacing (admin)** | — | — | `py-6` (24 px) padding del main scroll |

**Anti-reglas:**
- Nunca padding asimétrico arbitrario (p-7 17 5 23). Si se necesita, justificar.
- Nunca espaciado inferior a 4 px (excepto baseline).
- Nunca más de 32 px de gap **interno** en una card; si más, es porque la card es la página.

---

## 4. Bordes y radios

| Token | Valor | Cuándo |
|---|---|---|
| `--radius-none` | 0 | **Marketing CTAs**, AppFlag, eyebrow capsules. Sin excepciones. |
| `--radius-sm` | 4 | Tags secundarios |
| `--radius-md` | 6 | Botones admin, inputs admin, tabs |
| `--radius-lg` | 8 | Cards admin, modal cards |
| `--radius-full` | 9999 | Avatares, status dots, badges pill |

**Bordes:**
- `1px solid var(--color-base-300)` — divisores y bordes de card neutros.
- `2px solid var(--color-primary)` — card destacada (plan resaltado, opción seleccionada en modal). Nunca 3 px.
- `border-l-4 var(--color-secondary)` — bloque destacado dentro de texto largo (hero quote, alerta inline).
- `border-l-2 var(--color-primary)` — item activo de sidebar / nav vertical.
- **Nunca**: bordes punteados, dashed estilizados, bordes gradientes.

---

## 5. Sombras — sistema de elevación

| Token | Cuándo |
|---|---|
| `--shadow-sm` | Card hover (lift sutil). Botones flotantes inactivos. |
| `--shadow` (default) | Card destacada por encima del fondo, pero no modal. Sidebar derivable. |
| `--shadow-lg` | Sidebar del admin · header sticky con scroll · botones de acción flotante |
| `--shadow-xl` | Nav logo tab · Pionero card · POS cart panel |
| `--shadow-2xl` | AppFlag callouts · **modales** · receipt sheet |

**Reglas:**
- Una superficie nunca lleva 2 niveles de sombra simultáneamente.
- Si un elemento eleva por hover, va **un nivel arriba** (de `sm` a `lg`, no salta).
- **Nunca** sombras coloreadas, inset shadows, ni glows.

---

## 6. Iconografía — uso operativo

- **Set único:** Lucide vía Iconify. `<iconify-icon icon="lucide:…">`.
- **Tamaños canónicos:** 14 (inline meta), 16 (label + input prefix), 18 (button + topbar), 20 (card icon), 22 (CTA button), 28 (section illustration), 42 (empty state).
- **Color del icon:** **heredado** del texto (`currentColor`), excepto:
  - Iconos dentro de wells (cuadrados de fondo tinted) → el well lleva `bg-primary/10`, el icon `color: primary`.
  - Status dots → color literal del estado.
- Stroke por defecto. **No** `lucide:` con sufijo `-filled`. Si necesitas peso, sube el `width/height`.
- **Nunca** emoji en UI de producto. Excepción: copy editorial en blog (no implementado).

---

## 7. Hover / press / focus / disabled

Cada elemento interactivo tiene **los 4 estados**.

| Estado | Botón solid | Botón ghost | Card clicable | Link |
|---|---|---|---|---|
| **Default** | bg-primary, color #fff | transparente, color fg-2 | bg-base-100, border base-300 | color primary |
| **Hover** | bg-primary darker 10% | bg-base-200, color fg-1 | bg-primary/4, border primary/30 | underline + color base-content |
| **Active (pressed)** | bg-primary darker 18% | bg-base-300 | scale(0.99) opcional | — |
| **Focus-visible** | outline 2px primary, offset 2 | mismo | outline 2px primary, offset 2 | underline |
| **Disabled** | opacity 0.5, cursor not-allowed | mismo | opacity 0.5 | opacity 0.5 |
| **Loading** | spinner + texto, pointer-events none | — | — | — |

**Reglas duras:**
- Hover usa **color**, **no** `transform: scale`. Excepción única: CTA hero final con `hover:scale-105` (heredado de la fuente). No proliferarlo.
- Focus visible **siempre** con outline. Nunca solo color de borde.
- Disabled **nunca** se ve igual que activo con menos contraste; tiene `cursor: not-allowed` + opacity.
- Tiempo de transición: `150ms` admin, `200ms` marketing. Easing `var(--ease-reveal)`.

---

## 8. Botones — variantes y jerarquía

Tres prioridades de acción, **una sola primary por vista**.

```
[Primary]   acción principal de la vista
[Outline]   acción secundaria
[Ghost]     acción terciaria (cerrar, descartar, descartable)
```

**Tamaños:**
- `sm` — chips en toolbar, listas densas
- `md` — default
- `lg` — landing hero, modal final

**Reglas:**
- **Marketing** usa la clase `.btn-*` (radius 0, uppercase, tracking 0.18em).
- **Admin** usa `tc-btn-*` (radius 6, sentence case, no tracking).
- Botón con icono → icono **a la izquierda**, gap 6 px. Excepción: dropdown caret va a la derecha.
- Nunca >3 botones en un toolbar sin agruparlos en menú.
- "Cancelar" en modales es siempre `ghost`, nunca `outline`.
- Nunca dos `primary` adyacentes (excepto en hero, donde el segundo es `outline` aunque visualmente parezcan iguales).

---

## 9. Forms — comportamiento exigido

| Cosa | Regla |
|---|---|
| **Label** | Siempre por encima del input. Nunca `placeholder` como label. |
| **Required** | Marca con asterisco rojo después del label, no en placeholder. |
| **Placeholder** | Ejemplo realista: "tu@correo.com", "Arroz Rico 5 lb", no "Ingrese su email". Estilo Inter 400 fg-4. |
| **Hint** | 11 px fg-3, debajo del input, antes del error. |
| **Error** | 11 px error color, debajo. Reemplaza al hint. Mensaje **específico** ("El correo no es válido") no genérico ("Error"). |
| **Autofocus** | Solo en el primer campo de un formulario que sea el evento principal de la pantalla (login, búsqueda en POS, primer campo de modal). Nunca en formularios secundarios. |
| **Validation** | On blur, no on keystroke. Mostrar éxito (verde) **solo** en pantallas críticas (e-CF, pago). |
| **Submit** | Botón disabled mientras loading. Muestra spinner + label en pasado progresivo ("Guardando…", "Entrando…"). |
| **Enter** | Submitea el formulario. Modales siempre permiten Esc para cerrar y Enter para confirmar la acción primaria. |

**Anti-reglas:**
- Nunca borrar lo que el usuario tipeó al fallar la validación.
- Nunca mostrar 7 errores al mismo tiempo. Si fallan varios, muestra los del primer campo + alerta general.

---

## 10. Modales — comportamiento completo

**Estructura obligatoria:**

```
┌──────────────────────────────────────────┐
│  Header: título + (opcional) close ×     │  ← borde inferior base-300
├──────────────────────────────────────────┤
│  Body: contenido. Scrollable si >70vh.   │  ← padding 24
├──────────────────────────────────────────┤
│  Footer: [Ghost cancelar] [Primary save] │  ← bg-base-200 cuando hay acciones
└──────────────────────────────────────────┘
```

**Reglas operativas:**

| Cosa | Valor |
|---|---|
| **Backdrop** | `rgb(0 0 0 / 0.50)`, fade-in 150 ms |
| **Corner radius** | 10 px (un escalón por encima de las cards = 8 px) |
| **Sombra** | `var(--shadow-2xl)` |
| **Ancho** | xs 380 · sm 460 · md 600 · lg 800 px. Nunca > 90% viewport. |
| **Alto** | auto; `max-height: 80vh`, body scroll si excede |
| **Posición** | Centrado vertical y horizontal en viewport. Excepción: drawer (slide-in derecha) para detalles largos — fuera de scope de este sistema. |
| **Apertura** | `transform: scale(0.96) → 1` + `opacity: 0 → 1` en 180 ms, easing `--ease-reveal` |
| **Cierre** | Inverso, 120 ms |
| **Click backdrop** | Cierra **solo si** no hay datos sin guardar. Si los hay, mostrar diálogo de confirmación inline. |
| **Esc** | Siempre cierra (mismo flujo que el backdrop). |
| **Focus trap** | Focus se mueve al primer field interactivo al abrir. Tab cicla dentro del modal. Al cerrar, foco vuelve al botón que abrió. |
| **Botón close (×)** | Solo si el modal es informativo o tiene un footer con cancel + primary. Si el modal es destructivo o crítico, **no** mostrar ×; obligar al usuario a leer y elegir explícitamente. |
| **Anidación** | Máximo 2 modales apilados, y solo si el segundo es **confirmación** del primero. Más capas = mal diseño, replantear. |
| **Body lock** | `overflow: hidden` en `<body>` mientras hay modal abierto. |

**Variantes:**

- **Form modal** (default): título arriba, form en body, footer con Cancelar (ghost) + Acción (primary).
- **Choice modal** (POS payment): título + total destacado en header, grid de opciones grandes en body, sin footer porque cada opción es la acción.
- **Confirm destructive**: backdrop más oscuro `0.65`, sin ×, footer con Cancelar (ghost) + Eliminar (primary con `tc-btn-error` o icon-only). Texto del cuerpo nombra **explícitamente** lo que se borra.
- **Receipt / success**: ícono grande de check arriba, datos en mono, dos botones (Imprimir outline + Continuar primary).

---

## 11. Toasts — feedback no bloqueante

- Posición: **bottom-right**, stack vertical, gap 8 px.
- Ancho: `min 320 px`, `max 480 px`.
- Duración: success 4 s, info 4 s, warning 6 s, error 8 s. Hover pausa el timer.
- Animación: slide-up + fade (220 ms, `--ease-reveal`).
- Cierre manual: × en esquina superior derecha del toast.
- **Nunca**: toast para confirmaciones críticas (usa modal). Nunca > 4 toasts simultáneos (los más antiguos se reemplazan).
- Estructura: icono + título corto + body opcional + acción opcional ("Deshacer", "Ver").

---

## 12. Tablas — comportamiento

- **Header** sticky cuando la tabla scrollea dentro de su contenedor (`position: sticky; top: 0`).
- **Row hover** cambia el fondo a `var(--color-base-200)`. Nunca un transform.
- **Row click** navega; **no** abre modal. Si abre modal, el row debe llevar `data-clickable` y el cursor pointer; pero preferimos navegación a detalle.
- **Selección múltiple**: checkbox en primera columna, header con checkbox tri-state. Solo si hay bulk actions.
- **Empty state**: dentro del cuerpo, alto fijo 240 px, icono grande fg-4 + mensaje específico + 1 acción primaria.
- **Loading**: spinner centrado, **no** skeleton rows (en este sistema). Tabla mantiene el header con altura de filas vacías para evitar layout shift.
- **Sort**: indicador en header con `lucide:chevron-up/down`, tap cambia asc → desc → off.
- **Paginación**: pie de tabla con conteo a la izquierda, paginador join-button a la derecha (`< 1 / 12 >`).
- Filas: **alineación numérica a la derecha**, fechas y meta center-aligned, nombres a la izquierda.

---

## 13. Cards — anatomía

- Default: `bg-base-100 + border 1px base-300 + radius 8`.
- Compact: padding 16. Tight: padding 12.
- **Section header dentro de card**: H3 (14 px Inter 700) a la izquierda, acción (`a` color primary 11 px 600 + flecha →) a la derecha, gap 14 abajo.
- **Card clicable**: hover cambia `border-color` a `rgb(primary 0.30)` y `background` a `rgb(primary 0.04)`. Cursor pointer.
- **Card destacada** (resaltada): `border 2px primary` + `background rgb(primary 0.04)`. Nunca tres niveles de destaque en una página.

---

## 14. Empty states — copy + estructura

Plantilla obligatoria:

1. **Icono grande** (42 px) `lucide:*-x` o el icono del módulo en color `fg-4`.
2. **Mensaje principal** (14 px fg-3 medium): describe la ausencia con voz dominicana, no genérica.
   - ✅ "No hay ventas registradas todavía."
   - ❌ "No data."
3. **Mensaje secundario** (opcional, 12 px fg-4): qué pasa cuando hagas la acción.
4. **Acción primaria** (botón pequeño): el verbo más común que resuelve el vacío.
5. Opcional: **link a tutorial** o "Importar Excel" / "Importar CSV".

Ejemplos:
- Inventario vacío → "Agrega productos para empezar a vender" + [Nuevo producto] + [Importar CSV].
- Fiados vacío → "Sin saldos pendientes. Bien hecho." + (sin acción — celebrar).
- Ventas vacío → "Aún no has vendido hoy" + [Abrir POS].

---

## 15. Loading — qué mostrar y cuándo

| Duración esperada | UI |
|---|---|
| < 200 ms | Nada |
| 200–800 ms | Spinner inline (botón con texto en pasado progresivo) |
| > 800 ms | Spinner centrado de la región + bloqueo de interacción |
| > 3 s | + Texto secundario explicando ("Conectando con DGII…") |
| > 10 s | Toast de "Esto está tardando más de lo normal. ¿Quieres reintentar?" |

**Skeleton** no se usa en este sistema (la marca es texto-céntrica y el skeleton suena a "tech bro startup"). Si una pantalla tarda más de 1 segundo en aparecer es problema de backend, no de UI.

---

## 16. Z-index — escala completa

```
0       contenido base
10      sidebar shadow
20      sticky table header
30      sticky topbar
40      sidebar overlay (mobile drawer)
50      sticky navbar
60      dropdown / select popper
70      tooltip
80      drawer
90      modal backdrop
100     modal content
110     stacked modal (confirm sobre form)
120     toast
130     spotlight / tour
```

Nunca inventar valores fuera de esta escala. Si necesitas algo nuevo, refactorea.

---

## 17. Motion — un solo idioma

| Acción | Duración | Easing |
|---|---|---|
| Hover de color | 150 ms | `--ease-reveal` |
| Hover de transform | 200 ms | `--ease-reveal` |
| Apertura modal/dropdown | 180 ms | `--ease-reveal` |
| Cierre modal/dropdown | 120 ms | `--ease-reveal` |
| Reveal-on-scroll (landing) | 750–850 ms | `--ease-reveal` |
| Slide-down menu | 250 ms | `--ease-reveal` |
| Toast in | 220 ms | `--ease-reveal` |
| Toast out | 180 ms | `--ease-reveal` |
| Sparkline draw | — | sin animación, dibujo estático |

**Reglas:**
- Una sola curva: `cubic-bezier(0.16, 1, 0.3, 1)`. Si necesitas otra, pregúntate dos veces.
- **Nunca** spring, bounce, elastic. La marca es seria.
- **Nunca** auto-play de carruseles. Si hay imágenes rotativas (no las hay actualmente), el usuario controla.
- `prefers-reduced-motion`: respetar siempre. Reveals se vuelven fades de 200 ms sin translate.

---

## 18. Responsive — breakpoints

Tres breakpoints. Cuando se necesite intermedio, justificar.

```
< 640 px   mobile        sidebar oculto, drawer
640–1024  tablet        sidebar oculto, drawer; topbar denso
≥ 1024    desktop       sidebar fijo (256 px), layout completo
≥ 1440    wide          mayores anchos de container (1200 → 1280)
```

**Particularidades:**
- **POS es tablet-first**: la grilla de productos se diseña para `iPad 1024×768` en portrait/landscape. Botones mínimos 56×56 px.
- **Marketing es mobile-first**: el hero recoloca AppFlag de absolute a estático en mobile.
- **Admin desktop-first**: si te encuentras adaptando para mobile, considera si esa pantalla pertenece a una app móvil dedicada en su lugar.

---

## 19. Accesibilidad — mínimos no negociables

- Contraste **AA mínimo** para texto. WCAG AA = 4.5:1 (body), 3:1 (display ≥ 24 px bold). Auditar `fg-3` sobre `base-200`.
- **Focus visible** obligatorio para todo elemento interactivo.
- `aria-label` en botones icon-only.
- `role="alert"` en errors de form y toasts críticos.
- Modales con `role="dialog"` + `aria-modal="true"` + foco trap.
- Tablas: usar `<th scope="col">` siempre.
- Idioma: `<html lang="es">` en todo. Nunca mezclar copy inglés/español visible.
- Nunca usar color **solo** para transmitir información. Estado "vencido" lleva color rojo Y palabra "Vencido" o un icono.

---

## 20. Anti-patrones — lo que delata mal trabajo

- Cards con borde izquierdo coloreado **y** todo lo demás gris (look genérico SaaS 2021).
- Iconos hand-drawn SVG ("blob" + glassmorphism + sparkles).
- Doble negación en copy ("No olvides no marcar…").
- Botón "OK" como única acción.
- Mezclar Inter Black uppercase con Inter Regular sin scale jerárquico claro.
- Sombras gigantes (`box-shadow: 0 80px 200px ...`) que se ven en flotante.
- Diálogo de confirmación para acción reversible. Si lo puedo deshacer en 1 click, no preguntes.
- Tooltips encima de elementos primarios. Si la información es importante, va en el layout.
- Modales con scroll interno donde el contenido tendría que ser una página.
- Más de un nivel jerárquico de "destaque" por pantalla (no resaltar 3 cards con border-2 primary simultáneamente).

---

## Cómo aplicar este documento

1. Antes de añadir un componente nuevo, busca aquí si ya hay una regla aplicable.
2. Si la regla existe → seguirla, sin negociar.
3. Si **no** existe → proponer una nueva regla **antes** de codificar. Cualquier patrón nuevo debe poder describirse en 2 frases.
4. Discrepar de la regla solo cuando un caso de uso específico genuinamente lo justifique — y entonces agregar excepción aquí.

> Este documento prevalece sobre cualquier captura, cualquier mockup y cualquier "inspiración" externa. La consistencia es más valiosa que la genialidad ocasional.
