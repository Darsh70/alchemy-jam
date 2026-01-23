using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Animation")]
    public RectTransform titleRect;
    public float floatSpeed = 2f;
    public float floatHeight = 10f;

    [Header("UI Panels")]
    public GameObject creditsPanel;

    private Vector2 startPos;

    void Start()
    {
        if (titleRect != null) startPos = titleRect.anchoredPosition;

        // Start the BGM 
        if (AudioManager.Instance != null) 
        {
            AudioManager.Instance.PlayMusic("Theme");
        }
    }

    void Update()
    {
        if (titleRect != null)
        {
            float newY = startPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatHeight);
            titleRect.anchoredPosition = new Vector2(startPos.x, newY);
        }
    }

    public void OnPlayClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Click");
        
        // Load the game scene
        SceneManager.LoadScene(1); 
        AudioManager.Instance.StartBattleMusic();
    }

    public void OnCreditsClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Click");
        
        creditsPanel.SetActive(true);
    }
     public void OnCloseCreditsClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Click");
        
        creditsPanel.SetActive(false);
    }
}