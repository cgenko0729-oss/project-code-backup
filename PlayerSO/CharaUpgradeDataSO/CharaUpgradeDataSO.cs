using System.Collections.Generic;
using UnityEngine;

// １つの強化自体の種類
public enum UpgradeType
{
    None = 0,
    /* 共通タイプ */
    Vitality,                   // [生命力]体力増加
    GrowthPotenthial,           // [成長力]獲得経験値増加
    ContinuousChant,            // [連続詠唱]弾数増加、クールダウン減少
    IronHide,                   // [鉄壁の皮膚]防御力＆体力増加
    Toughness,                  // [強靭な肉体]移動スピード増加
    Suvivability,               // [生存本能]体力＆移動スピード増加
    Fluidity,                   // [流転]移動スピード増加、クールダウン減少
    BraveDash,                  // [勇敢なダッシュ]ダッシュクールダウン減少
    SharpNails,                 // [鋭い爪]ダメージ増加
    SharpenedClaws,             // [研ぎ澄まされた爪]ダメージ増加、攻撃するほどダメージ増加(３回まで重ねられる)
    KnightOffensiveMind,        // [騎士の攻めの精神]経験値取得でサイズ＆クリティカル増加
    KnightDefensiveMind,        // [騎士の守りの精神]コイン取得で受けるダメージ軽減、ダメージ増加
    FastChant,                  // [高速詠唱]クールダウン減少
    MagicPowerIncrease,         // [魔力増強]ダメージ＆体力増加
    TensionUp,                  // [テンションアップ]初期スキルの進化の効果発動時に追加攻撃が発生するようになる(追加攻撃の回数は倒した敵に応じて増加する)
    WonderPocket,               // [魔法使いの不思議なポケット]一定時間毎に確率でランダムなアイテム効果発動
    EmergencyBarrier,           // [緊急バリア魔法]体力が1/3以下の際にバフ獲得、受けたダメージの総量で追加デバフ獲得
    Scaling,                    // [スケーリング]ダメージ＆サイズ増加
    Haste,                      // [ヘイスト魔法]ダメージ増加、クールダウン減少
    Diffusion,                  // [拡散]ダメージ＆弾数増加
    SurvivalTechniques,         // [サバイバル術]取得したアイテムの効果量が1.5倍、獲得経験値が2.0倍になる
    ArcheryMasterySpeed,        // [弓矢の極意・速]移動スピードが早いほどダメージ増加とクールダウン減少
    ArcheryMasteryDiffusion,    // [弓矢の極意・散]初期スキルの弾数が多いほどサイズ＆クリティカル率増加
    BeastAgility,               // [獣の身のこなし]体力増加、スキルクールダウン減少
    KeenEye,                    // [観察眼]クリティカル率＆ダメージ増加
    MultiProcess,               // [並列思考]弾数増加
    WarriorsMettle,             // [戦士の誇り]最大体力に対して現在の体力の割合でダメージ＆移動スピード増加
    CrimsonPulse,               // [狂戦士の兆し]ダメージを受けるたびに、バフを獲得
    PackLeaderLoneLion,         // [群れを統べる者/孤高の獣]連れているペットの数に応じてバフ獲得
    /* Knight専用タイプ */
    //Kni_ApplyDamageAndDiffence, // ダメージと防御力増加
    /* Wizard専用タイプ */
    //Wiz_ApplyOneReviveChance,   // 一度だけ復活できるようになる
    /* Archer専用タイプ */
    //Arc_RabbitLegPower,         // [ウサギの脚力]
    /* Warrior専用タイプ */
    //War_LionSoul,               // [獅子魂]体力が少ないとダメージ増加
}

// １つの強化内であがるパラメータの種類
public enum BuffType
{
    IncreaseDamage,             // ダメージ増加
    IncreaseDefence,            // 防御力増加
    IncreaseCritChance,         // クリティカル率増加
    IncreaseHealth,             // 体力増加
    IncreaseProjectileNum,      // 弾数増加
    IncreaseExp,                // 獲得経験値増加
    DecreaseSkillCooldown,      // クールダウン減少
    IncreaseOneReviveChance,    // 一度だけ復活可能
    IncreaseMoveSpeed,          // 移動スピード増加
    DecreaseDashCooldown,       // ダッシュクールダウン減少
    IncreaseDamagePerAttack,    // (特殊)攻撃するほどダメージ増加
    ApplyBuffPerExp,            // (特殊)経験値取得でバフ獲得
    ApplyBuffPerCoin,           // (特殊)コイン取得でバフ獲得

    AddAttackPerFinalSkill,     // (特殊)初期スキルの進化後効果の発動で追加攻撃が発生(倒した敵の数に応じて追加攻撃の回数増加)
    IntervalRandomEffect,       // (特殊)一定時間毎にランダムで効果発動
    ApplyPinchBuff,             // (特殊)体力が1/3以下の際にバフ獲得、受けたダメージの総量で追加でバフ獲得
    
    IncreaseSkillSize,          // スキルのサイズ増加
    IncreaseItemEffectValue,    // アイテムの効果量増加
    ApplyBuffSpeedScaling,      // (特殊)素早さに基づいてバフ獲得
    ApplyBuffProjNumScaling,    // (特殊)初期スキルの弾数に基づいてバフ獲得
    ApplyBuffHealthScaling,     // (特殊)最大体力に対して現在の体力の割合でバフ獲得
    ApplyBuffPerDamage,         // (特殊)体力が減るたびにダメージ増加
    ApplyBuffPetScaling,        // (特殊)ペットの連れてきた数と連れてこなかった空き枠の数に基づいてバフ獲得
}

[System.Serializable]
public struct BuffParam
{
    public BuffType buffType;
    public float amount;
}

[CreateAssetMenu(fileName = "CharaUpgradeDataSO", menuName = "UpgradeData/CharaUpgradeDataSO")]
public class CharaUpgradeDataSO : ScriptableObject
{
    [Header("データID")]
    public string dataId;

    [Header("強化タイプ")]
    public UpgradeType upgradeType;

    [Header("どのキャラの強化項目かを表すJobId")]
    public JobId jobId;

    [Header("１レベルごとの変化量")]
    public BuffParam[] buffParam;

    [Header("解放に必要なコイン枚数")]
    public int unlockCoin;

    [Header("解放済みかどうかのフラグ")]
    public bool isUnlocked;

    [Header("解放できるスキルかどうかのフラグ")]
    public bool enableUnlock;

    [Header("解放に必要な前提のCharaUpgradeDataSO")]
    public CharaUpgradeDataSO requiredUpgrade;
        
    [Header("解放されることでロックされるCharaUpgradeDataSO")]
    public List<CharaUpgradeDataSO> exclusionUpgrades;
}

