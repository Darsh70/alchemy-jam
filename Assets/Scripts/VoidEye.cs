using UnityEngine;
using System.Collections;

public class SimpleVoidEye : MonoBehaviour
{
    [Header("Assets")]
    public GameObject normalExplosionPrefab; 
    public SpriteRenderer bombSprite; 

    [Header("Settings")]
    public float fallSpeed = 35f; 
    public float rotationSpeed = 300f; 
    public int damageAmount = 20;
    
    [Header("Heavy Bounce Settings")]
    public float bounceHeight = 2.0f; 
    public float bounceDuration = 0.4f; 

    void Start()
    {
        if (bombSprite != null)
        {
            bombSprite.color = new Color(0.6f, 0f, 1f); 
            transform.localScale = Vector3.one * 6f; 
        }

        StartCoroutine(FallBounceExplode());
    }

    IEnumerator FallBounceExplode()
    {
        // ─────────────────────────────────────────────────────────────
        // FALL 
        // ─────────────────────────────────────────────────────────────
        while (transform.position.y > 0)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        // Snap to floor ensures we don't tunnel underground
        transform.position = new Vector3(transform.position.x, 0, 0);

        // IMPACT 
        CameraShake.Instance.Shake(0.2f, 0.2f); 
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX("Explosion"); // Thud sound

        // ─────────────────────────────────────────────────────────────
        // BOUNCE 
        // ─────────────────────────────────────────────────────────────
        float timer = 0f;
        
        // Flash White immediately on impact
        if (bombSprite != null) bombSprite.color = Color.white; 

        while (timer < bounceDuration)
        {
            timer += Time.deltaTime;
            float t = timer / bounceDuration;
            
            // Starts fast, slows down at the top 
            float height = Mathf.Lerp(0, bounceHeight, 1 - (1 - t) * (1 - t));
            
            transform.position = new Vector3(transform.position.x, height, 0);
            transform.Rotate(0, 0, (rotationSpeed * 0.5f) * Time.deltaTime);
            yield return null;
        }

        // ─────────────────────────────────────────────────────────────
        // MIDAIR DETONATION 
        // ─────────────────────────────────────────────────────────────
        
        if (bombSprite != null) bombSprite.enabled = false;

        CameraShake.Instance.Shake(1.0f, 1.0f); // Massive Shake
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Explosion");
            // Play a second time slightly pitched down for bass
            yield return new WaitForSeconds(0.05f);
            AudioManager.Instance.PlaySFX("Explosion");
        }

        // CLUSTER EXPLOSIONS
        for (int i = 0; i < 15; i++)
        {
            Vector3 randomOffset = Random.insideUnitCircle * 4.0f; 
            GameObject exp = Instantiate(normalExplosionPrefab, transform.position + randomOffset, Quaternion.identity);
            
            float randomScale = Random.Range(2f, 5f);
            exp.transform.localScale = Vector3.one * randomScale;

            SpriteRenderer expSr = exp.GetComponent<SpriteRenderer>();
            if (expSr != null) 
            {
                expSr.color = Color.Lerp(Color.magenta, new Color(0.5f, 0f, 1f), Random.value);
            }

            yield return new WaitForSeconds(0.01f); 
        }

        // DAMAGE
        if (WaveManager.Instance != null)
        {
            var enemies = new System.Collections.Generic.List<Enemy>(WaveManager.Instance.ActiveEnemies);
            foreach (Enemy e in enemies)
            {
                if (e != null) 
                {
                    e.TakeDamage(damageAmount);
                    FeedbackManager.Instance.ShowText(damageAmount.ToString(), e.transform.position, Color.magenta);
                }
            }
        }

        yield return new WaitForSeconds(1.0f);
        Destroy(gameObject);
    }
}