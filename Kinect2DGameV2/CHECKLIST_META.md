# ? CHECKLIST RÁPIDO - CONFIGURACIÓN DE LA META

## ?? Configuración en Unity (Paso a Paso)

### 1?? PERSONAJE

Selecciona tu personaje en la jerarquía y verifica:

```
???????????????????????????????????????????????
? Inspector - Personaje                       ?
???????????????????????????????????????????????
? Tag: Player                          ?      ?
?                                              ?
? Transform                                    ?
?   Position: (X, Y, 0)                        ?
?                                              ?
? Sprite Renderer                              ?
?   Sprite: [tu sprite]                        ?
?   Order in Layer: 1                          ?
?                                              ?
? Rigidbody 2D                          ?     ?
?   Body Type: Dynamic                         ?
?   Gravity Scale: 0                           ?
?   Collision Detection: Continuous            ?
?   Constraints: Freeze Rotation (Z)           ?
?                                              ?
? Capsule Collider 2D                   ?     ?
?   Is Trigger: [ ] NO MARCADO          ?     ?
?   Size: (ajustado al sprite)                 ?
?                                              ?
? Script: HandTo2DUsingKinectManager    ?     ?
?   Target Object: (arrastra el personaje)     ?
?   Move Speed: 3                              ?
?   Message Text: (arrastra el TextMeshPro)    ?
?   [sprites configurados...]                  ?
???????????????????????????????????????????????
```

**?? IMPORTANTE:**
- [ ] Tag = "Player"
- [ ] Rigidbody2D presente
- [ ] Collider **NO** es Trigger

---

### 2?? META

Selecciona el objeto "Meta" en la jerarquía y verifica:

```
???????????????????????????????????????????????
? Inspector - Meta                             ?
???????????????????????????????????????????????
? Tag: Goal (opcional)                         ?
?                                              ?
? Transform                                    ?
?   Position: (X_final, Y_final, 0)            ?
?                                              ?
? Sprite Renderer (opcional)                   ?
?   Sprite: [sprite de meta]                   ?
?   Color: (verde/dorado)                      ?
?                                              ?
? Box Collider 2D                       ?     ?
?   Is Trigger: [?] MARCADO             ?     ?
?   Size: (cubrir área de meta)                ?
?                                              ?
? Script: MetaController                ?     ?
?   Message Text: (arrastra TextMeshPro) ?    ?
?   Personaje: (arrastra personaje)     ??     ?
?              (opcional si tiene tag Player)  ?
???????????????????????????????????????????????
```

**?? IMPORTANTE:**
- [ ] Nombre = "Meta" (exacto)
- [ ] Collider **SÍ** es Trigger
- [ ] messageText asignado

---

### 3?? PAREDES DEL LABERINTO

Selecciona las paredes y verifica:

```
???????????????????????????????????????????????
? Inspector - Pared/Laberinto                  ?
???????????????????????????????????????????????
? Sprite Renderer                              ?
?   Sprite: [imagen del laberinto]             ?
?   Order in Layer: 0                          ?
?                                              ?
? Polygon Collider 2D                   ?     ?
?   Is Trigger: [ ] NO MARCADO          ?     ?
?   [múltiples polígonos si es necesario]      ?
???????????????????????????????????????????????
```

**?? IMPORTANTE:**
- [ ] Polygon Collider 2D presente
- [ ] **NO** es Trigger

---

### 4?? INTERFAZ DE USUARIO (UI)

Verifica que tengas un objeto de texto en el Canvas:

```
???????????????????????????????????????????????
? Hierarchy                                    ?
???????????????????????????????????????????????
? Canvas                                       ?
? ?? MessageText (TextMeshPro - Text)   ?    ?
? ?  ?? Inspector:                             ?
? ?     Text: ""                               ?
? ?     Font Size: 36                          ?
? ?     Color: White                           ?
? ?     Alignment: Center                      ?
? ?     Anchor: Top Center                     ?
???????????????????????????????????????????????
```

---

## ?? PRUEBAS

### Test 1: Colisiones con Paredes
1. Presiona Play
2. Mueve el personaje hacia una pared
3. **? Esperado:** El personaje se detiene
4. **? Error:** El personaje atraviesa ? Revisa que el collider del personaje NO sea trigger

**Consola debe mostrar:**
```
[HandTo2DController] Colisión detectada con: Pared
```

---

### Test 2: Detección de Meta
1. Mueve el personaje hasta la meta
2. **? Esperado:** 
   - Aparece mensaje "¡Has llegado a la meta!"
   - El personaje se detiene
3. **? Error:** No aparece mensaje ? Revisa configuración abajo

**Consola debe mostrar uno o ambos:**
```
[MetaController] ¡Victoria alcanzada! Objeto: Personaje
[HandTo2DController] Jugador llegó a la meta: Meta
```

---

## ?? TROUBLESHOOTING

### ? Problema: "No aparece el mensaje al llegar a la meta"

**Soluciones en orden:**

1. **Verifica el nombre del objeto:**
   - Debe llamarse exactamente **"Meta"** (con M mayúscula)
   - O tener el tag "Goal"

2. **Verifica los colliders:**
   ```
   Personaje:
   - Tiene Collider2D: ?
   - Is Trigger: ? NO
   
   Meta:
   - Tiene Collider2D: ?
   - Is Trigger: ? SÍ
   ```

3. **Verifica el script MetaController:**
   - `messageText` está asignado: ?
   - El script está activo (checkbox marcado): ?

4. **Verifica la consola:**
   - ¿Aparecen mensajes de debug?
   - Si no, el trigger no se está disparando

5. **Revisa la Matrix de Colisiones:**
   - Edit ? Project Settings ? Physics 2D
   - Matriz de colisiones Layer
   - Default debe poder colisionar con Default

---

### ? Problema: "El mensaje aparece dos veces"

**Causa:** Hay dos sistemas de detección activos

**Solución (opcional):**
Si quieres solo un mensaje, comenta estas líneas en `HandTo2DController.cs`:

```csharp
// private void OnTriggerEnter2D(Collider2D other)
// {
//     if (reachedGoal) return;
//     ...
// }
```

---

### ? Problema: "El personaje atraviesa las paredes"

**Soluciones:**

1. **Verifica el Rigidbody2D:**
   - Body Type: Dynamic ?
   - Collision Detection: Continuous ?

2. **Verifica el collider del personaje:**
   - Is Trigger: ? NO

3. **Verifica las paredes:**
   - Tienen Polygon Collider 2D: ?
   - Is Trigger: ? NO

4. **Revisa la velocidad:**
   - Si `moveSpeed` > 10, puede atravesar
   - Usa valores entre 2-5

---

### ? Problema: "El personaje no se mueve"

**Soluciones:**

1. **Verifica el Kinect:**
   - ¿Está conectado y detectando?
   - Revisa la consola para mensajes del Kinect

2. **Verifica el Rigidbody2D:**
   - Body Type: Dynamic ?
   - Constraints: Solo Freeze Rotation Z

3. **Verifica el script:**
   - `targetObject` está asignado: ?
   - `moveSpeed` > 0: ?

---

## ?? CHECKLIST FINAL

Antes de probar, marca cada ítem:

### Personaje
- [ ] Tag "Player" asignado
- [ ] Rigidbody2D configurado (Dynamic, Gravity 0)
- [ ] Collider2D presente y **NO** es Trigger
- [ ] Script HandTo2DController presente
- [ ] `messageText` asignado en el script

### Meta
- [ ] Nombre del objeto es "Meta"
- [ ] Collider2D presente y **SÍ** es Trigger
- [ ] Script MetaController presente
- [ ] `messageText` asignado en el script
- [ ] `personaje` asignado (opcional)

### Paredes
- [ ] Polygon Collider 2D presente
- [ ] Colliders **NO** son Trigger

### UI
- [ ] Canvas presente
- [ ] TextMeshPro presente y activo
- [ ] Texto asignado en ambos scripts

### Proyecto
- [ ] Tag "Player" existe en el proyecto
- [ ] Tag "Goal" existe (opcional)
- [ ] Physics 2D Matrix permite colisiones Default-Default

---

## ? SI TODO ESTÁ BIEN

Deberías ver en la consola:

```
[HandTo2DController] Tag 'Player' asignado al personaje.
[MetaController] Meta configurada correctamente. Collider: BoxCollider2D | IsTrigger: True

... al mover el personaje ...

[HandTo2DController] Colisión detectada con: Pared

... al llegar a la meta ...

[MetaController] ¡Victoria alcanzada! Objeto: Personaje
[HandTo2DController] Jugador llegó a la meta: Meta
```

**Y en pantalla:**
```
??????????????????????????????
?                            ?
?  ¡Has llegado a la meta!  ?
?                            ?
??????????????????????????????
```

---

## ?? DIAGRAMA DE FLUJO

```
START
  ?
Personaje con Kinect
  ?
¿Se mueve? 
  ?? NO ? Revisar Kinect/Rigidbody
  ?? SÍ
      ?
¿Choca con paredes?
  ?? NO ? Revisar collider del personaje (NO trigger)
  ?? SÍ
      ?
¿Llega a la meta?
  ?? NO ? Continuar jugando
  ?? SÍ
      ?
¿Se dispara OnTriggerEnter2D?
  ?? NO ? Revisar collider de la meta (SÍ trigger)
  ?? SÍ
      ?
¿Aparece mensaje?
  ?? NO ? Revisar messageText asignado
  ?? SÍ
      ?
    ¡FUNCIONA! ?
```

---

**Última actualización:** 2024
**Archivos involucrados:**
- `Assets\MetaController.cs`
- `Assets\Scripts\HandTo2DController.cs`
- `Assets\Scripts\KinectHandAutoDetectMover.cs`
- `Assets\Scripts\KinectFullScan.cs`

