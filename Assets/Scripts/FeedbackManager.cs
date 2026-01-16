using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance;

    [Header("References")]
    public GameObject floatingTextPrefab;
    public Canvas gameCanvas;

    [Header("Colors")]
    public Color reactionColor = new Color(1f, 0.5f, 0f); 
    public Color healComboColor = new Color(0f, 1f, 0.5f); 
    public Color electricComboColor = new Color(1f, 1f, 0f); 

    void Awake()
    {
        Instance = this;
    }

    public void ShowText(string message, Vector3 worldPosition, Color color)
    {
        if (floatingTextPrefab == null || gameCanvas == null) return;

        Debug.Log($"Spawning Text: {message} at {worldPosition}");


        GameObject go = Instantiate(floatingTextPrefab, gameCanvas.transform);
        

        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        

        screenPosition += new Vector2(0, 50f); 

        go.transform.position = screenPosition;

        FloatingText ft = go.GetComponent<FloatingText>(); 
        ft.Setup(message, color);
    }
}