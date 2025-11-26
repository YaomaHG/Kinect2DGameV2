# ?? TABLA COMPARATIVA - CONFIGURACIÓN DE COLLIDERS

## Resumen Visual de Configuración

| Componente | Personaje | Meta | Paredes |
|------------|-----------|------|---------|
| **GameObject Name** | (Cualquiera) | **"Meta"** | (Cualquiera) |
| **Tag** | **Player** ? | Goal (opcional) | (ninguno) |
| **Rigidbody2D** | ? SÍ | ? NO | ? NO |
| **Collider Type** | Capsule/Circle | Box/Circle/Polygon | Polygon |
| **Is Trigger** | ? NO | ? SÍ | ? NO |
| **Script Principal** | HandTo2DController | MetaController | (ninguno) |
| **Función** | Personaje jugable | Detectar victoria | Obstáculos |

---

## ?? Configuración Detallada por Objeto

### 1. PERSONAJE (Jugador)

| Propiedad | Valor | ¿Por qué? |
|-----------|-------|-----------|
| **Tag** | `Player` | Para que la meta lo identifique |
| **Rigidbody2D ? Body Type** | `Dynamic` | Permite física y colisiones |
| **Rigidbody2D ? Gravity Scale** | `0` | No queremos gravedad (top-down) |
| **Rigidbody2D ? Collision Detection** | `Continuous` | Detecta colisiones rápidas |
| **Rigidbody2D ? Constraints** | `Freeze Rotation` | No gira el sprite |
| **Rigidbody2D ? Interpolation** | `Interpolate` | Movimiento suave |
| **Collider2D ? Is Trigger** | `? NO` | Debe chocar físicamente con paredes |
| **Layer** | `Default` | Capa estándar de Unity |

**Scripts:**
- `HandTo2DUsingKinectManager` (o uno de los alternativos)

**Resultado:** El personaje se mueve con física, choca con paredes, y puede entrar en triggers (la meta).

---

### 2. META (Objetivo)

| Propiedad | Valor | ¿Por qué? |
|-----------|-------|-----------|
| **Name** | `Meta` | El script lo busca por nombre |
| **Tag** | `Goal` (opcional) | Alternativa para detección |
| **Rigidbody2D** | ? NO necesita | Es estático |
| **Collider2D ? Is Trigger** | `? SÍ` | Debe detectar entrada sin bloquear |
| **Collider2D ? Size** | Cubrir área de meta | Para fácil detección |
| **Layer** | `Default` | Capa estándar de Unity |

**Scripts:**
- `MetaController`
  - `messageText`: TextMeshProUGUI asignado
  - `personaje`: GameObject del personaje (opcional)

**Resultado:** Cuando el personaje entra, dispara `OnTriggerEnter2D` y muestra el mensaje.

---

### 3. PAREDES (Laberinto)

| Propiedad | Valor | ¿Por qué? |
|-----------|-------|-----------|
| **Rigidbody2D** | ? NO necesita | Son estáticas |
| **Collider2D Type** | `Polygon Collider 2D` | Sigue la forma del sprite |
| **Collider2D ? Is Trigger** | `? NO` | Deben bloquear el movimiento |
| **Layer** | `Default` | Capa estándar de Unity |

**Scripts:** Ninguno necesario

**Resultado:** El personaje no puede atravesarlas, se detiene al chocar.

---

## ?? Matriz de Interacciones

### Personaje vs Otros Objetos

| Personaje + | Tipo de Collider | Is Trigger | Resultado |
|-------------|------------------|------------|-----------|
| **Pared** | Polygon 2D | ? NO | ? Colisión física (se detiene) |
| **Meta** | Box/Circle 2D | ? SÍ | ? OnTriggerEnter2D (detecta) |
| **Otro Personaje** | Capsule 2D | ? NO | ? Colisión física (rebota) |

---

## ?? Physics 2D Matrix (Edit ? Project Settings ? Physics 2D)

Verifica que la matriz de colisiones permita:

```
           Default  Player  Goal  Walls
Default       ?      ?     ?     ?
Player        ?      ?     ?     ?
Goal          ?      ?     ?     ?
Walls         ?      ?     ?     ?
```

**Nota:** Si usas las capas predeterminadas (Default para todo), no necesitas cambiar nada.

---

## ?? Configuración Visual Recomendada

### Personaje
```
???????????
?  ( ?° ?? ?°) ?  ? Sprite
?    ?    ?  ? Capsule Collider (visible en Scene)
???????????
   Tag: Player
   Collider: NO trigger
```

### Meta
```
???????????
?    ??   ?  ? Sprite (opcional)
?   [ ]   ?  ? Box Collider (visible como línea verde punteada)
???????????
   Name: "Meta"
   Collider: SÍ trigger
```

### Paredes
```
???????????
?         ?  ? Sprite del laberinto
?  ???    ?
?    ?    ?  ? Polygon Colliders (visibles como líneas verdes)
?         ?
???????????
   Colliders: NO trigger
```

---

## ? Tipos de Colisiones en Unity 2D

### Collision (OnCollisionEnter2D)
- **Requiere:** Ambos objetos con Collider, al menos uno con Rigidbody2D
- **Ninguno es Trigger:** ??
- **Resultado:** Colisión física, se bloquean mutuamente
- **Uso:** Personaje vs Paredes

### Trigger (OnTriggerEnter2D)
- **Requiere:** Ambos objetos con Collider, al menos uno con Rigidbody2D
- **Al menos uno es Trigger:** ?
- **Resultado:** Detección sin física, no se bloquean
- **Uso:** Personaje vs Meta, Power-ups, Zonas

---

## ?? Tests de Verificación

### Test 1: ¿El personaje tiene física correcta?
```
Condiciones:
? Rigidbody2D presente
? Body Type = Dynamic
? Collider NO es trigger

Prueba:
Mover el personaje hacia una pared

Resultado esperado:
? NO atraviesa la pared
? Se detiene al chocar
? Consola: "Colisión detectada con: Pared"
```

### Test 2: ¿La meta detecta correctamente?
```
Condiciones:
? Meta tiene Collider
? Meta IS Trigger = true
? Personaje tiene Collider NO trigger
? Personaje tiene tag "Player"

Prueba:
Mover el personaje dentro de la meta

Resultado esperado:
? Personaje entra (no se bloquea)
? Aparece mensaje "¡Has llegado a la meta!"
? Consola: "¡Victoria alcanzada!"
```

### Test 3: ¿Las paredes bloquean?
```
Condiciones:
? Paredes tienen Polygon Collider
? Paredes NO son trigger
? Personaje tiene Rigidbody2D Dynamic

Prueba:
Intentar mover el personaje a través de una pared

Resultado esperado:
? NO puede atravesar
? Se detiene en la pared
? El sprite no "entra" en la pared
```

---

## ?? Debugging Visual en Scene View

### Cómo ver los colliders en Unity:

1. **Selecciona el objeto** en la Jerarquía
2. En la Scene View, los colliders se ven como:
   - **Verde sólido** ? Collider normal (físico)
   - **Verde punteado** ? Collider con "Is Trigger"
3. Si no ves nada verde:
   - Botón **Gizmos** (esquina superior derecha de Scene)
   - Asegúrate de que esté activo

### Ejemplo Visual:

```
SCENE VIEW:

Personaje (verde sólido):
    ?????
    ? • ?  ? Collider visible
    ?????

Meta (verde punteado):
    ?????
    ? ???  ? Trigger visible
    ?????

Paredes (verde sólido):
    ???????
    ?     ?  ? Polygon visible
    ???????
```

---

## ?? Configuración Rápida (Copy-Paste Mental)

### Si estás configurando desde cero:

**1. Personaje:**
- Add Component ? Rigidbody 2D
  - Gravity Scale: 0
  - Collision Detection: Continuous
  - Constraints: Freeze Rotation Z
- Add Component ? Capsule Collider 2D
  - Is Trigger: ? NO
- Tag: Player
- Add Component ? HandTo2DUsingKinectManager

**2. Meta:**
- Add Component ? Box Collider 2D
  - Is Trigger: ? SÍ
- Rename to: "Meta"
- Add Component ? MetaController
  - Assign messageText

**3. Paredes:**
- Add Component ? Polygon Collider 2D
  - Is Trigger: ? NO
- (Ya está si usaste sprite con collider automático)

---

## ?? Referencia Rápida

### ¿Cuándo usar Is Trigger?

| Objeto | Is Trigger | Razón |
|--------|------------|-------|
| Personaje | ? NO | Necesita física para chocar |
| Paredes | ? NO | Deben bloquear |
| Meta | ? SÍ | Solo detectar, no bloquear |
| Power-ups | ? SÍ | Recoger sin bloquear |
| Zonas de daño | ? SÍ | Detectar entrada |
| Plataformas | ? NO | Soportar peso |

---

**?? Regla de oro:**

> Si quieres que **BLOQUEE** ? `Is Trigger = NO`  
> Si quieres que **DETECTE** ? `Is Trigger = SÍ`

---

? **Todo configurado correctamente si:**
- Personaje choca con paredes ?
- Personaje entra en meta ?
- Aparece mensaje al llegar ?
- Consola muestra logs apropiados ?

