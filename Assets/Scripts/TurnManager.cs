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
        playerManager.ResetActionPoints();
    }

    public void EndPlayerTurn()
    {
        if (currentState != TurnState.PlayerTurn) return;
        Debug.Log("End player turn");
        StartEnemyTurn();
    }

    // ----- ENEMY TURN -----
    void StartEnemyTurn()
    {
        currentState = TurnState.EnemyTurn;
        Debug.Log("Enemy turn");

        Invoke(nameof(EnemyAction), 0.75f);
    }

    void EnemyAction()
    {
        foreach (Enemy enemy in WaveManager.Instance.ActiveEnemies)
        {
            enemy.PerformTurn();
        }
        StartPlayerTurn();

    }

    public void GameOver()
    {
        currentState = TurnState.None;
        Debug.Log("Game Over");

    }
}
