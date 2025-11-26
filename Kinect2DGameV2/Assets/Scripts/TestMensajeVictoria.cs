using UnityEngine;
using TMPro;

public class TestMensajeVictoria : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    private bool messageShown = false;

    void Start()
    {
        Debug.Log("[TestMensaje] Script iniciado.");
        
        if (messageText == null)
        {
            TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>();
            Debug.Log("[TestMensaje] Textos encontrados: " + allTexts.Length);
            
            if (allTexts.Length > 0)
            {
                messageText = allTexts[0];
                Debug.Log("[TestMensaje] Usando: " + messageText.gameObject.name);
            }
        }
        
        if (messageText != null)
        {
            messageText.text = "";
            messageText.fontSize = 72;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.color = Color.white;
            messageText.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MostrarMensaje();
        }
    }

    void MostrarMensaje()
    {
        if (messageText == null) return;

        messageText.text = "¡MENSAJE DE VICTORIA!";
        messageText.fontSize = 72;
        messageText.color = new Color(1f, 1f, 0f, 1f); // Amarillo brillante
        messageText.fontStyle = FontStyles.Bold;
        messageText.alignment = TextAlignmentOptions.Center;
        messageText.gameObject.SetActive(true);
        messageText.enabled = true;
        messageText.ForceMeshUpdate(true, true);
        
        // Posicionar en el centro de la pantalla
        RectTransform rect = messageText.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(800, 200);
        
        messageShown = true;
        Debug.Log("MENSAJE MOSTRADO: " + messageText.text + " | Activo: " + messageText.gameObject.activeInHierarchy);
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 30), "Presiona ESPACIO para mostrar mensaje");
        if(messageShown)
        {
            GUI.Label(new Rect(10, 40, 300, 30), "Estado: MENSAJE VISIBLE");
        }
    }
}
