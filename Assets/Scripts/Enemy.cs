using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 5;
    public int health;
    public int attackDamage;
    public EnemyType enemyType;

    [Header("UI")]
    public Image healthBarFill;
    public Image healthBarGhost; 

    private Coroutine healthRoutine;
    private float ghostDelay = 0.5f;
    private float ghostSpeed = 2f;

    protected Animator animator;
    private Vector3 originalPosition;
    public GameObject currentStatusCircle;

    [Header("Effect Positions")]
    public float floorOffset = -0.5f; 
    public float headOffset = 1.8f;  

    [Header("Status")]
    public ElementType? currentElement = null;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        SetUpStatsByType();
    }

    void Start()
    {
        health = maxHealth;
        if (healthBarFill != null) healthBarFill.fillAmount = 1f;
        if (healthBarGhost != null) healthBarGhost.fillAmount = 1f;
        originalPosition = transform.position;
    }

    void SetUpStatsByType()
    {
        switch (enemyType)
        {
            case EnemyType.FireSlime: maxHealth = 10; attackDamage = 3; break;
            case EnemyType.WaterSlime: maxHealth = 12; attackDamage = 2; break;
            case EnemyType.ElectricSlime: maxHealth = 10; attackDamage = 4; break;
            case EnemyType.Skeleton: maxHealth = 20; attackDamage = 2; break;
            case EnemyType.Boss: maxHealth = 50; attackDamage = 10; break;
        }
    }

    void UpdateHealthUI()
    {
        if (healthBarFill == null) return;

        float targetFill = (float)health / maxHealth;


        if (healthBarGhost == null)
        {
            healthBarFill.fillAmount = targetFill;
            return;
        }

        float currentFill = healthBarFill.fillAmount;

        if (Mathf.Abs(targetFill - currentFill) < 0.001f) return; 


        if (healthRoutine != null) StopCoroutine(healthRoutine);

        // DAMAGE
        if (targetFill < currentFill)
        {
            healthBarGhost.color = new Color(1f, 0.33f, 0f); 
            healthBarGhost.fillAmount = currentFill;
            healthBarFill.fillAmount = targetFill;
            healthRoutine = StartCoroutine(AnimateGhostBar(targetFill));
        }
        // HEALING
        else if (targetFill > currentFill)
        {
            healthBarGhost.color = new Color(0f, 0.84f, 0.41f);
            healthBarGhost.fillAmount = targetFill;
            healthRoutine = StartCoroutine(AnimateHealBar(targetFill));
        }
    }

    IEnumerator AnimateGhostBar(float targetFill)
    {

        yield return new WaitForSeconds(ghostDelay);
        float t = 0f;
        float startFill = healthBarGhost.fillAmount;
        while (t < 1f)
        {
            t += Time.deltaTime * ghostSpeed;
            healthBarGhost.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }
        healthBarGhost.fillAmount = targetFill;
    }

    IEnumerator AnimateHealBar(float targetFill)
    {
        yield return new WaitForSeconds(ghostDelay);
        float t = 0f;
        float startFill = healthBarFill.fillAmount;
        while (t < 1f)
        {
            t += Time.deltaTime * ghostSpeed;
            healthBarFill.fillAmount = Mathf.Lerp(startFill, targetFill, t);
            yield return null;
        }
        healthBarFill.fillAmount = targetFill;
        healthBarGhost.fillAmount = targetFill;
    }

    public void ApplyElement(ElementType element, SpellType spellType)
    {
        Debug.Log($"Enemy hit with {element} {spellType}");

        if (spellType == SpellType.Status)
        {
            currentElement = element;
            Debug.Log($"Enemy is now affected by {element}");
        }
    }



    // ─────────────────────────────
    // Perform Enemy Turn
    // ─────────────────────────────
    public IEnumerator PerformTurn()
    {

        if (health <= 0) yield break;

        animator.SetTrigger("Attack");

        // Length of the attack animation before resetting position
        float attackAnimLength = 0.7f; 
        yield return new WaitForSeconds(attackAnimLength);

        transform.position = originalPosition;
    }

    public void PerformAttackMove()
    {
        transform.position = originalPosition + transform.right * 1f;

        if (PlayerManager.Instance != null)
        {
            Vector3 portaitPos = PlayerManager.Instance.magePortrait.position;

            string damageText = $"<size=150%>-{attackDamage}</size>";
            
            FeedbackManager.Instance.ShowScreenText(damageText, portaitPos, Color.red);

            PlayerManager.Instance.TakeDamage(attackDamage);
            
            CameraShake.Instance.Shake(0.1f, 0.1f);
        }
    }

    public void ResetPosition()
    {
        transform.position = originalPosition;
    }

    public void TakeDamage(int damage)
    {
        if (health <= 0) return;

        health -= damage;
        UpdateHealthUI();

        if (damage > 0)
        {
            string damageText = $"<size=150%>-{damage}</size>";
            FeedbackManager.Instance.ShowText(damageText, transform.position, Color.red);  
        }
        
        Debug.Log($"{gameObject.name} took {damage} damage. HP: {health}");

        if (health <= 0) Die();
        else
        {
            animator.SetTrigger("Hurt");
            PlayHurtEffect(); 
        }
    }

    private IEnumerator HurtAnimationRoutine()
    {
        transform.position = originalPosition + transform.up * 0.3f;
        yield return new WaitForSeconds(0.7f);
        transform.position = originalPosition;
    }

    public void PlayHurtEffect() 
    { 
        StartCoroutine(HurtAnimationRoutine());
    }

    public GameObject PlayTargetedAttackEffect(ElementType element, SpellType spellType) 
        { 

            Vector3 feetPos = transform.position + (Vector3.up * floorOffset);
            Vector3 headPos = transform.position + (Vector3.up * headOffset);

            Vector3 centerPos = (feetPos + headPos) / 2f;

            GameObject spawnedVFX = null;

            if (element == ElementType.Electricity)
            {
                if (spellType == SpellType.Single && PlayerManager.Instance.zapPrefab != null)
                {

                    spawnedVFX = Instantiate(PlayerManager.Instance.zapPrefab, feetPos, Quaternion.identity);
                }
                else if (spellType == SpellType.Skill && PlayerManager.Instance.lightningPrefab != null)
                {

                    spawnedVFX = Instantiate(PlayerManager.Instance.lightningPrefab, headPos, Quaternion.identity);
                    CameraShake.Instance.Shake(0.2f, 0.2f);
                }
            }
            
            if (element == ElementType.Water)
            {
                if (spellType == SpellType.Single && PlayerManager.Instance.waterBallPrefab != null)
                {
                    spawnedVFX = Instantiate(PlayerManager.Instance.waterBallPrefab, centerPos, Quaternion.identity);
                }
            }

            if (element == ElementType.Bomb)
            {
                if (spellType == SpellType.Single && PlayerManager.Instance.bombPrefab != null)
                {
                    spawnedVFX = Instantiate(PlayerManager.Instance.bombPrefab, feetPos, Quaternion.identity);
                }
            }
            return spawnedVFX;
        }

    void Die()
    {
        animator.SetBool("IsDead", true);

        PlayDeathEffect();

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.ClearDoTsForEnemy(this);
        }

        if (WaveManager.Instance != null)
            WaveManager.Instance.OnEnemyKill(this);

        Destroy(gameObject, 1.5f); 
        Debug.Log($"{gameObject.name} defeated");
    }

    public void PlayDeathEffect() { /* visual death FX logic */ }
}