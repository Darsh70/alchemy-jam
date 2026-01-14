using System.Collections;
using UnityEngine;

public class MagicCircle : MonoBehaviour
{
    public float lifetime = 1f;
    public float scaleInTime = 0.1f;
    public float fadeOutTime = 0.2f;

    [HideInInspector] public bool isPersistent = false;

    private SpriteRenderer sr;

    void Awake()
    {
        
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        StartCoroutine(ScaleIn());
        if (!isPersistent)
        {
            StartCoroutine(FadeOut());
            Destroy(gameObject, lifetime);
        }

        // Destroy(gameObject, lifetime);
    }
    public void DeactivateAndDestroy()
    {
        StopAllCoroutines();
        StartCoroutine(FadeAndDestroyRoutine());
    }

    private IEnumerator FadeAndDestroyRoutine()
    {
        float t = 0f;
        Color startColor = sr.color;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            sr.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0), t / fadeOutTime);
            yield return null;
        }
        Destroy(gameObject);
    }

    public void SetColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Water:
                sr.color = new Color(0f/255f,137f/255f,255f/255f);
                break;
            case ElementType.Electricity:
                sr.color = new Color(255f/255f,234f/255f, 0f/255f);
                break;
            case ElementType.Bomb:
                sr.color = new Color(240f/255f,246f/255f,240f/255f);
                break;
            default:
                sr.color = Color.white;
                break;
        }
    }

    IEnumerator ScaleIn()
    {
        float t = 0f;
        Vector3 startScale = new Vector3(0f, 0f, transform.localScale.z);
        Vector3 endScale = transform.localScale;

        while (t < scaleInTime)
        {
            t += Time.deltaTime;
            float progress = t / scaleInTime;
            transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            yield return null;
        }

        transform.localScale = endScale;
    }

    IEnumerator FadeOut()
    {
        // Wait until near the end of lifetime
        yield return new WaitForSeconds(lifetime - fadeOutTime);

        float t = 0f;
        Color startColor = sr.color;

        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float progress = t / fadeOutTime;
            sr.color = new Color(
                startColor.r,
                startColor.g,
                startColor.b,
                Mathf.Lerp(1f, 0f, progress)
            );
            yield return null;
        }

        sr.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }
}
