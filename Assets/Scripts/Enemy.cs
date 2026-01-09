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

    protected Animator animator;
    private Vector3 originalPosition;

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
        UpdateHealthUI();
        originalPosition = transform.position;
    }

    void SetUpStatsByType()
    {
        switch (enemyType)
        {
            case EnemyType.FireSlime: maxHealth = 10; attackDamage = 3; break;
            case EnemyType.WaterSlime: maxHealth = 12; attackDamage = 2; break;
            case EnemyType.ElectricSlime: maxHealth = 10; attackDamage = 4; break;
            case EnemyType.Skeleton: maxHealth = 20; attackDamage = 5; break;
            case EnemyType.Boss: maxHealth = 50; attackDamage = 10; break;
        }
    }

    void UpdateHealthUI()
    {
        healthBarFill.fillAmount = (float)health / maxHealth;
    }

    public void ApplyElement(ElementType element, SpellType spellType)
    {
        Debug.Log($"Enemy hit with {element} {spellType}");

        if (spellType == SpellType.Status)
        {
            currentElement = element;
            Debug.Log($"Enemy is now affected by {element}");
        }

        if (element == ElementType.Bomb && currentElement != null)
        {
            HandleReaction(currentElement.Value);
            currentElement = null;
        }
    }

    void HandleReaction(ElementType existing)
    {
        switch (existing)
        {
            case ElementType.Water:
                Debug.Log("Vaporize");
                TakeDamage(5);
                break;
            case ElementType.Electricity:
                Debug.Log("Overload");
                TakeDamage(6);
                break;
            default:
                Debug.Log("No Reaction");
                break;
        }
    }

    // ─────────────────────────────
    // Perform Enemy Turn
    // ─────────────────────────────
    public IEnumerator PerformTurn()
    {

        animator.SetTrigger("Attack");


        float attackAnimLength = 0.7f; 
        yield return new WaitForSeconds(attackAnimLength);

        transform.position = originalPosition;
    }


    public void PerformAttackMove()
    {

        transform.position = originalPosition + transform.right * 1f;

        // Apply damage at same frame
        if (TurnManager.Instance != null && TurnManager.Instance.playerManager != null)
        {
            TurnManager.Instance.playerManager.TakeDamage(attackDamage);
        }
    }


    public void ResetPosition()
    {
        transform.position = originalPosition;
    }

    public void TakeDamage(int damage)
    {
        if (health <= 0) return;

        PlayHurtEffect();
        health -= damage;
        UpdateHealthUI();
        Debug.Log($"Enemy took {damage} damage");

        if (health <= 0) Die();
    }

    public void PlayHurtEffect() { /* animation / flash */ }

    public void PlayTargetedAttackEffect(ElementType element) { /* need to add visuals */ }

    void Die()
    {
        PlayDeathEffect();
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnEnemyKill(this);

        Destroy(gameObject);
        Debug.Log("Enemy defeated");
    }

    public void PlayDeathEffect() { /* add visuals */ }
}