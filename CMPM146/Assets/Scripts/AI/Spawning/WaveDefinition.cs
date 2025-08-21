using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AI/Wave Definition", fileName = "Wave_")]
public class WaveDefinition : ScriptableObject
{
    [Tooltip("Delay before this wave begins spawning.")]
    public float startDelay = 2f;

    [Tooltip("Delay after the LAST enemy in this wave dies before next wave starts.")]
    public float postWaveDelay = 5f;

    [Tooltip("What to spawn for this wave, in order.")]
    public List<WaveEntry> entries = new();
}

[Serializable]
public class WaveEntry
{
    [Tooltip("Enemy prefab to spawn (must be a prefab).")]
    public GameObject enemyPrefab;

    [Min(1)]
    [Tooltip("How many to spawn in this entry.")]
    public int count = 5;

    [Min(0f)]
    [Tooltip("Seconds between consecutive spawns in this entry.")]
    public float interval = 0.5f;
}
