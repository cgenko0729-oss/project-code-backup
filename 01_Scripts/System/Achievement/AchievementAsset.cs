using NUnit.Framework;
using UnityEngine;


[CreateAssetMenu(menuName = "Achievements/AchievementData", fileName = "ACH_NewAchievement",order = -770)]
public class AchievementAsset : ScriptableObject
{
     [Tooltip("Stable unique key used in save files. Keep this constant.")]
    [SerializeField] private string key;

    [Header("Display")]
    public string displayName = "New Achievement";
    [TextArea] public string description = "Description...";
    public Sprite icon;
    public bool hidden = false;
    public int sortOrder = 0;
    public string[] tags;

    [Header("Logic")]
    public bool isCounter = false;
    [Min(1)] public int threshold = 1; // used if isCounter = true

    [Header("Platform (optional)")]
    [Tooltip("Steamworks Achievement API name, e.g. ACH_KILL_100")]
    public string steamId;

    public string Key => key;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Generate a stable key if empty, based on the asset GUID (rename-safe).
        if (string.IsNullOrEmpty(key))
        {
            var path = UnityEditor.AssetDatabase.GetAssetPath(this);
            var guid = UnityEditor.AssetDatabase.AssetPathToGUID(path);
            key = $"ach_{guid}".ToLowerInvariant();
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}

