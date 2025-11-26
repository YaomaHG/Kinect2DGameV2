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
            }

            // Detener el personaje si tiene Rigidbody2D
            Rigidbody2D rb = other.attachedRigidbody;
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            // Notificar al controlador del personaje (si existe)
            var controller = other.GetComponent<HandTo2DUsingKinectManager>();
            if (controller != null)
            {
                // El controlador ya tiene su propia lógica de meta, pero por si acaso
                Debug.Log("[MetaController] Controlador del personaje encontrado.");
            }
        }
    }
}
