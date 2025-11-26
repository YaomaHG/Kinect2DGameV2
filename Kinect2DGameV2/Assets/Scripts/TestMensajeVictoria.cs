using UnityEngine;
using TMPro;

/// <summary>
/// Script de prueba simple para verificar que el mensaje de victoria funciona.
/// Presiona la tecla ESPACIO para mostrar el mensaje sin necesidad de llegar a la meta.
/// </summary>
public class TestMensajeVictoria : MonoBehaviour
{
    public TextMeshProUGUI messageText;

    void Start()
    {
        Debug.Log("[TestMensaje] Script de prueba iniciado. Presiona ESPACIO para mostrar el mensaje.");
        
        // Buscar automáticamente el texto si no está asignado
        if (messageText == null)
        {
            TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
            Debug.Log("[TestMensaje] Encontrados " + allTexts.Length + " TextMeshProUGUI en la escena:");
            
            foreach (TextMeshProUGUI txt in allTexts)
            {
                Debug.Log("  - " + txt.gameObject.name + " (Padre: " + (txt.transform.parent != null ? txt.transform.parent.name : "ninguno") + ")");
                
                if (txt.gameObject.name.Contains("Mensaje") || txt.gameObject.name.Contains("Victoria"))
                {
                    messageText = txt;
                    Debug.Log("[TestMensaje] ¡Texto encontrado y asignado!: " + txt.gameObject.name);
                    break;
                }
            }
            
            if (messageText == null && allTexts.Length > 0)
            {
                messageText = allTexts[0];
                Debug.Log("[TestMensaje] Usando primer texto encontrado: " + messageText.gameObject.name);
            }
        }
        
        if (messageText != null)
        {
            Debug.Log("[TestMensaje] Texto configurado: " + messageText.gameObject.name);
            messageText.text = "";
        }
        else
        {
            Debug.LogError("[TestMensaje] NO SE ENCONTRÓ ningún TextMeshProUGUI. Crea uno en el Canvas.");
        }
    }

    void Update()
    {
        // Presiona ESPACIO para mostrar el mensaje
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MostrarMensaje();
        }
        
        // Presiona H para ocultar el mensaje
        if (Input.GetKeyDown(KeyCode.H))
        {
            OcultarMensaje();
        }
        
        // Presiona I para ver información del texto
        if (Input.GetKeyDown(KeyCode.I))
        {
            MostrarInfo();
        }
    }

    void MostrarMensaje()
    {
        if (messageText == null)
        {
            Debug.LogError("[TestMensaje] messageText es null!");
            return;
        }

        Debug.Log("[TestMensaje] ========== MOSTRANDO MENSAJE ==========");
        
        messageText.text = "¡PRUEBA DE MENSAJE DE VICTORIA!";
        messageText.gameObject.SetActive(true);
        messageText.enabled = true;
        messageText.color = Color.white;
        messageText.alpha = 1f;
        messageText.fontSize = 48;
        
        // Forzar actualización
        messageText.ForceMeshUpdate();
        
        Debug.Log("[TestMensaje] Texto: " + messageText.text);
        Debug.Log("[TestMensaje] GameObject: " + messageText.gameObject.name);
        Debug.Log("[TestMensaje] Activo: " + messageText.gameObject.activeInHierarchy);
        Debug.Log("[TestMensaje] Enabled: " + messageText.enabled);
        Debug.Log("[TestMensaje] Color: " + messageText.color);
        Debug.Log("[TestMensaje] Font Size: " + messageText.fontSize);
        Debug.Log("[TestMensaje] Posición: " + messageText.rectTransform.position);
        Debug.Log("[TestMensaje] Tamaño: " + messageText.rectTransform.sizeDelta);
        Debug.Log("[TestMensaje] =======================================");
    }

    void OcultarMensaje()
    {
        if (messageText != null)
        {
            messageText.text = "";
            Debug.Log("[TestMensaje] Mensaje ocultado.");
        }
    }

    void MostrarInfo()
    {
        if (messageText == null)
        {
            Debug.LogError("[TestMensaje] messageText es null!");
            return;
        }

        Debug.Log("[TestMensaje] ========== INFORMACIÓN DEL TEXTO ==========");
        Debug.Log("GameObject: " + messageText.gameObject.name);
        Debug.Log("Activo en jerarquía: " + messageText.gameObject.activeInHierarchy);
        Debug.Log("Activo self: " + messageText.gameObject.activeSelf);
        Debug.Log("Componente enabled: " + messageText.enabled);
        Debug.Log("Texto actual: \"" + messageText.text + "\"");
        Debug.Log("Color: " + messageText.color);
        Debug.Log("Alpha: " + messageText.alpha);
        Debug.Log("Font Size: " + messageText.fontSize);
        Debug.Log("Font Asset: " + (messageText.font != null ? messageText.font.name : "NULL"));
        Debug.Log("Material: " + (messageText.fontMaterial != null ? messageText.fontMaterial.name : "NULL"));
        
        RectTransform rect = messageText.rectTransform;
        Debug.Log("Posición: " + rect.position);
        Debug.Log("Posición local: " + rect.localPosition);
        Debug.Log("Tamaño: " + rect.sizeDelta);
        Debug.Log("Escala: " + rect.localScale);
        Debug.Log("Rect: " + rect.rect);
        Debug.Log("Anchors: Min=" + rect.anchorMin + ", Max=" + rect.anchorMax);
        
        Canvas canvas = messageText.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            Debug.Log("Canvas padre: " + canvas.name);
            Debug.Log("Canvas activo: " + canvas.gameObject.activeInHierarchy);
            Debug.Log("Canvas enabled: " + canvas.enabled);
            Debug.Log("Render Mode: " + canvas.renderMode);
        }
        else
        {
            Debug.LogWarning("NO hay Canvas padre!");
        }
        
        Debug.Log("[TestMensaje] ========================================");
    }

    void OnGUI()
    {
        // Mostrar instrucciones en pantalla
        GUI.Label(new Rect(10, 10, 400, 100), 
            "PRUEBA DE MENSAJE:\n" +
            "ESPACIO = Mostrar mensaje\n" +
            "H = Ocultar mensaje\n" +
            "I = Ver info en consola");
    }
}
