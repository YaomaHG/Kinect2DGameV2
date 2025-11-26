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


    void Start()
    {
        kinect = KinectManager.Instance;

        if (targetObject == null)
            targetObject = this.transform;

        spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
        rb = targetObject.GetComponent<Rigidbody2D>();

        // Validaci�n y configuraci�n del Rigidbody2D
        if (rb == null)
        {
            Debug.LogError("�El personaje necesita un Rigidbody2D! Agreg�ndolo autom�ticamente...");
            rb = targetObject.gameObject.AddComponent<Rigidbody2D>();
        }

        // Configuraci�n �ptima para colisiones en laberinto
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;  // Sin gravedad para movimiento 2D top-down
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;  // Evitar rotaci�n
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;  // Mejor detecci�n de colisiones
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;  // Movimiento m�s suave

        if (messageText != null)
            messageText.text = "";
    }


    void Update()
    {
        // Si ya lleg� a la meta, detener control
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

        // PEQUE�A magnitud = mano quieta
        if (dir2.magnitude < minDirectionMagnitude)
        {
            spriteRenderer.sprite = idleSprite;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Normalizamos para evitar valores raros del Kinect
        Vector2 nd = dir2.normalized;

        Vector2 finalDir = Vector2.zero;

        // --- 4 DIRECCIONES FINALES CON ZONA MUERTA REAL ---

        // Horizontal dominante
        if (Mathf.Abs(nd.x) > Mathf.Abs(nd.y))
        {
            if (nd.x > horizontalDeadzone)
            {
                finalDir = Vector2.right;
                spriteRenderer.sprite = rightSprite;
            }
            else if (nd.x < -horizontalDeadzone)
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
            if (nd.y > verticalDeadzone)
            {
                finalDir = Vector2.up;
                spriteRenderer.sprite = upSprite;
            }
            else if (nd.y < -verticalDeadzone)
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
        // Mantener Z fijo despu�s de todos los c�lculos de f�sica
        Vector3 p = targetObject.position;
        if (p.z != fixedZ)
        {
            p.z = fixedZ;
            targetObject.position = p;
        }
    }


    // META SIN TAG - Detecta colisi�n con trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Meta")
        {
            reachedGoal = true;
            rb.linearVelocity = Vector2.zero;
            
            if (messageText != null)
                messageText.text = "�Lo lograste, felicidades!";

            Debug.Log("Jugador lleg� a la meta");
        }
    }

    // Opcional: Detectar colisiones con paredes para debug
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Colisi�n detectada con: " + collision.gameObject.name);
    }
}
