using UnityEngine;

public class AccessibilityConfig : MonoBehaviour
{
    // Singleton para acceso global
    public static AccessibilityConfig Instance { get; private set; }

    // Configuraciones de accesibilidad
    [Header("Sensitivity Settings")]
    public float sensitivityX = 1.0f;
    public float sensitivityY = 1.0f;

    [Header("Hand Selection")]
    public bool useRightHand = true;

    [Header("Axis Inversion")]
    public bool invertX = false;
    public bool invertY = false;

    // Claves para PlayerPrefs
    private const string SENSITIVITY_X_KEY = "AccessibilitySensitivityX";
    private const string SENSITIVITY_Y_KEY = "AccessibilitySensitivityY";
    private const string USE_RIGHT_HAND_KEY = "AccessibilityUseRightHand";
    private const string INVERT_X_KEY = "AccessibilityInvertX";
    private const string INVERT_Y_KEY = "AccessibilityInvertY";

    void Awake()
    {
        // Patrón Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPreferences();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Cargar preferencias guardadas
    public void LoadPreferences()
    {
        sensitivityX = PlayerPrefs.GetFloat(SENSITIVITY_X_KEY, 1.0f);
        sensitivityY = PlayerPrefs.GetFloat(SENSITIVITY_Y_KEY, 1.0f);
        useRightHand = PlayerPrefs.GetInt(USE_RIGHT_HAND_KEY, 1) == 1;
        invertX = PlayerPrefs.GetInt(INVERT_X_KEY, 0) == 1;
        invertY = PlayerPrefs.GetInt(INVERT_Y_KEY, 0) == 1;

        Debug.Log($"[AccessibilityConfig] Configuración cargada: SensX={sensitivityX}, SensY={sensitivityY}, RightHand={useRightHand}");
    }

    // Guardar preferencias
    public void SavePreferences()
    {
        PlayerPrefs.SetFloat(SENSITIVITY_X_KEY, sensitivityX);
        PlayerPrefs.SetFloat(SENSITIVITY_Y_KEY, sensitivityY);
        PlayerPrefs.SetInt(USE_RIGHT_HAND_KEY, useRightHand ? 1 : 0);
        PlayerPrefs.SetInt(INVERT_X_KEY, invertX ? 1 : 0);
        PlayerPrefs.SetInt(INVERT_Y_KEY, invertY ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("[AccessibilityConfig] Configuración guardada exitosamente");
    }

    // Aplicar sensibilidad e inversión a un valor
    public float ApplySensitivityAndInversion(float rawValue, bool isXAxis)
    {
        float sensitivity = isXAxis ? sensitivityX : sensitivityY;
        bool invert = isXAxis ? invertX : invertY;

        float processed = rawValue * sensitivity;
        if (invert)
            processed *= -1f;

        return processed;
    }

    // Resetear a valores por defecto
    public void ResetToDefaults()
    {
        sensitivityX = 1.0f;
        sensitivityY = 1.0f;
        useRightHand = true;
        invertX = false;
        invertY = false;
        SavePreferences();
    }
}