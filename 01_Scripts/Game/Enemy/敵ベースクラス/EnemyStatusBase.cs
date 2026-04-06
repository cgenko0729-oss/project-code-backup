using UnityEngine;
using QFSW.MOP2;
using UnityEngine.Rendering;
using DamageNumbersPro;
using System.Collections;
using System.Collections.Generic;
using TigerForge;
using DG.Tweening;
using Hellmade.Sound;

public class EnemyStatusBase : MonoBehaviour, IDamageable
{
    [Header("敵プール指定")]
    public ObjectPool enemyPool;            // 敵本体を再利用するプール
    public ObjectPool enemyDeadEffectPool;　// 死亡エフェクト用プール
    public ObjectPool expPool;              // 経験値用プール
    public DamageNumber damageNumPrefab;　　// ダメージ数字エフェクトのプレハブ

    [Header("敵種類")]
    public EnemyType enemyType;         // 敵の種類
    public string enemyName;            // 敵の名前
    public int enemyId;                 // 敵のID
    public bool isPooled = true;       // プールから取得されたかどうか

    [Header("敵ステータス設定")]
    public float enemyHp;　　              // 敵のHP    
    public float enemyMaxHp;　            // 敵の最大HP
    public float enemyHpScaler; // 敵のHPスケーラー（1.0f で通常、1.5f で1.5倍など）

    public float enemyLifeCnt;  　       // 存在可能時間（秒）
    public float enemyLifeCntMax;       // 存在可能時間の最大値
    public bool isDeadByLifeTime = false; // 時間切れで死亡させるかどうか
    public bool isAlive = true;         // 生存フラグ

    public float enemyMoveSpd = 0.7f;　　 // 移動速度

    public float enemyAtkDamage;    　　　// 当たり判定でプレイヤーに与えるダメージ量

    [Header("ダメージエフェクト")]
    public Color startColor;
    public Color flashColor  = new Color(0.77f,0f,0f,1f);     // 被ダメージ時のフラッシュ色
    public Color iceColor = Color.blue; // 氷のデバフ時の色
    public Color GoldColor = Color.yellow; // 金色のデバフ時の色
    [SerializeField] float flashLengthPulseSec = 0.4f;  // フラッシュの継続時間
    const int  FlashPulses = 3;                         // フラッシュの繰り返し回数

    int   colorPropId = -1;            // シェーダーのカラーID
    float flashTimer;                  // フラッシュの残り時間
    Renderer[]              renderers;  // 子オブジェクトを含む全 Renderer
    MaterialPropertyBlock[] blocks;     // 各 Renderer 用のプロパティブロック

    public Color explodeColor = new Color(4, 4, 4, 1);
    public float explodeLengthPulseSec = 1.1f;
    public const int explodePulses = 3;
    public float explodeTimer;

    [Header("攻撃エフェクト")]
    public float attackLungeDistance = 0.5f;
    public float attackLungeDuration = 0.1f;
    public Ease  attackEase = Ease.OutQuad;

    public Vector3 punchRotationAmount = new Vector3(30f, 0f, 0f); //30 about X
    public float punchDuration        = 0.2f;
    public int   punchVibrato         = 10;   
    public float punchElasticity      = 0.5f; 
    public float attackCdCnt = 3f;
    public float attackCdCntMax = 3f; // 攻撃クールダウンタイマー

    public float attackStandbyCnt = 1f; // 攻撃待機時間
    public float attackStandbyCntMax = 1f; // 攻撃待機時間の最大値

    public float expDropRateBase = 70f; // 経験値ドロップ率のベース値

    public bool isSpiderDen = false;
    public int spiderDenId = -1;

    public bool hasHitFlashEffect = true; // 被ダメージフラッシュエフェクトを持つかどうか

    public bool isMidBoss = false;
    public bool isBoss = false;
    public bool isBossDead = false; // ボスが死亡したかどうか
    public bool isBossDragon = false;

    public SoundList deadSound = SoundList.SpiderMobDead; // 死亡時のサウンド

    public bool isEndingPhrase = false;

    public bool isDropCoin = false;

    public int currentGamePhase = -1;
    public bool isHpFixed = false;

    private static Camera mainCamera;
    private static TimeManager timeManager;
    private static GameQuestManager gameQuestManager;
    private static EnemyManager enemyManager;
    private static MapManager mapManager;
    private static StageManager stageManager;
    private static CoinSpawner coinSpawner;
    private static SoundEffect soundEffect;
    private static SkillEffectManager skillEffectManager;

    //== デバフ関連 ===
    EnemyActionBase enemyActionBase;
    //public bool isDebuffApplicable = true; // デバフが適用可能かどうか
    //public bool isDebuffApplying = false; // デバフが適用中かどうか
    public bool isBuffInit = false;
    public bool isDebuffApplied = false; // デバフが適用されたかどうか

    public bool isIceDebuff = false;  //unmovable or speedDown
    public bool isGoldDebuff = false;  //金色にするデバフ
    public bool isFireDebuff = false; //dot dmg per second
    public bool isPoisonDebuff = false;// speedDown + increase dmg received
    
    public bool isDebuffMarked = false; // ステータスが変更されたかどうかのフラグ DirtyFlg
    public bool isKillExplode = false;
    public bool isKillSkullSoul = false; // スキルの特性によるデバフ（敵を倒したときに発動する）
    public bool isKillBrightSoul = false;
    public bool isTreasureHunter = false;

    public float killExplodeSize = 1f;
    public float killExplodeDmg = 35f;

    public ParticleSystem fireDebuffPs;
    public ParticleSystem iceDebuffPs;
    public ParticleSystem poisonDebuffPs;

    public float fireDebuffCnt = 0f;
    public int fireDebuffDmgTime = 0;

    public float iceDebuffFactor = 1f; // 氷のデバフ倍率
    public float iceDebuffDuration = 5f; // 氷のデバフ持続時間

    public float poisonSpeedDownFactor = 1f; // スピードダウンの倍率
    public float poisonSpeedDownDuration = 5f; // スピードダウンの持続時間

    public float goldDebuffFactor = 1f; // 金色デバフ倍率
    public float goldDebuffDuration = 0f; // 金色デバフ持続時間


    public bool hasDropExped = false; //prevent drop twice exp in one life

    public bool isPushBack = false;
    public float pushBackCooldown = 0.5f;

    public bool isAttackPushBackPlayer = false;

    public bool canEnemyRotate = true;

    public bool canSpawnHpPotion = false;
    public bool canSpawnTraitChest = false;

    [Header("ペットデータ")]
    [SerializeField]private PetData petData; // この敵に関連付けられたペットデータ

    //最後に攻撃したやつを記録する
    private LastAttackType lastAttacker;

    // スピードダウン率
    public float SpdDownRate = 1f; 

    public int endlessEnemyHpStage = 0;
    public bool isFinalHpInit = false;
    public float finalHp = 100f;

    public void ApplyFireDebuff()
    {
        if (enemyType == EnemyType.Boss || enemyType == EnemyType.MidBoss || enemyType == EnemyType.NoEnemy || enemyType == EnemyType.Slime) return;
        //debug log enemy type and object.name
        //Debug.Log("Apply Fire Debuff to " + enemyType + " - " + gameObject.name);

        isDebuffApplied = true;

        if (!fireDebuffPs) return;
        isFireDebuff = true;
        fireDebuffDmgTime = 4;
        fireDebuffCnt = 1.4f;
        if (fireDebuffPs)fireDebuffPs.Play();
    }
    public void UpdateFireDebuff()
    {

        if (isFireDebuff)
        {
            if (fireDebuffDmgTime > 0)
            {
                fireDebuffCnt -= Time.deltaTime;
                if (fireDebuffCnt <= 0f)
                {
                    fireDebuffDmgTime--;
                    fireDebuffCnt = 1.5f;
                    DealFireDebuffDmg();
                }
            }else if(fireDebuffDmgTime <= 0)
            {
                isFireDebuff = false;
                fireDebuffPs.Stop();
            }

        }

    }

    public void DealFireDebuffDmg()
    {
        float dmg = SkillEffectManager.Instance.universalTrait.isFireEnhanced ? 40f : 20f;
        TakeDamage(dmg, true,SkillIdType.None);

    }

    public void ApplySpeedDownDebuff(float speedDownVal,float speedDowDuration)
    {
        if (enemyType == EnemyType.Boss || enemyType == EnemyType.MidBoss || enemyType == EnemyType.NoEnemy || enemyType == EnemyType.Slime) return;

        isDebuffApplied = true;
        isPoisonDebuff = true;
        poisonSpeedDownDuration = speedDowDuration; // スピードダウンの持続時間をリセット
        if(poisonSpeedDownFactor > speedDownVal) poisonSpeedDownFactor = speedDownVal; // スピードダウンの倍率を設定（現在の値より小さい場合のみ適用）
        if (poisonDebuffPs)poisonDebuffPs.Play();
    }

    public void ApplyIceDebuff()
    {
        if (enemyType == EnemyType.Boss || enemyType == EnemyType.MidBoss || enemyType == EnemyType.NoEnemy || enemyType == EnemyType.Slime) return;

        isDebuffApplied = true;
        isIceDebuff = true;
        iceDebuffDuration = 3f;
        // 氷のデバフ倍率を0に設定（移動不可状態）
        if (SkillEffectManager.Instance.universalTrait.isIceEnhanced)
        {
            iceDebuffFactor = 0f;
            enemyActionBase.isSeperateEnabled = false;
        }
        else
        {
            iceDebuffFactor = 0.35f;
        }
        
        SetIceDebuffColor();
        enemyActionBase.SetAnimSpeed(iceDebuffFactor);
        if (iceDebuffPs)iceDebuffPs.Play();
    }

    void UpdateIceDebuff()
    {
        if (isIceDebuff)
        {
            iceDebuffDuration -= Time.deltaTime; // 氷のデバフの持続時間を減少
            if (iceDebuffDuration <= 0f)
            {
                enemyActionBase.isSeperateEnabled = true;
                isIceDebuff = false; // 氷のデバフを無効化
                iceDebuffFactor = 1f; // 氷のデバフ倍率をリセット
                //enemyActionBase.isSeperateEnabled = true;
                ResetColor();
                enemyActionBase.ResetAnimSpeed();
                if (iceDebuffPs) iceDebuffPs.Stop();
            }
        }
    }

    public void UpdatePoisonSpeedDebuff()
    {
        poisonSpeedDownDuration -= Time.deltaTime; // スピードダウンの持続時間を減少

        if (isPoisonDebuff && poisonSpeedDownDuration <=0)
        {
            isPoisonDebuff = false; // スピードダウンを無効化
            poisonSpeedDownFactor = 1f; // スピードダウンの倍率をリセット
            if (poisonDebuffPs) poisonDebuffPs.Stop();
        }
    }

    public void SetIceDebuffColor()
    {
        isIceDebuff = true;

        for (int i = 0; i < renderers.Length; ++i)
        {
            var r = renderers[i];
            var pb = blocks[i];
            r.GetPropertyBlock(pb);
            pb.SetColor(colorPropId, iceColor);
            r.SetPropertyBlock(pb);
        }

    }

    public void SetGoldDebuffColor()
    {
        isGoldDebuff = true;

        Color richGold = new Color(1.0f, 0.75f, 0.1f);

        GoldColor = richGold;

        for (int i = 0; i < renderers.Length; ++i)
        {
            var r = renderers[i];
            var pb = blocks[i];
            r.GetPropertyBlock(pb);
            pb.SetColor(colorPropId, GoldColor);

            pb.SetFloat("_Metallic", 1.0f);  
            pb.SetFloat("_Glossiness", 0.9f); 

            if (r.sharedMaterial.HasProperty("_EmissionColor"))
            {
                pb.SetColor("_EmissionColor", richGold*1.5f);
            }

            r.SetPropertyBlock(pb);
        }
    }

    public void ApplyGoldDebuff(float duration)
    {
        if (enemyType == EnemyType.Boss || enemyType == EnemyType.MidBoss || enemyType == EnemyType.NoEnemy || enemyType == EnemyType.Slime) return;

        isDebuffApplied = true;
        isGoldDebuff = true;
        goldDebuffDuration = duration;

        goldDebuffFactor = 0f;
        SpdDownRate = 0f;
        enemyActionBase.isSeperateEnabled = false;

        SetGoldDebuffColor();
        enemyActionBase.SetAnimSpeed(goldDebuffFactor);
    }

    void UpdateGoldDebuff()
    {
        //早期リターン
        if (!ActivePetManager.Instance.isGoldMummyInGame) return;

        if (isGoldDebuff)
        {
            goldDebuffDuration -= Time.deltaTime;
            if (goldDebuffDuration <= 0f)
            {
                enemyActionBase.isSeperateEnabled = true;
                isGoldDebuff = false;
                goldDebuffFactor = 1f;
                SpdDownRate = 1f;
                ResetGoldColor();
                enemyActionBase.ResetAnimSpeed();
            }
        }
    }



    public void TakeKillTraitDebuff(bool _isKillExplode, bool _isKillDarkSoul, float _killExplodeSize = 1f, float _killExplodeDmg = 35f, bool _isKillbrightSoul = false, bool _isTreasureHunter = false)//this trait only trigger once after the skill hit
    {
        isDebuffMarked = true;

        isKillExplode = _isKillExplode; // スキルの特性によるデバフを適用 
        isKillSkullSoul = _isKillDarkSoul;
        isKillBrightSoul = _isKillbrightSoul;

        killExplodeSize = _killExplodeSize;
        killExplodeDmg = _killExplodeDmg;

        isTreasureHunter = _isTreasureHunter;

        //ShowDamageNumber(damage,true);
    }




    void Awake()
    {
        enemyId = (int)enemyType;

        renderers = GetComponentsInChildren<Renderer>(true);   // 子オブジェクトを含むすべての Renderer を取得
        blocks    = new MaterialPropertyBlock[renderers.Length];
        for (int i = 0; i < renderers.Length; ++i) blocks[i] = new MaterialPropertyBlock();

        foreach (var r in renderers) // 使うシェーダーの _Color 系プロパティIDを探す
        {
            var mat = r.sharedMaterial;
            if (!mat) continue;

            if (mat.HasProperty("_AlbedoColor")) colorPropId = Shader.PropertyToID("_AlbedoColor");    
            else if (mat.HasProperty("_BaseColor")) colorPropId = Shader.PropertyToID("_BaseColor"); // URP Lit
            else if (mat.HasProperty("_Color")) colorPropId = Shader.PropertyToID("_Color");     // Standard
            else if (mat.HasProperty("_TintColor")) colorPropId = Shader.PropertyToID("_TintColor"); // Unlit
            
            if (colorPropId != -1) break;
        }

        //if(!isBoss)startColor = renderers[0].sharedMaterial.GetColor(colorPropId); //dragon
        if(!isBossDragon)startColor = renderers[0].sharedMaterial.GetColor(colorPropId); //dragon boss use another shader ,special case handling

    }

    void OnEnable()                
    {
        ResetStatus();
        ResetColor();
        ResetGoldColor();
        

        ClearAllDebuffTrait(); // デバフをクリア

        if (TimeManager.Instance.isEndlessTimeStarted && endlessEnemyHpStage < TimeManager.Instance.endlessExtraPhrase)
        {
            if(!isFinalHpInit)
            {
                finalHp = enemyMaxHp;
                isFinalHpInit = true;
            }

            endlessEnemyHpStage = TimeManager.Instance.endlessExtraPhrase;
            enemyMaxHp =finalHp *( 1f + (TimeManager.Instance.endlessExtraPhrase * 0.28f)); //無限モードの追加段階ごとに25%増加 (42)
        }

    }
    

    void Start()
    {
        enemyHp = enemyMaxHp;
        enemyLifeCnt = enemyLifeCntMax;

        CacheStaticReferences();

        if(enemyType != EnemyType.Boss && enemyType != EnemyType.MidBoss)enemyActionBase = GetComponent<EnemyActionBase>();

        //debuff init
        if (fireDebuffPs) fireDebuffPs.Stop();
        if (iceDebuffPs) iceDebuffPs.Stop();
        if (poisonDebuffPs) poisonDebuffPs.Stop();

        if(enemyType == EnemyType.Mover)
        {
            switch (StageManager.Instance.mapData.stageDifficulty)
            {   
                case DifficultyType.None:
                    break;
                case DifficultyType.Normal:
                    enemyAtkDamage *= 1.0f;
                    break;
                case DifficultyType.Hard:
                    enemyAtkDamage *= 1.4f;
                    break;
                case DifficultyType.Nightmare:
                    enemyAtkDamage *= 2.0f;
                    break;
                case DifficultyType.Hell:
                    enemyAtkDamage *= 2.35f;
                    break;
                default:
                    break;
            }

        }

        SetMaxHpByEnemyType();
    }

    

    void Update()
    {
        //UnCastShadow();
        //buffCnt1 -= Time.deltaTime;
        
        UpdateEnemyLife();
        UpdateCounter();       
        UpdateHitFlash();

        UpdateFireDebuff();
        UpdateIceDebuff();
        UpdatePoisonSpeedDebuff();
        UpdateGoldDebuff();
        UpdatePushback();


    }

    void UpdateCounter()
    {
        attackCdCnt -= Time.deltaTime; // 攻撃クールダウンタイマー更新
    }

    void UpdateHitFlash()
    {
        if (flashTimer > 0f && hasHitFlashEffect) // 被ダメージフラッシュ処理
        {
            flashTimer -= Time.deltaTime;
            float t = Mathf.PingPong((flashLengthPulseSec - flashTimer) * FlashPulses * 2f,1f); // 0-1-0-1 
            PushFlash(t);
            if (flashTimer <= 0f)   PushFlash(0);
        }

        if(explodeTimer > 0f) // 爆発フラッシュ処理
        {
            explodeTimer -= Time.deltaTime;
            float t = Mathf.PingPong((explodeLengthPulseSec - explodeTimer) * explodePulses * 2f, 1f); // 0-1-0-1
            PushExplodeFlash(t);
            if (explodeTimer <= 0f) PushExplodeFlash(0);
        }
    }

    

    

    

    

    

    //=======Status Reset=======//
    private static void CacheStaticReferences()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (timeManager == null) timeManager = TimeManager.Instance;
        if (gameQuestManager == null) gameQuestManager = GameQuestManager.Instance;
        if (enemyManager == null) enemyManager = EnemyManager.Instance;
        if(skillEffectManager == null) skillEffectManager = SkillEffectManager.Instance;

        if (mainCamera == null) Debug.LogError("Main Camera is not set in EnemyStatusBase.");
        if (timeManager == null) Debug.LogError("TimeManager is not set in EnemyStatusBase.");
        if (gameQuestManager == null) Debug.LogError("GameQuestManager is not set in EnemyStatusBase.");
        if (enemyManager == null) Debug.LogError("EnemyManager is not set in EnemyStatusBase.");

        if (mapManager == null) mapManager = MapManager.Instance;
        if (stageManager == null) stageManager = StageManager.Instance;
        if (coinSpawner == null) coinSpawner = CoinSpawner.Instance;
        if (soundEffect == null) soundEffect = SoundEffect.Instance;
    }
    public void SetMaxHpByEnemyType()
    {
        if (isBoss || isMidBoss) return;
        if (!enemyManager) return; //?
        if (isHpFixed) return;
        //if (type == EnemyType.Boss || type == EnemyType.MidBoss || type == EnemyType.NoEnemy) return;

        if(currentGamePhase != enemyManager.currentGamePhase)
        {
            currentGamePhase = enemyManager.currentGamePhase; // 現在のゲームフェーズを更新
            enemyMaxHp = enemyManager.GetMaxHpForEnemy(enemyType, currentGamePhase);
            
            if (enemyType == EnemyType.Mover)enemyMoveSpd = enemyManager.GetMoveSpdForEnemy(enemyType, currentGamePhase);
        }


        if(currentGamePhase >= 6)
        {
            if(enemyType == EnemyType.Mover)expDropRateBase = 14f;
            if (enemyType == EnemyType.Flyer) expDropRateBase = 14f;
        }

    }

    public void ResetStatus() // ステータスを初期状態に戻す
    {
        //redesign enemyMaxHp
        SetMaxHpByEnemyType();

        enemyHp = enemyMaxHp;
        enemyLifeCnt = enemyLifeCntMax;
        isAlive = true;
        hasDropExped = false;

        isAlive   = true;
        flashTimer = 0f;      
        attackStandbyCnt = attackStandbyCntMax; // 攻撃待機時間をリセット

        isPushBack = false;

        canEnemyRotate = true;

        SpdDownRate = 1f;

        //Only when debuff polluted will reset 
        if (isDebuffApplied)
        {
            isDebuffApplied = false;
            isIceDebuff = false;
            isGoldDebuff = false;
            isFireDebuff = false;
            isPoisonDebuff = false;

            poisonSpeedDownFactor = 1f;
            iceDebuffFactor = 1f;

            if (fireDebuffPs) fireDebuffPs.Stop();
            if (iceDebuffPs) iceDebuffPs.Stop();
            if (poisonDebuffPs) poisonDebuffPs.Stop();

            enemyActionBase.ResetAnimSpeed();
            enemyActionBase.isSeperateEnabled = true;
        }
        

    }

    public void ClearAllDebuffTrait()
    {
        if (isDebuffMarked)
        {
            isKillExplode = false;
            isDebuffMarked = false;
            isKillSkullSoul = false;
            isKillBrightSoul = false;
            isTreasureHunter = false;
        }
    }

    //=======TakeDamage & Death=======//
    public void UpdateEnemyLife() // HP やライフタイマーのチェックと死亡処理呼び出し
    {
        if(isDeadByLifeTime)enemyLifeCnt -= Time.deltaTime * iceDebuffFactor * poisonSpeedDownFactor;

        HandleDeath();  
      
        if(enemyLifeCnt <= 0 && isDeadByLifeTime)
        { 
            DeadNoExp();
        }

    }

    public void HandleDeath()
    {
        if (enemyHp <= 0)
        {
            if (!isBoss)
            {
                Dead(this.lastAttacker);
            }
            else
            {
                if (isBossDead) return;
                soundEffect.Play(SoundList.FinalHitSe);
                BossDeadTimeDelay();
                Invoke("BossDeadAni", 1f);
                Collider col = GetComponent<Collider>();
                col.enabled = false;
            }
        }
    }

    public void TakeDamage(float damage, bool isShowDamageNumber = true, SkillIdType skillType = SkillIdType.None, bool isCritical = false,LastAttackType LastAttack = LastAttackType.Other)
    {
        if(isPushBack) return;

        // 受け取った攻撃者情報を保存しておく
        this.lastAttacker = LastAttack;

        DpsManager.Instance.ReportDamage(skillType, damage);

        float poisonDebuffPow = SkillEffectManager.Instance.universalTrait.isPoisonEnhanced ? 1.5f : 1.25f; 
        float dmgPoisonMultiplier = isPoisonDebuff ? poisonDebuffPow : 1; // 毒デバフが有効な場合、ダメージを2倍にする
        float criticalMultiplier = isCritical ? 1.35f : 1f; // クリティカルヒットの場合、ダメージを1.5倍にする

        bool isElementResonance = SkillEffectManager.Instance.universalTrait.isElementResonance;
        if (isElementResonance)
        {
            if (isPoisonDebuff || isFireDebuff || isIceDebuff) dmgPoisonMultiplier += 0.5f;
            
        }

        float finalDmg = damage * dmgPoisonMultiplier * criticalMultiplier;
        enemyHp -= finalDmg;
        
        flashTimer = flashLengthPulseSec; // フラッシュ開始

        if (enemyHp <= 0) {
            if(isShowDamageNumber)ShowDamageNumber(finalDmg,true,isCritical); // 死亡時ダメージ数字
            
            

            //if (isCritical) //Trait Critical Healer
            //{
            //    if (SkillEffectManager.Instance.universalTrait.isCriticalHealer)
            //    {
            //        if (Random.Range(0, 100) < 10)
            //        {
            //            EventManager.EmitEventData(GameEvent.ChangePlayerHp, 3f);
            //            Debug.Log("Critical Healer Triggered");
            //        }
            //    }               
            //}

            //if (SkillEffectManager.Instance.universalTrait.isTreasureHunter)
            //{
            //    if(Random.Range(0, 100) < 10)
            //    {
            //        soundEffect.Play(SoundList.DropCoin);
            //    coinSpawner.SpawnCoin(transform.position + new Vector3(0, 0f, 0),Random.Range(1, 3),1.4f);
            //    }              
            //}


            HandleDeath();
            hasDropExped = true;
        }
        else {
            if(isShowDamageNumber)ShowDamageNumber(finalDmg,false,isCritical); // 通常ダメージ数字
        }

        if (isDropCoin)
        {
            soundEffect.Play(SoundList.DropCoin);
            coinSpawner.SpawnCoin(transform.position + new Vector3(0, 0f, 0),Random.Range(1, 3),1.4f); // コインをドロップ
        }

        ClearAllDebuffTrait();


     }

    public void ShowDamageNumber(float dmg,bool isDead,bool isCirtical) // ダメージ数字表示エフェクトを生成
    {
        //if (dmg <= 1) return;
        Vector3 spawnPos =transform.position + Vector3.up * 1.5f;
        //Vector3 screenPos = mainCamera.WorldToScreenPoint(spawnPos); //needless

        if (isCirtical)
        {
            skillEffectManager.ShowCirticalDamageNumber(spawnPos, dmg,transform, isDead);
        }
        else
        {
            DamageNumber dmgNum = damageNumPrefab.Spawn(spawnPos,dmg,transform);
            dmgNum.enableFollowing = !isDead; // 生存中は数字が追従、死亡時は固定
        }
        

    }

    public void Dead(LastAttackType lastAttacker)  // HP切れで死亡
    {
        if(isBossDead) return;
        if (isBoss) return;
        if (hasDropExped) return;

        //最後に攻撃したやつの種類で処理を分ける
        if (lastAttacker == LastAttackType.FlyingDemon)
        {
            //FlyingDemonのキル扱いにする
            EventManager.EmitEvent(GameEvent.LastAttack_FlyingDemon);
        }
        else if(lastAttacker == LastAttackType.Pummy)
        {
            //Pummyのキル扱いにする
            EventManager.EmitEvent(GameEvent.LastAttack_Pummy);
        }
        else if(lastAttacker == LastAttackType.Anubis)
        {
            //nullチェック(無ければ飛ばす)
            if (petData != null)
            {
                //倒した敵がBossでなければ
                if (petData.petMonsterType != MonsterType.Boss)
                {
                    var reviveData = new Dictionary<string, object>();

                    reviveData["prefab"] = petData.petPrefab;
                    reviveData["position"] = transform.position;

                    //Anubisのキル扱いにする
                    EventManager.SetData(GameEvent.LastAttack_Anubis, reviveData);
                    EventManager.EmitEvent(GameEvent.LastAttack_Anubis);
                }
            }
        }
        else if (lastAttacker == LastAttackType.FlameDragonKing)
        {
            //炎王龍のキル扱いにする
            EventManager.EmitEvent(GameEvent.LastAttack_FlameDragonKing);
        }

        //ペット取得のチャンスを処理
        if (petData != null)
        {
            //仲間になる判定
            PetGetChanceManager.Instance.GetChancePet(petData, petData.getChance, transform.position);
        }

        //黄金デバフならコインを落とす
        if (isGoldDebuff)
        {
            int   coinSpaunAmount = Random.Range(3, 4);
            float coinRange       = 1.4f;

            if (coinSpawner != null)
            {
                soundEffect.Play(SoundList.DropCoin);
                coinSpawner.SpawnCoin(transform.position, coinSpaunAmount, coinRange);
            }
        }

        enemyDeadEffectPool.GetObject(transform.position);
        if (isKillExplode)
        {
            skillEffectManager.SpawnEnemyDeathExplosion(transform.position, SkillIdType.None, killExplodeDmg, killExplodeSize,isPoisonDebuff);
        }

        if (isKillSkullSoul)
        {
            skillEffectManager.SpawnSkullSoul(transform.position, SkillIdType.None);
        }

        if (isKillBrightSoul)
        {
            //Debug.Log("EnemySpawnBirghtSOul");
            skillEffectManager.SpawnBrightSoul(transform.position);
        }

        if (isTreasureHunter)
        {
            float chance = 5;
            if(SkillEffectManager.Instance.universalTrait.isLuckySeven) chance += 4;
            if (Random.Range(0, 100) <= chance)
            {
                soundEffect.Play(SoundList.DropCoin);
                coinSpawner.SpawnCoin(transform.position + new Vector3(0, 0f, 0), Random.Range(1, 3), 1.4f);
                ActiveBuffManager.Instance.AddStack(TraitType.TreasureHunter);
            }
        }

        if (Random.Range(0, 100) < expDropRateBase)
        {
            expPool.GetObject(transform.position);
            mapManager.AddMapExpCount();
        }
        
        if(isPooled)enemyPool.Release(gameObject);
        else Destroy(gameObject);


        if (gameQuestManager.isKillingQuestActive)
        {
            EventManager.EmitEventData("QuestEnemyKill", transform.position);
            if(isMidBoss) EventManager.EmitEvent("QuestBossKilled"); // ミッドボスの撃破クエストを通知
        }

        enemyManager.AddEnemyKill(enemyType); // 敵の撃破数を加算

        if (canSpawnHpPotion) CoinSpawner.Instance.SpawnHpPotionObj(transform.position);

        if (isMidBoss && canSpawnTraitChest) GameQuestManager.Instance.SpawnEnchantChest(transform.position);

        if(isSpiderDen)EventManager.EmitEventData("SpiderDenDestroy", spiderDenId);

        Invoke(nameof(PlayDeadSE), 0.15f);
    }

    public void DeadNoExp(bool isEffect = true) // 時間切れで死亡（経験値無し）
    {
        if (isBoss) return;

        if(isEffect)enemyDeadEffectPool.GetObject(transform.position);      
        
        if(isPooled)enemyPool.Release(gameObject);
        else Destroy(gameObject);
        
        enemyManager.AddEnemyKill(enemyType); // 敵の撃破数を加算 ??
    }

    public void BossDead()
    {
        EventManager.EmitEvent("isGameClear");

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            EnemyStatusBase es = enemy.GetComponent<EnemyStatusBase>();
            LayerMask enemySpiderLayer = LayerMask.GetMask("EnemySpider");
            if(enemy.layer != enemySpiderLayer) continue; 
            es.DeadNoExp();
        }

    }

    public void BossDeadAni()
    {
        if(stageManager.mapData.mapType == MapType.SpiderForest)
        {
            EnemyBossSpiderAction bossAction = GetComponent<EnemyBossSpiderAction>();
            bossAction.ChangeToDead();
        }
        else if (stageManager.mapData.mapType == MapType.AncientForest)
        {
            EnemyBossTurnipaAction bossAction = GetComponent<EnemyBossTurnipaAction>();
            bossAction.ChangeToDead();
        }
        else if (stageManager.mapData.mapType == MapType.Temple)
        {
            EnemyBossAnubisAction bossAction = GetComponent<EnemyBossAnubisAction>();
            bossAction.ChangeToDeath();
        }
        else if (stageManager.mapData.mapType == MapType.Desert)
        {
            EnemyBossDragonAction bossAction = GetComponent<EnemyBossDragonAction>();
            bossAction.ChangeToDead();
        }

        Invoke("BossDead", 1.5f);
    }

    public void BossDeadTimeDelay()
    {
        isBossDead = true; // ボスが死亡したフラグを立てる
        Time.timeScale = 0.21f; // 時間を遅くする
        Invoke(nameof(TimeReset), 0.21f); // 0.5秒後に時間を元に戻す
    }

    public void TimeReset()
    {
        Time.timeScale = 1f; // 時間を元に戻す
    }

    public void PlayDeadSE()
    {
        if(enemyManager.deadSoundFrequency <=0) soundEffect.Play(deadSound);
        enemyManager.deadSoundFrequency = 0.1f;
    }


    //======= AttackCollision =======//
    public void OnCollisionStay(Collision collision)  // プレイヤーとの衝突時にダメージを与える
    {
        if (!isAlive || attackCdCnt > 0f) return;
        //Debug.Log("Enemy collided with: " + collision.gameObject.name);

        if(isGoldDebuff) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            attackStandbyCnt -= Time.deltaTime; // 攻撃待機時間を減らす
            if (attackStandbyCnt > 0f) return; // 攻撃待機時間中は何もしない
            attackStandbyCnt = attackStandbyCntMax; // 攻撃待機時間をリセット
            attackCdCnt = attackCdCntMax; // 攻撃クールダウンタイマーをリセット

            //もしムキムキアヒルがいるなら追加効果発動
            if (ActivePetManager.Instance.isMuscleDuckInGame)
            {
                EventManager.EmitEventData(GameEvent.CounterPetAttack, this.transform);
            }
            EventManager.EmitEventData(GameEvent.ChangePlayerHp,-enemyAtkDamage); // プレイヤーにダメージを与える


            DoAttackAniTween();                                            // 攻撃アニメーションを再生
        }
    }

    


    //=======Color Flash & Attack Ani Helper=======//
    public void ExplodeFlash() // 爆発フラッシュを開始する
    {
        explodeTimer = explodeLengthPulseSec;

    }
    public void ResetColor()
    {
        isIceDebuff = false;
        
        for (int i = 0; i < renderers.Length; ++i)
        {
            var r = renderers[i];
            var pb = blocks[i];
            r.GetPropertyBlock(pb);
            pb.SetColor(colorPropId, startColor);
            r.SetPropertyBlock(pb);
        }
    }

    public void ResetGoldColor()
    {
        //早期リターン
        if (!ActivePetManager.Instance.isGoldMummyInGame) return;

        isGoldDebuff = false;
        
        for (int i = 0; i < renderers.Length; ++i)
        {
            var r = renderers[i];
            var pb = blocks[i];
            r.GetPropertyBlock(pb);
            pb.SetColor(colorPropId, startColor);
            pb.SetFloat("_Metallic", 0f);
            pb.SetFloat("_Glossiness", 0f);

            if (r.sharedMaterial.HasProperty("_EmissionColor"))
            {
                pb.SetColor("_EmissionColor", Color.black);
            }

            r.SetPropertyBlock(pb);
        }
    }

    void PushFlash(float weight)          // 被ダメージフラッシュの色を更新    
    {
        if (isIceDebuff) return;
        if (isGoldDebuff) return;

        Color tint = Color.Lerp(startColor, flashColor, weight); //Color.white

        for (int i = 0; i < renderers.Length; ++i)
        {
            var r  = renderers[i];
            var pb = blocks[i];
            r.GetPropertyBlock(pb);          
            pb.SetColor(colorPropId, tint);
            r.SetPropertyBlock(pb);
        }
    }

    void PushExplodeFlash(float weight)  // 爆発フラッシュの色を更新
    {
        Color tint = Color.Lerp(startColor, flashColor, weight);

        for (int i = 0; i < renderers.Length; ++i)
        {
            var r  = renderers[i];
            var pb = blocks[i];
            r.GetPropertyBlock(pb);          
            pb.SetColor(colorPropId, tint);
            r.SetPropertyBlock(pb);
        }
    }

    void DoAttackAniTween()
    {
        canEnemyRotate = false;

        transform.DOPunchRotation(punchRotationAmount,punchDuration,vibrato: punchVibrato,elasticity: punchElasticity).SetEase(Ease.OutQuad).onComplete = () => {
            canEnemyRotate = true;
        };

        if (enemyType == EnemyType.Boss || isAttackPushBackPlayer)
        {
            PushBackPlayer();
        }

    }

    public void PushBackPlayer()
    {
        //GameObject player = GameObject.FindGameObjectWithTag("Player");
        //float playerPosY = player.transform.position.y; // プレイヤーの Y 座標を取得
        //Vector3 playerPosition = player.transform.position;

        //Vector3 pushDirection = (playerPosition - transform.position).normalized; // 敵からプレイヤーへの方向を計算
        //pushDirection.y = playerPosY;
        //float pushDistance = 3.5f; // プレイヤーを押し戻す距離

        //player.transform.DOMove(playerPosition + pushDirection * pushDistance, 0.2f).SetEase(Ease.OutQuad); // プレイヤーを押し戻す

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return; 

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController == null) return;

        Vector3 playerPosition = player.transform.position;
        Vector3 pushDirection = (playerPosition - transform.position).normalized; // 敵からプレイヤーへの方向を計算

        pushDirection.y = 0; 
        
        if (pushDirection.sqrMagnitude > 0)
        {
            pushDirection.Normalize();
        }
        else
        {
            pushDirection = player.transform.forward; 
        }
        
        float pushDistance = 3.5f; // プレイヤーを押し戻す距離
        float pushDuration = 0.2f;
        playerController.ApplyPushback(pushDirection, pushDistance, pushDuration);

    }

    public void DynamicPushBackEnemyItselfAwayFromPlayer(float _awayDistance = 2.1f, float _pushDuration = 0.14f)
    {
        if (isPushBack) return;
        if (enemyType == EnemyType.Boss) return;

        isPushBack = true;
        pushBackCooldown = 0.49f;

        Vector3 pushDirection = Vector3.zero; 
        if(enemyActionBase)pushDirection= (transform.position - enemyActionBase.playerTrans.position).normalized; // プレイヤーから敵への方向を計算 , add null check to prevent no actionBase reference error

        pushDirection.y = 0;

        if (pushDirection.sqrMagnitude > 0)
        {
            pushDirection.Normalize();
        }
        else
        {
            pushDirection = transform.forward;
        }

        float pushDistance = _awayDistance; // 敵を押し戻す距離 ,1.4 
        float pushDuration = _pushDuration;

        if(enemyType == EnemyType.Surrounder)
        {
            pushDistance = 0.42f; //0.35
        }

        transform.DOMove(transform.position + pushDirection * pushDistance, pushDuration).SetEase(Ease.OutQuad);

    }

    public void PushBackEnemyItselfAwayFromPlayer()
    {
        if (isPushBack) return;
        if (enemyType == EnemyType.Boss) return;
        if(isMidBoss) return; 

        isPushBack = true;
        pushBackCooldown = 0.49f;

        Vector3 pushDirection = Vector3.zero; 
        if(enemyActionBase)pushDirection= (transform.position - enemyActionBase.playerTrans.position).normalized; // プレイヤーから敵への方向を計算 , add null check to prevent no actionBase reference error

        pushDirection.y = 0;

        if (pushDirection.sqrMagnitude > 0)
        {
            pushDirection.Normalize();
        }
        else
        {
            pushDirection = transform.forward;
        }

        float pushDistance = 2.1f; // 敵を押し戻す距離 ,1.4 
        float pushDuration = 0.14f;
        
        //float jumpPower = 2.1f;

        if(enemyType == EnemyType.Surrounder)
        {
            pushDistance = 0.42f; //0.35
        }

        transform.DOMove(transform.position + pushDirection * pushDistance, pushDuration).SetEase(Ease.OutQuad);
    
        //transform.DOJump(transform.position + pushDirection * pushDistance, jumpPower, 1, pushDuration).SetEase(Ease.OutQuad);
    }

    public void UpdatePushback()
    {
        if (isPushBack)
        {
            pushBackCooldown -= Time.deltaTime;
            if (pushBackCooldown <= 0f) isPushBack = false;

        }
    }

    public void DisableShadow()
    {
        foreach (var r in renderers)
            {
                r.shadowCastingMode = ShadowCastingMode.Off; // シャドウをオフにする
            }
    }

    public void UnCastShadow()
    {
        if (!isEndingPhrase && timeManager.gameTimeLeft <= 350)
        {
            isEndingPhrase = true;

            if(enemyType == EnemyType.Bomber || enemyType == EnemyType.Boss || enemyType == EnemyType.Caster) return; // ドラゴンとボススパイダーはシャドウをオフにしない

            foreach (var r in renderers)
            {
                r.shadowCastingMode = ShadowCastingMode.Off; // シャドウをオフにする
            }
        }
    }


}
