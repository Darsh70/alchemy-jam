using UnityEngine;
using System.Collections.Generic;
using System.Collections; 

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;
    public List<Enemy> ActiveEnemies = new();

    [Header("Wave Settings")]
    public int currentWave = 1;
    public Transform enemyParent;
    public List<GameObject> enemyPrefabs;
    public float waveDelay = 2.0f; 

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SpawnWave()
    {
        ActiveEnemies.Clear();
        int enemyCount = 1;

        // Determine number of enemies
        if (currentWave <= 2)
            enemyCount = Random.Range(1, 3); // 1 or 2
        else if (currentWave <= 5)
            enemyCount = Random.Range(1, 4); // 1–3
        else
            enemyCount = Random.Range(1, 3); // 1–2 for boss waves

        int bossesSpawned = 0;

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject prefab = ChooseEnemyForWave(ref bossesSpawned);
            GameObject enemyGO = Instantiate(prefab, enemyParent);
            enemyGO.transform.position = new Vector3(i * 5f, 0, 0);

            Enemy enemy = enemyGO.GetComponent<Enemy>();
            ActiveEnemies.Add(enemy);

            Debug.Log($"Wave {currentWave}: Spawned {enemy.enemyType}");
        }
    }

    GameObject ChooseEnemyForWave(ref int bossesSpawned)
    {
        List<GameObject> possibleEnemies = new()
        {
            enemyPrefabs[3],
            enemyPrefabs[0], 
            enemyPrefabs[1], 
            enemyPrefabs[2]  
        };


        if (currentWave == 6)
            possibleEnemies = new List<GameObject> { enemyPrefabs[4] };


        if (currentWave > 6)
        {
            if (bossesSpawned < 1)
                possibleEnemies.Add(enemyPrefabs[4]); 
        }


        if (currentWave > 4 && !possibleEnemies.Contains(enemyPrefabs[3]))
            possibleEnemies.Add(enemyPrefabs[3]);

        GameObject chosen = possibleEnemies[Random.Range(0, possibleEnemies.Count)];


        if (chosen == enemyPrefabs[4])
            bossesSpawned++;

        return chosen;
    }

    public void OnEnemyKill(Enemy enemy)
    {
        ActiveEnemies.Remove(enemy);

        if (ActiveEnemies.Count == 0)
        {
             StartCoroutine(PrepareNextWave());
        }
    }
    IEnumerator PrepareNextWave()
    {
        Debug.Log("Wave cleared! Waiting for next wave...");
        
        // Wait for 2 seconds (allows death animations to finish)
        yield return new WaitForSeconds(waveDelay);

        if (GameManager.Instance != null)
            GameManager.Instance.AddWaveCleared();

        currentWave++;
        Debug.Log($"Wave {currentWave} starting...");
        
        SpawnWave();
        
        TurnManager.Instance.StartPlayerTurn();
    }
}
