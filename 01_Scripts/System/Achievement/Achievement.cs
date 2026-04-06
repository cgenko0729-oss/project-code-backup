using UnityEngine;


[CreateAssetMenu(fileName = "New Achievement", menuName = "AchievementData",order = -759)]
public class Achievement : ScriptableObject
{
    public AchievementType achievementType = AchievementType.Incremental;

    public string id; // Unique ID, e.g., "stage1_cleared"
    public string steamApiName; // The API Name you set in Steamworks backend
    public string title;
    [TextArea]
    public string description;
    public Sprite icon;

    [Header("Progress Tracking")]
    public bool isIncremental; // Is this a multi-step achievement? (e.g., kill 100 enemies)
    public int valueToUnlock; // The value needed to unlock (e.g., 100 for enemies killed)

    public int rewardCoin = 0;

    public bool isCoinReward = true; // only coin as reward


}

