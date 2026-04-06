using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Achievements/Database", fileName = "AchievementDatabase")]
public class AchievementDatabase : ScriptableObject
{
    public List<AchievementAsset> achievements = new();

    public AchievementAsset GetByKey(string key)
        => achievements.Find(a => a && a.Key == key);

}

