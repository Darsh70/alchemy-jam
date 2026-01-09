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


    private void UpdateButtonVisuals(SpellButtonUI ui, bool isPressed)
    {
        if (ui.iconImage == null || ui.keyImage == null || ui.container == null) return;

        ui.keyImage.sprite = isPressed ? ui.keyPressed : ui.keyNormal;

        Vector2 currentPos = ui.container.anchoredPosition;
        if (isPressed)
            ui.container.anchoredPosition = new Vector2(currentPos.x, currentPos.y - clickShiftAmount);
        else
            ui.container.anchoredPosition = new Vector2(currentPos.x, currentPos.y + clickShiftAmount);
    }


    public void OnAttackZ(InputAction.CallbackContext context)
    {

        if (context.started) UpdateButtonVisuals(bombUI, true);
        if (context.canceled) UpdateButtonVisuals(bombUI, false);


        if (context.performed)
        {
            if (TurnManager.Instance.currentState != TurnState.PlayerTurn)
                return;

            Debug.Log("Attack Z - Bomb");
            spellMenu.ShowButtonsForElement(ElementType.Bomb);
        }
    }

    public void OnAttackX(InputAction.CallbackContext context)
    {
        if (context.started) UpdateButtonVisuals(waterUI, true);
        if (context.canceled) UpdateButtonVisuals(waterUI, false);

        if (context.performed)
        {
            if (TurnManager.Instance.currentState != TurnState.PlayerTurn)
                return;
            
            Debug.Log("Attack X - Water");
            spellMenu.ShowButtonsForElement(ElementType.Water);
        }
    }

    public void OnAttackC(InputAction.CallbackContext context)
    {
        if (context.started) UpdateButtonVisuals(electricityUI, true);
        if (context.canceled) UpdateButtonVisuals(electricityUI, false);

        if (context.performed)
        {
            if (TurnManager.Instance.currentState != TurnState.PlayerTurn)
                return;

            Debug.Log("Attack C - Electricity");
            spellMenu.ShowButtonsForElement(ElementType.Electricity);
        }
    }
}