using UnityEngine;
using UnityEngine.InputSystem;

public class EnemySelectionManager : MonoBehaviour
{
    public PlayerManager playerManager;

    void Update()
    {
        if (TurnManager.Instance.currentState != TurnState.PlayerTurn) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        EnemySelector hoveredEnemy = null;

        foreach (Enemy enemy in WaveManager.Instance.ActiveEnemies)
        {
            EnemySelector selector = enemy.GetComponent<EnemySelector>();
            if (selector == null) continue;

            if (selector.GetComponent<Collider2D>().OverlapPoint(worldPos))
            {
                hoveredEnemy = selector;
                selector.Highlight(true);
            }
            else
            {
                selector.Highlight(false);
            }
        }

        if (hoveredEnemy != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            playerManager.targetEnemy = hoveredEnemy.GetComponent<Enemy>();
            Debug.Log("Selected enemy: " + playerManager.targetEnemy.name);
        }
    }
}
