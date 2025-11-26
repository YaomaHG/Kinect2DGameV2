# ?? Guía Rápida: Solución al Problema de la Meta

## ? Problema Detectado

Después de implementar las colisiones físicas con `Rigidbody2D`, la meta dejó de funcionar.

### Causa Principal
El problema ocurre porque hay **dos sistemas de detección de meta** y la configuración de colliders puede estar incorrecta.

---

## ? Solución Implementada

### ?? Configuración Correcta

#### 1. **Personaje** (El objeto que controlas con Kinect)

```
GameObject: "Personaje" (o el nombre que uses)
?? Tag: "Player" ? (se asigna automáticamente ahora)
?? Rigidbody2D
?  ?? Body Type: Dynamic
?  ?? Gravity Scale: 0
?  ?? Collision Detection: Continuous
?  ?? Constraints: Freeze Rotation
?? Collider2D (CapsuleCollider2D o CircleCollider2D)
?  ?? Is Trigger: ? NO (para colisiones con paredes)
?? Script: HandTo2DUsingKinectManager
```

#### 2. **Meta** (El objeto que detecta la victoria)

```
GameObject: "Meta"
?? Tag: "Goal" (opcional)
?? Collider2D (BoxCollider2D, CircleCollider2D, o PolygonCollider2D)
?  ?? Is Trigger: ? SÍ (para detectar entrada sin bloquear)
?? Script: MetaController
   ?? messageText: (asignar TextMeshProUGUI)
   ?? personaje: (opcional - si no se asigna, detecta por tag)
```

#### 3. **Paredes del Laberinto**

```
GameObject: "Laberinto" o "Paredes"
?? Polygon Collider 2D
   ?? Is Trigger: ? NO (para bloquear movimiento)
```

---

## ?? Cambios Realizados

### En `MetaController.cs`:
? Verifica automáticamente que la meta tenga collider trigger  
? Detecta al personaje por:
   - Referencia directa (`personaje`)
   - Tag "Player"
   - Presencia de Rigidbody2D
? Detiene el personaje automáticamente al llegar  
? Evita múltiples detecciones con flag `goalReached`  

### En `HandTo2DController.cs`:
? Asigna automáticamente el tag "Player" al personaje  
? Verifica que el collider del personaje NO sea trigger  
? Detecta la meta por nombre ("Meta") o tag ("Goal")  
? Método público `ReachGoal()` para notificación externa  
? Sistema dual de detección (OnTriggerEnter2D + método público)  

---

## ?? Pasos para Verificar en Unity

### Paso 1: Verificar el Personaje
1. Selecciona tu personaje en la Jerarquía
2. En el Inspector, verifica:
   - [ ] **Tag** = "Player"
   - [ ] **Rigidbody2D** presente
   - [ ] **Collider2D** presente
   - [ ] **Collider2D ? Is Trigger** = ? DESMARCADO
   - [ ] Script `HandTo2DUsingKinectManager` presente

### Paso 2: Verificar la Meta
1. Selecciona el objeto "Meta" en la Jerarquía
2. En el Inspector, verifica:
   - [ ] **Collider2D** presente (BoxCollider2D, CircleCollider2D, etc.)
   - [ ] **Collider2D ? Is Trigger** = ? MARCADO
   - [ ] Script `MetaController` presente
   - [ ] `messageText` asignado (arrastra el objeto de UI)
   - [ ] `personaje` asignado (opcional, arrastra el personaje)

### Paso 3: Verificar las Paredes
1. Selecciona las paredes del laberinto
2. Verifica:
   - [ ] **Polygon Collider 2D** presente
   - [ ] **Is Trigger** = ? DESMARCADO

### Paso 4: Crear el Tag "Player" (si no existe)
1. Selecciona el personaje
2. En Inspector ? Tag ? "Add Tag..."
3. Presiona el botón "+"
4. Escribe "Player"
5. Guarda
6. Vuelve al personaje y asígnale el tag "Player"

### Paso 5: Probar
1. Presiona Play
2. Mueve el personaje con el Kinect
3. Verifica en la **Consola**:
   - Al chocar con paredes: `[HandTo2DController] Colisión detectada con: [nombre]`
   - Al llegar a la meta: `[MetaController] ¡Victoria alcanzada!`

---

## ?? Troubleshooting

### Problema: El personaje atraviesa la meta sin detectarla

**Solución:**
1. ? Verifica que la Meta tenga `Is Trigger = true`
2. ? Verifica que el Personaje tenga un Collider (NO trigger)
3. ? Verifica que el objeto se llame exactamente "Meta" O tenga tag "Player"
4. ? Revisa la consola para ver si hay mensajes de debug

### Problema: El personaje ya no choca con las paredes

**Solución:**
1. ? Verifica que el collider del Personaje NO sea trigger
2. ? Verifica que los colliders de las Paredes NO sean trigger
3. ? Verifica que el Rigidbody2D esté en modo `Dynamic`

### Problema: Se muestra el mensaje dos veces

**Solución:**
- Esto es normal, hay dos sistemas de detección (uno en el personaje, uno en la meta)
- Para usar solo uno, desactiva `OnTriggerEnter2D` en `HandTo2DController.cs`

### Problema: No aparece el mensaje en pantalla

**Solución:**
1. Verifica que `messageText` esté asignado en el Inspector (ambos scripts)
2. Verifica que el objeto TextMeshProUGUI esté activo en la escena
3. Verifica que el Canvas tenga `Canvas Scaler` configurado correctamente

---

## ?? Diagrama de Flujo

```
Personaje se mueve
    ?
¿Collider trigger?
    ?? NO ? Choca con paredes ?
    ?? SÍ ? Atraviesa paredes ?
    ?
Personaje entra en Meta
    ?
¿Meta tiene trigger?
    ?? SÍ ? OnTriggerEnter2D se dispara ?
    ?? NO ? No detecta entrada ?
    ?
MetaController detecta:
    ?? ¿Es el personaje asignado?
    ?? ¿Tiene tag "Player"?
    ?? ¿Tiene Rigidbody2D?
    ?
Si cualquiera es verdadero:
    ?? Muestra mensaje
    ?? Detiene al personaje
    ?? Marca como completado
```

---

## ?? Resumen de Configuración

| Objeto | Collider | Is Trigger | Script | Tag |
|--------|----------|------------|--------|-----|
| **Personaje** | ? Sí | ? NO | HandTo2DController | Player |
| **Meta** | ? Sí | ? SÍ | MetaController | Goal (opcional) |
| **Paredes** | ? Sí | ? NO | - | - |

---

## ? Verificación Final

Ejecuta este checklist:

- [ ] El personaje choca con las paredes (no las atraviesa)
- [ ] El personaje puede entrar en la meta
- [ ] Al entrar en la meta aparece el mensaje
- [ ] Al llegar a la meta el personaje se detiene
- [ ] La consola muestra mensajes de debug apropiados

Si todos los puntos están marcados, ¡la meta está funcionando correctamente! ??

---

**Fecha:** $(Get-Date -Format "yyyy-MM-dd HH:mm")  
**Archivos modificados:** 
- `Assets\MetaController.cs`
- `Assets\Scripts\HandTo2DController.cs`
