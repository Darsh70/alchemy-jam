using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    private static readonly WaitForSeconds _waitForSeconds1 = new WaitForSeconds(1);

    [Header("Stats")]
    public int maxHealth = 60;
    public int currentHealth;

    public int maxActionPoints = 2;
    public int currentActionPoints;

    public Enemy targetEnemy;

    [Header("UI")]
    public Image healthBarFill;
    public Image healthBarGhost;
    public Image[] actionPointsIcons;
    public Sprite activeAPSprite;
    public Sprite emptyAPSprite;

    private float ghostDelay = 0.5f;
    private float ghostSpeed = 2f; 
    private Coroutine healthRoutine;


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
    public GameObject voidEyePrefab;

    [Header("UI VFX")]
    public RectTransform magePortrait; 

    [Header("Button UI")]
    public TextMeshProUGUI bombCount;



    // ─────────────────────────────
    // Spell History (for combos)
    // ─────────────────────────────
    private Queue<SpellRecord> spellHistory = new();

    struct SpellRecord
    {
        public ElementType element;
        public SpellType spellType;
        public int turnCast;

        public SpellRecord(ElementType e, SpellType s,int t)
        {
            element = e;
            spellType = s;
            turnCast = t;
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
        UpdateBombCounterUI();
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
            if (TurnManager.Instance.currentState == TurnState.Stopped) return;
            if (!GameManager.Instance.isGameActive) return;
            if (TurnManager.Instance.currentState != TurnState.PlayerTurn) return;

            if (spellType == SpellType.Ultimate && element == ElementType.Bomb && bombCounter < 3)
            {
                AudioManager.Instance.PlaySFX("Error");
                Debug.Log($"Ultimate not ready. Charges: {bombCounter}/3");
                return;
            }

            bool costsAP = spellType == SpellType.Skill || spellType == SpellType.Status;
            bool endsTurn = spellType == SpellType.Single || spellType == SpellType.Skill || spellType == SpellType.Ultimate;

            if (costsAP && currentActionPoints <= 0)
            {
                AudioManager.Instance.PlaySFX("Error");
                Debug.Log("Out of AP");
                return;
            }

            if (targetEnemy == null && WaveManager.Instance.ActiveEnemies.Count > 0)
                targetEnemy = WaveManager.Instance.ActiveEnemies[0];

            if (targetEnemy == null && spellType != SpellType.Status)
            {
                AudioManager.Instance.PlaySFX("Error");
                Debug.LogWarning("No enemies available");
                return;
            }

            AudioManager.Instance.PlaySFX("Click");

            // ---------------------------------------------------------
            // Save the element BEFORE CheckComboOrReaction wipes it!
            // ---------------------------------------------------------
            ElementType? storedReactionElement = null;
            if (targetEnemy != null) 
            {
                storedReactionElement = targetEnemy.currentElement;
            }
            // ---------------------------------------------------------

            int damage = GetBaseDamage(element, spellType);
            
            RecordSpell(element, spellType);
            
            // This function sets targetEnemy.currentElement to null if reaction happens!
            bool reactionTriggered = CheckComboOrReaction(targetEnemy, element, spellType);

            if (reactionTriggered)
            {
                damage = 0; 
            }

            Debug.Log($"Casting {spellType} {element}. Base Damage: {damage}");


            if (spellType == SpellType.Skill && element == ElementType.Water)
            {
                Heal(5);
            }
            else if (spellType == SpellType.Status)
            {
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

                    // 1. Spawn the visual
                    GameObject spellObject = targetEnemy.PlayTargetedAttackEffect(element, spellType);
                
                    // 2. CONFIGURE BOMB
                    if (element == ElementType.Bomb && spellObject != null)
                    {
                        TriggerExplosion triggerScript = spellObject.GetComponent<TriggerExplosion>();
                        
                        if (triggerScript != null && targetEnemy != null)
                        {

                            Vector3 feetPos = targetEnemy.transform.position + (Vector3.up * targetEnemy.floorOffset);
                            Vector3 headPos = targetEnemy.transform.position + (Vector3.up * targetEnemy.headOffset);
                            Vector3 centerPos = (feetPos + headPos) / 2f;


                            Vector3 offset = centerPos - spellObject.transform.position;


                            triggerScript.SetExplosionOffset(offset);
                          

                            if (storedReactionElement != null)
                            {
                                if (storedReactionElement == ElementType.Water)
                                {

                                    triggerScript.SetupVaporize(targetEnemy, 15);
                                }
                                else if (storedReactionElement == ElementType.Electricity)
                                {

                                    triggerScript.SetupOverload(10);
                                }
                            }
                        }
                    }

                    float delay = 0.3f;
                    if (element == ElementType.Bomb && spellType == SpellType.Single)
                    {
                        delay = 7f / 12f; 
                    }
                    else if (element == ElementType.Electricity && spellType == SpellType.Single)
                    {
                        AudioManager.Instance.PlaySFX("Zap");
                        delay = 3f/12f;
                    }
                    else if (element == ElementType.Electricity && spellType == SpellType.Skill)
                    {
                        AudioManager.Instance.PlaySFX("Lightning");
                        delay = 1f/12f;
                    }
                    else if (element == ElementType.Water && spellType == SpellType.Single)
                    {
                        AudioManager.Instance.PlaySFX("WaterBall");
                        delay = 9f/12f;
                    }
                    
                    StartCoroutine(DelayedDamage(targetEnemy, damage, element, spellType, delay));

                    targetEnemy.ApplyElement(element, spellType);
                }
            }


            if (element == ElementType.Bomb && spellType == SpellType.Single)
            {
                bombCounter++;
                UpdateBombCounterUI();
                Debug.Log($"Bomb counter: {bombCounter}/3");
            }
            else if (spellType == SpellType.Ultimate)
            {
                bombCounter = 0; 
            }


            if (spellType == SpellType.Single && element != ElementType.Bomb)
            {
                if (currentActionPoints < maxActionPoints)
                {
                    currentActionPoints++;
                    UpdateActionPoints();
                }
            }
            else if (costsAP)
            {
                UseActionPoint();
            }

            if (endsTurn || (currentActionPoints == 0 && spellType != SpellType.Status))
            {
                StartCoroutine(EndTurnAfterDelay(1.2f));
            }

            spellMenu.HideAll();
        }

    void UpdateBombCounterUI()
    {
        if (bombCount == null) return;

        int remaining = 3 - bombCounter;

        if (remaining > 0)
        {
            bombCount.text = $"{remaining}x BOMB"; 
            bombCount.color = Color.white; 
        }
        else
        {
            // Shows explicit confirmation instead of disappearing
            bombCount.text = "\nREADY!"; 
            bombCount.color = Color.magenta; // Use your Void purple color
        }
    }

    void RecordSpell(ElementType element, SpellType spellType)
    {
        int currentTurn = TurnManager.Instance.currentTurnIndex;

        // Save it in the record
        spellHistory.Enqueue(new SpellRecord(element, spellType, currentTurn));
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
                Debug.Log("OVERLOAD Configured.");
                GameManager.Instance.LogReaction("Overload");
                reactionTriggered = true; 
            }
            else if (enemy.currentElement == ElementType.Water)
            {

                Debug.Log("VAPORIZE Configured.");
                GameManager.Instance.LogReaction("Vaporize");
                reactionTriggered = true; 
            }

            if (reactionTriggered)
            {

                if (enemy.currentStatusCircle != null)
                {
                    Destroy(enemy.currentStatusCircle);
                    enemy.currentStatusCircle = null;
                }
                // Consume the element
                enemy.currentElement = null;
                return true; 
            }
        }


        // COMBOS 
        if (spellHistory.Count < 2) return false;

        SpellRecord[] history = spellHistory.ToArray();
        SpellRecord first = history[0];
        SpellRecord second = history[1];

        if (first.spellType == SpellType.Single && first.element == ElementType.Water &&
            second.spellType == SpellType.Skill && second.element == ElementType.Water)
        {
            string msg = "HYDRO THERAPY\n<size=70%>Regen: 5HP / 3 Turns</size>";
            FeedbackManager.Instance.ShowText(msg, enemy.transform.position + Vector3.up * 1f, FeedbackManager.Instance.healComboColor);
            GameManager.Instance.LogCombo("HydroTherapy");
            ApplyHoT(5, 3);
        }

        if (first.spellType == SpellType.Single && first.element == ElementType.Electricity &&
            second.spellType == SpellType.Skill && second.element == ElementType.Electricity)
        {
            string msg = "ELECTRO FIELD\n<size=70%>3 DMG / 3 Turns</size>";
            FeedbackManager.Instance.ShowText(msg, enemy.transform.position + Vector3.up * 1f, FeedbackManager.Instance.electricComboColor);
            GameManager.Instance.LogCombo("ElectroField");
            foreach (Enemy e in WaveManager.Instance.ActiveEnemies)
            {
                if (e != null)
                {
                    AddDoT(e, ElementType.Electricity, 4, 3, null);
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
                offset = Vector3.up * e.floorOffset; 
            }
        }
        else
        {

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
        int oldHealth = currentHealth;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        int actualHealed = currentHealth - oldHealth;

        UpdateHealthUI();


        if (actualHealed > 0)
        {

            if (magePortrait != null)
            {
                AudioManager.Instance.PlaySFX("Heal");
                string healText = $"<size=150%>+{actualHealed}</size>";
                FeedbackManager.Instance.ShowScreenText(healText, magePortrait.position, Color.green);
            }
        }

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
            GameManager.Instance.TriggerGameOver(false);
        }
        UpdateHealthUI();
    }

    void UseActionPoint()
    {
        currentActionPoints--;
        UpdateActionPoints();
    }

    void UpdateHealthUI() 
    {
        if (healthBarFill == null || healthBarGhost == null) return;

        float targetFill = (float)currentHealth / maxHealth;
        float currentFill = healthBarFill.fillAmount;

        if (Mathf.Abs(targetFill - currentFill) < 0.001f) return;

        // Stop any running animation so we don't glitch out on rapid hits
        if (healthRoutine != null) StopCoroutine(healthRoutine);


        if (targetFill < currentFill)
        {

            healthBarGhost.color = new Color(1f, 0.33f, 0f); 
            healthBarGhost.fillAmount = currentFill;

            healthBarFill.fillAmount = targetFill;


            healthRoutine = StartCoroutine(AnimateGhostBar(targetFill));
        }

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
    public void CastBlackHole()
    {
        if (bombCounter < 3) return;
        CastSpell(ElementType.Bomb, SpellType.Ultimate);
        Vector3 spawnPos = new Vector3(0, 10, 0); 
        Instantiate(voidEyePrefab, spawnPos, Quaternion.identity); 
        bombCounter = 0;
        UpdateBombCounterUI();
    } 
    public void CastWaterBall() => CastSpell(ElementType.Water, SpellType.Single);
    public void CastHeal() => CastSpell(ElementType.Water, SpellType.Skill);
    public void CastRain() => CastSpell(ElementType.Water, SpellType.Status);
    public void CastZap() => CastSpell(ElementType.Electricity, SpellType.Single);
    public void CastLightning() => CastSpell(ElementType.Electricity, SpellType.Skill);
    public void CastCharge() => CastSpell(ElementType.Electricity, SpellType.Status);

    int GetBaseDamage(ElementType elementType, SpellType spellType)
    {
        if (spellType == SpellType.Status || spellType == SpellType.Ultimate) return 0;
        if (elementType == ElementType.Water && spellType == SpellType.Skill) return 0;
        if (elementType == ElementType.Electricity && spellType == SpellType.Skill) return 5;

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

        if (spellType == SpellType.Skill && 
            lastSpell.spellType == SpellType.Single && 
            lastSpell.element == element)
        {
            return true; 
        }

        return false;
    }
    public bool IsAnyComboReady()
    {

        if (spellHistory.Count == 0) return false;


        SpellRecord[] arr = spellHistory.ToArray();
        SpellRecord lastSpell = arr[arr.Length - 1];

        if (lastSpell.spellType == SpellType.Single && lastSpell.element != ElementType.Bomb)
        {
            if (lastSpell.turnCast < TurnManager.Instance.currentTurnIndex)
            {
                return true;
            }
        }

        return false;
    }
}