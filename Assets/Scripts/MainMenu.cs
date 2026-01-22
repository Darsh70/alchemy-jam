using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Animation")]
    public RectTransform titleRect;
    public float floatSpeed = 2f;
    public float floatHeight = 10f;

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
    }

    public void OnQuitClicked()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Click");
        
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}