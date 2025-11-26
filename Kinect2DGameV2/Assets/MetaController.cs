using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MetaController : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public GameObject personaje;

    private bool goalReached = false;

    private void Start()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider2D>();
        }
        
        if (!col.isTrigger)
        {
            col.isTrigger = true;
        }

        if (messageText == null)
        {
            TextMeshProUGUI[] allTexts = GameObject.FindObjectsOfType<TextMeshProUGUI>();
            
            foreach (TextMeshProUGUI txt in allTexts)
            {
                if (txt.gameObject.name.Contains("Mensaje") || txt.gameObject.name.Contains("Victoria"))
                {
                    messageText = txt;
                    break;
                }
            }
            
            if (messageText == null && allTexts.Length > 0)
            {
                messageText = allTexts[0];
            }
        }

        if (messageText != null)
        {
            Canvas parentCanvas = messageText.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("[MetaController] ¡¡¡EL TEXTO NO ESTÁ DENTRO DE UN CANVAS!!! NO SE VERÁ.");
                
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas == null)
                {
                    Debug.Log("[MetaController] Creando Canvas automáticamente...");
                    GameObject canvasObj = new GameObject("Canvas");
                    canvas = canvasObj.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();
                }
                
                messageText.transform.SetParent(canvas.transform, false);
                Debug.Log("[MetaController] Texto movido dentro del Canvas: " + canvas.name);
            }
            else
            {
                Debug.Log("[MetaController] ✓ Texto está dentro del Canvas: " + parentCanvas.name);
            }
            
            messageText.text = "";
            messageText.fontSize = 6;
            messageText.color = Color.white;
            messageText.gameObject.SetActive(true);
            
            Debug.Log("[MetaController] Mensaje configurado correctamente.");
        }
        else
        {
            Debug.LogError("[MetaController] NO SE ENCONTRÓ NINGÚN TextMeshProUGUI EN LA ESCENA.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (goalReached) return;

        bool isPlayer = false;

        if (personaje != null && other.gameObject == personaje)
        {
            isPlayer = true;
        }
        else if (other.CompareTag("Player"))
        {
            isPlayer = true;
        }
        else if (other.attachedRigidbody != null)
        {
            isPlayer = true;
        }

        if (isPlayer)
        {
            goalReached = true;
            Debug.Log("[MetaController] ¡Victoria alcanzada!");

            if (messageText != null)
            {
                messageText.text = "¡Has llegado a la meta!";
                messageText.fontSize = 6;
                messageText.color = new Color(1f, 1f, 0f, 1f);
                messageText.fontStyle = FontStyles.Bold;
                messageText.gameObject.SetActive(true);
                messageText.enabled = true;
                messageText.ForceMeshUpdate(true, true);
                
                Debug.Log("[MetaController] ★★★ MENSAJE ACTIVADO ★★★");
            }
            else
            {
                Debug.LogError("[MetaController] messageText es NULL.");
            }

            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            var controller = other.GetComponent<HandTo2DUsingKinectManager>();
            if (controller != null)
            {
                controller.ReachGoal();
            }
        }
    }
}
