using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    public float delay = 0.7f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject,delay);
    }

}
