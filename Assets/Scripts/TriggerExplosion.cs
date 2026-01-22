using UnityEngine;
using System.Collections.Generic; // Needed for List

public enum ReactionEffectType
{
    None,
    Vaporize, 
    Overload  
}

public class TriggerExplosion : MonoBehaviour
{
    [Header("Default")]
    public GameObject normalExplosion;

    [Header("Reactions")]
    public GameObject vaporizeExplosion; 
    public GameObject overloadExplosion; 

    // Internal State
    private ReactionEffectType currentReaction = ReactionEffectType.None;
    private Vector3 explosionOffset = Vector3.zero;
    
    // Text Data
    private string delayedFeedbackText = "";
    private Color delayedFeedbackColor = Color.white;

    // Damage Data (The Payload)
    private Enemy singleTarget;
    private int damageAmount;


    public void SetExplosionOffset(Vector3 offset) => explosionOffset = offset;

    public void SetupVaporize(Enemy target, int damage)
    {
        currentReaction = ReactionEffectType.Vaporize;
        singleTarget = target;
        damageAmount = damage;
        delayedFeedbackText = "VAPORIZE!";
        ColorUtility.TryParseHtmlString("#0089ff", out delayedFeedbackColor);

    }

    public void SetupOverload(int damage)
    {
        currentReaction = ReactionEffectType.Overload;
        damageAmount = damage; // AOE damage amount
        delayedFeedbackText = "OVERLOAD\n<size=70%>(STUN)</size>";
        delayedFeedbackColor = FeedbackManager.Instance.reactionColor;
    }


    public void Explode()
    {

        GameObject prefabToSpawn = normalExplosion;
        string soundName = "Explosion";
        switch (currentReaction)
        {
            case ReactionEffectType.Vaporize:
                if (vaporizeExplosion != null) prefabToSpawn = vaporizeExplosion;
                break;
            case ReactionEffectType.Overload:
                if (overloadExplosion != null) prefabToSpawn = overloadExplosion;
                break;
        }

        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, transform.position + explosionOffset, Quaternion.identity);
        }


        if (!string.IsNullOrEmpty(delayedFeedbackText))
        {
            Vector3 textPos = transform.position + explosionOffset + (Vector3.up * 0.7f);
            FeedbackManager.Instance.ShowText(delayedFeedbackText, textPos, delayedFeedbackColor);
        }


        if (currentReaction == ReactionEffectType.Vaporize)
        {
            if (singleTarget != null)
            {
                soundName = "Vaporize";
                singleTarget.TakeDamage(damageAmount);
                CameraShake.Instance.Shake(0.2f, 0.4f);
            }
        }
        else if (currentReaction == ReactionEffectType.Overload)
        {

            if (WaveManager.Instance != null)
            {
               
                List<Enemy> allEnemies = new List<Enemy>(WaveManager.Instance.ActiveEnemies);
                foreach (Enemy e in allEnemies)
                {
                    if (e != null) e.TakeDamage(damageAmount);
                    e.isStunned = true; 
                }
            }
            CameraShake.Instance.Shake(0.3f, 0.3f);
        }
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(soundName);
        }

        Destroy(gameObject);
    }
}