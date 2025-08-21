using UnityEngine;

[DisallowMultipleComponent]
public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // If the enemy has Health, route death through it so the spawner hears.
        other.GetComponent<Health>()?.Die();
    }
}
