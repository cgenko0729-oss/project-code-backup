using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public enum SoundList 
{ 
    Slash,                        //スラッシュ
    Thunder,                      //雷
    CircleBall,                   //隕石
    SpiderMobDead,                //スパイダーの死亡
    PlayerGetDamage,              //プレイヤーがダメージを受ける
    PlayerDash,　                 //プレイヤーダッシュ
    LevelUp,　                    //レベルアップ
    UiHover,　                    //UIの上に乗せる時
    UiClick,　                    //UIをクリックした時
    GetExp,　                     //経験値を取得した時
    BoostSe,　                    //ブースト音
    SpiderBossPhrase2Se,　        //スパイダーボスのセリフ2
    SpiderBossThunderBallSe,　    //スパイダーボスの雷ボール
    SpiderBossDeadSe,　           //スパイダーボスの死亡
    SpiderBossMagicAtkSe,　       //スパイダーボスの魔法攻撃
    SpiderBossWebShotAtkSe,　     //スパイダーボスのウェブショット攻撃
    SpiderBossStickyWebSe,　      //スパイダーボスのスティッキィウェブ
    SpiderBossFireAtKSe,　        //スパイダーボスのファイヤーアットK
    SpiderBossFallGroundSe,　     //スパイダーボスの落下
    SpiderBossDashSe,　           //スパイダーボスのダッシュ
    GetItem,　                    //アイテムを取得した時
    HitItemBox,　                 //アイテムボックスに当たった時
    FinalHitSe,                   //最終ヒット時のSE
    SwapButtonSe,                 //スワップボタンを押した時
    ButtonClickSe,                //ボタンをクリックした時
    ButtonCancelSe,               //ボタンをキャンセルした時
    DebugkeySe,                   //デバッグキーを押した時
    CountDownwarningSe,           //カウントダウンの警告音
    ScoreCountSe,                 //スコアカウントのSE
    GameClearSe,                  //ゲームクリアのSE
    GameOverSe,                   //ゲームオーバーのSE
    ShopBuySe,                    //ショップでアイテムを購入した時
    ShopRefundSe,                 //ショップでアイテムを返金した時
    ShopClickSe,                  //ショップでアイテムをクリックした時
    ShopOpenSe,                   //ショップを開いた時
    ShopCloseSe,                  //ショップを閉じた時
    ArrowSe,                      //矢のSE
    MagicSe,                      //魔法のSE
    BoomerangSe,                  //ブーメランのSE
    BoomerangEvolveSe,            //ブーメランの進化後の追加SE
    BounceKnifeSe,                //バウンスナイフのSE
    TurnipaBossScremSe,           //ターンパボスの叫び声
    TurnipaBossTreeRootSe,        //ターンパボスの木の根
    TurnipaBossTreeTrapSe,
    TurnipaBossPosionCloudSe,     //ターンパボスの毒雲
    TurnipaBossAcidSplashSe,      //ターンパボスの酸の飛沫
    TurnipaBossPharse2Se,
    TurnipaBossCurveProjectileSe, //ターンパボスのカーブプロジェクタイル
    TurnipaBossPlaceAoeSe,
    TurnipaBossSummonSlimeSe,     //ターンパボスのスライム召喚
    DropCoin,                     //コインをドロップした時
    PickCoin,                     //コインを拾った時
    HitRemnant,                   //レムナントに当たった時
    NagaShout,                    //蛇の叫び声
    FireBallExplosion,            //ファイアボールの爆発
    TurnipaBossBigBombSe,         //ターンパボスのビッグボム
    TurnipaBossPlaceMultipleAoeSe, //ターンパボスの複数のAOE
    QuestFinishSe,            //クエスト完了のSE
    TurnipaBossFlySe,            //ターンパボスの飛行SE
    HammerSe,                     //ハンマーのSE
    TornadoSe,                     //トルネードのSE
    IceSe,                         //アイスのSE
    TraitAfterSkillExplosionSe,    //スキル後の爆発SE
    TraitAcidExplosionSe,         //酸の爆発SE
    TraitRevengeSe,             //復讐のSE
    TraitItemPickupSe,         //アイテムピックアップのSE
    TraitDashCastSe,         //ダッシュキャストのSE
    Bite_WeakSe,                //弱い噛みつきのSE
    Bite_StrongSe,              //強い噛みつきのSE
    PunchSe,                    //パンチのSE
    NeedleStickSe,               //針刺しのSE
    HealSE,                     //回復のSE
    ParrySe,                       //パリィのSE
    SoulEatSe,                    //ソウルイートのSE
    LastLightSe,                  //爆発前の光SE
    PetExplosionSe,               //ペット爆発SE
    SheddingSe,                   //生え変わるSE
    None,                         //なし
    SoulDropSe,                   //ソウルドロップSE
    SoulGetSe,                    //ソウルゲットSE
    MidBossExplodeSe1,            //ミッドボスの爆発SE1
    MidBossExplodeSe2,            //ミッドボスの爆発SE2
    MidBossExplodeSe3,            //ミッドボスの爆発SE3
    MidBossSpellSe1,              //ミッドボスの呪文SE1
    MidBossSpellSe2,              //ミッドボスの呪文SE2
    EnemyMageShotSe,            //敵のメイジショットSE
    BigThunderSe,               //大きな雷SE
    SwordSlashSe,               //剣の斬撃SE
}

