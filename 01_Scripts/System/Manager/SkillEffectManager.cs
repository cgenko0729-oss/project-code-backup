using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using DamageNumbersPro;

public class SkillEffectManager : Singleton<SkillEffectManager>
{

    public TraitData universalTrait;
    public List<TraitData> playerTraitList;

    public Transform playerTrans;
    public Vector3 playerpos;
    public PlayerController playerControl;
    public PlayerState playerState;

    private Vector3 lastPlayerPos; 
    public bool isStandStillEnabled = false;
    public bool isPlayerStandStill = false;
    public bool isPlayerStopMoving = false;
    public float playerStopMovingAccumulateTimeCnt = 0f;
    public float playerStopMovingAccumulateTimeCntMax = 5f;
    public ParticleSystem standStillEffect;
    public ParticleSystem stopMoveAccumulateEffect;
    public ParticleSystem standStillHealEffect;
    public bool isStandStillHealEffectPlaying = false;
    public GameObject standStillShieldObj;

    public ObjectPool skillExplosionPoolObj;
    public ObjectPool skillFirePoolObj;
    public ObjectPool skillSkullSoulPoolObj;
    public ObjectPool skillBrightSoulPoolObj;
    public ObjectPool enemyExplosionPoolObj;

    public ObjectPool skillSplitMagicBulletPoolObj;

    public ObjectPool skillWolfPoolObj;

    public ObjectPool skillBlackHoleExplosionPoolObj;

    //public ObjectPool bombExplosionPoolObj;

    public DamageNumber damageNumCirt;

    public GameObject playerCloneObj;

    //public ObjectPool dashCastEffectObjPool;
    //public ObjectPool getDamageCastEffectObjPool;

    public GameObject dashCastEffectObj;
    public GameObject getDamageCastEffectObj;
    public GameObject getItemCastEffectObj;
    public GameObject getHealCastEffectObj;
    public GameObject walkCastEffectObj;



    public float dashCastEffectCnt = 1f;
    public float getDamageCastEffectCnt = 1f;
    public float getItemCastEffectCnt = 1f;

    public AudioClip afterSkillExplosionSe;
    public AudioClip acidExplosionSe;
    public AudioClip revengeSe;
    public AudioClip afterDashWindSe;
    public AudioClip pickUpItemSe;
    public AudioClip shieldBreakSe;

    public float afterSkillExplosionCdCnt = 0.21f;
    public float acidExplosionCdCnt = 0.21f;

    public GameObject HpChangeIceExplosionObj;
    
    public GameObject DashShieldObj;
    public TraitShield shieldObj;
    public bool isPlayerShieldActive = false;

    public float pickItemCastCdCnt = 0.7f;

    public float moveFireCnt = 0.1f;
    public float moveFireCntMax = 0.14f;
    public Vector3 previousFireSpawnPos = Vector3.zero;
    public float spawnDistNeed = 0.7f;

    public float playerWalkedDistAmount = 0f;
    public float walkDistRequiredToCast = 10f; //14
    public float walkDistUpdateInterval = 0.5f;
    public Vector3 previousePlayerPos = Vector3.zero;
    public int distNum = 0;
    public int oldDistNum = 0;


    public int brightSoulNum = 0;

    public List<TraitData> allGameTraitsList;

    public GameObject currentTraitFrame;
    public TextMeshProUGUI currentTraitName;
    public TextMeshProUGUI currentTraitDesc;

    public GameObject relateTraitWindowObj;
    public List<RelatedTraitUIItem> relatedTraitUIItems;


    public GameObject relifeEffectObj;

    public bool isFireSpeedUp = false;
    public float fireSpeedUpDuration = 3f;
    private float fireSpeedUpAmount = 40f;
    public ParticleSystem firewalkEffect;
    public GameObject fireWalkerDetectorObj;

    public bool isIsolatorEnabled = false;
    public bool isPlayerIsolated = false;
    public float isolatorCheckCnt = 1f;
    public ParticleSystem isolatorEffect;

    public bool isAfterDashAddSpdEnabled = false;
    public float afterDashAddSpdAmount = 30f;
    public float afterDashAddSpdDuration = 2.19f;

    public GameObject foodHealItemObj;

    public bool isGetDamageAddAttackEnabled = false;
    public float getDamageAddAttackDuration = 7f;
    public ParticleSystem getDamageAddAttackEffect;

    public GameObject dashPushBackEnemyObj;

    public bool isGetItemCastEnabled = false;

    public bool isBerserkerEnabled = false;
    public bool isBerserkerMode = false;
    private float berserkerAttackAddAmount = 50f;
    public ParticleSystem berserkerModeEffect;

    public int currentPetNum = 0;

    public bool isPlayerFixRot = false;
    public float playerFixRotCnt = 0f;

    //public float isGetDamageAddAttack = 0f;

    public int strawManNum = 0;
    public GameObject strawManPrefab;

    public bool isAddedMaxDashCharge = false;

    public GameObject giftAngel;
    private float giftAngelSpawnCd = 3.5f;

    private float playerGetBiggSize = 0.7f;
    public float playerGetSmallSize = 0.77f;



    private void OnEnable()
    {
        EventManager.StartListening(GameEvent.PlayerDashEnd, HandleAfterDashAction);
        EventManager.StartListening(GameEvent.PlayerGetDamage, HandlePlayerGetDamageAction); // HP変化時にスキル発動を登録
        EventManager.StartListening(GameEvent.PlayerGetItem, HandleGetItemAction);
        EventManager.StartListening(GameEvent.ItemBoxDestroyed, HandleDestroyItemBox);
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameEvent.PlayerDashEnd, HandleAfterDashAction);
        EventManager.StopListening(GameEvent.PlayerGetDamage, HandlePlayerGetDamageAction); // HP変化時のスキル発動登録解除
        EventManager.StopListening(GameEvent.PlayerGetItem, HandleGetItemAction);
        EventManager.StopListening(GameEvent.ItemBoxDestroyed, HandleDestroyItemBox);

    }

    void SpawnGiftAngel()
    {
        //spawn gift angel 21 unit away from player
        Vector3 spawnPos = playerTrans.position + new Vector3(Random.Range(-21f, 21f), 10f, Random.Range(-21f, 21f));
        Instantiate(giftAngel, spawnPos, Quaternion.identity);
        ActiveBuffManager.Instance.AddStack(TraitType.GiftAngel);
        //ActiveBuffManager.Instance.AddStack(TraitType.GiftAngel);
    }

    void UpdateSpawnGiftAngel()
    {
        if(!universalTrait.isGiftAngel) return;

        giftAngelSpawnCd -= Time.deltaTime;
        if(giftAngelSpawnCd <= 0f)
        {
            giftAngelSpawnCd = 60f;
            SpawnGiftAngel();
        }

    }

    public void SpawnStrawMan()
    {
        if (strawManNum >= 7) return;
        strawManNum += 1;
        Vector3 spawnPos = playerTrans.position + new Vector3(Random.Range(-2f, 2f), 0.91f, Random.Range(-2f, 2f));
        Instantiate(strawManPrefab, spawnPos, Quaternion.identity);
        ActiveBuffManager.Instance.AddStack(TraitType.GetDamageSummonPuppet);
    }

    public void SpawnBlackHoleExplosion(Vector3 pos) { }

    public float dropFoodCdCnt = 0f;

    public void SpawnFoodItemObj
        (Vector3 pos)
    {
        if(dropFoodCdCnt > 0f) return;
        dropFoodCdCnt = 3f;
        Instantiate(foodHealItemObj, new Vector3(pos.x,0.35f,pos.z), Quaternion.identity);

    }

    public void HandleDestroyItemBox()
    {
        if (universalTrait.isChestItemAddDamge)
        {
            BuffManager.Instance.gobalDamageAdd += 4f;
            ActiveBuffManager.Instance.AddStack(TraitType.GetChestItemAddDamage);
        }

    }

    public void ApplyFireSpeedUp()
    {
        if (!isFireSpeedUp)
        {
            isFireSpeedUp = true;
            fireSpeedUpDuration = 2.1f;
            BuffManager.Instance.gobalMoveSpeed += fireSpeedUpAmount;
            BuffManager.Instance.gobalCritChanceAdd += 40f;
            firewalkEffect.Play();
            ActiveBuffManager.Instance.AddStack(TraitType.FireWalker);
           
        }
        else if (isFireSpeedUp)
        {
            fireSpeedUpDuration = 2.1f;
        }

    }

    public void UpdateFireSpeedUp()
    {
        fireSpeedUpDuration -= Time.deltaTime;

        if(fireSpeedUpDuration <=0 && isFireSpeedUp)
        {
            isFireSpeedUp = false;
            BuffManager.Instance.gobalMoveSpeed -= fireSpeedUpAmount;
            BuffManager.Instance.gobalCritChanceAdd -= 40f;
            firewalkEffect.Stop();
            ActiveBuffManager.Instance.ReduceStack(TraitType.FireWalker);
            Debug.Log("Fire Speed Up Ended");
        }

    }

    public void SpawnReviveEffect()
    {
        Instantiate(relifeEffectObj, playerTrans.position, Quaternion.identity);
        SoundEffect.Instance.Play(SoundList.QuestFinishSe);
    }

    void HandleAfterDashAction()
    {
        if (universalTrait.isDashShield)
        {
            SpawnDashShieldObj();
            ActiveBuffManager.Instance.AddStack(TraitType.DashGetShield);
        }

        if (universalTrait.isAfterDashAddSpd)
        {
            ActiveBuffManager.Instance.AddStack(TraitType.AfterDashAddMoveSpeed);
            if (!isAfterDashAddSpdEnabled)
            {
                isAfterDashAddSpdEnabled = true;
                BuffManager.Instance.gobalMoveSpeed += afterDashAddSpdAmount;
                afterDashAddSpdDuration = 2.19f;
                SoundEffect.Instance.PlayOneSound(afterDashWindSe);
            }
            else
            {
                afterDashAddSpdDuration = 2.19f;
            }
            
        }

        if (universalTrait.isPlayerWalkFire)
        {
            ActiveBuffManager.Instance.AddStack(TraitType.FireSpeedUp);
        }

    }

    void HandlePlayerGetDamageAction()
    {

        if (universalTrait.isStrawMan)
        {
            SpawnStrawMan();
        }

        if (universalTrait.isGetDamageIceExplosion)
        {
            SpawnIceExplosionObj();
            SoundEffect.Instance.Play(SoundList.IceSe);
        }

        if (universalTrait.isGetDamageAddAttack)
        {
            if (!isGetDamageAddAttackEnabled)
            {
                isGetDamageAddAttackEnabled = true;
                getDamageAddAttackDuration = 7f;
                BuffManager.Instance.gobalDamageAdd += 50f;
                BuffManager.Instance.gobalCooldownAdd += 50f;
                ActiveBuffManager.Instance.AddStack(TraitType.GetDamageAddAttack);
                if (getDamageAddAttackEffect)
                {
                    getDamageAddAttackEffect.gameObject.SetActive(true);
                    getDamageAddAttackEffect.Play();

                }
            }
            else
            {
                getDamageAddAttackDuration = 7f;
            }
            
        }

    }

    void UpdateGetDamageAddAttack()
    {
        if(!isGetDamageAddAttackEnabled) return;
        getDamageAddAttackDuration -= Time.deltaTime;
        if (getDamageAddAttackDuration <= 0)
        {
            isGetDamageAddAttackEnabled = false;
            BuffManager.Instance.gobalDamageAdd -= 50f;
                BuffManager.Instance.gobalCooldownAdd -= 50f;
            if (getDamageAddAttackEffect)
            {
                getDamageAddAttackEffect.gameObject.SetActive(false);
                getDamageAddAttackEffect.Stop();

            }
            ActiveBuffManager.Instance.ReduceStack(TraitType.GetDamageAddAttack);
        }

    }

    void HandleGetItemAction()
    {
        
    }

    void UpdateAfterDashAddSpd()
    {
        if (!isAfterDashAddSpdEnabled) return;

        afterDashAddSpdDuration -= Time.deltaTime;

        if (afterDashAddSpdDuration <= 0 && isAfterDashAddSpdEnabled)
        {
            isAfterDashAddSpdEnabled = false;
            BuffManager.Instance.gobalMoveSpeed -= afterDashAddSpdAmount;
        }
    }


    void UpdatePlayerWalkDist()
    {
        if(universalTrait.isWalkCast == false) return;

        walkDistUpdateInterval -= Time.deltaTime;

        if(walkDistUpdateInterval <= 0)
        {
            walkDistUpdateInterval = 0.1f;
            playerWalkedDistAmount += Vector3.Distance(playerTrans.position, previousePlayerPos);
            previousePlayerPos = playerTrans.position;

            distNum = (int)playerWalkedDistAmount;
            if (distNum != oldDistNum)
            {
                if (distNum > oldDistNum) ActiveBuffManager.Instance.AddStack(TraitType.WalkReleaseSkill,false);

                oldDistNum = distNum;
            }

            if (playerWalkedDistAmount >= walkDistRequiredToCast)
            {
                playerWalkedDistAmount = 0f;
                EventManager.EmitEvent("PlayWalkCertainAmount");
                ActiveBuffManager.Instance.AddStack(TraitType.WalkReleaseSkill,true);
                ActiveBuffManager.Instance.SetStacks(TraitType.WalkReleaseSkill,1);
                //SpawnSkillFireObj(playerTrans.position, SkillIdType.Meteor);
            }
        }

       

    }

    private void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
        playerControl = playerTrans.GetComponent<PlayerController>();
        playerState = playerTrans.GetComponent<PlayerState>();

        ResetUniversalTrait();

        previousePlayerPos = playerTrans.position;

        //loop all traits data and set trait.isSelected to false
        for(int i = 0; i < allGameTraitsList.Count; i++)
        {
            allGameTraitsList[i].isSelected = false;
        }



    }

    private void Update()
    {
        playerpos = playerTrans.position;

        dashCastEffectCnt -= Time.deltaTime;
        getDamageCastEffectCnt -= Time.deltaTime;
        getItemCastEffectCnt -= Time.deltaTime;

        afterSkillExplosionCdCnt -= Time.deltaTime;
        acidExplosionCdCnt -= Time.deltaTime;

        pickItemCastCdCnt -= Time.deltaTime;

        //TestSpawnSkillExplosion();

        dropFoodCdCnt -= Time.deltaTime;


        playerFixRotCnt -= Time.deltaTime;
        if(isPlayerFixRot && playerFixRotCnt <= 0f)
        {
            isPlayerFixRot = false;
        }


        HandleStandstillLogic();
        UpdatePlayerWalkDist();
        HandlePlayerWalkFire();
        
        UpdateFireSpeedUp();
        UpdaetIsolatorEffect();
        UpdateAfterDashAddSpd();
        UpdateGetDamageAddAttack();
        UpdateBerserkerMode();
        UpdateSpawnGiftAngel();
    }

    public void UpdaetIsolatorEffect()
    {

        if(!isIsolatorEnabled) return;

        isolatorCheckCnt -= Time.deltaTime;

        if (isolatorCheckCnt > 0) return;
        isolatorCheckCnt = 1f;

        //cast a sphere cast OverlapSphereNonAlloc in player position with radius 5f to check if there is any enemy around with layer "Enemy" and "EnemyBoss"
        Collider[] hitColliders = Physics.OverlapSphere(playerTrans.position, 4.2f, LayerMask.GetMask("EnemySpider", "EnemyBat","EnemyMushroom","EnemyDragon"));

        if (hitColliders.Length <= 0)
        {
            if (!isPlayerIsolated)
            {
                isPlayerIsolated = true;
                isolatorEffect.gameObject.SetActive(true);
                isolatorEffect.Play();
                ActiveBuffManager.Instance.AddStack(TraitType.Isolator);
            }
        }
        else
        {
            if (isPlayerIsolated)
            {
                isPlayerIsolated = false;
                
                isolatorEffect.Stop();
                isolatorEffect.gameObject.SetActive(false);
                ActiveBuffManager.Instance.ReduceStack(TraitType.Isolator);
            }
        }
    }

    public void UpdateBerserkerMode()
    {
        if(!isBerserkerEnabled) return;

        float playerHpPercent = (playerState.NowHp / playerState.MaxHp) * 100f;

        if (!isBerserkerMode)
        {
            if(playerHpPercent < 50f)
            {
                isBerserkerMode = true;
                BuffManager.Instance.gobalDamageAdd += berserkerAttackAddAmount;
                BuffManager.Instance.gobalCritChanceAdd += 50f;
                ActiveBuffManager.Instance.AddStack(TraitType.Berserker);
                berserkerModeEffect.gameObject.SetActive(true);
                berserkerModeEffect.Play();
            }
        }
        else
        {
            if (playerHpPercent >= 50f)
            {
                isBerserkerMode = false;
                BuffManager.Instance.gobalDamageAdd -= berserkerAttackAddAmount;
                BuffManager.Instance.gobalCritChanceAdd -= 50f;
                ActiveBuffManager.Instance.SetStacks(TraitType.Berserker,0);
                berserkerModeEffect.Stop();
                berserkerModeEffect.gameObject.SetActive(false);
            }

        }

    }

    public void HandlePlayerWalkFire()
    {
        if (!playerControl.isDashing) return;

        if (universalTrait.isPlayerWalkFire)
        {
            //moveFireCnt -= Time.deltaTime;
            //if (moveFireCnt <= 0)
            //{
            //    Vector3 nowSpawnPos = playerTrans.position;
            //    float distNowPre = Vector3.Distance(nowSpawnPos, previousFireSpawnPos);
            //    if(distNowPre < spawnDistNeed) return; //前回のスポーン位置と近い場合はスキルを発動しない

            //    previousFireSpawnPos = nowSpawnPos; 
            //    moveFireCnt = moveFireCntMax;
            //    SpawnSkillFireObj(playerTrans.position, SkillIdType.Meteor);
            //}

            Vector3 nowSpawnPos = playerTrans.position;
                float distNowPre = Vector3.Distance(nowSpawnPos, previousFireSpawnPos);
                if(distNowPre < spawnDistNeed) return; //前回のスポーン位置と近い場合はスキルを発動しない

                previousFireSpawnPos = nowSpawnPos; 
                moveFireCnt = moveFireCntMax;
                SpawnSkillFireObj(playerTrans.position, SkillIdType.Meteor,14f);

        }
    }

    public float standStillHealCnt = 0.5f;
    public void HandleStandstillLogic()
    {
        if (universalTrait.isStandStillGetHeal)
        {
                if (Vector3.Distance(playerpos, lastPlayerPos) < 0.01f)
            {
                

                standStillHealCnt += Time.deltaTime;
                if (standStillHealCnt >= 3)
                {
                    standStillHealCnt = 0;

                    if (!isStandStillHealEffectPlaying)
                    {
                        standStillHealEffect.Play();
                        isStandStillHealEffectPlaying = true;
                    }
                    
                    EventManager.EmitEventData(GameEvent.ChangePlayerHp, 2f);
                    ActiveBuffManager.Instance.AddStack(TraitType.StandStillGetHeal);
                        //SpawnGetHealCastEffect();

                    
                }
            }
            else
            {
                standStillHealCnt = 0f;

                if (isStandStillHealEffectPlaying)
                {
                    standStillHealEffect.Stop();
                    isStandStillHealEffectPlaying = false;
                    ActiveBuffManager.Instance.ReduceStack(TraitType.StandStillGetHeal);
                }
            }

        }
        


        if (Vector3.Distance(playerpos, lastPlayerPos) < 0.01f && isStandStillEnabled)
        {
            if (!isPlayerStopMoving)
            {
                stopMoveAccumulateEffect.Play();
            }

            isPlayerStopMoving = true;
            playerStopMovingAccumulateTimeCnt += Time.deltaTime;

            if (playerStopMovingAccumulateTimeCnt >= playerStopMovingAccumulateTimeCntMax)
            {
                if (!isPlayerStandStill)
                {
                    stopMoveAccumulateEffect.Stop();
                    standStillEffect.Play();
                    isPlayerStandStill = true;
                    ActiveBuffManager.Instance.AddStack(TraitType.StandStillEnhance);
                    // You can add any logic here that should trigger when the player enters standstill mode.
                    //Debug.Log("Player has entered standstill mode.");
                }
            }
        }
        else
        {
            if (isPlayerStopMoving)
            {
                stopMoveAccumulateEffect.Stop();
            }

            // Reset the counters and flags if the player is moving.
            isPlayerStopMoving = false;
            playerStopMovingAccumulateTimeCnt = 0f;

            if (isPlayerStandStill)
            {
                standStillEffect.Stop();
                isPlayerStandStill = false;
                ActiveBuffManager.Instance.ReduceStack(TraitType.StandStillEnhance);
                // You can add any logic here that should trigger when the player exits standstill mode.
                //Debug.Log("Player has exited standstill mode.");
            }
        }

        // Update the last known position for the next frame's comparison.
        lastPlayerPos = playerpos;
    }

    public void SetUniversalTrait(TraitData data)
    {
        if (!universalTrait.isDashShield)
        {
            universalTrait.isDashShield = data.isDashShield;
            if (data.isDashShield) ActiveBuffManager.Instance.AddStack(TraitType.DashGetShield);
        }

        if (!universalTrait.isGetDamageIceExplosion)
        {
            universalTrait.isGetDamageIceExplosion = data.isGetDamageIceExplosion;
            if(data.isGetDamageIceExplosion) ActiveBuffManager.Instance.AddStack(TraitType.GetDamageIceExplosion); 
        }
        universalTrait.gobalCritChanceAdd += data.gobalCritChanceAdd;
        if(!universalTrait.isPlayerWalkFire) universalTrait.isPlayerWalkFire = data.isPlayerWalkFire;
        if(!universalTrait.isCriticalHealer) universalTrait.isCriticalHealer = data.isCriticalHealer;
        if(!universalTrait.isFireEnhanced) universalTrait.isFireEnhanced = data.isFireEnhanced;
        if (!universalTrait.isIceEnhanced) universalTrait.isIceEnhanced = data.isIceEnhanced;
        if (!universalTrait.isPoisonEnhanced) universalTrait.isPoisonEnhanced = data.isPoisonEnhanced;
        if (!universalTrait.isStandStillEnhance) universalTrait.isTreasureHunter = data.isTreasureHunter;

        if (!universalTrait.isStandStillGetHeal)
        {
            universalTrait.isStandStillGetHeal = data.isStandStillGetHeal;
            if (data.isStandStillGetHeal)
            {
                ActiveBuffManager.Instance.AddStack(TraitType.StandStillGetHeal);
                ActiveBuffManager.Instance.ReduceStack(TraitType.StandStillGetHeal);
            }


        }
        
        if (!universalTrait.isStandStillGetShield)
        {
            universalTrait.isStandStillGetShield = data.isStandStillGetShield;
            if (data.isStandStillGetShield) ActiveBuffManager.Instance.AddStack(TraitType.StandStillGetShield);
        }

        if (!universalTrait.isReviveOnce)
        {
            universalTrait.isReviveOnce = data.isReviveOnce;
            if(data.isReviveOnce)ActiveBuffManager.Instance.AddStack(TraitType.Reviver);
        }

        if (!universalTrait.isHealDouble)
        {
            universalTrait.isHealDouble = data.isHealDouble;
            if (data.isHealDouble) ActiveBuffManager.Instance.AddStack(TraitType.HealDouble);
        }

        if (!universalTrait.isDebuffImmnity)
        {
            universalTrait.isDebuffImmnity = data.isDebuffImmnity;
            if (data.isDebuffImmnity) ActiveBuffManager.Instance.AddStack(TraitType.DebuffDisable);
        }
        
        if (!universalTrait.isItemEffectDoubled)
        {
            universalTrait.isItemEffectDoubled = data.isItemEffectDoubled;
            if (data.isItemEffectDoubled) ActiveBuffManager.Instance.AddStack(TraitType.ItemEffectDouble);
        }
        
        if(!universalTrait.isFairJudge) universalTrait.isFairJudge = data.isFairJudge;
        
        if (!universalTrait.isFireWalker)
        {
            if (data.isFireWalker)
            {
                Instantiate(fireWalkerDetectorObj);
                ActiveBuffManager.Instance.AddStack(TraitType.FireWalker);
                ActiveBuffManager.Instance.ReduceStack(TraitType.FireWalker);
            }
            universalTrait.isFireWalker = data.isFireWalker;
        }

        if (data.isDoubleDash)
        {
            if (!isAddedMaxDashCharge)
            {
                isAddedMaxDashCharge = true;
                playerControl.maxDashCharges += 1;
            }
            //playerControl.maxDashCharges = 2;

        }

        if (!universalTrait.isDashPushBackEnemy)
        {
            universalTrait.isDashPushBackEnemy = data.isDashPushBackEnemy;
            if(data.isDashPushBackEnemy)dashPushBackEnemyObj.SetActive(true);
        }

        if (!universalTrait.isPetGetStronger) universalTrait.isPetGetStronger = data.isPetGetStronger;

        if (!universalTrait.isAfterDashAddSpd) universalTrait.isAfterDashAddSpd = data.isAfterDashAddSpd;
        if(!universalTrait.isLuckySeven) universalTrait.isLuckySeven = data.isLuckySeven;
        
        if (!universalTrait.isGetDamageAddAttack)
        {
          universalTrait.isGetDamageAddAttack = data.isGetDamageAddAttack;
            if (data.isGetDamageAddAttack)
            {
                ActiveBuffManager.Instance.AddStack(TraitType.GetDamageAddAttack);
                ActiveBuffManager.Instance.ReduceStack(TraitType.GetDamageAddAttack);
            }
        }

        if (!universalTrait.isChestItemAddDamge)
        {
            universalTrait.isChestItemAddDamge = data.isChestItemAddDamge;
            if (data.isChestItemAddDamge) ActiveBuffManager.Instance.AddStack(TraitType.GetChestItemAddDamage);
        }

        if (!universalTrait.isBerserker)
        {
            universalTrait.isBerserker = data.isBerserker;
            if (data.isBerserker)
            {
                isBerserkerEnabled = true;
                ActiveBuffManager.Instance.AddStack(TraitType.Berserker);
                ActiveBuffManager.Instance.ReduceStack(TraitType.Berserker);
            }
        }

        if (!universalTrait.isGetCoinAddCrit)
        {
            universalTrait.isGetCoinAddCrit = data.isGetCoinAddCrit;
            if (data.isGetCoinAddCrit)
            {
                ActiveBuffManager.Instance.AddStack(TraitType.PickCoinGetCrit);
                ActiveBuffManager.Instance.ReduceStack(TraitType.PickCoinGetCrit);
            }
        }

 
        if (!universalTrait.isLonelyMan)
        {
            universalTrait.isLonelyMan = data.isLonelyMan;
            if (data.isLonelyMan)
            {
                currentPetNum = ActivePetManager.Instance.activePets.Count;
                if(currentPetNum == 0)
                {
                    ActiveBuffManager.Instance.AddStack(TraitType.LonelyMan);
                    Debug.Log("LonelyManActivate, no pet detected");
                    BuffManager.Instance.gobalDamageAdd += 10f;
                    BuffManager.Instance.gobalCritChanceAdd += 10f;
                    BuffManager.Instance.gobalPlayerDefenceAdd += 10f;
                    SkillManager.Instance.luck += 10f;
                    BuffManager.Instance.gobalMoveSpeed += 10f;
                }
                else
                {
                    Debug.Log("LonelyManDeactivated, pet detected");

                }
            }
            

        }

        if (!universalTrait.isStrawMan)
        {
            universalTrait.isStrawMan = data.isStrawMan;
            if (data.isStrawMan)
            {
                ActiveBuffManager.Instance.AddStack(TraitType.GetDamageSummonPuppet);
                ActiveBuffManager.Instance.ReduceStack(TraitType.GetDamageSummonPuppet);
            }
            
        }

        if (!universalTrait.isGiftAngel)
        {
            universalTrait.isGiftAngel = data.isGiftAngel;
            if (data.isGiftAngel) ActiveBuffManager.Instance.AddStack(TraitType.GiftAngel);
        }

        if (!universalTrait.isPlayerGetBigger)
        {
            universalTrait.isPlayerGetBigger = data.isPlayerGetBigger;
            Vector3 nowScale = playerTrans.localScale;
            Vector3 scaleAdd = new Vector3(playerGetBiggSize, playerGetBiggSize, playerGetBiggSize);
            if (data.isPlayerGetBigger)
            {
                playerTrans.localScale = nowScale + scaleAdd;
                //BuffManager.Instance.gobalPlayerDefenceAdd += 10f;
                //BuffManager.Instance.gobalMoveSpeed -= 15f;
                //BuffManager.Instance.gobalSizeAdd += 50f;
                //BuffManager.Instance.gobalDurationAdd += 50f;
            }

        }

        if (!universalTrait.isElementResonance)
        {
            universalTrait.isElementResonance = data.isElementResonance;
        }

        SkillManager.Instance.luck += data.gobalLuckAdd;

    
        BuffManager.Instance.gobalExpGain += data.expGainAdd;
        BuffManager.Instance.gobalPickUpRange += data.pickUpRangeAdd;

        BuffManager.Instance.gobalDamageAdd += data.gobalDamageAdd;
        BuffManager.Instance.gobalCooldownAdd += data.gobalCooldownAdd;
        BuffManager.Instance.gobalDurationAdd += data.gobalDurationAdd;
        BuffManager.Instance.gobalSpeedAdd += data.gobalSpeedAdd;
        BuffManager.Instance.gobalSizeAdd += data.gobalSizeAdd;
        //BuffManager.Instance.gobalbul += data.gobalProjectilNumAdd;   
        BuffManager.Instance.gobalCritChanceAdd += data.gobalCritChanceAdd;

        BuffManager.Instance.gobalMoveSpeed += data.gobalMoveSpeedAdd;

        BuffManager.Instance.gobalPlayerDefenceAdd += data.gobalPlayerDefenceAdd;

        BuffManager.Instance.gobalGoldGain += data.gobalGoldGainAdd;

        if(data.playerMaxHpAdd > 0f)
        {
            PlayerState player = playerTrans.GetComponent<PlayerState>();
            player.MaxHp += data.playerMaxHpAdd;
            EventManager.EmitEventData(GameEvent.ChangePlayerHp, data.playerMaxHpAdd); 
        }



        //add trait to list 
        playerTraitList.Add(data);

    }

    public void ResetUniversalTrait()
    {
        universalTrait.isDashShield = false;
        universalTrait.isGetDamageIceExplosion = false;
        universalTrait.isCriticalHealer = false;
        universalTrait.isTreasureHunter = false;

        universalTrait.gobalCritChanceAdd = 0f;

        universalTrait.ClearAllData();

    }


    public void SpawnDashShieldObj()
    {
        if (!isPlayerShieldActive)
        {
            shieldObj = Instantiate(DashShieldObj, playerTrans.position, Quaternion.identity).GetComponent<TraitShield>();
            isPlayerShieldActive = true;
        }
        else
        {
            shieldObj.ResetShieldTime();
        }
        


    }

    public void SpawnStandStillShieldObj()
    {
        Instantiate(standStillShieldObj, playerTrans.position, Quaternion.identity);

    }

    public void SpawnIceExplosionObj()
    {
        ActiveBuffManager.Instance.AddStack(TraitType.GetDamageIceExplosion);
        var effect = Instantiate(HpChangeIceExplosionObj, playerTrans.position, Quaternion.identity);
    }

    public void SpawnDashCastEffect()
    {
        if(dashCastEffectCnt <= 0)
        {
            dashCastEffectCnt = 1f;
            var effect = Instantiate(dashCastEffectObj, playerTrans.position, Quaternion.identity);
        }

    }

    public void SpawnGetDamageCastEffect()
    {
        if (getDamageCastEffectCnt <= 0)
        {
            getDamageCastEffectCnt = 1f;
            var effect = Instantiate(getDamageCastEffectObj, playerTrans.position, Quaternion.identity);
        }

    }

    public void SpawnGetHealCastEffect()
    {
        var effect = Instantiate(getHealCastEffectObj, playerTrans.position, Quaternion.identity);
    }


    public void SpawnWalkCastEffect()
    {
        if (getItemCastEffectCnt <= 0)
        {
            getItemCastEffectCnt = 1f;
            var effect = Instantiate(walkCastEffectObj, playerTrans.position, Quaternion.identity);
        }
    }

    public void SpawnGetItemCastEffect()
    {
        if (getItemCastEffectCnt <= 0)
        {
            getItemCastEffectCnt = 1f;
            var effect = Instantiate(getItemCastEffectObj, playerTrans.position, Quaternion.identity);
        }

    }


    public void ShowCirticalDamageNumber(Vector3 spawnPos,float dmg,Transform numTrans, bool isDead)
    {
        DamageNumber dmgN = damageNumCirt.Spawn(spawnPos, dmg, numTrans);
        dmgN.enableFollowing = !isDead;
    }

    [SerializeField] float explosionRadius = 30f;
    [SerializeField] int explosionCount = 200;
    public void TestSpawnSkillExplosion()
    {
        ////if press t 
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //     Vector3 center = playerTrans.position;

        //    // Spawn all in one frame. If this is too heavy, use the coroutine version below.
        //    for (int i = 0; i < explosionCount; i++)
        //    {
        //        // Disk sample around player
        //        Vector2 rnd = Random.insideUnitCircle * explosionRadius;
        //        Vector3 pos = new Vector3(center.x + rnd.x, center.y, center.z + rnd.y); // start above ground for ray

               

        //        SpawnSkillExplosion(pos, SkillIdType.None);
        //    }

        //}

    }

    public void SpawnBrightSoul(Vector3 pos)
    {
        int rand = Random.Range(0, 100);
        float chance = 30f;
        if (universalTrait.isLuckySeven) chance += 7f;
        if (rand > chance) return;
        
       skillBrightSoulPoolObj.GetObject(pos, Quaternion.identity);
        //EventManager.EmitEventData(GameEvent.ChangePlayerHp, 2f);

        brightSoulNum += 1;
        ActiveBuffManager.Instance.AddStack(TraitType.KillEnemyBrightSoul);

        if(brightSoulNum >= 4)
        {
            //skillBrightSoulPoolObj.GetObject(playerTrans.position, Quaternion.identity);
            brightSoulNum = 0;
            EventManager.EmitEventData(GameEvent.ChangePlayerHp, 1f);
            ActiveBuffManager.Instance.SetStacks(TraitType.KillEnemyBrightSoul,1);
        }

    }

    public void SpawnSkullSoul(Vector3 pos, SkillIdType _skillType = SkillIdType.None)
    {       
        int rand = Random.Range(0, 100);
        float chance = 30f;
        if (universalTrait.isLuckySeven) chance += 7f;
        if (rand > chance) return;

        SkillModelBase sm = skillSkullSoulPoolObj.GetObjectComponent<SkillModelBase>(pos, Quaternion.identity);
        sm.skillIdType = _skillType;
        sm.skillDuration = 10f;
        sm.skillDamage = 25f;
        sm.InitStatusSizeAndPosY();
    }

    public void SpawnSplitMagicBullet(Vector3 pos, SkillIdType _skillType, bool _isMoveFire, bool _isPushback, bool _isEnchantFire, bool _isEnchantIce, bool _isEnchantPoison, float _dmg, bool isFinishCastExplode, bool isKillEnemyExplode, bool isBrightSoul)
    {
        int projectileCount = 3;
        float angleStep = 360f / projectileCount;

        for(int i = 0; i < projectileCount; i++)
        {
            float currentAngle = i * angleStep;
            Vector3 dir = (Quaternion.AngleAxis(currentAngle, Vector3.up) * Vector3.forward).normalized;
            Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

            SkillModelBase sm = skillSplitMagicBulletPoolObj.GetObjectComponent<SkillModelBase>(pos, rot);
            sm.skillIdType = _skillType;
            sm.skillDuration = 0.77f;
            sm.skillDamage = _dmg;
            

            sm.CopyTrait(isFinishCastExplode, isKillEnemyExplode, false, false, _isMoveFire, _isPushback, _isEnchantFire, _isEnchantIce, _isEnchantPoison, false);
            sm.InitStatusSizeAndPosY();

            SkillForwardMove smMove = sm.GetComponent<SkillForwardMove>();
            smMove.moveVec = dir * 14f;

        }

        

    }

    public void SpawnEnemyDeathExplosion(Vector3 pos, SkillIdType _skillType = SkillIdType.None, float damage = 30f, float size = 1f, bool isPoisonDebuff = false)
    {
        //float chance = isPoisonDebuff ? 50f : 25f;
        float chance = 50f;
        if(universalTrait.isLuckySeven) chance += 7f;
        int rand = Random.Range(0, 100);
        if (rand > chance) return; 


        pos.y = 0.49f;    
        pos.y *= size; //more size , height higher

        SkillModelBase sm = enemyExplosionPoolObj.GetObjectComponent<SkillModelBase>(pos, Quaternion.identity);
        sm.skillIdType = _skillType;
        sm.skillDuration = 0.7f;
        sm.skillSize = size;
        sm.skillDamage = damage;
        sm.InitStatusSizeAndPosY();

        if (acidExplosionCdCnt <= 0)
        {
            acidExplosionCdCnt = 0.21f;
            SoundEffect.Instance.Play(SoundList.TraitAcidExplosionSe);
        }
    }

    //public void SpawnBombExplosion(Vector3 pos, float damage, float size, 

    public void SpawnSkillExplosion(Vector3 pos, SkillIdType _skillType, float damage = 30f, float size = 1f)
    {
        //int rand = Random.Range(0, 100);
        //if (rand < 50) return;

        pos.y = 1f; //height fixed with 1
        SkillModelBase sm = skillExplosionPoolObj.GetObjectComponent<SkillModelBase>(pos, Quaternion.identity);
        sm.skillIdType = _skillType;
        sm.skillDuration = 0.7f;
        sm.skillSize = size;
        sm.skillDamage = damage;
        sm.InitStatusSizeAndPosY();

        if(afterSkillExplosionCdCnt <= 0)
        {
            afterSkillExplosionCdCnt = 0.5f;
            SoundEffect.Instance.Play(SoundList.TraitAfterSkillExplosionSe);
        }
        

        //sm.SetSkill(_skillType,1,1,"",0.7f,1,)

        //Debug.Log($"SpawnSkillExplosion: {pos}"); //Debug
    }

    public void SpawnSkillWolfObj(Vector3 startPos, Vector3 targetPos,float duration)
    {
        SkillModelBase sm = skillWolfPoolObj.GetObjectComponent<SkillModelBase>(startPos, Quaternion.identity);
        SkillWolfMove move = sm.GetComponent<SkillWolfMove>();
        move.StartMove(startPos, targetPos, 2.8f,duration);
        sm.skillDamage = 20f;
        sm.skillDuration = duration;
        sm.InitStatusSizeAndPosY();
    }

    public void SpawnSkillFireObj(Vector3 pos, SkillIdType _skillType, float duration = 7f)
    {
        pos.y = 0.5f;

        SkillModelBase sm = skillFirePoolObj.GetObjectComponent<SkillModelBase>(pos, Quaternion.identity);
        sm.skillDamage = 21f;
        if(universalTrait.isFireEnhanced) sm.skillDamage = 49f; 
        if(universalTrait.isElementResonance) sm.skillDamage *= 1.5f;

        sm.skillIdType = _skillType;
        sm.skillDuration = duration;
        sm.transform.position = pos;
        sm.InitStatusSizeAndPosY();

        //Debug.Log($"SpawnSkillFireObj: {pos}"); //Debug

        //var effect = Instantiate(skillFireObj, pos, Quaternion.identity);
        //Destroy(effect, 7.0f); // 3秒後にエフェクトを破棄


    }

    public void ShowRelatedTraitWindow(List<TraitData> relatedTraits, int slotId)
    {
        relateTraitWindowObj.SetActive(true);
        MenuOpenAnimator ani = relateTraitWindowObj.GetComponent<MenuOpenAnimator>();
        ani.PlayeMenuAni(true);

        //for (int i = 0; i < 4; i++)
        //{
        //    if (i < relatedTraits.Count)
        //    {
        //        relatedTraitUIItems[i].gameObject.SetActive(true); //show only needed items
        //    }
        //    else
        //    {
        //        relatedTraitUIItems[i].gameObject.SetActive(false);
        //    }
        //}

        if (slotId == 0)
        {
            RectTransform rt = relateTraitWindowObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(-483f, -365f);
        }
        else if (slotId == 1)
        {
            RectTransform rt = relateTraitWindowObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(121f, -365f);
        }
        else if (slotId == 2)
        {
            RectTransform rt = relateTraitWindowObj.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(714f, -365f);
        }

        for (int i = 0; i < relatedTraitUIItems.Count; i++)
         {
             if (i < relatedTraits.Count)
             {
                 relatedTraitUIItems[i].SetData(relatedTraits[i]);
             }
             else
             {
                 relatedTraitUIItems[i].Hide();
             }
         }

    }

    public void HideRelatedTraitWindow()
    {
        //if (relateTraitWindowObj != null)
        //{
        //    relateTraitWindowObj.SetActive(false);
        //}

        MenuOpenAnimator ani = relateTraitWindowObj.GetComponent<MenuOpenAnimator>();
        ani.PlayeMenuAni(false);
    }

    public void CloseRelatedTraitWindow()
    {
        if (relateTraitWindowObj != null)
        {
            relateTraitWindowObj.SetActive(false);
        }
    }

    public void ShowCurrentTraitName(string _name , string _desc, Vector2 _showPos)
    {
        currentTraitFrame.gameObject.SetActive(true);
        currentTraitDesc.text = _desc;
        currentTraitName.text = _name;

        RectTransform rt = currentTraitFrame.GetComponent<RectTransform>();
        rt.anchoredPosition = _showPos;

    }

    public void HideCurrentTraitName()
    {
        currentTraitFrame.gameObject.SetActive(false);
        currentTraitDesc.text = "";
        currentTraitName.text = "";

    }

}

