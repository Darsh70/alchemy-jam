using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public class SpellButtonUI
{
    public RectTransform container; 
    public Image iconImage;        
    public Image keyImage;          

    [Header("Sprites")]     
    public Sprite keyNormal;
    public Sprite keyPressed;

    [HideInInspector] public float startY;
}

public class PlayerInputHandler : MonoBehaviour
{
    public SpellMenu spellMenu;
    public PlayerManager playerManager;

    [Header("Button UI Settings")]
    public SpellButtonUI bombUI;        
    public SpellButtonUI waterUI;     
    public SpellButtonUI electricityUI; 

    [Header("Shift Down Settings")]
    public float clickShiftAmount = 2f; 

    void Start()
    {
        if(bombUI.container != null) bombUI.startY = bombUI.container.anchoredPosition.y;
        if(waterUI.container != null) waterUI.startY = waterUI.container.anchoredPosition.y;
        if(electricityUI.container != null) electricityUI.startY = electricityUI.container.anchoredPosition.y;
    }

    void Update()
    {
        if (TurnManager.Instance != null && TurnManager.Instance.currentState != TurnState.PlayerTurn)
        {
            UpdateButtonVisuals(bombUI, false);
            UpdateButtonVisuals(waterUI, false);
            UpdateButtonVisuals(electricityUI, false);
        }
    }

    private void UpdateButtonVisuals(SpellButtonUI ui, bool isPressed)
    {
        if (ui.iconImage == null || ui.keyImage == null || ui.container == null) return;
        ui.keyImage.sprite = isPressed ? ui.keyPressed : ui.keyNormal;
        float targetY = isPressed ? ui.startY - clickShiftAmount : ui.startY;
        ui.container.anchoredPosition = new Vector2(ui.container.anchoredPosition.x, targetY);
    }

    // ─────────────────────────────
    // INPUT HANDLERS
    // ─────────────────────────────

    public void OnAttackZ(InputAction.CallbackContext context)
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive) return;

        if (context.started) UpdateButtonVisuals(bombUI, true);
        if (context.canceled) UpdateButtonVisuals(bombUI, false);

        if (context.performed)
        {
            if (TurnManager.Instance.currentState != TurnState.PlayerTurn) return;
            Debug.Log("Attack Z - Bomb");
            spellMenu.ShowButtonsForElement(ElementType.Bomb);
        }
    }

    public void OnAttackX(InputAction.CallbackContext context)
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive) return;

        if (context.started) UpdateButtonVisuals(waterUI, true);
        if (context.canceled) UpdateButtonVisuals(waterUI, false);

        if (context.performed)
        {
            if (TurnManager.Instance.currentState != TurnState.PlayerTurn) return;
            Debug.Log("Attack X - Water");
            spellMenu.ShowButtonsForElement(ElementType.Water);
        }
    }

    public void OnAttackC(InputAction.CallbackContext context)
    {
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive) return;

        if (context.started) UpdateButtonVisuals(electricityUI, true);
        if (context.canceled) UpdateButtonVisuals(electricityUI, false);

        if (context.performed)
        {
            if (TurnManager.Instance.currentState != TurnState.PlayerTurn) return;
            Debug.Log("Attack C - Electricity");
            spellMenu.ShowButtonsForElement(ElementType.Electricity);
        }
    }
}