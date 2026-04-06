using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound;

/// <summary>
/// スキルエフェクトの共通挙動（サイズ更新・寿命管理・
/// 衝突判定・ダメージ付与など）を実装する基底クラス。
/// 子クラスは HandleSkill(), HandEndAction, HandleOnHitAction を override して
/// パーティクル再生・コライダーなど
/// </summary>

public abstract class SkillModelBase : MonoBehaviour
{

    [Header("スキルプールとエフェクトオブジェクト、手動で設定する必要があります")]
    public ObjectPool effectObjPool;            // エフェクトオブジェクトのプール
    public ParticleSystem ps;                   // パーティクルシステムのキャッシュ
    public ParticleSystem finalPs;              //進化したスキルのパーティクルシステム（最終スキル用）
    public ParticleSystem subPs; // サブパーティクルシステム（必要に応じて使用）
    

    public GameObject skillMuzzleObject;
    public GameObject effectAfterMathObj;
    public bool isEffectAfterMath = false;
    public bool isSkillMuzzle = false; //スキルを終わる時のエフェクト
    public bool isFinalSkill = false; // 最終スキルかどうかのフラグ

    public bool hasEndAction = false; // スキル終了時に特別なアクションがあるかどうかのフラグ
    public bool isEndActionFinished = false; // スキル終了時のアクションが完了したかどうかのフラグ

    public bool hasOnHitAction = false; // スキルがヒットしたときに特別なアクションがあるかどうかのフラグ
    public bool isOnHitActionFinished = false; // スキルがヒットしたときのアクションが完了したかどうかのフラグ

    [Header("スキル情報(Casterが自動で設定するので、入力する必要はない)")]
    public SkillIdType skillIdType;             // スキルの種類
    public Vector3 skillBaseSize = Vector3.one; // スキルエフェクトの基準サイズ
	public float skillDuration;                 // スキル効果の継続時間（秒）
	public float skillSpeed;                    // スキル移動速度
	public float skillDamage;                   // スキルが与えるダメージ量
	public float skillSize;                      // スキルの拡大縮小倍率
	public int skillId;                         // スキルを識別するID
	public int skillLevel;                      // スキルのレベル
	public string skillName;                    // スキル名

    public float skillCriticalChance = 0f; // スキルのクリティカルヒット率

    [Header("ほか")]
    public float collisionStartTime;            // 衝突判定を開始する時間（秒）
    public float collisionEndTime;              // 衝突判定を終了する時間（秒）
    public float posYOffset = 0.5f;             // スキル生成位置のYオフセット
    public float posYOffsetScalerWithSize = 0f; // スキルサイズに応じたYオフセット倍率

    public Collider skillCollider;
    public bool isColliderInit = false;
    
    //Trait//
    public bool isAfterDashCast = false; // ダッシュ後にスキルを発動するかどうか
    public bool isAfterDashEnhanced = false; // ダッシュ後にスキルを強化するかどうか

    public bool isGetHealCast = false;
    public bool isHpChangeCast = false; // HP変化時にスキルを発動するかどうか
    public bool isHpChangeEnhanced = false; // HP変化時にスキルを強化するかどうか

    public bool isFinishCastExplosion = false;
    public bool isFinishCastSplit = false;

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

    public bool isPushback = false;

    public bool isGetItemCast = false;
    public bool isGetItemEnhanced = false;

    public bool isColorTinted = false;      //色を変えるかどうか
    public Color tintedColor = Color.white; //変える色

    public bool isTreasureHunter = false; //トレジャーハンター特性

    public bool isSlotAddDamage = false; //スロットでダメージ増加
    public bool isGambler = false; //ギャンブラー特性
    public bool isFairJudge = false; //フェアジャッジ特性
    public bool isAfterCastSpeedAdd = false; //キャスト後にスピードアップ

    public bool isDealMoreDamageFullHp = false; //満タン時にダメージ増加
    public bool isKillEnemyDropFood = false;

    public float skillMoveCnt = 0f;
    public float skillMoveCntMax = 0.035f;
    public float spawnDistNeed = 0.77f;
    public Vector3 previousFireSpawnPos = Vector3.zero;

    public Vector3 traitExplosionOffset = Vector3.zero;

    public bool isWalkFire = false; //歩く火炎エフェクト

    public bool isKillEnemyBigger = false;

    public float originSkillSize = 1f;
    public float enemyBiggerSizeAdd = 0f;

    public bool isELementalist = false;

    public bool isShowDamageNumber = true; //ダメージ数値を表示するかどうか


    private void OnEnable()
    {
        //Please do not do it here in OnEnable , as the staus has not get updated , causing the delay status bug!!! , instead , do the skillInit in castSkill Manually!
        //HandleSkillInit();                       // ★子クラス固有処理
        //UpdateSkillSize();
        //AdjustY();
        //
        //isOnHitActionFinished = false;

        if (!isColliderInit)
        {
            isColliderInit = true;
            skillCollider = GetComponent<Collider>();
        }

    }

    public void InitStatusSizeAndPosY()
    {
        HandleSkillInit();                       // ★子クラス固有処理
        UpdateSkillSize();
        AdjustY();

        isOnHitActionFinished = false;

        if (isKillEnemyBigger)
        {
            ActiveBuffManager.Instance.SetStacks(TraitType.KillEnemyBigger, 1);
            enemyBiggerSizeAdd = 0f;
            originSkillSize = skillBaseSize.x * skillSize;
        }
    }

    private void OnDisable()
    {
        UpdateSkillSize();
    }

    void Start()
    {
        if (!isColliderInit)
        {
            isColliderInit = true;
            skillCollider = GetComponent<Collider>();
        }

        UpdateSkillSize();
        //AdjustY(); //do not adjust Y at start as it cause double Y offset and twice height issue of first skill object 

    }

    void Update()
    {
        UpdateSkillLife();
        HandleSkillUpdateAction();

        //Fire Path
        skillMoveCnt -= Time.deltaTime;
        if (skillMoveCnt <= 0 && isSkillMovingDropFire)
        {
            Vector3 nowSpawnPos = transform.position;
            float distNowPre = Vector3.Distance(nowSpawnPos, previousFireSpawnPos);
            if(distNowPre < spawnDistNeed) return; //前回のスポーン位置と近い場合はスキルを発動しない

            previousFireSpawnPos = nowSpawnPos; 
            skillMoveCnt = skillMoveCntMax;
            SkillEffectManager.Instance.SpawnSkillFireObj(transform.position, skillIdType);
            //Debug.Log($"SpawnSkillFireObj: {transform.position}"); //Debug
        }

    }

    protected void EnableCollision()
    {
        skillCollider.enabled = true;

        //GetComponent<Collider>().enabled = true; //Optimize-Required
    }
    protected void DisableCollision()
    {
        skillCollider.enabled = false;
        //GetComponent<Collider>().enabled = false;
    }

    public void SetSkill(SkillIdType type, int id, int level, string name, float duration, float speed, float damage, float size,bool _isFinalSkill) 
    {
        // スキル情報をまとめて設定するヘルパー関数
        // ItemManagerのパワーアップ効果も考慮してダメージ算出
        skillIdType = type;
        skillId       = id;
        skillLevel    = level;
        skillName     = name;
        skillDuration = duration;
        skillSpeed    = speed ;
        skillDamage   = damage * ItemManager.Instance.powUpAmount;
        skillSize     = size;
        isFinalSkill = _isFinalSkill;

    }

    public void CopyTrait(bool isfinalExplode, bool isKillExplode, bool isKillSkull, bool isKillBright, bool isMovingFire, bool _isPushback, bool isFire, bool isIce, bool isPoison, bool isTreasureHunter)
    {
        isFinishCastExplosion = isfinalExplode;
        isKillEnemyExplosion = isKillExplode;
        isKillEnemySkullSoul = isKillSkull;
        isKillEnemyBrightSoul = isKillBright;
        isSkillMovingDropFire = isMovingFire;
        isPushback = _isPushback;
        isEnchantFire = isFire;
        isEnchantIce = isIce;
        isEnchantPoison = isPoison;
        this.isTreasureHunter = isTreasureHunter;


    }

    public void SetTrait(TraitData data)
    {
        // TraitDataを受け取り、スキルの特性を設定するヘルパー関数
        isAfterDashCast = data.isAfterDashCast;
        isAfterDashEnhanced = data.isAfterDashEnhanced;
        isHpChangeCast = data.isHpChangeCast;
        isHpChangeEnhanced = data.isHpChangeEnhanced;

        isFinishCastExplosion = data.isFinishCastExplosion;
        isFinishCastSplit = data.isFinishCastSplit;


        isKillEnemyExplosion = data.isKillEnemyExplosion;
        isKillEnemySkullSoul = data.isKillEnemySkullSoul;
        isKillEnemyBrightSoul = data.isKillEnemyBrightSoul;

        isSkillMovingDropFire = data.isSkillMovingDropFire;
        isSkillMovingDropSpike = data.isSkillMovingDropSpike;

        isDoubleCast = data.isDoubleCast;

        isEnchantFire = data.isEnchantFire;
        isEnchantIce = data.isEnchantIce;
        isEnchantPoison = data.isEnchantPoison;
        isEnchantLightning = data.isEnchantLightning;
        
        isPushback = data.isPushback;

        isGetItemCast = data.isGetHealItemCast;
        isGetItemEnhanced = data.isGetHealItemEnhanced;

        skillCriticalChance = data.critChanceAdd + data.tempCritAdd;

        if (data.isBloodPrice)
        {
            skillCriticalChance += 30f;
        }

        if(data.isIsolator && SkillEffectManager.Instance.isPlayerIsolated)
        {
            skillCriticalChance += 50f;
        }

        if (data.isLuckAddCrit)
        {
            skillCriticalChance += (BuffManager.Instance.gobalLuckAdd + SkillManager.Instance.luck);
        }

        isGetHealCast = data.isGetHealItemCast;

        isTreasureHunter = data.isTreasureHunter;

        isSlotAddDamage = data.isSlotAddDamage;
        isGambler = data.isGambler;
        isFairJudge = data.isFairJudge;
        isAfterCastSpeedAdd = data.isAfterCastSpeedAdd;

        isDealMoreDamageFullHp = data.isDealMoreDamageFullHp;
        isKillEnemyDropFood = data.isKillEnemyDropFood;

        isKillEnemyBigger = data.isKillEnemeyBigger;
        isELementalist = data.isELementalist;

        if (isELementalist)
        {
            //rand isEnchantFIre , isEnchantIce , isEnchantPoison
            int randEle = Random.Range(0, 3);
            if (randEle == 0)
            {
                isEnchantFire = true;
            }
            else if (randEle == 1)
            {
                isEnchantIce = true;
            }
            else if (randEle == 2)
            {
                isEnchantPoison = true;
            }
        }


    }

    public void SetTraitManual(bool _isAfterDashCast, bool _isAfterDashEnhanced, bool _isHpChangeCast, bool _isHpChangeEnhanced, bool _isFinishCastExplosion, bool _isFinishCastSplit, bool _isKillEnemyExplosion, bool _isKillEnemySkullSoul, bool _isKillEnemyBrightSoul, bool _isSkillMovingDropFire, bool _isSkillMovingDropSpike, bool _isDoubleCast, bool _isEnchantFire, bool _isEnchantIce, bool _isEnchantPoison, bool _isEnchantLightning, bool _isPushback, bool _isGetHealItemCast, bool _isGetHealItemEnhanced, float _critChanceAdd)
    {
        isAfterDashCast = _isAfterDashCast;
        isAfterDashEnhanced = _isAfterDashEnhanced;
        isHpChangeCast = _isHpChangeCast;
        isHpChangeEnhanced = _isHpChangeEnhanced;

        isFinishCastExplosion = _isFinishCastExplosion;
        isFinishCastSplit = _isFinishCastSplit;

        isKillEnemyExplosion = _isKillEnemyExplosion;
        isKillEnemySkullSoul = _isKillEnemySkullSoul;
        isKillEnemyBrightSoul = _isKillEnemyBrightSoul;

        isSkillMovingDropFire = _isSkillMovingDropFire;
        isSkillMovingDropSpike = _isSkillMovingDropSpike;

        isDoubleCast = _isDoubleCast;

        isEnchantFire = _isEnchantFire;
        isEnchantIce = _isEnchantIce;
        isEnchantPoison = _isEnchantPoison;
        isEnchantLightning = _isEnchantLightning;

        isPushback = _isPushback;

        isGetItemCast = _isGetHealItemCast;
        isGetItemEnhanced = _isGetHealItemEnhanced;

        skillCriticalChance = _critChanceAdd + SkillEffectManager.Instance.universalTrait.tempCritAdd;

    }

    public void SetSkillCondition(bool _isAfterDashCast, bool isAfterDashEnhanced, bool _isFinishCastExplosion, bool _isSkillMovingDropFire, bool _isSkillMovingDropSpike)
    {

    }

    public void DestroySkill()
    {
        if(hasEndAction && !isEndActionFinished)
        {
            HandleSkillEndAction();
            return;
        }

        if (isFinishCastExplosion)
        {
            SkillEffectManager.Instance.SpawnSkillExplosion(transform.position,skillIdType);
        }

        if (ps != null) ps.Stop();
        if (finalPs != null) finalPs.Stop();
        if (effectObjPool != null) effectObjPool.Release(this.gameObject);

    }

    public void ClearOutSkill()
    {
        if (ps != null) ps.Stop();
        if (finalPs != null) finalPs.Stop();
        if (effectObjPool != null) effectObjPool.Release(this.gameObject);
    }

    public void UpdateSkillLife() // スキルの寿命を管理し、時間切れでエフェクトを停止してプールへ返却
    {
        skillDuration -= Time.deltaTime;
        if (skillDuration > 0) return;

        if (skillDuration <= 0 && hasEndAction && !isEndActionFinished)
        {
            HandleSkillEndAction();
            return;
        }

        if (isFinishCastExplosion)
        {
            //float  ExplodeChance = isEnchantFire ? 100f : 50f;
            SkillEffectManager.Instance.SpawnSkillExplosion(transform.position + traitExplosionOffset,skillIdType,skillDamage * 0.56f,skillSize);
        }

        if (isFinishCastSplit)
        {
            SkillEffectManager.Instance.SpawnSplitMagicBullet(transform.position, skillIdType, isSkillMovingDropFire, isPushback, isEnchantFire, isEnchantIce, isEnchantPoison, skillDamage * 0.33f, isFinishCastExplosion, isKillEnemyExplosion, isKillEnemyBrightSoul);
        }

      

        if (ps != null) ps.Stop();
        if (finalPs != null) finalPs.Stop();
        if (effectObjPool != null)
        {
            effectObjPool.Release(this.gameObject);          
            //Debug.Log($"Skill Released to Pool: {skillName} - {skillIdType}");
        }

    }

    public  void UpdateSkillSize()
    {
        transform.localScale = new Vector3(skillBaseSize.x * skillSize, skillBaseSize.y * skillSize, skillBaseSize.z * skillSize);
    }

    public void UpdateYOffsetBySize()
    {
        // Clamp skillSize so weird values don't break it
    float t = Mathf.Clamp01(skillSize / 7f); // 0 → 0, 400 → 1

    // Linearly interpolate between 0.28 and 0.57
    posYOffset = Mathf.Lerp(0.35f, 1.4f, t);
    }

    protected void AdjustY()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y + (posYOffset * (1 + (posYOffsetScalerWithSize/100)) ), transform.position.z);
    }

    private void OnTriggerEnter(Collider col) // 敵にはダメージを与え、破壊可能オブジェクトにはOnHitを呼び出す
    {
        if (col.CompareTag("Enemy")) {
            EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();

            if (hasOnHitAction && !isOnHitActionFinished)
            {
                HandleSkillOnHitAction(col);
            }

            float finalDamage = skillDamage;

            

            //Handle Ciritical Hit Case
            bool isCirt = false;
            float FinalCrit = skillCriticalChance + BuffManager.Instance.gobalCritChanceAdd;
            if (FinalCrit > 1f)
            {
                if (Random.Range(0f, 100f) < FinalCrit)
                {
                    float critDmg = 1.0f;

                    if(SkillEffectManager.Instance.universalTrait.isCritDamageAdd) critDmg += 0.4f;

                    finalDamage *= critDmg;
                    isCirt = true;
                }
            }

            if (isDealMoreDamageFullHp)
            {
                if (enemyStat.enemyHp >= (enemyStat.enemyMaxHp * 0.9))
                {
                    finalDamage *= 1.5f;
                    Debug.Log("DealMoreDamageFullHp Triggered" );
                }
            }



            //if (isGambler)
            //{
            //    if(SkillEffectManager.Instance.universalTrait.isFairJudge)
            //    {
            //        finalDamage *= 2f;
            //        ActiveBuffManager.Instance.AddStack(TraitType.Gambler);
            //    }
            //    else
            //    {
            //        int rand50 = Random.Range(0, 100);
            //        if(rand50 > 50) finalDamage *= 2f;
            //        else if(rand50 < 25) finalDamage *= 0f;
            //    }

            //}

            if (isKillEnemyExplosion || isKillEnemySkullSoul || isKillEnemyBrightSoul || isTreasureHunter ||isKillEnemyDropFood || isKillEnemyBigger)
            {
                if(finalDamage >= enemyStat.enemyHp)
                {
                    if (isKillEnemyBigger)
                    {
                        if (enemyBiggerSizeAdd < 1)
                        {
                            ActiveBuffManager.Instance.AddStack(TraitType.KillEnemyBigger);
                            enemyBiggerSizeAdd += 0.05f;
                            transform.localScale = new Vector3(skillBaseSize.x * (skillSize + enemyBiggerSizeAdd), skillBaseSize.y * (skillSize + enemyBiggerSizeAdd), skillBaseSize.z * (skillSize + enemyBiggerSizeAdd));
                        }
                        //skillSize += 0.15f;
                        //UpdateSkillSize();
                    }

                    if (isKillEnemyDropFood)
                    {
                        float chance = 3f;
                        if (SkillEffectManager.Instance.universalTrait.isLuckySeven) chance += 7f;
                        if (Random.Range(0f, 100f) <= chance)
                        {
                            SkillEffectManager.Instance.SpawnFoodItemObj(enemyStat.transform.position);
                        }
                    }

                    enemyStat.TakeKillTraitDebuff(isKillEnemyExplosion, isKillEnemySkullSoul,skillSize,skillDamage/2,isKillEnemyBrightSoul , isTreasureHunter);
                }
            }
            
            if (isEnchantFire)
            {
                enemyStat.ApplyFireDebuff();
            }

            if (isEnchantIce)
            {
                enemyStat.ApplyIceDebuff();
            }

            if (isEnchantPoison)
            {
                enemyStat.ApplySpeedDownDebuff(0.56f,3f);
            }

            //敵にダメージを与える
            enemyStat.TakeDamage(finalDamage,isShowDamageNumber,skillIdType,isCirt);   
            
            if (isPushback)
            {
                enemyStat.PushBackEnemyItselfAwayFromPlayer();
            }

            //Debug on hit infor
            //Debug.Log("Skill Hit Enemy: " + enemyStat.gameObject.name + " | Damage: " + finalDamage + " | IsCrit: " + isCirt);


        }

        //破壊可能オブジェクトに当たったらOnHitを呼び出す
        if (col.CompareTag("DestroyObj"))
        {
            DestroyObjectController destroyObj = col.GetComponent<DestroyObjectController>();
            if (destroyObj != null) destroyObj.OnHit();
        }
    }



    /// <summary>
    /// 各スキル固有の初期化処理を実装してください
    /// （パーティクル再生、コライダー切り替えなど）
    /// </summary>
    protected abstract void HandleSkillInit();

    protected abstract void HandleSkillUpdateAction();

    protected abstract void HandleSkillEndAction();

    protected abstract void HandleSkillOnHitAction(Collider col);


}

