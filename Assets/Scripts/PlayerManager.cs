using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
    public Sprite activeAPSprite;
    public Sprite emptyAPSprite;

    [Header("Spells")]
    public GameObject magicCirclePrefab;
    public SpellMenu spellMenu;
    private int bombCounter = 0; 

    [Header("VFX")]
    public GameObject zapPrefab;
    public GameObject lightningPrefab;
    public GameObject chargePrefab;
    public GameObject dotPrefab;
    public GameObject waterBallPrefab;
    public GameObject rainPrefab;
    public GameObject bombPrefab;

    [Header("UI VFX")]
    public Image healGlowImage;



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
    // Turn Ticks
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

            if (dotPrefab != null)
            {

                Vector3 spawnPos = enemy.transform.position; 
                
                GameObject vfx = Instantiate(dotPrefab, spawnPos, Quaternion.identity);
            }

            if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.1f, 0.05f);


            dots[i].turnsLeft--;

            if (dots[i].turnsLeft <= 0)
            {
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
        if (TurnManager.Instance.currentState == TurnState.Stopped) 
            return;
        
        if (!GameManager.Instance.isGameActive) return;

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


        if (targetEnemy == null && WaveManager.Instance.ActiveEnemies.Count > 0)
            targetEnemy = WaveManager.Instance.ActiveEnemies[0];

        if (targetEnemy == null && spellType != SpellType.Status)
        {
            Debug.LogWarning("No enemies available");
            return;
        }

        // Damage Calculation and Reaction Check
        // Get base damage , then check if reaction overrides it
        int damage = GetBaseDamage(element, spellType);
        
        RecordSpell(element, spellType);
        
        bool reactionTriggered = CheckComboOrReaction(targetEnemy, element, spellType);

        if (reactionTriggered)
        {
            damage = 0; // If Overload/Vaporize, the Bomb deals 0 additional damage
        }

        Debug.Log($"Casting {spellType} {element}. Base Damage: {damage}");


        if (spellType == SpellType.Skill && element == ElementType.Water)
        {

            Heal(3);
        }
        else if (spellType == SpellType.Status)
        {
            // Status Spells
            if (targetEnemy != null)
            {
                GameObject mc = SpawnMagicCircle(targetEnemy.transform, element, spellType, true);
                targetEnemy.currentStatusCircle = mc;
                targetEnemy.ApplyElement(element, spellType);
            }
        }
        else 
        {
            // Attack Spells 
            if (targetEnemy != null)
            {

                SpawnMagicCircle(targetEnemy.transform, element, spellType);
                targetEnemy.PlayTargetedAttackEffect(element, spellType);
                float delay = 0.3f;
                if (element == ElementType.Bomb && spellType == SpellType.Single)
                {
                    delay = 7f / 12f; 
                }
                else if (element == ElementType.Electricity && spellType == SpellType.Single)
                {
                    delay = 3f/12f;
                }
                else if (element == ElementType.Water && spellType == SpellType.Single)
                {
                    delay = 9f/12f;
                }
                
                StartCoroutine(DelayedDamage(targetEnemy, damage, element, spellType, delay));

                targetEnemy.ApplyElement(element, spellType);
            }
        }


        if (element == ElementType.Bomb && spellType == SpellType.Single)
        {
            bombCounter++;
            Debug.Log($"Bomb counter: {bombCounter}/3");
        }
        else if (spellType == SpellType.Ultimate)
        {
            bombCounter = 0; 
        }


        if (spellType == SpellType.Single)
        {
            // Single spells generate 1 AP
            if (currentActionPoints < maxActionPoints)
            {
                currentActionPoints++;
                UpdateActionPoints();
            }
        }
        else if (costsAP)
        {
            // Status/Skill spells consume 1 AP
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


    bool CheckComboOrReaction(Enemy enemy, ElementType element, SpellType spellType)
    {
        bool reactionTriggered = false;

        // REACTIONS 
        if (enemy != null && element == ElementType.Bomb && enemy.currentElement != null)
        {
            if (enemy.currentElement == ElementType.Electricity)
            {
                FeedbackManager.Instance.ShowText("OVERLOAD!", enemy.transform.position, FeedbackManager.Instance.reactionColor);
                Debug.Log("OVERLOAD! 6 DMG to all enemies.");
                GameManager.Instance.LogReaction("Overload");
                List<Enemy> allEnemies = new List<Enemy>(WaveManager.Instance.ActiveEnemies);
                foreach (Enemy e in allEnemies)
                {
                    if (e != null) e.TakeDamage(6);
                }
                CameraShake.Instance.Shake(0.3f, 0.3f);
                reactionTriggered = true; 
            }
            else if (enemy.currentElement == ElementType.Water)
            {
                FeedbackManager.Instance.ShowText("VAPORIZE!", enemy.transform.position, FeedbackManager.Instance.reactionColor);
                Debug.Log("VAPORIZE! 9 DMG to target.");
                enemy.TakeDamage(9);
                GameManager.Instance.LogReaction("Vaporize");
                CameraShake.Instance.Shake(0.2f, 0.4f);
                reactionTriggered = true; 
            }

            if (reactionTriggered)
            {
                // Consume status aura
                if (enemy.currentStatusCircle != null)
                {
                    Destroy(enemy.currentStatusCircle);
                    enemy.currentStatusCircle = null;
                }
                enemy.currentElement = null;
                return true; 
            }
        }

        // COMBOS 
        if (spellHistory.Count < 2) return false;

        SpellRecord[] history = spellHistory.ToArray();
        SpellRecord first = history[0];
        SpellRecord second = history[1];

        if (first.spellType == SpellType.Skill && first.element == ElementType.Water &&
            second.spellType == SpellType.Single && second.element == ElementType.Water)
        {
            string msg = "HYDRO THERAPY\n<size=70%>Regen: 3HP / 3 Turns</size>";
            FeedbackManager.Instance.ShowText(msg, enemy.transform.position + Vector3.up * 1f, FeedbackManager.Instance.healComboColor);
            GameManager.Instance.LogCombo("HydroTherapy");
            ApplyHoT(2, 3);
        }

        if (first.spellType == SpellType.Skill && first.element == ElementType.Electricity &&
            second.spellType == SpellType.Single && second.element == ElementType.Electricity)
        {
            string msg = "ELECTRO FIELD\n<size=70%>3 DMG / 3 Turns</size>";
            FeedbackManager.Instance.ShowText(msg, enemy.transform.position + Vector3.up * 1f, FeedbackManager.Instance.electricComboColor);
            GameManager.Instance.LogCombo("ElectroField");
            foreach (Enemy e in WaveManager.Instance.ActiveEnemies)
            {
                if (e != null)
                {
                    AddDoT(e, ElementType.Electricity, 1, 3, null);
                }
            }
        }

        return false; 
    }
    GameObject SpawnMagicCircle(Transform enemyTransform, ElementType element, SpellType spellType, bool persistent = false)
    {
        Enemy e = enemyTransform.GetComponent<Enemy>();

        Vector3 offset = Vector3.zero;

        if (e != null)
        {
            // Use the enemy's specific settings
            if (spellType == SpellType.Status)
            {
                offset = Vector3.up * e.headOffset;
            }
            else
            {
                offset = Vector3.up * e.floorOffset; // usually a negative number
            }
        }
        else
        {
            // Fallback default if script is missing
            offset = (spellType == SpellType.Status) ? Vector3.up * 1.8f : Vector3.down * 0.5f;
        }
        
        GameObject mc = Instantiate(magicCirclePrefab, enemyTransform.position + offset, Quaternion.identity);

        mc.transform.SetParent(enemyTransform);

        MagicCircle magicCircleComponent = mc.GetComponent<MagicCircle>();
        if (magicCircleComponent != null)
        {
            magicCircleComponent.isPersistent = persistent;
            magicCircleComponent.SetColor(element);
        }

        // If Electricity Status, spawn the Charge VFX as a child
        if (persistent && element == ElementType.Electricity && chargePrefab != null)
        {
            GameObject charge = Instantiate(chargePrefab, mc.transform.position, Quaternion.identity);
            charge.transform.SetParent(mc.transform); 
        }
        else if (persistent && element == ElementType.Water && rainPrefab != null)
        {
            GameObject rain = Instantiate(rainPrefab, mc.transform.position, Quaternion.identity);
            rain.transform.SetParent(mc.transform);
        }

        return mc;
    }

    IEnumerator DelayedDamage(Enemy enemy, int damage, ElementType element, SpellType spellType,float delayTime)
    {
        yield return new WaitForSeconds(delayTime); 
        if (enemy == null) yield break;
        enemy.TakeDamage(damage);
    }

    void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();

        if (healGlowImage != null)
        {
            StopCoroutine("HealGlowRoutine"); 
            StartCoroutine(HealGlowRoutine());
        }

    }

    IEnumerator HealGlowRoutine()
    {
        float duration = 1f;
        float elapsed = 0f;
        Color color = healGlowImage.color;

        // Fade In
        while (elapsed < duration * 0.3f)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0, 0.7f, elapsed / (duration * 0.3f));
            healGlowImage.color = color;
            yield return null;
        }

        // Fade Out
        elapsed = 0f;
        while (elapsed < duration * 0.7f)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(0.7f, 0, elapsed / (duration * 0.7f));
            healGlowImage.color = color;
            yield return null;
        }

        color.a = 0;
        healGlowImage.color = color;
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
            GameManager.Instance.TriggerGameOver();
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
            if (apIndexFromLeft < currentActionPoints) {
            actionPointsIcons[i].sprite = activeAPSprite;
            actionPointsIcons[i].color = Color.white;
        } else {
            actionPointsIcons[i].sprite = emptyAPSprite;

            actionPointsIcons[i].color = new Color(1, 1, 1, 0.3f); 
        }
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

    IEnumerator EndTurnAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        TurnManager.Instance.EndPlayerTurn();
    }

    public bool IsReactionAvailable()
    {
        if (WaveManager.Instance == null) return false;

        foreach (Enemy e in WaveManager.Instance.ActiveEnemies)
        {
            if (e != null && e.currentElement != null)
                return true;
        }
        return false;
    }

    public bool IsComboReady(ElementType element, SpellType spellType)
    {
        if (spellHistory.Count == 0) return false;

        // Get the last spell cast
        SpellRecord lastSpell = spellHistory.ToArray()[spellHistory.Count - 1];

        if (spellType == SpellType.Single && 
            lastSpell.spellType == SpellType.Skill && 
            lastSpell.element == element)
        {
            return true; 
        }

        return false;
    }
}