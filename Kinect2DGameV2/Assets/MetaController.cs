using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MetaController : MonoBehaviour
{
    public TextMeshProUGUI messageText;
    public GameObject personaje;  // Arrastra tu personaje aquí

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == personaje)
        {
            Debug.Log("¡Victoria alcanzada!");

            if (messageText != null)
                messageText.text = "¡Has llegado a la meta!";
        }
    }
}
