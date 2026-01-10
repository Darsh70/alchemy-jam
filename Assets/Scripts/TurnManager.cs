using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public TurnState currentState = TurnState.PlayerTurn;
    public PlayerManager playerManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (WaveManager.Instance != null)
            StartWave();
    }

    public void StartWave()
    {
        WaveManager.Instance.SpawnWave();
        StartPlayerTurn();
    }

    // ----- PLAYER TURN ------
    public void StartPlayerTurn()
    {
        currentState = TurnState.PlayerTurn;
        Debug.Log("Player turn started");

        if (playerManager != null)
        {
            //Process Heal-over-Time at the start of the turn
            playerManager.TickHoTs();
        }
    }

    public void EndPlayerTurn()
    {
        if (currentState != TurnState.PlayerTurn) return;
        Debug.Log("End player turn");

        StartCoroutine(EnemyTurnRoutine());
    }

    // ----- ENEMY TURN -----
    IEnumerator EnemyTurnRoutine()
    {
        currentState = TurnState.EnemyTurn;
        Debug.Log("Enemy turn started");


        List<Enemy> enemies = new List<Enemy>(WaveManager.Instance.ActiveEnemies);

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

            // Process Damage-over-Time for this specific enemy
            if (playerManager != null)
            {
                playerManager.TickEnemyDoTs(enemy);
            }

            // Check if the DoT killed the enemy. If so, skip their turn.
            if (enemy == null || enemy.health <= 0)
            {
                yield return new WaitForSeconds(0.3f); // Brief pause to show they died from DoT
                continue;
            }

            Debug.Log($"Enemy {enemy.name} attacking");

            // Wait for this enemy to fully perform its turn
            yield return enemy.PerformTurn();
            
            // Small pause between different enemies acting
            yield return new WaitForSeconds(0.5f);
        }

        StartPlayerTurn();
    }

    public void GameOver()
    {
        currentState = TurnState.None;
        Debug.Log("Game Over");
    }
}