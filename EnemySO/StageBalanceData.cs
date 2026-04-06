using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewStageBalanceData",order = -775)]  
public class StageBalanceData : ScriptableObject
{
    public MapType mapType;
    public DifficultyBalanceData normalModeStats;
    public DifficultyBalanceData hardModeStats;
    public DifficultyBalanceData nightmareModeStats;
    public DifficultyBalanceData hellModeStats;
}

