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
            messageText.text = "";
            messageText.fontSize = 72;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.color = Color.white;
            messageText.gameObject.SetActive(true);
            
            // Posicionar en el centro
            RectTransform rect = messageText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(800, 200);
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
                messageText.text = "¡HAS LLEGADO A LA META!";
                messageText.fontSize = 72;
                messageText.color = new Color(1f, 1f, 0f, 1f); // Amarillo brillante
                messageText.fontStyle = FontStyles.Bold;
                messageText.alignment = TextAlignmentOptions.Center;
                messageText.gameObject.SetActive(true);
                messageText.enabled = true;
                messageText.ForceMeshUpdate(true, true);
                
                Debug.Log("MENSAJE ACTIVADO: " + messageText.text);
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
