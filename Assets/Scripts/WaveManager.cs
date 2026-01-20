using UnityEngine;
using System.Collections; 
using System.Collections.Generic;


[System.Serializable]
public class WaveProfile
{
    public string waveName;
    public List<GameObject> enemiesInWave; 
}

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;
    public List<Enemy> ActiveEnemies = new();

    [Header("Wave Configuration")]
    public int currentWave = 1;
    public List<WaveProfile> allWaves; 
    
    [Header("Spawn Settings")]
    public Transform enemyParent;
    public float waveDelay = 2.0f;
    

    public float centerOffset = 0f; 
    
    public float spacingX = 4.0f; 
    
    public float spawnY = 0f;


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

        if (currentWave > allWaves.Count)
        {
            Debug.Log("ALL WAVES CLEARED! VICTORY!");
            // GameManager.Instance.TriggerVictory(); 
            return;
        }

        WaveProfile profile = allWaves[currentWave - 1];
        Debug.Log($"Starting Wave {currentWave}: {profile.waveName}");


        int enemyCount = profile.enemiesInWave.Count;
        
        // Calculate how wide the whole group is
        // (If 1 enemy, width is 0. If 3 enemies, width is 2 * spacing)
        float totalWidth = (enemyCount - 1) * spacingX;

        // Find the far-left start point so the group is centered
        float startX = centerOffset - (totalWidth / 2f);

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject prefab = profile.enemiesInWave[i];
            
            if (prefab != null)
            {
                GameObject enemyGO = Instantiate(prefab, enemyParent);
                
                // Place current enemy based on StartX
                float xPos = startX + (i * spacingX);
                
                enemyGO.transform.position = new Vector3(xPos, spawnY, 0);

                Enemy enemy = enemyGO.GetComponent<Enemy>();
                ActiveEnemies.Add(enemy);
            }
        }
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
        if (GameManager.Instance != null)
            GameManager.Instance.AddWaveCleared();

        yield return new WaitForSeconds(waveDelay);

        currentWave++;
        SpawnWave();
        
        // Only start turn if we actually spawned something 
        if (currentWave <= allWaves.Count + 1)
        {
            TurnManager.Instance.StartPlayerTurn();
        }
    }
}