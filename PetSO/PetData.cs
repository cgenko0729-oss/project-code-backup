using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillInfo
{
    [Header("スキルの種類（アクティブかパッシブか）")]
    public SkillType skilltype;

    [Header("スキルのアイコン")]
    public Sprite skillicon;

    [Header("スキルの名前")]
    public string skillName;

    [Header("スキルの説明")]
    [TextArea(5, 5)]
    public string skillDescription;
}

public enum SkillType
{
    Active,
    Passive
}

[CreateAssetMenu(fileName = "PetData_PetName", menuName = "GameScene/Pet/PetData")]
public class PetData : ScriptableObject
{
    [Header("ペットの基本情報")]
    public PetType petType = PetType.None;

    [Header("ペットの種族")]
    public PetRace race = PetRace.None;

    [Header("ペットの元居た立場")]
    public MonsterType petMonsterType = MonsterType.None;

    [Header("ペットの名前")]
    public string petName = "";

    [Header("ペットのアイコン")]
    public Sprite petIcon;

    [Header("ペットの役割")]
    [Tooltip("このペットが持つ役割（複数設定可能）")]
    public List<PetRole> petRoles = new List<PetRole>();

    [Header("このペットの攻撃力")]
    public float attackPower = 0.0f;

    [Header("ゲーム内データ")]
    public GameObject petPrefab;

    [Header("アンロック情報")]
    [Tooltip("このペットが解放済みかどうか")]
    public bool isUnlocked;

    [Header("仲間になる確率（％）")]
    [Range(0.0f, 100.0f)]
    public float getChance = 0.0f;

    public MapType petMap = MapType.SpiderForest;

    #region パッシブスキル、アクティブスキルの有無---------

    [Header("スキルの有無フラグ")]
    public bool hasActiveSkill  = false;
    public bool hasPassiveSkill = false;

    [Header("アクティブスキルの残りクールタイム")]
    public float activeSkillRemainingCooldown = 0.0f;

    [Header("アクティブスキルのクールタイム")]
    public float activeSkillTotalCooldown = 0.0f;

    [Header("アクティブスキルをどれだけ持続させるかの時間")]
    public float activeSkillDuration = 0.0f;

    [Header("スキルの汎用カウント")]
    public int   skillGenericCount = 0;

    [Header("このペットが持つスキル（複数設定可能）")]
    public List<SkillInfo> skills = new List<SkillInfo>();

    #endregion --------------------------------------------

    [Tooltip("ペットの説明文")]
    [TextArea(5, 5)]
    public string description;
}
