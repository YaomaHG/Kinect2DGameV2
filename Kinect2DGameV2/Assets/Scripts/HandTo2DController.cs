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

    // Mensaje
    public TextMeshProUGUI messageText;


    void Start()
    {
        kinect = KinectManager.Instance;

        if (targetObject == null)
            targetObject = this.transform;

        spriteRenderer = targetObject.GetComponent<SpriteRenderer>();

        if (messageText != null)
            messageText.text = "";
    }


    void Update()
    {
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
            return;
        }

        Vector3 handPos = kinect.GetJointPosition(userId, handIndex);

        if (!kinect.IsJointTracked(userId, elbowIndex))
        {
            spriteRenderer.sprite = idleSprite;
            return;
        }

        Vector3 elbowPos = kinect.GetJointPosition(userId, elbowIndex);
        Vector3 direction = handPos - elbowPos;

        Vector2 dir2 = new Vector2(direction.x, direction.y);

        // PEQUEÑA magnitud = mano quieta
        if (dir2.magnitude < minDirectionMagnitude)
        {
            spriteRenderer.sprite = idleSprite;
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
                return;
            }
        }

        // Movimiento final
        Rigidbody2D rb = targetObject.GetComponent<Rigidbody2D>();
        rb.MovePosition(rb.position + finalDir * moveSpeed * Time.deltaTime);


        // Mantener Z
        Vector3 p = targetObject.position;
        p.z = fixedZ;
        targetObject.position = p;
    }


    // META SIN TAG
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Meta")
        {
            if (messageText != null)
                messageText.text = "¡Lo lograste, felicidades!";

            Debug.Log("Jugador llegó a la meta");
        }
    }
}
