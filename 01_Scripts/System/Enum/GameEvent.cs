using UnityEngine;

public static class GameEvent
{
    /*プレイヤー関係*/
    public const string PlayerDash = nameof(PlayerDash);
    public const string PlayerDashEnd = nameof(PlayerDashEnd);
    public const string PlayerAttack = nameof(PlayerAttack);
    public const string PlayerGetItem = nameof(PlayerGetItem);
    //PlayerLevelUp
    public const string PlayerLevelUp = nameof(PlayerLevelUp);
    public const string ChangePlayerHp = nameof(ChangePlayerHp);
    public const string PlayerHpChanged = nameof(PlayerHpChanged);

    public const string PlayerGetDamage = nameof(PlayerGetDamage);
    public const string PlayerGetHeal = nameof(PlayerGetHeal);

    /*ゲームステート関係*/
    public const string GameStart = nameof(GameStart);
    public const string GameOver = nameof(GameOver);

    /*ボス戦関係*/
    public const string StartBossFight = nameof(StartBossFight);
    public const string BossPhase2Start = nameof(BossPhase2Start);
    public const string AllSpiderDenDestroyed = nameof(AllSpiderDenDestroyed);
    public const string SpiderDenDestroy = nameof(SpiderDenDestroy);   
    public const string CutSceneStart   = nameof(CutSceneStart);
    public const string CutSceneEnd   = nameof(CutSceneEnd);

    /*システム関係*/
    public const string ChangeFacingModeCursor = nameof(ChangeFacingModeCursor);
    public const string ChangeFacingModeMoveDir = nameof(ChangeFacingModeMoveDir);
    public const string isPauseMenu = nameof(isPauseMenu);   //ポーズ画面状態の場合
    public const string isOptionMenu = nameof(isOptionMenu);   //オプション画面状態の場合
    public const string pushTitlebtn = nameof(pushTitlebtn); //タイトルのスタートが押された場合
    public const string OnActiveSkillDataWindow = nameof(OnActiveSkillDataWindow);      // スキルデータウィンドウの表示
    public const string OnInactiveSkillDataWindow = nameof(OnInactiveSkillDataWindow);  // スキルデータウィンドウを閉じる
    public const string OnActivePEnchDataWindow = nameof(OnActivePEnchDataWindow);      // プレイヤーエンチャントデータウィンドウの表示
    public const string OnInactivePEnchDataWindow = nameof(OnInactivePEnchDataWindow);  // プレイヤーエンチャントデータウィンドウを閉じる
    public const string LastAttack_FlyingDemon = nameof(LastAttack_FlyingDemon); //最後の攻撃が飛行デーモンの場合
    public const string LastAttack_Pummy = nameof(LastAttack_Pummy); //最後の攻撃がプミーの場合
    public const string LastAttack_Anubis = nameof(LastAttack_Anubis); //最後の攻撃がアヌビスの場合
    public const string LastAttack_FlameDragonKing = nameof(LastAttack_FlameDragonKing); //最後の攻撃が砂漠の炎王龍の場合
    public const string turnipaKingSkillPowUp = nameof(turnipaKingSkillPowUp); //カブキングのスキル強化

    public const string AllAttackStart = nameof(AllAttackStart);　//全攻撃開始

    public const string PushObjectArriveTarget = nameof(PushObjectArriveTarget);


    public const string ItemBoxDestroyed = nameof(ItemBoxDestroyed);

    public const string LanguageChanged = nameof(LanguageChanged);

    public const string SceneChanged = nameof(SceneChanged);

    public const string ChangeCameraMode = nameof(ChangeCameraMode);

    public const string PetGet = nameof(PetGet);

    public const string PetAttack = nameof(PetAttack);

    public const string CounterPetAttack = nameof(CounterPetAttack);
}
