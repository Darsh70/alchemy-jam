using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 50f;
    public float fadeDuration = 1.5f;

    private TextMeshProUGUI textMesh;
    private float timer;
    private Color startColor;

    public void Setup(string message, Color color)
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        textMesh.text = message;
        textMesh.color = color;
        startColor = color;
        timer = 0;
    }

    void Update()
    {

        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
        
        if (textMesh != null)
        {
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }

        if (timer >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }
}