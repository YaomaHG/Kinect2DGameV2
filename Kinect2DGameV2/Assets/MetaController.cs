using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MetaController : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public GameObject personaje;  // Arrastra tu personaje aquí (OPCIONAL - si no se asigna, detecta cualquier objeto con tag "Player")

    private bool goalReached = false;

    private void Start()
    {
        // Verificar que este objeto tenga un collider marcado como trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("[MetaController] La meta necesita un Collider2D. Agregando BoxCollider2D...");
            col = gameObject.AddComponent<BoxCollider2D>();
        }
        
        if (!col.isTrigger)
        {
            Debug.LogWarning("[MetaController] El Collider de la meta debe ser Trigger. Activando...");
            col.isTrigger = true;
        }

        // Buscar automáticamente el TextMeshProUGUI si no está asignado
        if (messageText == null)
        {
            Debug.LogWarning("[MetaController] messageText no está asignado. Buscando 'MensajeVictoria'...");
            
            // Buscar por nombre en toda la escena
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name == "MensajeVictoria")
                {
                    messageText = obj.GetComponent<TextMeshProUGUI>();
                    if (messageText != null)
                    {
                        Debug.Log("[MetaController] ¡MensajeVictoria encontrado! Asignado automáticamente.");
                        break;
                    }
                }
            }
            
            // Si aún no se encontró, buscar en Canvas/MensajeVictoria
            if (messageText == null)
            {
                Canvas canvas = GameObject.FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    Transform mensajeTransform = canvas.transform.Find("MensajeVictoria");
                    if (mensajeTransform != null)
                    {
                        messageText = mensajeTransform.GetComponent<TextMeshProUGUI>();
                        if (messageText != null)
                        {
                            Debug.Log("[MetaController] MensajeVictoria encontrado en Canvas. Asignado automáticamente.");
                        }
                    }
                }
            }
            
            if (messageText == null)
            {
                Debug.LogError("[MetaController] No se pudo encontrar 'MensajeVictoria'. Asigna el TextMeshProUGUI en el Inspector.");
            }
        }

        // Asegurarse de que el mensaje esté oculto al inicio
        if (messageText != null)
        {
            messageText.text = "";
            messageText.gameObject.SetActive(true); // Asegurarse de que esté activo
            Debug.Log("[MetaController] Mensaje de victoria configurado correctamente.");
        }

        Debug.Log("[MetaController] Meta configurada correctamente. Collider: " + col.GetType().Name + " | IsTrigger: " + col.isTrigger);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (goalReached) return; // Ya se alcanzó la meta

        bool isPlayer = false;

        // Verificar si es el personaje específico (si está asignado)
        if (personaje != null && other.gameObject == personaje)
        {
            isPlayer = true;
        }
        // O verificar si tiene el tag "Player"
        else if (other.CompareTag("Player"))
        {
            isPlayer = true;
        }
        // O verificar si el objeto o su transform tiene Rigidbody2D (es el objeto controlado)
        else if (other.attachedRigidbody != null)
        {
            isPlayer = true;
        }

        if (isPlayer)
        {
            goalReached = true;
            Debug.Log("[MetaController] ¡Victoria alcanzada! Objeto: " + other.gameObject.name);

            if (messageText != null)
            {
                messageText.text = "¡Has llegado a la meta!";
                messageText.gameObject.SetActive(true); // Asegurarse de que sea visible
                Debug.Log("[MetaController] Mensaje mostrado en pantalla: " + messageText.text);
            }
            else
            {
                Debug.LogError("[MetaController] messageText es null. El mensaje no se puede mostrar.");
            }

            // Detener el personaje si tiene Rigidbody2D
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                Debug.Log("[MetaController] Personaje detenido.");
            }

            // Notificar al controlador del personaje (si existe)
            var controller = other.GetComponent<HandTo2DUsingKinectManager>();
            if (controller != null)
            {
                controller.ReachGoal();
                Debug.Log("[MetaController] Controlador del personaje notificado.");
            }
        }
        else
        {
            Debug.Log("[MetaController] Objeto no identificado como jugador: " + other.gameObject.name);
        }
    }
}
