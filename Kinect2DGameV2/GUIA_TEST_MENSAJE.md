# ?? GUÍA DE PRUEBA RÁPIDA - Mensaje de Victoria

## ?? Objetivo
Probar que el mensaje de victoria funciona **SIN** necesidad de mover el personaje o usar el Kinect.

---

## ?? Pasos para Probar

### Paso 1: Agregar el Script de Prueba

1. **Crea un GameObject vacío:**
   - Click derecho en la Jerarquía
   - `Create Empty`
   - Renombrar a `TestMensaje`

2. **Agrega el script de prueba:**
   - Selecciona `TestMensaje`
   - En el Inspector, click `Add Component`
   - Busca `TestMensajeVictoria`
   - Agrégalo

3. **(Opcional) Asigna el texto:**
   - Si tienes `MensajeVictoria` en el Canvas
   - Arrástralo al campo `Message Text`
   - Si no lo asignas, el script lo buscará automáticamente

---

### Paso 2: Ejecutar la Prueba

1. **Presiona Play** ??

2. **Observa la Consola:**
   ```
   [TestMensaje] Script de prueba iniciado. Presiona ESPACIO...
   [TestMensaje] Encontrados X TextMeshProUGUI en la escena:
     - MensajeVictoria (Padre: Canvas)
   [TestMensaje] ¡Texto encontrado y asignado!: MensajeVictoria
   ```

3. **Presiona la tecla ESPACIO** ??
   - Debe aparecer en pantalla: "¡PRUEBA DE MENSAJE DE VICTORIA!"
   - En la consola verás información detallada

4. **Presiona H** para ocultar el mensaje

5. **Presiona I** para ver información técnica

---

## ?? Interpretando los Resultados

### ? Si el mensaje APARECE en pantalla:

**Conclusión:** El Canvas y el TextMeshProUGUI funcionan correctamente.

**Problema:** El MetaController no está activando el mensaje correctamente.

**Solución:** Revisa que:
- La meta tenga el collider como Trigger
- El personaje tenga tag "Player"
- El personaje esté llegando realmente a la meta

---

### ? Si el mensaje NO aparece en pantalla:

**Mira la consola y busca:**

#### Caso 1: "NO SE ENCONTRÓ ningún TextMeshProUGUI"
```
[TestMensaje] NO SE ENCONTRÓ ningún TextMeshProUGUI. Crea uno en el Canvas.
```

**Problema:** No existe ningún objeto de texto en la escena.

**Solución:**
1. Click derecho en el Canvas
2. `UI ? Text - TextMeshPro`
3. Renombrar a `MensajeVictoria`

---

#### Caso 2: Se encontró pero no se ve

**Mira los logs cuando presionas ESPACIO:**

```
[TestMensaje] Texto: ¡PRUEBA DE MENSAJE DE VICTORIA!
[TestMensaje] Activo: True
[TestMensaje] Enabled: True
[TestMensaje] Color: RGBA(1.000, 1.000, 1.000, 1.000)
[TestMensaje] Font Size: 48
```

**Si todos son correctos pero no se ve:**

1. **Presiona I** para ver más información
2. Busca en la consola:

```
Canvas padre: Canvas
Canvas activo: True
Render Mode: ScreenSpaceOverlay
```

**Posibles problemas:**

- **Posición:** Está fuera de la pantalla
  ```
  Posición: (9999, 9999, 0)  ? FUERA DE PANTALLA
  ```
  **Solución:** En el Inspector del texto, ponle posición (0, 0, 0)

- **Tamaño:** Es muy pequeño
  ```
  Tamaño: (0, 0)  ? DEMASIADO PEQUEÑO
  ```
  **Solución:** Tamaño mínimo (400, 100)

- **Sin Canvas:**
  ```
  NO hay Canvas padre!
  ```
  **Solución:** Mueve el texto dentro del Canvas

- **Font Asset null:**
  ```
  Font Asset: NULL
  ```
  **Solución:** Asigna una fuente en el Inspector

---

## ?? Checklist de Diagnóstico

Cuando presiones **I**, verifica estos valores:

| Propiedad | Valor Correcto | Si está mal |
|-----------|----------------|-------------|
| `Activo en jerarquía` | True | Activar el GameObject |
| `Componente enabled` | True | Marcar checkbox del componente |
| `Color` | RGBA(1, 1, 1, 1) | Cambiar a blanco opaco |
| `Alpha` | 1 | Cambiar a 1 |
| `Font Size` | >= 36 | Aumentar tamaño |
| `Font Asset` | No NULL | Asignar fuente |
| `Canvas padre` | Existe | Mover dentro del Canvas |
| `Canvas activo` | True | Activar el Canvas |
| `Render Mode` | ScreenSpaceOverlay | Cambiar en Canvas |

---

## ?? Controles del Test

| Tecla | Acción |
|-------|--------|
| **ESPACIO** | Mostrar mensaje de prueba |
| **H** | Ocultar mensaje |
| **I** | Mostrar información en consola |

---

## ?? Soluciones Rápidas

### Problema: "Activo: False"
```csharp
// En Unity:
Hierarchy ? Selecciona MensajeVictoria ? Marca el checkbox ?
```

### Problema: "Color RGBA(1, 1, 1, 0)" (Alpha = 0)
```csharp
// En Unity:
Inspector ? TextMeshPro ? Vertex Color ? Alpha = 255
```

### Problema: "Posición: (9999, 9999, 0)"
```csharp
// En Unity:
Inspector ? Rect Transform ? 
  Preset: Center-Middle
  Pos X: 0, Pos Y: 0
```

### Problema: "Font Asset: NULL"
```csharp
// En Unity:
Inspector ? TextMeshPro ? Font Asset ? 
  Selecciona "LiberationSans SDF" o cualquier fuente TMP
```

---

## ? Una Vez que Funcione

1. **Si el mensaje aparece con ESPACIO:**
   - El Canvas funciona ?
   - El TextMeshProUGUI funciona ?
   - El problema está en MetaController

2. **Revisa que el MetaController:**
   - Esté asignado a la Meta
   - Tenga el messageText asignado (o lo busque automáticamente)
   - El collider de la Meta sea Trigger
   - El personaje tenga tag "Player"

3. **Desactiva o elimina TestMensaje:**
   - Ya no lo necesitas
   - El MetaController ya debería funcionar

---

## ?? Captura de Pantalla para Verificar

**En el Game View, deberías ver:**

```
??????????????????????????????????????????
?                                        ?
?                                        ?
?                                        ?
?  ¡PRUEBA DE MENSAJE DE VICTORIA!     ?
?                                        ?
?                                        ?
?                                        ?
?                                        ?
?  PRUEBA DE MENSAJE:                   ?
?  ESPACIO = Mostrar mensaje             ?
?  H = Ocultar mensaje                   ?
?  I = Ver info en consola               ?
??????????????????????????????????????????
```

---

## ?? Si AÚN No Funciona

Copia TODA la salida de la consola cuando presiones **I** y pégala aquí:

```
[TestMensaje] ========== INFORMACIÓN DEL TEXTO ==========
[Pega aquí toda la información]
```

Con esa información podré decirte exactamente qué está mal.

---

**Fecha:** 2024  
**Script creado:** `Assets\Scripts\TestMensajeVictoria.cs`  
**Propósito:** Diagnóstico rápido del mensaje de victoria

