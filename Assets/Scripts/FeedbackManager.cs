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


            GameObject go = Instantiate(floatingTextPrefab, gameCanvas.transform);
            

            go.transform.localScale = Vector3.one; 

            if (Camera.main != null)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

                float randomX = Random.Range(-50f, 50f);
                

                float randomY = Random.Range(50f, 120f); 


                screenPosition.x += randomX;
                screenPosition.y += randomY;

                screenPosition.z = 0; 

                go.transform.position = screenPosition;
            }

            FloatingText ft = go.GetComponent<FloatingText>(); 
            if (ft != null) ft.Setup(message, color);
        }

    public void ShowScreenText(string message, Vector3 screenPosition, Color color)
        {
            if (floatingTextPrefab == null || gameCanvas == null) return;

            GameObject go = Instantiate(floatingTextPrefab, gameCanvas.transform);
            go.transform.localScale = Vector3.one;

            float randomY = Random.Range(100f, 200f);
            float randomX = Random.Range(-30f, 30f); 
            
            Vector3 offset = new Vector3(randomX, randomY, 0); 

            go.transform.position = screenPosition + offset;

            FloatingText ft = go.GetComponent<FloatingText>();
            if (ft != null)
            {
                ft.Setup(message, color);
            }
        }
}