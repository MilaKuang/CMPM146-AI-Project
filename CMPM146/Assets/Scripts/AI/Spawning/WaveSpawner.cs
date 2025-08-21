using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // for the optional wave label

[DisallowMultipleComponent]
public class WaveSpawner : MonoBehaviour
{
    [Header("Scene References")]
    [Tooltip("Where enemies can appear. Place a few empties around the map and drop them here.")]
    public List<Transform> spawnPoints = new();

    [Header("Waves")]
    [Tooltip("Ordered list of waves. We'll loop if 'loop' is true.")]
    public List<WaveDefinition> waves = new();

    [Tooltip("Loop waves after finishing the last one (endless mode).")]
    public bool loop = false;

    [Header("UI (Optional)")]
    [Tooltip("(Optional) UGUI Text for showing Wave #. Leave empty if not needed.")]
    public Text waveLabel;

    // Events so other systems can react (music, doors, etc.).
    public System.Action<int> OnWaveStarted; // index
    public System.Action<int> OnWaveCleared; // index

    // internal state
    private int currentWaveIndex = -1; // so first ++ lands on 0
    private int aliveEnemies = 0;
    private bool spawning = false;

    private void Start()
    {
        // sanity checks to save me from silent fails
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("[WaveSpawner] No spawn points assigned.");
            enabled = false; return;
        }
        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("[WaveSpawner] No waves assigned.");
            enabled = false; return;
        }

        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        while (true)
        {
            // advance wave index (wrap if looping)
            currentWaveIndex++;
            if (currentWaveIndex >= waves.Count)
            {
                if (!loop)
                {
                    Debug.Log("[WaveSpawner] All waves complete.");
                    yield break; // done
                }
                currentWaveIndex = 0; // wrap
            }

            var wave = waves[currentWaveIndex];

            // announce wave start
            OnWaveStarted?.Invoke(currentWaveIndex);
            if (waveLabel) waveLabel.text = $"Wave {currentWaveIndex + 1}";

            // optional pre-delay per wave
            if (wave.startDelay > 0f) yield return new WaitForSeconds(wave.startDelay);

            // spawn entries
            spawning = true;
            foreach (var entry in wave.entries)
            {
                if (entry == null || entry.enemyPrefab == null || entry.count <= 0) continue;

                for (int i = 0; i < entry.count; i++)
                {
                    Spawn(entry.enemyPrefab);
                    if (entry.interval > 0f) yield return new WaitForSeconds(entry.interval);
                    else yield return null; // spread across frames even if instant
                }
            }
            spawning = false;

            // wait until everything we spawned is gone
            while (aliveEnemies > 0) yield return null;

            // wave is clear → tell listeners
            OnWaveCleared?.Invoke(currentWaveIndex);

            // post-wave chill (gives the player a breather)
            if (wave.postWaveDelay > 0f) yield return new WaitForSeconds(wave.postWaveDelay);
        }
    }

    private void Spawn(GameObject prefab)
    {
        if (spawnPoints.Count == 0 || prefab == null) return;

        var point = spawnPoints[Random.Range(0, spawnPoints.Count)];
        var go = Instantiate(prefab, point.position, point.rotation);

        // try to hook the Health death event so we can track alive count
        var health = go.GetComponent<Health>();
        if (health == null) health = go.GetComponentInChildren<Health>(); // some prefabs hide it on a child

        aliveEnemies++;
        if (health != null)
        {
            // local method to capture this instance safely and avoid leaks
            void OnDead(Health h)
            {
                if (h != null) h.OnDeath -= OnDead;
                aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
            }
            health.OnDeath += OnDead;
        }
        else
        {
            // if there's no Health component, we assume the enemy will call back via other means (not ideal)
            Debug.LogWarning($"[WaveSpawner] Spawned '{prefab.name}' without Health. Consider adding Health to prefab.");
        }
    }

    // --- Public helpers I like to have around ---
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public bool IsSpawning => spawning;
    public int AliveEnemies => aliveEnemies;

    // Draw gizmos so spawn points are obvious while editing.
    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null) return;
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.75f);
        foreach (var t in spawnPoints)
        {
            if (t == null) continue;
            Gizmos.DrawSphere(t.position, 0.25f);
        }
    }
}
