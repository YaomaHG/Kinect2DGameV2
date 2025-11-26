using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AccessibilityUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Slider sensitivityXSlider;
    public Slider sensitivityYSlider;
    public Toggle handToggle;
    public Toggle invertXToggle;
    public Toggle invertYToggle;
    public Button resetButton;

    [Header("Labels")]
    public TextMeshProUGUI sensitivityXLabel;
    public TextMeshProUGUI sensitivityYLabel;

    [Header("Player Reference")]
    public HandTo2DUsingKinectManager playerController;

    void Start()
    {
        // Cargar configuración inicial
        LoadUIFromConfig();

        // Conectar eventos de UI
        sensitivityXSlider.onValueChanged.AddListener(OnSensitivityXChanged);
        sensitivityYSlider.onValueChanged.AddListener(OnSensitivityYChanged);
        handToggle.onValueChanged.AddListener(OnHandToggleChanged);
        invertXToggle.onValueChanged.AddListener(OnInvertXChanged);
        invertYToggle.onValueChanged.AddListener(OnInvertYChanged);
        resetButton.onClick.AddListener(OnResetButtonClicked);
    }

    void LoadUIFromConfig()
    {
        if (AccessibilityConfig.Instance == null) return;

        sensitivityXSlider.value = AccessibilityConfig.Instance.sensitivityX;
        sensitivityYSlider.value = AccessibilityConfig.Instance.sensitivityY;
        handToggle.isOn = AccessibilityConfig.Instance.useRightHand;
        invertXToggle.isOn = AccessibilityConfig.Instance.invertX;
        invertYToggle.isOn = AccessibilityConfig.Instance.invertY;

        UpdateLabels();
    }

    void OnSensitivityXChanged(float value)
    {
        if (AccessibilityConfig.Instance != null)
        {
            AccessibilityConfig.Instance.sensitivityX = value;
            AccessibilityConfig.Instance.SavePreferences();
        }
        UpdateLabels();
    }

    void OnSensitivityYChanged(float value)
    {
        if (AccessibilityConfig.Instance != null)
        {
            AccessibilityConfig.Instance.sensitivityY = value;
            AccessibilityConfig.Instance.SavePreferences();
        }
        UpdateLabels();
    }

    void OnHandToggleChanged(bool value)
    {
        if (AccessibilityConfig.Instance != null)
        {
            AccessibilityConfig.Instance.useRightHand = value;
            AccessibilityConfig.Instance.SavePreferences();
        }
    }

    void OnInvertXChanged(bool value)
    {
        if (AccessibilityConfig.Instance != null)
        {
            AccessibilityConfig.Instance.invertX = value;
            AccessibilityConfig.Instance.SavePreferences();
        }
    }

    void OnInvertYChanged(bool value)
    {
        if (AccessibilityConfig.Instance != null)
        {
            AccessibilityConfig.Instance.invertY = value;
            AccessibilityConfig.Instance.SavePreferences();
        }
    }

    void OnResetButtonClicked()
    {
        if (playerController != null)
        {
            playerController.ResetPlayerPosition();
        }
    }

    void UpdateLabels()
    {
        if (sensitivityXLabel != null)
            sensitivityXLabel.text = $"Sensibilidad Horizontal: {sensitivityXSlider.value:F1}x";

        if (sensitivityYLabel != null)
            sensitivityYLabel.text = $"Sensibilidad Vertical: {sensitivityYSlider.value:F1}x";
    }
}