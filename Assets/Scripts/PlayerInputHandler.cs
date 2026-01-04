using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public SpellMenu spellMenu;
  
    public PlayerManager playerManager;

    public void OnAttackZ(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (TurnManager.Instance.currentState != TurnState.PlayerTurn)
                return;

            Debug.Log("Attack Z");
            spellMenu.ShowButtonsForElement(ElementType.Bomb);
        }
    }

    public void OnAttackX(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (TurnManager.Instance.currentState != TurnState.PlayerTurn)
                return;
            
            Debug.Log("Attack X");
            spellMenu.ShowButtonsForElement(ElementType.Water);
        }
    }

    public void OnAttackC(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (TurnManager.Instance.currentState != TurnState.PlayerTurn)
                return;

            Debug.Log("Attack C");
            spellMenu.ShowButtonsForElement(ElementType.Electricity);
        }
    }
}
