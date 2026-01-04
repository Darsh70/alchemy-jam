using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class EnemySelector : MonoBehaviour
{
    [HideInInspector] public SpriteRenderer sr;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.color = normalColor;
    }

    public void Highlight(bool highlight)
    {
        sr.color = highlight ? highlightColor : normalColor;
    }
}
