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
            
            // Buscar TODOS los TextMeshProUGUI en la escena
            TextMeshProUGUI[] allTexts = GameObject.FindObjectsOfType<TextMeshProUGUI>();
            Debug.Log("[MetaController] Encontrados " + allTexts.Length + " objetos TextMeshProUGUI en la escena.");
            
            foreach (TextMeshProUGUI txt in allTexts)
            {
                Debug.Log("[MetaController] TextMeshProUGUI encontrado: " + txt.gameObject.name);
                
                if (txt.gameObject.name == "MensajeVictoria" || txt.gameObject.name.Contains("Mensaje"))
                {
                    messageText = txt;
                    Debug.Log("[MetaController] ¡MensajeVictoria encontrado! Asignado: " + txt.gameObject.name);
                    break;
                }
            }
            
            // Si aún no se encontró, usar el primero que esté en un Canvas
            if (messageText == null && allTexts.Length > 0)
            {
                foreach (TextMeshProUGUI txt in allTexts)
                {
                    if (txt.GetComponentInParent<Canvas>() != null)
                    {
                        messageText = txt;
                        Debug.LogWarning("[MetaController] Usando primer TextMeshProUGUI encontrado en Canvas: " + txt.gameObject.name);
                        break;
                    }
                }
            }
            
            if (messageText == null)
            {
                Debug.LogError("[MetaController] No se pudo encontrar ningún TextMeshProUGUI. Crea uno en el Canvas y nómbralo 'MensajeVictoria'.");
            }
        }

        // Asegurarse de que el mensaje esté oculto al inicio
        if (messageText != null)
        {
            messageText.text = "";
            
            // Asegurarse de que el objeto Y el Canvas estén activos
            messageText.gameObject.SetActive(true);
            Canvas canvas = messageText.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.gameObject.SetActive(true);
                Debug.Log("[MetaController] Canvas activo: " + canvas.name);
            }
            
            // Configuración para asegurar visibilidad
            messageText.enabled = true;
            messageText.alpha = 1f;
            
            // Verificar color
            if (messageText.color.a < 0.1f)
            {
                messageText.color = new Color(1f, 1f, 1f, 1f); // Blanco opaco
                Debug.LogWarning("[MetaController] Color del texto era transparente, cambiado a blanco opaco.");
            }
            
            Debug.Log("[MetaController] Mensaje de victoria configurado. GameObject: " + messageText.gameObject.name + 
                      " | Activo: " + messageText.gameObject.activeInHierarchy + 
                      " | Enabled: " + messageText.enabled +
                      " | Color: " + messageText.color);
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
                // FORZAR VISIBILIDAD TOTAL
                messageText.text = "¡Has llegado a la meta!";
                messageText.gameObject.SetActive(true);
                messageText.enabled = true;
                messageText.alpha = 1f;
                
                // Asegurar color visible
                Color col = messageText.color;
                col.a = 1f; // Alpha máximo
                messageText.color = col;
                
                // Forzar actualización
                messageText.ForceMeshUpdate();
                
                Debug.Log("[MetaController] ===== MENSAJE DE VICTORIA =====");
                Debug.Log("[MetaController] Texto: " + messageText.text);
                Debug.Log("[MetaController] GameObject: " + messageText.gameObject.name);
                Debug.Log("[MetaController] Activo en jerarquía: " + messageText.gameObject.activeInHierarchy);
                Debug.Log("[MetaController] Componente enabled: " + messageText.enabled);
                Debug.Log("[MetaController] Color: " + messageText.color);
                Debug.Log("[MetaController] Alpha: " + messageText.alpha);
                Debug.Log("[MetaController] Font Size: " + messageText.fontSize);
                Debug.Log("[MetaController] Rect: " + messageText.rectTransform.rect);
                Debug.Log("[MetaController] ================================");
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
    
    // Método público para probar el mensaje manualmente
    public void TestMessage()
    {
        if (messageText != null)
        {
            messageText.text = "PRUEBA DE MENSAJE";
            messageText.color = Color.white;
            messageText.fontSize = 48;
            Debug.Log("[MetaController] Mensaje de prueba mostrado.");
        }
        else
        {
            Debug.LogError("[MetaController] No hay messageText asignado.");
        }
    }
}
