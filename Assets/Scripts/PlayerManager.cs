using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public int maxHealth = 10;
    public int currentHealth;

    public int maxActionPoints = 3;
    public int currentActionPoints;
    public Enemy targetEnemy;

    [Header("UI")]
    public Image healthBarFill;
    public Image[] actionPointsIcons;

    public GameObject magicCirclePrefab;

    public SpellMenu spellMenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
        currentActionPoints = maxActionPoints;

        UpdateHealthUI();
        UpdateActionPoints();
    }

    void UpdateHealthUI()
    {
        healthBarFill.fillAmount = (float)currentHealth/maxHealth;
    }

    void UpdateActionPoints()
    {
        for(int i = 0; i < actionPointsIcons.Length; i++)
        {
            int apIndexFromLeft = actionPointsIcons.Length - 1- i;
            actionPointsIcons[i].color = apIndexFromLeft < currentActionPoints ? Color.white : new Color(1,1,1,0.25f);
        }
    }

    public void UseActionPoint()
    {
        if (TurnManager.Instance.currentState != TurnState.PlayerTurn) 
            return;
        if (currentActionPoints > 0)
        {
            currentActionPoints--;
            Debug.Log("Action Point used. Remaining" + currentActionPoints);
            UpdateActionPoints();

            if (currentActionPoints == 0)
            {
                TurnManager.Instance.EndPlayerTurn();
            }
        }
        else
        {
            Debug.Log("No Action Points left");
        }
    }

    public void ResetActionPoints()
    {
        currentActionPoints = maxActionPoints;
        UpdateActionPoints();
    }

    public void CastSpell(ElementType element, SpellType spellType)
    {
        if (TurnManager.Instance.currentState != TurnState.PlayerTurn)
            return;
        
                if (currentActionPoints <= 0)
        {
            Debug.Log("Out of AP");
            return;
        }

        Debug.Log($"Casting {spellType} {element}");

        if (spellType != SpellType.AOE && targetEnemy == null)
        {
            if (WaveManager.Instance.ActiveEnemies.Count > 0)
                targetEnemy = WaveManager.Instance.ActiveEnemies[0];
            else
            {
                Debug.LogWarning("No enemies selected");
                return;
            }
        }

        if (spellType == SpellType.Single || spellType == SpellType.Status)
        {
            SpawnMagicCircle(targetEnemy.transform, element, spellType);
        }

        UseActionPoint();
        int damage = GetBaseDamage(element, spellType);

        if(spellType == SpellType.AOE)
        {
            foreach(Enemy enemy in new List<Enemy>(WaveManager.Instance.ActiveEnemies))
            {
                SpawnMagicCircle(enemy.transform, element, spellType);
                if (damage > 0)
                {
                    StartCoroutine(DelayedDamage(enemy, damage, element));
                }
            }
        }
        else
        {

            if (damage > 0)
            {
                StartCoroutine(DelayedDamage(targetEnemy, damage, element));
            }
            
            targetEnemy.ApplyElement(element, spellType);
        }
        
        Debug.Log($"CASTING: {element}");

        spellMenu.HideAll();
    }

    public void SpawnMagicCircle(Transform enemyTransform, ElementType element, SpellType spellType)
    {
        Vector3 offset = Vector3.zero;

        if (spellType == SpellType.Status)
            offset = Vector3.up * 0.6f; 
        else
            offset = Vector3.down * 0.5f;

        Vector3 spawnPos = enemyTransform.position + offset;

        GameObject mc = Instantiate(magicCirclePrefab, spawnPos, Quaternion.identity);
        MagicCircle magicCircle = mc.GetComponent<MagicCircle>();
        if (magicCircle != null)
        {
            magicCircle.SetColor(element); 
        }
        
    }

    IEnumerator DelayedDamage(Enemy enemy, int damage, ElementType element)
    {
        yield return new WaitForSeconds(1);
        enemy.PlayTargetedAttackEffect(element);
        enemy.TakeDamage(damage);

    }


    public void TakeDamage(int damage)
    {
        PlayHurtEffect();
        currentHealth -= damage;
        if (currentHealth < 0) {
            currentHealth = 0;
            TurnManager.Instance.GameOver();
        }

        UpdateHealthUI();
        Debug.Log($"Player took {damage} damage. Current HP {currentHealth}");
    }

    public void CastBomb()
    {
        CastSpell(ElementType.Bomb, SpellType.Single);
    }

    public void CastWaterBall()
    {
        CastSpell(ElementType.Water, SpellType.Single);
    }

    public void CastWaterBlast()
    {
        CastSpell(ElementType.Water, SpellType.AOE);
    }

    public void CastRain()
    {
        CastSpell(ElementType.Water, SpellType.Status);
    }

    public void CastZap()
    {
        CastSpell(ElementType.Electricity, SpellType.Single);
    }

    public void CastLightning()
    {
        CastSpell(ElementType.Electricity, SpellType.AOE);
    }

    public void CastCharge()
    {
        CastSpell(ElementType.Electricity, SpellType.Status);
    }

    int GetBaseDamage(ElementType elementType, SpellType spellType)
    {
        if (spellType == SpellType.Status)
            return 0;
        
        if (elementType == ElementType.Water && spellType == SpellType.AOE)
            return 2;
        
        if (elementType == ElementType.Electricity && spellType == SpellType.AOE)
            return 3;

        return elementType switch
        {
            ElementType.Bomb => 3,
            ElementType.Water => 2,
            ElementType.Electricity => 2,
            _ => 1,
        };
    }

    public void PlayHurtEffect()
    {
        
    }

}
