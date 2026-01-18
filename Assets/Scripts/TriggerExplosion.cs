using UnityEngine;

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

    private ReactionEffectType currentReaction = ReactionEffectType.None;


    public void SetReactionType(ReactionEffectType type)
    {
        currentReaction = type;
    }

    // Called by Animation Event
    public void Explode()
    {
        GameObject prefabToSpawn = normalExplosion;

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
            Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}