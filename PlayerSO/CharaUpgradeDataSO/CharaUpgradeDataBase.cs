using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharaUpgradeDataBase", menuName = "UpgradeData/CharaUpgradeDataBase")]
public class CharaUpgradeDataBase : ScriptableObject
{
    [Header("ジョブID")]
    public JobId jobId;

    [Header("このキャラの強化に使用されたコインの総数")]
    public int useTotalCoin = 0;

    [Header("ジョブごとの強化できるステート")]
    public List<CharaUpgradeDataSO> Upgrades;
}


