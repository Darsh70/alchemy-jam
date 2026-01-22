using UnityEngine;
using UnityEngine.UI;

public class SpellHighlight : MonoBehaviour
{
    public enum HighlightMode { Reaction, Combo }

    [Header("Configuration")]
    public HighlightMode mode;
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
        CheckLogic();
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

    void CheckLogic()
    {
        if (PlayerManager.Instance == null) return;

        bool active = false;

        // REACTION CHECK
        if (mode == HighlightMode.Reaction)
        {
            active = PlayerManager.Instance.IsReactionAvailable();
        }
        // COMBO CHECK
        else if (mode == HighlightMode.Combo)
        {
            active = PlayerManager.Instance.IsComboReady(element, spellType);
        }

        shouldPulse = active;
        
        if (!active) 
        {
            ResetColor();
        }
    }

    void ResetColor()
    {
        if (buttonImage != null) buttonImage.color = originalColor;
    }
}