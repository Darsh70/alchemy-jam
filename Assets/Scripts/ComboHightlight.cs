using UnityEngine;
using UnityEngine.UI;

public class ComboHighlight : MonoBehaviour
{
    [Header("Configuration")]
    public ElementType element;  
    public SpellType spellType;  

    [Header("Visuals")]
    public Color pulseColor = new Color(0.5f, 1f, 1f); 
    public float pulseSpeed = 5f;

    private Image buttonImage;
    private Color originalColor;
    private bool shouldPulse = false;

    void Awake()
    {
        buttonImage = GetComponent<Image>();
        if (buttonImage != null) originalColor = buttonImage.color;
    }

    void OnEnable()
    {
        if (PlayerManager.Instance == null) return;

        if (PlayerManager.Instance.IsComboReady(element, spellType))
        {
            shouldPulse = true;
        }
        else
        {
            shouldPulse = false;
            ResetColor();
        }
    }

    void OnDisable()
    {
        ResetColor();
    }

    void Update()
    {
        if (shouldPulse && buttonImage != null)
        {
            float t = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            buttonImage.color = Color.Lerp(originalColor, pulseColor, t);
        }
    }

    void ResetColor()
    {
        if (buttonImage != null) buttonImage.color = originalColor;
    }
}