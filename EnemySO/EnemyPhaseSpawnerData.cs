using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class EnemyPhaseSpawnerData 
{
    public EnemyType enemyType;

    [Tooltip("The maximum number of this enemy type allowed on the map for each phase.")]
    public int[] maxNumByPhase = new int[7];

    [Tooltip("The minimum number of enemies to spawn in a single wave for each phase.")]
    public int[] minEnemiesByPhase = new int[7];

    [Tooltip("The maximum number of enemies to spawn in a single wave for each phase.")]
    public int[] maxEnemiesByPhase = new int[7];
    
    [Tooltip("The base cooldown between spawn waves for each phase.")]
    public float[] spawnCooldownByPhase = new float[7];

}

