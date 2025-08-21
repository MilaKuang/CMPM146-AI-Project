using UnityEngine;

public class KillAfterSeconds : MonoBehaviour
{
    [Min(0.1f)] public float lifetime = 3f; // tweak in Inspector during testing
    private void Start() => Invoke(nameof(Go), lifetime);
    private void Go() => GetComponent<Health>()?.Die();
}
