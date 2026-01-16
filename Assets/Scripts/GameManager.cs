using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public bool isGameActive = true;

    [Header("UI References")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI statsText;
    public CanvasGroup spellMenuCanvasGroup;

    public TextMeshProUGUI wavesCount;

    // ─────────────────────────────
    // STAT TRACKING
    // ─────────────────────────────
    public int wavesCleared = 0;
    
    private HashSet<string> discoveredReactions = new HashSet<string>();
    private HashSet<string> discoveredCombos = new HashSet<string>();

    private int totalReactions = 2; 
    private int totalCombos = 2;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddWaveCleared()
    {
        wavesCleared++;
        if (wavesCount != null)
        {
            wavesCount.text = $"WAVE: {wavesCleared}/10";
        }
    }

    public void LogReaction(string name)
    {
        if (!discoveredReactions.Contains(name))
        {
            discoveredReactions.Add(name);
            Debug.Log($"Reaction Discovered: {name}");
        }
    }

    public void LogCombo(string name)
    {
        if (!discoveredCombos.Contains(name))
        {
            discoveredCombos.Add(name);
            Debug.Log($"Combo Discovered: {name}");
        }
    }

    public void TriggerGameOver()
    {
        if (!isGameActive) return;

        isGameActive = false;
        
        if (statsText != null)
        {
            statsText.text = 
                $"<size=120%>GAME OVER</size>\n\n" +
                $"Waves Cleared: {wavesCleared}/10\n" +
                $"You Discovered:</size>\n" + 
                $"{discoveredReactions.Count}/{totalReactions} Reaction and {discoveredCombos.Count}</color>/{totalCombos} Combos";
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (spellMenuCanvasGroup != null)
        {
            spellMenuCanvasGroup.interactable = false;
            spellMenuCanvasGroup.blocksRaycasts = false;
            spellMenuCanvasGroup.alpha = 0.5f; 
        }

        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.StopTurns();
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}