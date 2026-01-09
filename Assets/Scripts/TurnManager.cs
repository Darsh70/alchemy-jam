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
        Debug.Log("Player turn");
        // if (PlayerManager.Instance != null)
        //     PlayerManager.Instance.ResetTurn();
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
        Debug.Log("Enemy turn");

        List<Enemy> enemies = WaveManager.Instance.ActiveEnemies;

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null) continue;

            Debug.Log($"Enemy {enemy.name} attacking");

            // Wait for this enemy to fully perform its turn
            yield return enemy.PerformTurn();
        }

        // After all enemies acted, start player turn
        StartPlayerTurn();
    }

    public void GameOver()
    {
        currentState = TurnState.None;
        Debug.Log("Game Over");
    }
}
