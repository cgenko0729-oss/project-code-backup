using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening.Core.Easing;
using TigerForge;
using QFSW.MOP2;
using System.Linq;

using Hellmade.Sound;
using DG.Tweening; // Eazy Sound Manager 

/// <summary>
/// スキルキャスター基底クラス。<br/>
/// ・共通ステータス計算 / DPS 計算<br/>
/// ・クールダウン、ダッシュ中の停止管理<br/>
/// ・プレイヤーの Transform 取得、前方ベクトル更新<br/>
/// 子クラスは <c>PerformCast()</c> だけを実装すれば
/// 新しいスキルを簡単に追加できる。
/// </summary>

public abstract class SkillCasterBase : MonoBehaviour
{

    /// <summary>
    /// スキルを生成・管理するコンポーネント。
    /// クールダウン管理
    /// ● ステータスの最終値計算
    /// ● 各スキル固有の生成ロジック呼び出し
    /// </summary>

    public readonly Dictionary<SkillStatusType,int> statusLevels = new();  　// ステータスタイプごとの現在レベルを保持
    public bool IsFullyMaxed() => casterLevel >= casterLevelMax;   　// スキルが最大レベルに達しているか判定
    public int CurrentStatusLevel(SkillStatusType t) => statusLevels.TryGetValue(t, out var lv) ? lv : 0;　// 指定ステータスの現在レベルを取得（未登録なら0）

    [Header("基本情報")]
   
    public SkillIdType casterIdType;                                // このキャスターのスキルID種類
    public int casterLevel = 1;                                    // 現在レベル
    public int casterLevelMax = 10;                                // 最大レベル
    public string casterName;                                      // スキル名表示用
    public string skillDescription;                             // スキル説明文
    public string skillNameFinal;                           // スキル名(進化後)
    public string skillDescriptionFinal;                        // スキル説明文(進化後)
    public float nowDps;                                    // 現在のDPS（ダメージ/秒）計算用変数
    public bool isActivated = false;                        // スキルが有効化済みか
    public bool isFinalSkill;
    public bool isPlayerBaseSkill = false;                  // 基本攻撃スキルかどうか
    public float playerAnimationDelayCnt = 0.3f;          // プレイヤーのアニメーション遅延カウント（スキル発動時に使用）
    public bool isStopCasting = false;                      // キャスト停止フラグ
    public float stopCastCnt = 0f;                          // 停止カウントタイマー

    public ObjectPool skillObjPool;                             // スキルのエフェクト本体
    public Sprite casterSpriteImage;                            // スキルアイコン画像（UI用）
    
    public GameObject skillEffectGUI;                           // UIエフェクト表示用オブジェクト
    public int casterId;                                           // キャスター内部ID
    public int characterJobId = -1;

    public bool isBuffManagerProjectileBuffApplied = false; // BuffManagerの弾数バフが適用されたかどうか

    [Header("スキルのステータス基礎値")]  
    public float castCoolDown;                                   // 現在のクールダウンタイマー
    public float castCoolDownMaxBase = 3;                        // クールダウン基礎値
    public float durationBase        = 2.5f;                     // 持続時間基礎値
    public float speedBase           = 3;                        // 発射速度基礎値
    public float damageBase          = 50;                       // ダメージ基礎値
    public float sizeBase            = 1;                        // エフェクトサイズ基礎値
    public int projectileNumBase     = 1;                        // 弾数基礎値

    [Header("スキルのステータス倍率スケーラ (％)／個数")]
    public float coolDownFactor = 1f;
    public float castCoolDownMaxScaler = 1f;                      // クールダウン倍率（%）
    public float durationScaler = 1f;                             // 持続時間倍率（%）
    public float speedScaler = 1f;                                // 発射速度倍率（%）
    public float damageScaler = 1f;                               // ダメージ倍率（%）
    public float sizeScaler = 1f;                                 // サイズ倍率（%）
    public int projectileNumScaler = 1;                           // 弾数（個数）

    [Header("計算後のステータス最終値(基礎値*倍率)")]
    public float castCoolDownFinal;                                // 計算後クールダウン
    public float durationFinal;                                    // 計算後持続時間
    public float speedFinal;                                       // 計算後発射速度
    public float damageFinal;                                      // 計算後ダメージ
    public float sizeFinal;                                        // 計算後サイズ
    public int projectileNumFinal;                                 // 計算後弾数

    [Header("スキルのステータス最大値")]
    public int projectileNumMax = 7;                                    // 弾数最大値
    public float castCooldownMax;                                   // クールダウン最大値
    public float durationMax;                                       // 持続時間最大値
    public float speedMax;                                          // 発射速度最大値
    public float damageMax;                                         // ダメージ最大値
    public float sizeMax;                                           // エフェクトサイズ最大値

    //public int notRelatedStatusType1;
    //public int notRelatedStatusType2;    

    protected PlayerState playerStatus;
    protected PlayerController playerControl;
    protected Transform playerTran;
    public Transform effFolderTrans;

    public Vector3 castPosOffset;
    public Vector3 playerForwardVec;
    public float castDistance;

    public static readonly Quaternion prefabFix = Quaternion.Euler(-90f, 0f, 90f);

    public SkillElementType skillElementType;

    public bool hasInitBaseSkill = false;

    BuffManager bm;

    public bool isUnlock = true;

    public bool isMultiBulletEnabled = false; //多発できるかどうか

    public List<TraitData> traitDataHoldingList = new();
    public TraitData currentTrait;

    public List<TraitData> availableTraitList = new(); // 利用可能なトレイトデータのリスト
    public TraitData[] traitPairs = new TraitData[2]; // トレイトペアのデータを保持する配列

    public bool isAfterDashCast = false; // ダッシュ後にスキルを発動するかどうか
    public bool isAfterDashEnhanced = false; // ダッシュ後にスキルを強化するかどうか

    public bool isGetHealCast = false; // 回復時にスキルを発動するかどうか
    public bool isHpChangeCast = false; // HP変化時にスキルを発動するかどうか
    public bool isHpChangeEnhanced = false; // HP変化時にスキルを強化するかどうか

    public bool isFinishCastExplosion = false;　 // キャスト終了時に爆発するかどうか
    public bool isFinishCastSplit = false; //　 キャスト終了時に弾を分裂させるかどうか

    public bool isKillEnemyExplosion = false;
    public bool isKillEnemySkullSoul = false;
    public bool isKillEnemyBrightSoul = false;

    public bool isSkillMovingDropFire = false;
    public bool isSkillMovingDropSpike = false;

    public bool isDoubleCast = false;

    public bool isEnchantFire = false; // スキルが炎のエンチャントを持つかどうか
    public bool isEnchantIce = false;
    public bool isEnchantPoison = false;
    public bool isEnchantLightning = false;

    public bool isDebugTraitOnce = false;

    public bool isJustDash = false;
    public bool isJustHpChange = false;

    public bool isPushback = false;

    public bool isGetItemCast = false;
    public bool isGetItemEnhanced = false;

    public float skillCriticChance = 0f;

    public bool isAfterCastSpeedAdd = false;

    public bool isAfterCastSummonGhost = false;

    public bool isWalkCast = false;

    public bool isTreasureHunter = false;

    public bool isSlotAddDamage = false;
    public bool isGambler = false;
    public bool isFairJudge = false;

    public bool isStandStillEnhance = false;

    public bool isDealMoreDamageFullHp = false;
    public bool isKillEnemyDropFood = false;

    public bool isBloodPrice = false;

    public int previousSlotNum = 0;

    public bool isAutoAttackSupported = false;
    public bool autoMode = false;
    private float autoSearchRange = 14.9f;
    public float fixRotTime = 0.21f;
    public Transform autoAttackTarget = null;

    //public void UpdateTraitData()
    //{
    //    currentTrait.isAfterDashCast = isAfterDashCast;
    //    currentTrait.isAfterDashEnhanced = isAfterDashEnhanced;
    //    currentTrait.isHpChangeCast = isHpChangeCast;
    //    currentTrait.isHpChangeEnhanced = isHpChangeEnhanced;
    //    currentTrait.isFinishCastExplosion = isFinishCastExplosion;
    //    currentTrait.isSkillMovingDropFire = isSkillMovingDropFire;
    //    currentTrait.isSkillMovingDropSpike = isSkillMovingDropSpike;
    //    currentTrait.isDoubleCast = isDoubleCast;



    //}

    public void ActivateCaster() { isActivated = true; }

    private void OnEnable()
    {
        EventManager.StartListening(GameEvent.PlayerDash, StopCasting); // ダッシュ時にキャスト停止を登録
        EventManager.StartListening(GameEvent.PlayerDashEnd, HandleAfterDashAction);
        EventManager.StartListening(GameEvent.PlayerGetDamage, HandlePlayerGetDamageAction); // HP変化時にスキル発動を登録
        EventManager.StartListening(GameEvent.PlayerGetHeal, HandlePlayerGetHealAction);
        EventManager.StartListening(GameEvent.PlayerGetItem, HandleGetItemAction);
        EventManager.StartListening("PlayWalkCertainAmount", HandleWalkCastAction);

        EventManager.StartListening(GameEvent.ChangeCameraMode, EnableAutoMode);

    }

    private void OnDisable()
    {
        EventManager.StopListening(GameEvent.PlayerDash, StopCasting);
        EventManager.StopListening(GameEvent.PlayerDashEnd, HandleAfterDashAction);
        EventManager.StopListening(GameEvent.PlayerGetDamage, HandlePlayerGetDamageAction);
        EventManager.StopListening(GameEvent.PlayerGetHeal, HandlePlayerGetHealAction);
        EventManager.StopListening(GameEvent.PlayerGetItem, HandleGetItemAction);
        EventManager.StopListening("PlayWalkCertainAmount", HandleWalkCastAction);
        EventManager.StopListening(GameEvent.ChangeCameraMode, EnableAutoMode);
    }

    void EnableAutoMode()
    {
        bool isAuto = EventManager.GetBool(GameEvent.ChangeCameraMode);

        if(isAuto)autoMode = true;
        else autoMode = false;
    }

    public void PlayerFaceNearEnemy()
    {
        autoAttackTarget = null;
        autoAttackTarget = GetNearestEnemy();

            if (autoAttackTarget != null)
            {
                Vector3 directionToTarget = autoAttackTarget.position - playerTran.position;
                Vector3 flatDir = directionToTarget;
                flatDir.y = 0; 

                if (flatDir != Vector3.zero)
                {
                    playerTran.rotation = Quaternion.LookRotation(flatDir);                   
                    playerForwardVec = flatDir.normalized; 
                }
            }
    }

    public Transform GetNearestEnemy()
    {
        Collider[] hits = Physics.OverlapSphere(playerTran.position, autoSearchRange);
        Transform nearestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                float dist = Vector3.Distance(playerTran.position, hit.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestEnemy = hit.transform;
                }
            }
        }
        return nearestEnemy;
    }

    public void RandTraitPairFromAvailableTraitList()
    {
        if (availableTraitList.Count < 2) return; // 利用可能なトレイトが2つ未満の場合は何もしない

        //List<TraitData> list = availableTraitList; only reference the own list , not copy 
        List<TraitData> list = new List<TraitData>(availableTraitList);

        int id1 = Random.Range(0, list.Count);
        traitPairs[0] = availableTraitList[id1]; // 最初のトレイト
        list.RemoveAt(id1); // 選択したトレイトをリストから削除
        
        int id2 = Random.Range(0, list.Count); // 残りのトレイトからもう1つ選択     
        traitPairs[1] = availableTraitList[id2]; // 2つ目のトレイト

    }

    public void SetDebugTraitOnce()
    {
        if (isDebugTraitOnce)
        {
            isDebugTraitOnce = false;

            currentTrait.isAfterDashCast = isAfterDashCast;
            currentTrait.isAfterDashEnhanced = isAfterDashEnhanced;
            currentTrait.isHpChangeCast = isHpChangeCast;
            currentTrait.isHpChangeEnhanced = isHpChangeEnhanced;
            currentTrait.isFinishCastExplosion = isFinishCastExplosion;
            currentTrait.isFinishCastSplit = isFinishCastSplit;
            currentTrait.isKillEnemyExplosion = isKillEnemyExplosion;
            currentTrait.isKillEnemySkullSoul = isKillEnemySkullSoul;
            currentTrait.isKillEnemyBrightSoul = isKillEnemyBrightSoul;
            currentTrait.isSkillMovingDropFire = isSkillMovingDropFire;
            currentTrait.isSkillMovingDropSpike = isSkillMovingDropSpike;
            currentTrait.isDoubleCast = isDoubleCast;
            currentTrait.isEnchantFire = isEnchantFire;
            currentTrait.isEnchantIce = isEnchantIce;
            currentTrait.isEnchantPoison = isEnchantPoison;
            currentTrait.isEnchantLightning = isEnchantLightning;
            currentTrait.isPushback = isPushback;

            currentTrait.critChanceAdd = skillCriticChance;

            currentTrait.isGetHealCast = isGetItemCast;

            currentTrait.isGetHealItemCast = isGetItemCast;
            currentTrait.isGetHealItemEnhanced = isGetItemEnhanced;

            currentTrait.isAfterCastSpeedAdd = isAfterCastSpeedAdd;
            currentTrait.isAfterCastSummonGhost = isAfterCastSummonGhost;

            currentTrait.isWalkCast = isWalkCast;

            currentTrait.isTreasureHunter = isTreasureHunter;
            currentTrait.isSlotAddDamage = isSlotAddDamage;
            currentTrait.isGambler = isGambler;
            currentTrait.isFairJudge = isFairJudge;

            currentTrait.isStandStillEnhance = isStandStillEnhance;

            currentTrait.isDealMoreDamageFullHp = isDealMoreDamageFullHp;
            currentTrait.isKillEnemyDropFood = isKillEnemyDropFood;

            //currentTrait.isGetHealItemCast


        }

    }

    public void SetSkillTrait(TraitData data)
    {
        if(!isAfterDashCast)isAfterDashCast = data.isAfterDashCast;
        if (!isAfterDashEnhanced) isAfterDashEnhanced = data.isAfterDashEnhanced;
        if (!isHpChangeCast) isHpChangeCast = data.isHpChangeCast;
        if (!isHpChangeEnhanced) isHpChangeEnhanced = data.isHpChangeEnhanced;

        if (!isFinishCastExplosion) isFinishCastExplosion = data.isFinishCastExplosion;
        if (!isFinishCastSplit) isFinishCastSplit = data.isFinishCastSplit;
        if (!isKillEnemyExplosion) isKillEnemyExplosion = data.isKillEnemyExplosion;
        if (!isKillEnemySkullSoul) isKillEnemySkullSoul = data.isKillEnemySkullSoul;
        if (!isKillEnemyBrightSoul) isKillEnemyBrightSoul = data.isKillEnemyBrightSoul;

        if (!isSkillMovingDropFire) isSkillMovingDropFire = data.isSkillMovingDropFire;
        if (!isSkillMovingDropSpike) isSkillMovingDropSpike = data.isSkillMovingDropSpike;
        if (!isDoubleCast) isDoubleCast = data.isDoubleCast;

        if (!isEnchantFire) isEnchantFire = data.isEnchantFire;
        if (!isEnchantIce) isEnchantIce = data.isEnchantIce;
        if (!isEnchantPoison) isEnchantPoison = data.isEnchantPoison;
        if (!isEnchantLightning) isEnchantLightning = data.isEnchantLightning;

        if (!isPushback) isPushback = data.isPushback;

        if (!isGetItemCast)
        {
            isGetItemCast = data.isGetHealItemCast;
            if (data.isGetHealItemCast) SkillEffectManager.Instance.isGetItemCastEnabled = true;
        }
        if (!isGetItemEnhanced) isGetItemEnhanced = data.isGetHealItemEnhanced;

        if(!isGetHealCast) isGetHealCast = data.isGetHealCast;

        if(!isAfterCastSpeedAdd) isAfterCastSpeedAdd = data.isAfterCastSpeedAdd;
        if (!isAfterCastSummonGhost) isAfterCastSummonGhost = data.isAfterCastSummonGhost;

        if (!isWalkCast) isWalkCast = data.isWalkCast;

        if (data.isWalkCast)
        {
            SkillEffectManager.Instance.universalTrait.isWalkCast = true;
        }

        if (!isTreasureHunter) isTreasureHunter = data.isTreasureHunter;
        if (!isSlotAddDamage)
        {
            isSlotAddDamage = data.isSlotAddDamage;
            

        }

        

        if (!isGambler) isGambler = data.isGambler;
        if (!isFairJudge) isFairJudge = data.isFairJudge;

        if (!isStandStillEnhance)
        {
            isStandStillEnhance = data.isStandStillEnhance;
            if(data.isStandStillEnhance) ActiveBuffManager.Instance.AddStack(TraitType.StandStillEnhance);
        }
        if(isStandStillEnhance) SkillEffectManager.Instance.isStandStillEnabled = true;

        if(!isDealMoreDamageFullHp) isDealMoreDamageFullHp = data.isDealMoreDamageFullHp;
        if(!isKillEnemyDropFood) isKillEnemyDropFood = data.isKillEnemyDropFood;

        traitDataHoldingList.Add(data); // トレイトデータを保持リストに追加

        if (isSlotAddDamage && previousSlotNum != traitDataHoldingList.Count)
        {
            float diff = (traitDataHoldingList.Count) - previousSlotNum;
            previousSlotNum = traitDataHoldingList.Count;
            damageScaler +=  diff * 25f;

            for(int i = 0; i < (int)diff; i++)
            {
                ActiveBuffManager.Instance.AddStack(TraitType.SlotAddDamage);
            }

        }

        if(!isBloodPrice) isBloodPrice = data.isBloodPrice;



        //===//????
        if (!currentTrait) return;
        if (!currentTrait.isAfterDashCast) currentTrait.isAfterDashCast = data.isAfterDashCast;
        if (!currentTrait.isAfterDashEnhanced) currentTrait.isAfterDashEnhanced = data.isAfterDashEnhanced;
        if (!currentTrait.isHpChangeCast) currentTrait.isHpChangeCast = data.isHpChangeCast;
        if (!currentTrait.isHpChangeEnhanced) currentTrait.isHpChangeEnhanced = data.isHpChangeEnhanced;

        if (!currentTrait.isFinishCastExplosion) currentTrait.isFinishCastExplosion = data.isFinishCastExplosion;
        if (!currentTrait.isFinishCastSplit) currentTrait.isFinishCastSplit = data.isFinishCastSplit;
        if (!currentTrait.isKillEnemyExplosion) currentTrait.isKillEnemyExplosion = data.isKillEnemyExplosion;
        if (!currentTrait.isKillEnemySkullSoul) currentTrait.isKillEnemySkullSoul = data.isKillEnemySkullSoul;
        if (!currentTrait.isKillEnemyBrightSoul) currentTrait.isKillEnemyBrightSoul = data.isKillEnemyBrightSoul;

        if (!currentTrait.isSkillMovingDropFire) currentTrait.isSkillMovingDropFire = data.isSkillMovingDropFire;
        if (!currentTrait.isSkillMovingDropSpike) currentTrait.isSkillMovingDropSpike = data.isSkillMovingDropSpike;
        if (!currentTrait.isDoubleCast) currentTrait.isDoubleCast = data.isDoubleCast;

        if (!currentTrait.isEnchantFire) currentTrait.isEnchantFire = data.isEnchantFire;
        if (!currentTrait.isEnchantIce) currentTrait.isEnchantIce = data.isEnchantIce;
        if (!currentTrait.isEnchantPoison) currentTrait.isEnchantPoison = data.isEnchantPoison;
        if (!currentTrait.isEnchantLightning) currentTrait.isEnchantLightning = data.isEnchantLightning;

        if (!currentTrait.isPushback) currentTrait.isPushback = data.isPushback;
        if (!currentTrait.isGetHealItemCast) currentTrait.isGetHealItemCast = data.isGetHealItemCast;
        if (!currentTrait.isGetHealItemEnhanced) currentTrait.isGetHealItemEnhanced = data.isGetHealItemEnhanced;
        
        if (!currentTrait.isGetHealCast) currentTrait.isGetHealCast = data.isGetHealCast;

        if (!currentTrait.isAfterCastSpeedAdd) currentTrait.isAfterCastSpeedAdd = data.isAfterCastSpeedAdd;
        if (!currentTrait.isAfterCastSummonGhost) currentTrait.isAfterCastSummonGhost = data.isAfterCastSummonGhost;

        if (!currentTrait.isWalkCast) currentTrait.isWalkCast = data.isWalkCast;

        if (!currentTrait.isTreasureHunter) currentTrait.isTreasureHunter = data.isTreasureHunter;
        if (!currentTrait.isSlotAddDamage) currentTrait.isSlotAddDamage = data.isSlotAddDamage;
        if (!currentTrait.isGambler) currentTrait.isGambler = data.isGambler;
        if (!currentTrait.isFairJudge) currentTrait.isFairJudge = data.isFairJudge;

        if (!currentTrait.isStandStillEnhance) currentTrait.isStandStillEnhance = data.isStandStillEnhance;

        if (!currentTrait.isIsolator)
        {
            currentTrait.isIsolator = data.isIsolator;
            if(currentTrait.isIsolator)SkillEffectManager.Instance.isIsolatorEnabled = true;
        }

        if (!currentTrait.isDealMoreDamageFullHp) currentTrait.isDealMoreDamageFullHp = data.isDealMoreDamageFullHp;
        if (!currentTrait.isKillEnemyDropFood) currentTrait.isKillEnemyDropFood = data.isKillEnemyDropFood;
        if(!currentTrait.isLuckAddCrit) currentTrait.isLuckAddCrit = data.isLuckAddCrit;

        if(!currentTrait.isKillEnemeyBigger) currentTrait.isKillEnemeyBigger = data.isKillEnemeyBigger;

        if (!currentTrait.isELementalist) currentTrait.isELementalist = data.isELementalist;

        if(!currentTrait.isBloodPrice) currentTrait.isBloodPrice = data.isBloodPrice;

        damageScaler += data.damageAdd;
        coolDownFactor *= 1f - data.cooldownAdd / 100f;
        durationScaler += data.durationAdd;
        speedScaler += data.speedAdd;
        sizeScaler += data.sizeAdd;
        projectileNumScaler += data.projectilNumAdd;

        currentTrait.tempDamageAdd += data.tempDamageAdd;
        currentTrait.tempCooldownAdd += data.cooldownAdd;
        currentTrait.tempProjectilNumAdd += data.tempProjectilNumAdd;
        currentTrait.tempSizeAdd += data.tempSizeAdd;
        currentTrait.tempDurationAdd += data.tempDurationAdd;
        currentTrait.tempSpeedAdd += data.tempSpeedAdd;
        currentTrait.tempCritAdd += data.tempCritAdd;


        skillCriticChance += data.critChanceAdd;
        currentTrait.critChanceAdd = skillCriticChance;  //or currentTrait.critChanceAdd;

        casterLevelMax += data.skillMaxLevelAdd;

        //TraitManager.Instance.turnGobalDamage += data.gobalDamageAdd;
        //TraitManager.Instance.turnGobalCooldown += data.gobalCooldownAdd;
        //TraitManager.Instance.turnGobalDuration += data.gobalDurationAdd;
        //TraitManager.Instance.turnGobalSpeed += data.gobalSpeedAdd;
        //TraitManager.Instance.turnGobalSize += data.gobalSizeAdd;
        //TraitManager.Instance.turnGobalProjectileNum += data.gobalProjectilNumAdd;   
        //TraitManager.Instance.turnCritChanceAdd += data.gobalCritChanceAdd;
        
        SkillManager.Instance.luck += data.gobalLuckAdd;

        //if(data.gobalPlayerDefenceAdd != 0)
        //{
        //    BuffManager.Instance.gobalPlayerDefenceAdd += data.gobalPlayerDefenceAdd;
        //}

        //if(data.expGainAdd != 0)
        //{
        //    BuffManager.Instance.gobalExpGain += data.expGainAdd;
        //}



        if (!currentTrait.isSniper)
        {
            if (data.isSniper)
            {
                currentTrait.isSniper = true;

                // スナイパー効果を適用: for sniper trait, projectileNum will become 1, every projectile will increase damage by 50% and size by 30 %
                int oldProjectileNum = projectileNumBase + projectileNumScaler;
                ActiveBuffManager.Instance.AddStack(TraitType.Sniper);
                ActiveBuffManager.Instance.SetStacks(TraitType.Sniper, projectileNumScaler);
                projectileNumBase = 1;
                projectileNumScaler = 0;
                damageScaler += (oldProjectileNum - 1) * 100; // もともとの弾数分ダメージアップ
                sizeScaler += (oldProjectileNum - 1) * 50;
                durationScaler += (oldProjectileNum - 1) * 5;
                speedScaler += (oldProjectileNum - 1) * 5;

                //Debug.Log("Sniper Trait Applied");
                //Debug.Log($"ProjectileNum: {projectileNumBase + projectileNumScaler}, DamageScaler: {damageScaler}, SizeScaler: {sizeScaler}");

            }
        }

    }

    void StopCasting()
    {
        isStopCasting = true; // スキルを止める
        stopCastCnt = 0.7f;    // 1秒間スキルを止める
    }

    void HandleAfterDashAction()
    {
        if (isAfterDashEnhanced)
        {
            isJustDash = true;
            SkillEffectManager.Instance.SpawnDashCastEffect();
            ActiveBuffManager.Instance.AddStack(TraitType.DashEnhancedSkill);
        }

        if (isAfterDashCast)
        {
           Invoke(nameof(CastSkillInstantly), 0.14f); 
            SkillEffectManager.Instance.SpawnDashCastEffect();
            ActiveBuffManager.Instance.AddStack(TraitType.DashReleaseSkill);
        }

        
    }

    void HandlePlayerGetDamageAction()
    {
        if (isHpChangeEnhanced) isJustHpChange = true;

        if (isHpChangeCast)
        {
            CastSkillInstantly();
            SkillEffectManager.Instance.SpawnGetDamageCastEffect();
            ActiveBuffManager.Instance.AddStack(TraitType.HpChangeReleaseSkill);

            

            EventManager.EmitEvent("TriggerTraitRevenge");
        }
    }

    void HandlePlayerGetHealAction()
    {
        if (isGetHealCast)
        {
            CastSkillInstantly();
            SkillEffectManager.Instance.SpawnGetHealCastEffect();
            ActiveBuffManager.Instance.AddStack(TraitType.HpGetHealCast);
            Debug.Log("Heal Cast Skill Triggered");
        }

    }

    void HandleGetItemAction()
    {

        if (isGetItemCast && SkillEffectManager.Instance.pickItemCastCdCnt <=0)
        {
            SkillEffectManager.Instance.pickItemCastCdCnt = 0.5f;

            CastSkillInstantly();
            SkillEffectManager.Instance.SpawnGetItemCastEffect();
            ActiveBuffManager.Instance.AddStack(TraitType.PickUpReleaseSkill);
        }

    }

    void HandleWalkCastAction()
    {
        if (currentTrait.isWalkCast)
        {
            CastSkillInstantly();
            //SkillEffectManager.Instance.SpawnGetItemCastEffect();
            SkillEffectManager.Instance.SpawnWalkCastEffect();
            ActiveBuffManager.Instance.AddStack(TraitType.WalkReleaseSkill);
        }
    }

    void DelayApplyBuffManagerBuff() //add projectile buff from BuffManager after certain delay
    {
        if (!isBuffManagerProjectileBuffApplied)
        {
            projectileNumScaler += BuffManager.Instance.gobalInitSkillProjNumAdd;
            isBuffManagerProjectileBuffApplied = true;
        }
    }

    void Start()
    {
        bm = BuffManager.Instance; // バフマネージャーのインスタンスを取得

        playerTran = GameObject.FindGameObjectWithTag("Player").transform;
        playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();
        playerControl = playerTran.GetComponent<PlayerController>();
        CalFinalStatus();

        RandTraitPairFromAvailableTraitList();


        //when every game stat clear out current trait data as a pure container
        if (currentTrait) currentTrait.ClearAllData();

        for(int i = 0; i < availableTraitList.Count; i++)
        {
            availableTraitList[i].isSelected = false;
        }

        //isUnlock = AchievementManager.Instance.isSkillUnlocked(casterIdType);

        DOVirtual.DelayedCall(0.5f, DelayApplyBuffManagerBuff);

    }
    void Update()
    {

        if(!hasInitBaseSkill && isPlayerBaseSkill)
        {
            hasInitBaseSkill = true;
            if(characterJobId == (int)GameManager.Instance.playerData.jobId)
            {
                isActivated = true;
            }
        }

        if (isPlayerBaseSkill && characterJobId != (int)GameManager.Instance.playerData.jobId) return;

        if(!isActivated) return; // スキルが有効化されていない場合は何もしない

        UpdatePlayerInfo();               // プレイヤー情報更新
        UpdateSkill();                    // スキルタイマー＆発動処理

        stopCastCnt -= Time.deltaTime;
        if(stopCastCnt <= 0 && isStopCasting) isStopCasting = false;
        if (casterLevel > 10) casterLevel = 10; //Level 制限

        UpdateDPS();                     // DPS更新
        DebugKey();

        SetDebugTraitOnce();




    }

    public void CastSkillInstantly()
    {
        
        HandleCastSkill();
        //castCoolDown = castCoolDownFinal;
    }

    public void UpdateSkill()
    {
        if (EnemyManager.Instance.playerCannotCast) return;

        UpdateSkillCoolDown(); // クールダウンタイマー更新

        if (castCoolDown <= 0 && !isStopCasting) 
        {
             if (!playerStatus.IsAliveFlg) return;
            if (GameManager.Instance.stateMachine.State == GameState.GameClear) return;
            if(CutSceneManager.Instance.isCutSceneStarted) return;
            

            HandleCastSkill();
            castCoolDown = castCoolDownFinal;
        }

    }

    public void UpdateSkillCoolDown()
    {
        //if (isPlayerBaseSkill) 
        //{
        //    //if(!isStopCasting) castCoolDown -= Time.deltaTime;
        //    castCoolDown -= Time.deltaTime;
        //}
        //else castCoolDown -= Time.deltaTime;

        if(!isStopCasting) castCoolDown -= Time.deltaTime;

    }

    public void HandleCastSkill()
    {
        if (EnemyManager.Instance.playerCannotCast) return;

        CalFinalStatus();

        if(isPlayerBaseSkill)
        {
            if(EnemyManager.Instance.playerCannotMove) return;
            EventManager.EmitEvent("PlayerAttack");    //攻撃イベント発行
            Invoke("CastSkill", playerAnimationDelayCnt);

            if (autoMode && isAutoAttackSupported)
            {
                PlayerFaceNearEnemy();
                if (!playerControl.useMouseRotation)
                {
                    SkillEffectManager.Instance.playerFixRotCnt = fixRotTime;
                    SkillEffectManager.Instance.isPlayerFixRot = true;
                }
                
            }
        }
        else
        {
            if (autoMode && isAutoAttackSupported)
            {
                PlayerFaceNearEnemy();
                //SkillEffectManager.Instance.playerFixRotCnt = fixRotTime;
                //SkillEffectManager.Instance.isPlayerFixRot = true;
            }
            CastSkill();                                // 即時スキル発動
        }

        if (isAfterCastSpeedAdd)
        {
            EventManager.EmitEvent("AfterCastSpeedAdd");
        }

    }

    protected abstract void CastSkill(); // スキル固有の発動処理を実装する抽象メソッド


    public void UpdatePlayerInfo()
    {
        playerForwardVec = playerTran.forward;
    }

    public void CalFinalStatus()
    {

        if (isJustDash)
        {
            isJustDash = false;
            damageFinal = damageBase * (1 + damageScaler / 100 + currentTrait.tempDamageAdd/100 + bm.gobalDamageAdd/100 + bm.gobalItemEffectAdd) ;
            speedFinal = speedBase * (1 + speedScaler / 100 + currentTrait.tempSpeedAdd / 100);
            sizeFinal = sizeBase * (1 + sizeScaler / 100 + currentTrait.tempSizeAdd / 100 + bm.gobalSizeAdd / 100);
            projectileNumFinal = projectileNumBase + projectileNumScaler + currentTrait.tempProjectilNumAdd;
            castCoolDownFinal = (Mathf.Max(castCoolDownMaxBase * coolDownFactor * (1f - currentTrait.tempCooldownAdd / 100f), castCoolDownMaxBase * 0.2f)) * (1- (bm.gobalCooldownAdd/100));
            durationFinal = durationBase * (1 + durationScaler / 100 + currentTrait.tempDurationAdd / 100);
            if (durationFinal > castCoolDownFinal && casterIdType == SkillIdType.CircleBall) durationFinal = castCoolDownFinal;       // 最大持続時間
            if(projectileNumFinal > projectileNumMax) projectileNumFinal = projectileNumMax;

        }
        else
        {
            damageFinal = damageBase * (1 + damageScaler / 100 + bm.gobalDamageAdd/100);
            speedFinal = speedBase * (1 + speedScaler / 100 + bm.gobalSpeedAdd/100);
            sizeFinal = sizeBase * (1 + sizeScaler / 100 + bm.gobalSizeAdd/100);
            projectileNumFinal = projectileNumBase +projectileNumScaler;
            castCoolDownFinal = (Mathf.Max(castCoolDownMaxBase * coolDownFactor, castCoolDownMaxBase * 0.2f)) * (1- (bm.gobalCooldownAdd/100));
            durationFinal = durationBase * (1 + durationScaler / 100 + bm.gobalDurationAdd/100);
            //if (castCoolDownFinal < 0.14f) castCoolDownFinal = 0.35f;                       // 最小クールダウン
            if (durationFinal > castCoolDownFinal && casterIdType == SkillIdType.CircleBall) durationFinal = castCoolDownFinal;       // 最大持続時間
            if(projectileNumFinal > projectileNumMax) projectileNumFinal = projectileNumMax; // 最大弾数制限
        }

        //if (isSlotAddDamage)
        //{
        //    int slot = traitDataHoldingList.Count;
        //    damageFinal *= (1 + slot * 0.15f);
        //}

        if (isBloodPrice)
        {
            damageFinal *= 1.3f;

        }

        if (isGambler)
        {
            float rand = Random.Range(0f, 1f);
            if (rand <= 0.5f || SkillEffectManager.Instance.universalTrait.isFairJudge)
            {
                damageFinal *= 2f;
                ActiveBuffManager.Instance.AddStack(TraitType.Gambler);
            }
            else
            {
                damageFinal = 0;
            }
        }

        if (isStandStillEnhance && SkillEffectManager.Instance.isPlayerStandStill)
        {
            sizeFinal *= 1.42f;
            damageFinal *= 1.35f;
            ActiveBuffManager.Instance.AddStack(TraitType.StandStillEnhance);
        }

        //if (currentTrait.isLuckAddCrit)
        //{
        //    skillCriticChance = SkillManager.Instance.luck;
        //}

    }


    public void IncreaseStatus(SkillStatusType type, float value, bool isLevelUp)　　// スキルのレベルを上げる処理(ステータスアップ)
    {
         int cur = CurrentStatusLevel(type);
        statusLevels[type] = cur + 1; 

        if(isLevelUp)casterLevel++;

        //if(type == SkillStatusType.Cooldown) castCoolDownMaxScaler -= value;
         if(type == SkillStatusType.Cooldown) coolDownFactor *= 1f - value / 100f;
        else if (type == SkillStatusType.Speed) speedScaler += value;
        else if (type == SkillStatusType.Duration) durationScaler += value;
        else if (type == SkillStatusType.Damage) damageScaler += value;
        else if (type == SkillStatusType.Size) sizeScaler += value;
        else if (type == SkillStatusType.ProjectileNum) projectileNumScaler += (int)value;



    }

    public void EnableSkillCollider(GameObject skillObj)
    {
        skillObj.GetComponent<Collider>().enabled = true;
    }
    public void DisableSkillCollider(GameObject skillObj)
    {
        skillObj.GetComponent<Collider>().enabled = false;
    }

    public void DebugKey()
    {
        string skillName = casterIdType.ToString() + "lv";
        DebugMenu.Instance.ShowInt(skillName, casterLevel);
    }

    public void UpdateDPS() //武器DPS更新
    {
         float oneShot = damageFinal * projectileNumFinal;
         float cycle   = Mathf.Max(castCoolDownFinal, 0.001f);  // safety
         nowDps = oneShot / cycle;
    }


}
