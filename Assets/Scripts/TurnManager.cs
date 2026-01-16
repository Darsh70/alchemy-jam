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
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive) return;
        WaveManager.Instance.SpawnWave();
        StartPlayerTurn();
    }
        public void StopTurns()
    {
        currentState = TurnState.Stopped;
        StopAllCoroutines(); // Stops the enemy turn immediately 
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
        currentState = TurnState.Busy;
        Debug.Log("End player turn - Transitioning...");
  
        StartCoroutine(EnemyTurnRoutine());
    }

    // ----- ENEMY TURN -----
    IEnumerator EnemyTurnRoutine()
    {
        //yield return new WaitForSeconds(0.2f);
        currentState = TurnState.EnemyTurn;
        Debug.Log("Enemy turn started");


        List<Enemy> enemies = new List<Enemy>(WaveManager.Instance.ActiveEnemies);

        foreach (Enemy enemy in enemies)
        {
            if (currentState == TurnState.Stopped) yield break;
            if (enemy == null) continue;

            // Process Damage-over-Time for this specific enemy
            if (playerManager != null)
            {
                playerManager.TickEnemyDoTs(enemy);
            }

            yield return new WaitForSeconds(0.6f);

            // Check if the DoT killed the enemy. If so, skip their turn.
            if (enemy == null || enemy.health <= 0)
            {
                yield return new WaitForSeconds(0.3f);
                continue;
            }

            Debug.Log($"Enemy {enemy.name} attacking");

            // Wait for this enemy to fully perform its turn
            yield return enemy.PerformTurn();
            
            // Small pause between different enemies acting
            //yield return new WaitForSeconds(0.2f);
        }

        StartPlayerTurn();
    }

}