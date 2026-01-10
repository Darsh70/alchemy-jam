using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private static readonly WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1);

    [Header("Stats")]
    public int maxHealth = 10;
    public int currentHealth;

    public int maxActionPoints = 2;
    public int currentActionPoints;

    public Enemy targetEnemy;

    [Header("UI")]
    public Image healthBarFill;
    public Image[] actionPointsIcons;

    [Header("Spells")]
    public GameObject magicCirclePrefab;
    public SpellMenu spellMenu;
    private int bombCounter = 0; 

    // ─────────────────────────────
    // Spell History (for combos)
    // ─────────────────────────────
    private Queue<SpellRecord> spellHistory = new();

    struct SpellRecord
    {
        public ElementType element;
        public SpellType spellType;

        public SpellRecord(ElementType e, SpellType s)
        {
            element = e;
            spellType = s;
        }
    }

    // ─────────────────────────────
    // Active Effects (HoT & DoT)
    // ─────────────────────────────
    private List<HoTRecord> activeHoTs = new();
    private Dictionary<Enemy, List<DoT>> activeDoTs = new();

    private class HoTRecord
    {
        public int amount;
        public int turnsLeft;
        public HoTRecord(int amt, int turns) { amount = amt; turnsLeft = turns; }
    }

    private class DoT
    {
        public ElementType element;
        public int damage;
        public int turnsLeft;
        public GameObject magicCircle;

        public DoT(ElementType e, int dmg, int turns, GameObject mc)
        {
            element = e;
            damage = dmg;
            turnsLeft = turns;
            magicCircle = mc;
        }
    }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentActionPoints = maxActionPoints;

        UpdateHealthUI();
        UpdateActionPoints();
    }

    // ─────────────────────────────
    // Turn Ticks (Called by TurnManager)
    // ─────────────────────────────

    public void TickHoTs()
    {
        for (int i = activeHoTs.Count - 1; i >= 0; i--)
        {
            Heal(activeHoTs[i].amount);
            activeHoTs[i].turnsLeft--;

            if (activeHoTs[i].turnsLeft <= 0)
                activeHoTs.RemoveAt(i);
        }
    }

    public void TickEnemyDoTs(Enemy enemy)
    {
        if (enemy == null || !activeDoTs.ContainsKey(enemy)) return;

        List<DoT> dots = activeDoTs[enemy];
        for (int i = dots.Count - 1; i >= 0; i--)
        {
            enemy.TakeDamage(dots[i].damage);
            dots[i].turnsLeft--;

            if (dots[i].turnsLeft <= 0)
            {
                if (dots[i].magicCircle != null) Destroy(dots[i].magicCircle);
                dots.RemoveAt(i);
            }
        }

        if (dots.Count == 0) activeDoTs.Remove(enemy);
    }

    public void ClearDoTsForEnemy(Enemy enemy)
    {
        if (activeDoTs.ContainsKey(enemy))
        {
            foreach (var dot in activeDoTs[enemy])
            {
                if (dot.magicCircle != null) Destroy(dot.magicCircle);
            }
            activeDoTs.Remove(enemy);
        }
    }

    // ─────────────────────────────
    // Core Casting Logic
    // ─────────────────────────────
    public void CastSpell(ElementType element, SpellType spellType)
    {
        if (TurnManager.Instance.currentState != TurnState.PlayerTurn)
            return;

        if (spellType == SpellType.Ultimate && element == ElementType.Bomb && bombCounter < 3)
        {
            Debug.Log($"Ultimate not ready. Charges: {bombCounter}/3");
            return;
        }

        bool costsAP = spellType == SpellType.Skill || spellType == SpellType.Status;
        bool endsTurn = spellType == SpellType.Single || spellType == SpellType.Skill;

        if (costsAP && currentActionPoints <= 0)
        {
            Debug.Log("Out of AP");
            return;
        }

        Debug.Log($"Casting {spellType} {element}");

        if (spellType == SpellType.Skill && element == ElementType.Water)
        {
            Heal(3);
        }
        else
        {
            if (targetEnemy == null && WaveManager.Instance.ActiveEnemies.Count > 0)
                targetEnemy = WaveManager.Instance.ActiveEnemies[0];

            if (targetEnemy == null && spellType != SpellType.Status)
            {
                Debug.LogWarning("No enemies available");
                return;
            }

            int damage = GetBaseDamage(element, spellType);

            if (spellType != SpellType.Status)
            {
                SpawnMagicCircle(targetEnemy.transform, element, spellType);
                if (damage > 0)
                    StartCoroutine(DelayedDamage(targetEnemy, damage, element));

                targetEnemy.ApplyElement(element, spellType);
            }
            else
            {
                if (targetEnemy != null)
                {
                    GameObject mc = SpawnMagicCircle(targetEnemy.transform, element, spellType, true);
                    AddDoT(targetEnemy, element, 1, 3, mc);
                }
            }
        }

        // Bomb counter for Ultimate attack
        if (element == ElementType.Bomb && spellType == SpellType.Single)
        {
            bombCounter++;
            Debug.Log($"Bomb counter: {bombCounter}/3");
        }
        else if (spellType == SpellType.Ultimate)
        {
            bombCounter = 0; 
        }

        RecordSpell(element, spellType);
        CheckComboOrReaction(targetEnemy, element, spellType);

        if (spellType == SpellType.Single)
        {
            if (currentActionPoints < maxActionPoints)
            {
                currentActionPoints++;
                UpdateActionPoints();
            }
        }
        else if (spellType == SpellType.Skill || spellType == SpellType.Status)
        {
            UseActionPoint();
        }

        if (endsTurn || currentActionPoints == 0)
        {
            StartCoroutine(EndTurnAfterDelay(1.2f));
        }

        spellMenu.HideAll();
    }

    void RecordSpell(ElementType element, SpellType spellType)
    {
        spellHistory.Enqueue(new SpellRecord(element, spellType));
        if (spellHistory.Count > 2)
            spellHistory.Dequeue();
    }

    void CheckComboOrReaction(Enemy enemy, ElementType element, SpellType spellType)
    {
        if (enemy != null && spellType == SpellType.Single && element == ElementType.Bomb && enemy.currentElement != null)
        {
            HandleReaction(enemy, enemy.currentElement.Value);
            enemy.currentElement = null;
        }

        if (spellHistory.Count < 2) return;

        SpellRecord[] history = spellHistory.ToArray();
        SpellRecord first = history[0];
        SpellRecord second = history[1];

        // Combo: HoT 
        if (first.spellType == SpellType.Skill && first.element == ElementType.Water &&
            second.spellType == SpellType.Single && second.element == ElementType.Water)
        {
            ApplyHoT(2, 3);
        }

        // Combo: AoE DoT
        if (first.spellType == SpellType.Skill && first.element == ElementType.Electricity &&
            second.spellType == SpellType.Single && second.element == ElementType.Electricity)
        {
            foreach (Enemy e in WaveManager.Instance.ActiveEnemies)
            {
                GameObject mc = SpawnMagicCircle(e.transform, ElementType.Electricity, SpellType.Status, true);
                AddDoT(e, ElementType.Electricity, 1, 3, mc);
            }
        }
    }

    GameObject SpawnMagicCircle(Transform enemyTransform, ElementType element, SpellType spellType, bool persistent = false)
    {
        Vector3 offset = spellType == SpellType.Status ? Vector3.up * 0.6f : Vector3.down * 0.5f;
        GameObject mc = Instantiate(magicCirclePrefab, enemyTransform.position + offset, Quaternion.identity);
        
        MagicCircle magicCircle = mc.GetComponent<MagicCircle>();
        if (magicCircle != null) magicCircle.SetColor(element);

        if (!persistent) Destroy(mc, 1.0f);
        return mc;
    }

    IEnumerator DelayedDamage(Enemy enemy, int damage, ElementType element)
    {
        yield return _waitForSeconds1;
        if (enemy == null) yield break;
        enemy.PlayTargetedAttackEffect(element);
        enemy.TakeDamage(damage);
    }

    void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    void ApplyHoT(int amount, int turns)
    {
        activeHoTs.Add(new HoTRecord(amount, turns));
    }

    void AddDoT(Enemy enemy, ElementType element, int damage, int turns, GameObject magicCircle)
    {
        if (!activeDoTs.ContainsKey(enemy))
            activeDoTs[enemy] = new List<DoT>();

        activeDoTs[enemy].Add(new DoT(element, damage, turns, magicCircle));
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            TurnManager.Instance.GameOver();
        }
        UpdateHealthUI();
    }

    void UseActionPoint()
    {
        currentActionPoints--;
        UpdateActionPoints();
    }

    void UpdateHealthUI() => healthBarFill.fillAmount = (float)currentHealth / maxHealth;

    void UpdateActionPoints()
    {
        for (int i = 0; i < actionPointsIcons.Length; i++)
        {
            int apIndexFromLeft = actionPointsIcons.Length - 1 - i;
            actionPointsIcons[i].color = apIndexFromLeft < currentActionPoints ? Color.white : Color.red;
        }
    }

    public void CastBomb() => CastSpell(ElementType.Bomb, SpellType.Single);
    public void CastBlackHole() => CastSpell(ElementType.Bomb, SpellType.Ultimate);
    public void CastWaterBall() => CastSpell(ElementType.Water, SpellType.Single);
    public void CastHeal() => CastSpell(ElementType.Water, SpellType.Skill);
    public void CastRain() => CastSpell(ElementType.Water, SpellType.Status);
    public void CastZap() => CastSpell(ElementType.Electricity, SpellType.Single);
    public void CastLightning() => CastSpell(ElementType.Electricity, SpellType.Skill);
    public void CastCharge() => CastSpell(ElementType.Electricity, SpellType.Status);

    int GetBaseDamage(ElementType elementType, SpellType spellType)
    {
        if (spellType == SpellType.Status) return 0;
        if (elementType == ElementType.Water && spellType == SpellType.Skill) return 0;
        if (elementType == ElementType.Electricity && spellType == SpellType.Skill) return 3;

        if (elementType == ElementType.Bomb && spellType == SpellType.Ultimate) return 25;

        return elementType switch
        {
            ElementType.Bomb => 3,
            ElementType.Water => 2,
            ElementType.Electricity => 2,
            _ => 1
        };
    }

    void HandleReaction(Enemy enemy, ElementType element)
    {
        if (element == ElementType.Water) enemy.TakeDamage(4);
        if (element == ElementType.Electricity)
        {
            foreach (Enemy e in WaveManager.Instance.ActiveEnemies) e.TakeDamage(2);
        }
    }

    IEnumerator EndTurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TurnManager.Instance.EndPlayerTurn();
    }
}