using UnityEngine;

public class VfxImpact : MonoBehaviour
{
    public float duration = 0.1f;
    public float magnitute = 0.1f;
    public void TriggerShake()
    {
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(duration, magnitute);
        }
    }
}
