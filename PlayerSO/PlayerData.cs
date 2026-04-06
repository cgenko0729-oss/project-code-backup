using NUnit.Framework;
using UnityEngine;

public enum JobId
{
    DogKnight,
    Wizard,
    Archer,
    Warrior,
    MAX,
}

[CreateAssetMenu(fileName = "PlayerData", menuName = "GameData/PlayerData")]
public class PlayerData : ScriptableObject
{
    // 各キャラ１体分のステータスデータ
    [System.Serializable]
    public struct StatusData
    {
        [Header("基本的なステータス")]
        public JobId jobId;
        public float maxHp;
        public float moveSpeed;
        public float dashSpeed;

        [Header("ダッシュ継続時間(秒)")]
        public float dashDuration;

        [Header("ダッシュクールダウン(秒)")]
        public float dashCoolDown;

        [Header("キャラの解放済みフラグ")]
        public bool isUnlocked;
    }

    [Header("選択中のジョブの種類")]
    public JobId jobId;

    [Header("全キャラの各ステータス")]
    public StatusData[] StatusDataList;
}
