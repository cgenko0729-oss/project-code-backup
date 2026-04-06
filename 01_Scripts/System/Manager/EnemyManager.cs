using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool


public struct SpawnerValues
{
    public int MaxNum;
    public int MinEnemiesToSpawn;
    public int MaxEnemiesToSpawn;
    public float SpawnCooldown;
}

public class EnemyManager : Singleton<EnemyManager>
{
    [Header("現在の敵数")]
    public int allEnemyNum = 0; //全敵数
    public int moverNum = 0; //現スパイダー数
    public int bomberNum = 0; //現ドラゴン数
    public int FlyerNum = 0; //現コウモリ数
    public int SurrounderNum = 0; //現キノコ数
    public int CasterNum = 0; //現魔法使い数
    public int EliteMoverNum = 0; //現スパイダーエリート数

    [Header("敵撃破数")]
    public int allEnemyKillNum = 0; //全敵撃破数
    public int moverKillNum = 0; //スパイダー撃破数
    public int bomberKillNum = 0; //ドラゴン撃破数
    public int FlyerKillNum = 0; //コウモリ撃破数
    public int SurrounderKillNum = 0; //キノコ撃破数
    public int CasterKillNum = 0; //魔法使い撃破数
    public int EliteMoverKillNum = 0; //スパイダーエリート撃破数

    [Header("敵最大数")]
    public int moverMaxNum = 500; //スパイダー最大数
    public int bomberMaxNum = 4; //ドラゴン最大数
    public int FlyMaxNum = 77; //コウモリ最大数
    public int SurrounderMaxNum = 40; //キノコ最大数
    public int CasterMaxNum = 10; //魔法使い最大数
    public int EliteMoverMaxNum = 1; //スパイダーエリート最大数

    public float deadSoundFrequency = 0.1f; //敵撃破時のサウンド再生間隔

    public Transform systemFolderTrans;

    public float moverHpMax = 100f;
    public float bomberHpMax = 420f;
    public float flyerHpMax = 70f;
    public float surrounderHpMax = 350f;

    public int currentGamePhase = 0;

    public List<StageBalanceData> allStageBalanceData;
    private Dictionary<MapType, StageBalanceData> stageDataLookup;
    
    private Dictionary<EnemyType, float[]> currentHpDataLookup;
    private Dictionary<EnemyType, float[]> curretnMoveSpdDataLookup;

    private Dictionary<EnemyType, EnemyPhaseSpawnerData> currentSpawnerDataLookup;

    public float checkNegativeEnemyNumCnt = 1f;


    public ObjectPool surrounderWallColliderPool;
    



    public Transform playerTrans;

    public GameObject cannotCastEffectObj; 
    public bool playerCannotCast = false;
    public float playerCannotCastDuration = 0f;

    public GameObject cannotDashEffectObj;
    public bool playerCannotDash = false;
    public float playerCannotDashDuration = 0f;

    public GameObject cannotMoveEffectObj;
    public bool playerCannotMove = false;
    public float playerCannotMoveDuration = 0f;

    public List<Image> cannotCastImage = new List<Image>();
    public Image cannotDashImage;


    public GameObject SpawnSurroundWallObj(Vector3 pos, Quaternion rot)
    {
        GameObject obj =  surrounderWallColliderPool.GetObject(pos, rot);

        return obj;
    }

    private void InitDataLookup()
    {
        stageDataLookup = new Dictionary<MapType, StageBalanceData>();

        foreach (var data in allStageBalanceData)
        {
            if (data != null && !stageDataLookup.ContainsKey(data.mapType))
            {
                stageDataLookup.Add(data.mapType, data);
            }
        }
        currentHpDataLookup = new Dictionary<EnemyType, float[]>();
        curretnMoveSpdDataLookup = new Dictionary<EnemyType, float[]>();
        currentSpawnerDataLookup = new Dictionary<EnemyType, EnemyPhaseSpawnerData>();
    }

    public void PrepareForStage(MapType map, DifficultyType difficulty)
    {
        if (!stageDataLookup.TryGetValue(map, out StageBalanceData stageData))
        {
            Debug.LogError($"[EnemyManager] No balance data found for map: {map}.");
            return;
        }

        DifficultyBalanceData difficultyData = stageData.normalModeStats;
        if(difficulty == DifficultyType.Hard) difficultyData = stageData.hardModeStats;
        else if (difficulty == DifficultyType.Nightmare) difficultyData = stageData.nightmareModeStats;
        else if (difficulty == DifficultyType.Hell) difficultyData = stageData.hellModeStats;
        

        // Clear and populate the fast HP lookup dictionary for the current stage/difficulty
        currentHpDataLookup.Clear();
        foreach (var enemyData in difficultyData.enemyHpDataList)
        {
            currentHpDataLookup[enemyData.enemyType] = enemyData.hpByPhase;
        }

        curretnMoveSpdDataLookup.Clear();
        foreach (var enemyData in difficultyData.enemyHpDataList)
        {
            curretnMoveSpdDataLookup[enemyData.enemyType] = enemyData.moveSpdByPhase;
        }

        currentSpawnerDataLookup.Clear();
        foreach (var spawnerData in difficultyData.enemySpawnerDataList)
        {
            currentSpawnerDataLookup[spawnerData.enemyType] = spawnerData;
        }

        //Debug.Log($"EnemyManager prepared for {map} on {difficulty} mode. Loaded HP data for {currentHpDataLookup.Count} enemy types.");
    }

    void Start()
    {
        systemFolderTrans = GameObject.FindGameObjectWithTag("SystemFolder").transform;

        InitDataLookup();
        PrepareForStage(StageManager.Instance.mapData.mapType, StageManager.Instance.mapData.stageDifficulty);

        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public float GetMaxHpForEnemy(EnemyType type, int gamePhase)
    {   
        
        if (gamePhase < 1 || gamePhase > 7)
        {
            Debug.LogWarning($"Invalid game phase requested: {gamePhase}. Clamping to 1-7.");
            gamePhase = Mathf.Clamp(gamePhase, 1, 7);
        }

        if (currentHpDataLookup.TryGetValue(type, out float[] hpValues))
        {
            
            if (gamePhase - 1 < hpValues.Length) // Subtract 1 because arrays are 0-indexed (Phase 1 = index 0)
            {
                return hpValues[gamePhase - 1];
            }
        }

        Debug.LogError($"[EnemyManager] Could not find HP data for EnemyType: {type} in the current stage/difficulty settings.");
        return 10f; // Return a default value to prevent crashes
    }

    public float GetMoveSpdForEnemy(EnemyType type, int gamePhase)
    {

        if (gamePhase < 1 || gamePhase > 7)
        {
            Debug.LogWarning($"Invalid game phase requested: {gamePhase}. Clamping to 1-7.");
            gamePhase = Mathf.Clamp(gamePhase, 1, 7);
        }

        if (curretnMoveSpdDataLookup.TryGetValue(type, out float[] moveSpdValues))
        {

            if (gamePhase - 1 < moveSpdValues.Length) // Subtract 1 because arrays are 0-indexed (Phase 1 = index 0)
            {
                return moveSpdValues[gamePhase - 1];
            }
        }

        Debug.LogError($"[EnemyManager] Could not find Move Speed data for EnemyType: {type} in the current stage/difficulty settings.");
        return 1f; // Return a default value to prevent crashes
    }

    public SpawnerValues GetSpawnerValuesForEnemy(EnemyType type, int gamePhase)
    {
        

        gamePhase = Mathf.Clamp(gamePhase, 1, 7);
        int phaseIndex = gamePhase - 1; // Convert to 0-based index

        if (currentSpawnerDataLookup.TryGetValue(type, out EnemyPhaseSpawnerData data))
        {
            // Check if arrays are properly sized to prevent errors
            if (phaseIndex < data.maxNumByPhase.Length &&
                phaseIndex < data.minEnemiesByPhase.Length &&
                phaseIndex < data.maxEnemiesByPhase.Length &&
                phaseIndex < data.spawnCooldownByPhase.Length)
            {
                return new SpawnerValues
                {
                    MaxNum = data.maxNumByPhase[phaseIndex],
                    MinEnemiesToSpawn = data.minEnemiesByPhase[phaseIndex],
                    MaxEnemiesToSpawn = data.maxEnemiesByPhase[phaseIndex],
                    SpawnCooldown = data.spawnCooldownByPhase[phaseIndex]
                };
            }
        }

        //Debug.LogError($"[EnemyManager] Could not find Spawner data for EnemyType: {type} in phase {gamePhase}. Returning default values.");
        // Return safe default values to prevent crashes
        return new SpawnerValues { MaxNum = 10, MinEnemiesToSpawn = 1, MaxEnemiesToSpawn = 1, SpawnCooldown = 500f };

    }

    public bool CheckEnemyNumLimit(EnemyType enemyType)
    {
        // Get the MaxNum for the CURRENT game phase from our data
        int maxNum = GetSpawnerValuesForEnemy(enemyType, currentGamePhase).MaxNum;

        switch (enemyType)
        {
            case EnemyType.Mover:       return moverNum < maxNum;
            case EnemyType.Bomber:      return bomberNum < maxNum;
            case EnemyType.Flyer:       return FlyerNum < maxNum;
            case EnemyType.Surrounder:  return SurrounderNum < maxNum;
            case EnemyType.Caster:      return CasterNum < maxNum;
            case EnemyType.EliteMover:  return EliteMoverNum < maxNum;
            case EnemyType.Slime: return true;  // スライムは無制限
            case EnemyType.MidBoss: return true;  // ミッドボスは無制限
            case EnemyType.Boss: return true;  // ボスは無制限
            //default:                    return true;
            default:                    return false ;
        }
    }



    void Update()
    {
        deadSoundFrequency -= Time.deltaTime; //敵撃破時のサウンド再生間隔を減らす

        currentGamePhase = TimeManager.Instance.gamePhrase;

        checkNegativeEnemyNumCnt -= Time.deltaTime;

        if(checkNegativeEnemyNumCnt <= 0) //safety check for simutineouly kill to cause moverNum negative questions 
        {
            checkNegativeEnemyNumCnt = 0.5f;
            if(moverNum < 0) moverNum = 0;
            if(allEnemyNum < 0) allEnemyNum = 0;
        }

        UpdatePlayerDebuff();

        //if press F
        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    ApplyCannotCast();
        //}
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    ApplyCannotDash();
        //}
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    ApplyCannotMove();
        //}

    }

    public void AddEnemyCount(EnemyType enemyType)　//敵数を増やす
    {
        switch (enemyType)
        {
            case EnemyType.Mover:
                moverNum++;
                break;
            case EnemyType.Bomber:
                bomberNum++;
                break;
            case EnemyType.Flyer:
                FlyerNum++;
                break;
            case EnemyType.Surrounder:
                SurrounderNum++;
                break;
            case EnemyType.Caster:
                CasterNum++;
                break;
            case EnemyType.EliteMover:
                EliteMoverNum++;
                break;
        }
        allEnemyNum++;
    }

    public void AddEnemyKill(EnemyType enemyType)　//敵撃破数を増やす
    {
        switch (enemyType)
        {
            case EnemyType.Mover:
                moverKillNum++;
                moverNum--; // スパイダー撃破時は現スパイダー数も減らす
                break;
            case EnemyType.Bomber:
                bomberKillNum++;
                bomberNum--; // ドラゴン撃破時は現ドラゴン数も減らす
                break;
            case EnemyType.Flyer:
                FlyerKillNum++;
                FlyerNum--; // コウモリ撃破時は現コウモリ数も減らす
                break;
            case EnemyType.Surrounder:
                SurrounderKillNum++;
                SurrounderNum--; // キノコ撃破時は現キノコ数も減らす
                break;
            case EnemyType.Caster:
                CasterKillNum++;
                CasterNum--; // 魔法使い撃破時は現魔法使い数も減らす
                break;
            case EnemyType.EliteMover:
                EliteMoverKillNum++;
                EliteMoverNum--; // スパイダーエリート撃破時は現スパイダーエリート数も減らす
                break;
        }
        allEnemyKillNum++;
        allEnemyNum--;
    }

    public void UpdatePlayerDebuff()
    {

        playerCannotDashDuration -= Time.deltaTime;
        playerCannotCastDuration -= Time.deltaTime;
        playerCannotMoveDuration -= Time.deltaTime;


        if (playerCannotDash && playerCannotDashDuration <= 0)
        {
            playerCannotDash = false;
            RemoveCannotDashDebuff();
        }
        if (playerCannotCast && playerCannotCastDuration <= 0)
        {
            playerCannotCast = false;
            RemoveCannotCastDebuff();
        }
        if (playerCannotMove && playerCannotMoveDuration <= 0)
        {
            RemovePlayerCannotMove();
            playerCannotMove = false;
        }
    }

    public void ApplyCannotDash()
    {
        if (SkillEffectManager.Instance.universalTrait.isDebuffImmnity)
        {
            ActiveBuffManager.Instance.AddStack(TraitType.DebuffDisable);
            return;
        }

        //dotween to scale down the image scale from 2.1 to 1 and    fade in the image from 0.1 to 1 in 0.3 seconds
        cannotDashImage.transform.localScale = Vector3.one * 2.1f;
        cannotDashImage.gameObject.SetActive(true);
        cannotDashImage.DOFade(1f, 0.3f);
        cannotDashImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

        //spawn cannot dash effect obj at player position
        Instantiate(cannotDashEffectObj, playerTrans.position, Quaternion.identity);

        playerCannotDash = true;
        playerCannotDashDuration = 14.9f;


    }

    public void ApplyCannotCast()
    {
        if (SkillEffectManager.Instance.universalTrait.isDebuffImmnity)
        {
            ActiveBuffManager.Instance.AddStack(TraitType.DebuffDisable);
            return;
        }

        //dotween to scale down all the image scale from 2.1 to 1 and    fade in all the image from 0.1 to 1 in 0.3 seconds:
        foreach (var img in cannotCastImage)
        {
            img.transform.localScale = Vector3.one * 2.1f;
            img.gameObject.SetActive(true);
            img.DOFade(1f, 0.3f);
            img.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).OnComplete(() => {
                //img.gameObject.SetActive(true);
            });
        }

        //spawn cannot cast effect obj at player position
        Instantiate(cannotCastEffectObj, playerTrans.position, Quaternion.identity);

        playerCannotCast = true;
        playerCannotCastDuration = 14f;

    }

    public void ApplyCannotMove()
    {
        if (SkillEffectManager.Instance.universalTrait.isDebuffImmnity)
        {
            ActiveBuffManager.Instance.AddStack(TraitType.DebuffDisable);
            return;
        }

        //spawn cannot move effect obj at player position
        Instantiate(cannotMoveEffectObj, playerTrans.position, Quaternion.identity);

        playerCannotMove = true;
        playerCannotMoveDuration = 7f;

        Animator playerAni = playerTrans.GetComponent<Animator>();
        playerAni.SetTrigger("GetHit");
        Invoke("DelayStopPlayerAni", 0.21f);


    }

    public void DelayStopPlayerAni()
    {
        Animator playerAni = playerTrans.GetComponent<Animator>();
        playerAni.speed = 0f;
    }

    public void ResetPlayerAniSpd()
    {
        Animator playerAni = playerTrans.GetComponent<Animator>();
        playerAni.speed = 1f;

    }

    public void RemovePlayerCannotMove()
    {
        ResetPlayerAniSpd();
    }


    public void RemoveCannotDashDebuff()
    {

        cannotDashImage.DOFade(0f, 0.3f);
        cannotDashImage.transform.DOScale(2.1f, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
            cannotDashImage.gameObject.SetActive(false);
        });

    }

    public void RemoveCannotCastDebuff()
    {
        foreach (var img in cannotCastImage)
        {
            img.DOFade(0f, 0.3f);
            img.transform.DOScale(2.1f, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                img.gameObject.SetActive(false);
            });

        }
    }

    //public bool CheckEnemyNumLimit(EnemyType enemyType) //敵数の上限チェック
    //{
    //    switch (enemyType)
    //    {
    //        case EnemyType.Mover:
    //            return moverNum < moverMaxNum;
    //        case EnemyType.Bomber:
    //            return bomberNum < bomberMaxNum;
    //        case EnemyType.Flyer:
    //            return FlyerNum < FlyMaxNum;
    //        case EnemyType.Surrounder:
    //            return SurrounderNum < SurrounderMaxNum;
    //        case EnemyType.Caster:
    //            return CasterNum < CasterMaxNum;
    //        case EnemyType.EliteMover:
    //            return EliteMoverNum < EliteMoverMaxNum;
    //        default:
    //            return false;
    //    }
    //}





}

