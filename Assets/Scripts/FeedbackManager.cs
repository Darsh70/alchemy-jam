using UnityEngine;
using TMPro;

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

    private bool useRightSide = true;

    private Vector3 offsetRight = new Vector3(60f, 150f, 0f);

    private Vector3 offsetLeft = new Vector3(-60f, 120f, 0f); 

    [Header("Hint UI Settings")]
    public GameObject indicatorObject;    
    public TextMeshProUGUI indicatorText;

    void Awake()
    {
        Instance = this;
        if (indicatorObject != null) indicatorObject.SetActive(false);
    }

    void Update()
    {

        if (PlayerManager.Instance == null || GameManager.Instance == null || TurnManager.Instance == null) return;
        
        // Only show hints during Player Turn and Active Game
        if (!GameManager.Instance.isGameActive || TurnManager.Instance.currentState != TurnState.PlayerTurn)
        {
            HideIndicator();
            return;
        }

        if (PlayerManager.Instance.IsAnyComboReady())
        {
            ShowIndicator("COMBO\n AVAILABLE!");
        }
        else if (PlayerManager.Instance.IsReactionAvailable())
        {
            ShowIndicator("REACTION\n AVAILABLE!");
        }
        else
        {
            HideIndicator();
        }
    }

    void ShowIndicator(string message)
    {
        if (indicatorObject != null && !indicatorObject.activeSelf) 
            indicatorObject.SetActive(true);
            
        if (indicatorText != null) 
            indicatorText.text = message;
    }

    void HideIndicator()
    {
        if (indicatorObject != null && indicatorObject.activeSelf) 
            indicatorObject.SetActive(false);
    }
    public void ShowText(string message, Vector3 worldPosition, Color color)
        {
            if (floatingTextPrefab == null || gameCanvas == null) return;


            GameObject go = Instantiate(floatingTextPrefab, gameCanvas.transform);
            

            go.transform.localScale = Vector3.one; 

            if (Camera.main != null)
            {
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

                float randomX = Random.Range(-100f, 0f);
                

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

            Vector3 selectedOffset = useRightSide ? offsetRight : offsetLeft;

            go.transform.position = screenPosition + selectedOffset;

            useRightSide = !useRightSide;

            FloatingText ft = go.GetComponent<FloatingText>();
            if (ft != null)
            {
                ft.Setup(message, color);
            }
        }
}