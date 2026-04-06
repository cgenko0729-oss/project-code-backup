using UnityEngine;


public enum AchievementType
{
    SingleAction, // For achievements unlocked by a single event (e.g., clear a stage)
    Incremental   // For achievements that require accumulating a value (e.g., kill 100 enemies)
}
