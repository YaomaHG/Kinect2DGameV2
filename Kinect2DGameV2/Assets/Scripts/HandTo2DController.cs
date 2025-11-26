using UnityEngine;
using TMPro;

public class HandTo2DUsingKinectManager : MonoBehaviour
{
    public Transform targetObject;
    public bool useRightHand = true;
    public float moveSpeed = 3.0f;
    public float minDirectionMagnitude = 0.08f;
    public float fixedZ = 0f;

    KinectManager kinect;

    // Zonas muertas REALMENTE eficaces
    public float verticalDeadzone = 0.25f;      // ZONA MUERTA GRANDE PARA EVITAR QUE BAJE
    public float horizontalDeadzone = 0.15f;

    // sprites
    public Sprite idleSprite;
    public Sprite upSprite;
    public Sprite downSprite;
    public Sprite leftSprite;
    public Sprite rightSprite;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // Mensaje
    public TextMeshProUGUI messageText;

    // Control de meta
    private bool reachedGoal = false;

    // NUEVO: Para reset de posición
    private Vector3 initialPosition;


    void Start()
    {
        kinect = KinectManager.Instance;

        if (targetObject == null)
            targetObject = this.transform;

        // NUEVO: Guardar posición inicial
        initialPosition = targetObject.position;

        spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        rb = targetObject.GetComponent<Rigidbody2D>();

        // Validación y configuración del Rigidbody2D
        if (rb == null)
        {
            Debug.LogError("¡El personaje necesita un Rigidbody2D! Agregándolo automáticamente...");
            rb = targetObject.gameObject.AddComponent<Rigidbody2D>();
        }

        // Configuración óptima para colisiones en laberinto
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;  // Sin gravedad para movimiento 2D top-down
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;  // Evitar rotación
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // Mejor detección de colisiones
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;  // Movimiento más suave

        // IMPORTANTE: Asegurarse de que el personaje tenga un collider NO-trigger para paredes
        Collider2D col = targetObject.GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("[HandTo2DController] El personaje necesita un Collider2D. Agregando CapsuleCollider2D...");
            col = targetObject.gameObject.AddComponent<CapsuleCollider2D>();
        }

        // El collider del personaje NO debe ser trigger (para chocar con paredes)
        if (col.isTrigger)
        {
            Debug.LogWarning("[HandTo2DController] El collider del personaje no debe ser Trigger (para colisiones con paredes). Desactivando Trigger...");
            col.isTrigger = false;
        }

        // Agregar tag "Player" si no lo tiene (para que la meta lo detecte)
        if (!targetObject.CompareTag("Player"))
        {
            try
            {
                targetObject.tag = "Player";
                Debug.Log("[HandTo2DController] Tag 'Player' asignado al personaje.");
            }
            catch
            {
                Debug.LogWarning("[HandTo2DController] No se pudo asignar tag 'Player'. Asegúrate de que el tag existe en el proyecto.");
            }
        }

        if (messageText != null)
            messageText.text = "";
    }


    void Update()
    {
        // Si ya llegó a la meta, detener control
        if (reachedGoal)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (kinect == null) return;
        if (!kinect.IsInitialized() || !kinect.IsUserDetected()) return;

        uint userId = kinect.GetPlayer1ID();
        if (userId == 0) userId = kinect.GetPlayer2ID();
        if (userId == 0) return;

        if (!kinect.IsPlayerCalibrated(userId)) return;

        // NUEVO: Usar configuración de mano del sistema de accesibilidad si existe
        if (AccessibilityConfig.Instance != null)
        {
            useRightHand = AccessibilityConfig.Instance.useRightHand;
        }

        int handIndex = useRightHand ?
            (int)KinectWrapper.NuiSkeletonPositionIndex.HandRight :
            (int)KinectWrapper.NuiSkeletonPositionIndex.HandLeft;

        int elbowIndex = useRightHand ?
            (int)KinectWrapper.NuiSkeletonPositionIndex.ElbowRight :
            (int)KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft;

        if (!kinect.IsJointTracked(userId, handIndex))
        {
            spriteRenderer.sprite = idleSprite;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 handPos = kinect.GetJointPosition(userId, handIndex);

        if (!kinect.IsJointTracked(userId, elbowIndex))
        {
            spriteRenderer.sprite = idleSprite;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector3 elbowPos = kinect.GetJointPosition(userId, elbowIndex);
        Vector3 direction = handPos - elbowPos;

        Vector2 dir2 = new Vector2(direction.x, direction.y);

        // PEQUEÑA magnitud = mano quieta
        if (dir2.magnitude < minDirectionMagnitude)
        {
            spriteRenderer.sprite = idleSprite;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Normalizamos para evitar valores raros del Kinect
        Vector2 nd = dir2.normalized;

        // --- NUEVO: APLICAR CONFIGURACIONES DE ACCESIBILIDAD ---
        float processedX = nd.x;
        float processedY = nd.y;

        if (AccessibilityConfig.Instance != null)
        {
            processedX = AccessibilityConfig.Instance.ApplySensitivityAndInversion(nd.x, true);
            processedY = AccessibilityConfig.Instance.ApplySensitivityAndInversion(nd.y, false);
        }

        Vector2 processedDir = new Vector2(processedX, processedY);

        // Renormalizar después de aplicar sensibilidad
        if (processedDir.magnitude > 0.01f)
        {
            processedDir = processedDir.normalized;
        }

        Vector2 finalDir = Vector2.zero;

        // --- 4 DIRECCIONES FINALES CON ZONA MUERTA REAL ---

        // Horizontal dominante
        if (Mathf.Abs(processedDir.x) > Mathf.Abs(processedDir.y))
        {
            if (processedDir.x > horizontalDeadzone)
            {
                finalDir = Vector2.right;
                spriteRenderer.sprite = rightSprite;
            }
            else if (processedDir.x < -horizontalDeadzone)
            {
                finalDir = Vector2.left;
                spriteRenderer.sprite = leftSprite;
            }
            else
            {
                spriteRenderer.sprite = idleSprite;
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }
        else  // Vertical dominante
        {
            if (processedDir.y > verticalDeadzone)
            {
                finalDir = Vector2.up;
                spriteRenderer.sprite = upSprite;
            }
            else if (processedDir.y < -verticalDeadzone)
            {
                finalDir = Vector2.down;
                spriteRenderer.sprite = downSprite;
            }
            else
            {
                spriteRenderer.sprite = idleSprite;
                rb.linearVelocity = Vector2.zero;
                return;
            }
        }

        // Movimiento usando velocidad - RESPETA COLISIONES
        rb.linearVelocity = finalDir * moveSpeed;
    }

    void LateUpdate()
    {
        // Mantener Z fijo después de todos los cálculos de física
        Vector3 p = targetObject.position;
        if (p.z != fixedZ)
        {
            p.z = fixedZ;
            targetObject.position = p;
        }
    }


    // META SIN TAG - Detecta colisión con trigger (método alternativo si no usas MetaController)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (reachedGoal) return;

        if (other.gameObject.name == "Meta" || other.CompareTag("Goal"))
        {
            reachedGoal = true;
            rb.linearVelocity = Vector2.zero;

            if (messageText != null)
                messageText.text = "¡Lo lograste, felicidades!";

            Debug.Log("[HandTo2DController] Jugador llegó a la meta: " + other.gameObject.name);
        }
    }

    // Método público para que MetaController pueda notificar externamente
    public void ReachGoal()
    {
        if (reachedGoal) return;

        reachedGoal = true;
        rb.linearVelocity = Vector2.zero;

        if (messageText != null)
            messageText.text = "¡Lo lograste, felicidades!";

        Debug.Log("[HandTo2DController] Meta alcanzada (notificado externamente)");
    }

    // NUEVO: Método público para resetear posición del personaje
    public void ResetPlayerPosition()
    {
        if (reachedGoal) return;

        rb.linearVelocity = Vector2.zero;

        // Buscar posición inicial (puedes ajustar esto según tu escena)
        GameObject startPoint = GameObject.Find("StartPoint");
        if (startPoint != null)
        {
            targetObject.position = startPoint.transform.position;
            Debug.Log("[HandTo2DController] Posición reseteada usando StartPoint");
        }
        else
        {
            // Usar la posición inicial guardada en Start()
            targetObject.position = initialPosition;
            Debug.Log("[HandTo2DController] Posición reseteada usando posición inicial guardada");
        }
    }

    // Detectar colisiones con paredes para debug
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("[HandTo2DController] Colisión detectada con: " + collision.gameObject.name);
    }
}