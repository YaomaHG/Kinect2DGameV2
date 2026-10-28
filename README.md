# Kinect2DGameV2

Juego 2D de laberinto controlado mediante los movimientos de la mano capturados por un sensor Kinect. El jugador debe guiar al personaje por el escenario, evitar las paredes y alcanzar la meta. El proyecto también incluye una configuración de accesibilidad para elegir la mano, ajustar la sensibilidad y hacer inversiones de los ejes.

## Características

- Movimiento del personaje mediante la dirección de la mano respecto al codo.
- Seguimiento de la mano derecha o izquierda.
- Movimiento en cuatro direcciones con zonas muertas para reducir movimientos accidentales.
- Colisiones 2D con las paredes del laberinto.
- Detección de llegada a la meta y mensaje de victoria.
- Panel de accesibilidad con:
  - Sensibilidad horizontal y vertical.
  - Selección de mano derecha o izquierda.
  - Inversión del eje horizontal y vertical.
  - Restablecimiento de la posición del personaje.
- Persistencia de las preferencias mediante `PlayerPrefs`.
- Herramientas de depuración para inspeccionar usuarios y articulaciones detectadas por Kinect.
- Código y escenas de demostración incluidos en `Assets/KinectSDK/KinectDemos`.

## Requisitos

- Unity **6000.2.6f1**.
- Sensor Kinect compatible con la integración incluida en el proyecto.
- Controladores y runtime del Kinect instalados en el equipo.
- En la práctica, la integración nativa depende de `Kinect.dll`, por lo que debe ejecutarse en una plataforma compatible con ese runtime.
- Para ejecutar el control por movimiento se necesita que el sensor detecte y calibre al usuario.

El proyecto contiene los scripts de integración (`KinectManager`, `KinectWrapper`, gestos, filtros y demos) dentro de `Kinect2DGameV2/Assets/KinectSDK`.

## Estructura del repositorio

```text
.
├── README.md
└── Kinect2DGameV2/
    ├── Assets/
    │   ├── KinectSDK/          # Integración, wrapper y demos de Kinect
    │   ├── Scenes/             # Escena principal
    │   ├── Scripts/            # Control del jugador y utilidades
    │   ├── Sprites/            # Recursos gráficos
    │   ├── Accesibility*.cs    # Configuración y UI de accesibilidad
    │   └── MetaController.cs   # Lógica de la meta y victoria
    ├── Packages/               # Dependencias de Unity
    ├── ProjectSettings/        # Configuración del proyecto
    └── *.md                    # Guías técnicas y de diagnóstico
```

## Abrir el proyecto

1. Instala Unity Hub y Unity `6000.2.6f1`.
2. Clona el repositorio:

   ```bash
   git clone https://github.com/YaomaHG/Kinect2DGameV2.git
   ```

3. En Unity Hub selecciona **Add/Open** y elige la carpeta:

   ```text
   Kinect2DGameV2/Kinect2DGameV2
   ```

4. Espera a que Unity importe los paquetes y assets.
5. Abre `Assets/Scenes/SampleScene.unity`.
6. Conecta y enciende el Kinect antes de pulsar **Play**.

La escena `SampleScene.unity` está configurada como la escena inicial de compilación.

## Cómo jugar

1. Colócate frente al sensor hasta que Kinect detecte y calibre tu cuerpo.
2. Selecciona la mano que utilizarás en el panel de accesibilidad.
3. Extiende la mano respecto al codo para indicar una dirección:
   - Mano hacia la derecha: mover a la derecha.
   - Mano hacia la izquierda: mover a la izquierda.
   - Mano hacia arriba: mover hacia arriba.
   - Mano hacia abajo: mover hacia abajo.
4. El personaje se detiene cuando la articulación no está siendo rastreada o el movimiento queda dentro de la zona muerta.
5. Llega al objeto `Meta` para mostrar el mensaje de victoria y detener el movimiento.

El personaje usa un `Rigidbody2D` sin gravedad, un `Collider2D` no trigger y detección continua para interactuar con las paredes del laberinto.

## Accesibilidad y configuración

La configuración se gestiona mediante `AccessibilityConfig` y su interfaz `AccessibilityUIManager`.

| Opción | Descripción |
|---|---|
| Sensibilidad horizontal | Ajusta la respuesta del movimiento en el eje X. |
| Sensibilidad vertical | Ajusta la respuesta del movimiento en el eje Y. |
| Usar mano derecha | Cambia entre la mano derecha y la izquierda. |
| Invertir eje horizontal | Invierte la dirección horizontal. |
| Invertir eje vertical | Invierte la dirección vertical. |
| Restablecer | Devuelve el personaje al `StartPoint` o a su posición inicial. |

Las sensibilidades están configuradas en la escena entre `0.5x` y `3x`, con valor inicial `1x`. Los cambios se guardan automáticamente en `PlayerPrefs`.

## Componentes principales

- `Assets/Scripts/HandTo2DController.cs`: obtiene la mano y el codo del usuario, calcula la dirección y mueve el personaje.
- `Assets/AccesibilityConfig.cs`: almacena sensibilidad, mano seleccionada e inversión de ejes.
- `Assets/AccesibilityUIManager.cs`: conecta los controles de la interfaz con la configuración.
- `Assets/MetaController.cs`: configura el trigger de la meta, muestra el mensaje de victoria y detiene al jugador.
- `Assets/Scripts/KinectHandAutoDetectMover.cs`: alternativa de diagnóstico que busca automáticamente usuario y articulación utilizables.
- `Assets/Scripts/KinectFullScan.cs`: escaneo de usuarios y articulaciones para depuración.
- `Assets/Scripts/KinectDebugProbe.cs` y `KinectDebugProbe2.cs`: mensajes de diagnóstico del estado del Kinect.
- `Assets/Scripts/TestMensajeVictoria.cs`: prueba independiente del mensaje de victoria.
- `Assets/KinectSDK/KinectScripts/`: `KinectManager`, `KinectWrapper`, filtros, gestos y controladores de ejemplo.

## Solución de problemas

### El personaje no se mueve

- Comprueba que el Kinect esté conectado y reconocido por el sistema.
- Verifica que el runtime/controlador del sensor esté instalado.
- Asegúrate de estar dentro del campo de visión y de que el usuario haya sido calibrado.
- Revisa la consola de Unity en busca de mensajes de `KinectManager`, `KinectFullScan` o `KinectHandAutoDetectMover`.
- Confirma que la mano y el codo seleccionados estén visibles para el sensor.

### El personaje atraviesa paredes o no llega a la meta

- El personaje debe tener un `Collider2D` no trigger y un `Rigidbody2D`.
- Las paredes del laberinto deben tener sus `Collider2D` configurados.
- El objeto de llegada debe llamarse `Meta`, tener un `Collider2D` trigger y estar asociado a `MetaController`.
- Comprueba que el personaje tenga el tag `Player`; el controlador intenta asignarlo automáticamente.

### No aparece el mensaje de victoria

- Comprueba que exista un `TextMeshProUGUI` llamado, preferiblemente, `MensajeVictoria`.
- Verifica que el texto pertenezca a un `Canvas`.
- Asigna el campo `messageText` de `MetaController` si la detección automática no encuentra el texto.
- Para probarlo sin Kinect, consulta `GUIA_TEST_MENSAJE.md`.

## Documentación adicional

- `CHECKLIST_META.md`: comprobaciones de configuración de la meta.
- `GUIA_TEST_MENSAJE.md`: prueba del mensaje de victoria sin mover al personaje.
- `INFORME_CAMBIOS_COLISIONES.md`: cambios y recomendaciones de colisiones.
- `TABLA_CONFIGURACION_COLLIDERS.md`: configuración de colliders del escenario.
- `RESUMEN_SOLUCION_META.md`: resumen de la solución de la meta.
- `SOLUCION_MENSAJE_VICTORIA.md`: diagnóstico del mensaje de victoria.
- `SOLUCION_PROBLEMA_META.md`: resolución de problemas relacionados con la meta.

## Desarrollo

Los cambios de código se realizan dentro de `Kinect2DGameV2/Assets`. Para mantener la integridad del proyecto Unity, deben conservarse los archivos `.meta` junto a sus assets y scripts correspondientes.

No hay una suite de pruebas automatizadas configurada para el juego. La validación principal se realiza ejecutando `SampleScene` en el editor, comprobando la detección del Kinect, el movimiento, las colisiones, la configuración de accesibilidad y la llegada a la meta.

## Licencia

Este repositorio no incluye actualmente un archivo de licencia. Los derechos de uso y distribución de los assets, del código propio y de la integración de Kinect deben confirmarse con los autores y las licencias originales de las dependencias incluidas.
