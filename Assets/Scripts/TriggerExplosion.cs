using UnityEngine;

public class TriggerExplosion : MonoBehaviour
{
    public GameObject bombExplosion;
    public void Explode()
    {
        if (bombExplosion != null)
        {
            Instantiate(bombExplosion, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
