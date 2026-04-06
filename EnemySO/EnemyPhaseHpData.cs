using UnityEngine;


[System.Serializable]
public class EnemyPhaseHpData
{
    public EnemyType enemyType;

    [Tooltip("HP values for each game phase. Index 0 = Phase 1, Index 1 = Phase 2, etc.")]
    public float[] hpByPhase = new float[7]; // Array to hold HP for 7 phases
    public float[] moveSpdByPhase = new float[7]; // Array to hold Move Speed for 7 phases
    //public float[] attackByPhase = new float[7]; // Array to hold Attack for 7 phases
}

