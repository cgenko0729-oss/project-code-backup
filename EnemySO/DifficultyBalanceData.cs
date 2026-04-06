using UnityEngine;
using System.Collections.Generic;

//All enemy balance data By difficulty (Normal or Hard).
[System.Serializable]
public class DifficultyBalanceData
{
    public DifficultyType difficulty;
    public List<EnemyPhaseHpData> enemyHpDataList;

    public List<EnemyPhaseSpawnerData> enemySpawnerDataList; 
}

