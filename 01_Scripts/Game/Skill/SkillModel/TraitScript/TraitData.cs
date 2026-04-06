using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using Cysharp.Threading.Tasks;

[CreateAssetMenu(fileName = "NewTraitData",order = -773)]                        
public class TraitData : ScriptableObject
{
    public TraitType traitType = TraitType.None;
    public TraitJobType traitJobType = TraitJobType.Universal;

    public string traitName = "名前";
    public string traitDescription = "説明";
    public Sprite icon;

    public List<TraitData> relatedTraitList = new List<TraitData>();

    public bool isDemoRecommendedTrait = false;

    public bool isSelected = false; //whether the player has already taken this trait
    public bool isUniversalApplied = false;
    public bool isTraitRepeateable = true; //whether the player can take this trait multiple times

    public bool isAfterDashCast = false;
    public bool isAfterDashEnhanced = false;

    public bool isGetHealCast = false;
    public bool isHpChangeCast = false; 
    public bool isHpChangeEnhanced = false;

    public bool isGetHealItemCast = false;
    public bool isGetHealItemEnhanced = false;

    public bool isWalkCast = false;

    public bool isFinishCastExplosion = false;
    public bool isFinishCastSplit = false; //inactive


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

    public bool isDashShield = false;
    public bool isDashCastIce = false;
    public bool isDashCastFire = false;
    public bool isSniper = false;
    public bool isDoubleDash = false;

    public bool isGetDamageIceExplosion = false;

    public bool isCriticalHealer = false;
    public bool isTreasureHunter = false;

    public bool isStandStillEnhance = false;
    public bool isStandStillGetShield = false;
    public bool isStandStillGetHeal = false;

    public bool isPlayerWalkFire = false;

    public bool isFireEnhanced = false;
    public bool isIceEnhanced = false;
    public bool isPoisonEnhanced = false;
    public bool isCritEnhanced = false;

    public bool isAfterCastSpeedAdd = false;
    public bool isAfterCastSummonGhost = false;

    public bool isHealDouble = false;

    public bool isReviveOnce = false;
    public bool isSlotAddDamage = false;
    public bool isGambler = false;
    public bool isFairJudge = false;

    public bool isCritDamageAdd = false;
    public bool isBulletShield = false;
    public bool isItemEffectDoubled = false;

    public bool isIsolator = false;
    public bool isDebuffImmnity = false;
    public bool isFireWalker = false;

    public bool isAfterDashAddSpd = false;
    public bool isLuckySeven = false;
    public bool isDealMoreDamageFullHp = false;
    public bool isChestItemAddDamge = false;
    public bool isKillEnemyDropFood = false;

    public bool isGetDamageAddAttack = false;
    public bool isDashPushBackEnemy = false;
    public bool isBerserker = false;
    public bool isPetGetStronger = false;
    public bool isLuckAddCrit = false;
    public bool isGetCoinAddCrit = false;
    public bool isKillEnemeyBigger = false;

    public bool isLonelyMan = false;
    public bool isELementalist = false;
    public bool isMoveSpeedAddAttack = false;

    public bool isStrawMan = false;
    public bool isGiftAngel = false;
    public bool isElementResonance = false;
    public bool isBloodPrice = false;
    public bool isPlayerGetBigger = false;
    public bool isPlayerGetSmaller = false;
    public bool isAddCoinGain = false;

    public bool isTraitUnlocked = false;

    // public bool isAttackAddMoveSpd = false;

    //Permanent Stat Changes
    public float damageAdd = 0f;
    public float cooldownAdd = 0f;
    public float durationAdd = 0f;
    public float speedAdd = 0f;
    public float sizeAdd = 0f;
    public int projectilNumAdd = 0;

    //Passive
    public float dashCooldownAdd = 0f;
    public float dashMaxTimeAdd = 0f;
    public float pickUpRangeAdd = 0f;
    public float expGainAdd = 0f;
    public float hpRegenAdd = 0f;
    public float hpMaxAdd = 0f;

    //Add to all Skills
    public float gobalDamageAdd = 0f;
    public float gobalCooldownAdd = 0f;
    public float gobalDurationAdd = 0f;
    public float gobalSpeedAdd = 0f;
    public float gobalSizeAdd = 0f;
    public int gobalProjectilNumAdd = 0;
    public float gobalCritChanceAdd = 0f;
    public float gobalLuckAdd = 0f;
    public float gobalPlayerDefenceAdd = 0f; //プレーヤの防御力 damage *(1 + gobalPlayerDefenceAdd/100)
    public float gobalMoveSpeedAdd = 0f;
    public float gobalGoldGainAdd = 0f;

    //
    public float tempDamageAdd = 0f;
    public float tempCooldownAdd = 0f;
    public float tempDurationAdd = 0f;
    public float tempSpeedAdd = 0f;
    public float tempSizeAdd = 0f;
    public int tempProjectilNumAdd = 0;
    public float tempCritAdd;

    public float critChanceAdd = 0f;

    public int skillMaxLevelAdd = 0;

    public float playerMaxHpAdd = 0f;

    public void CheckUnlockStatus()
    {
        isTraitUnlocked = AchievementManager.Instance.IsTraitUnlocked(traitType);
    }

    public void ClearAllData()
    {
        //isSelected = false;
        //isUniversalApplied = false;

        isAfterDashCast = false;
        isAfterDashEnhanced = false;

        isGetHealCast = false;
        isHpChangeCast = false;
        isHpChangeEnhanced = false;

        isGetHealItemCast = false;
        isGetHealItemEnhanced = false;

        isWalkCast = false;

        isDoubleDash = false;

        isFinishCastExplosion = false;
        isFinishCastSplit = false; //inactive

        isKillEnemyExplosion = false;
        isKillEnemySkullSoul = false;
        isKillEnemyBrightSoul = false;
        isSkillMovingDropFire = false;

        isSkillMovingDropSpike = false;

        isDoubleCast = false;
        isEnchantFire = false;
        isEnchantIce = false;
        isEnchantPoison = false;
        isEnchantLightning = false;

        isPushback = false;

        isDashShield = false;
        isGetDamageIceExplosion = false;

        isSniper = false;
        isCriticalHealer = false;
        isTreasureHunter = false;

        isStandStillEnhance = false;
        isStandStillGetShield = false;
        isStandStillGetHeal = false;

        isPlayerWalkFire = false;

        isFireEnhanced = false;
        isIceEnhanced = false;
        isPoisonEnhanced = false;
        isCritEnhanced = false;

        isAfterCastSpeedAdd = false;
        isAfterCastSummonGhost = false;

        isHealDouble = false;

        isReviveOnce = false;
        isSlotAddDamage = false;
        isGambler = false;
        isFairJudge = false;

        isCritDamageAdd = false;
        isBulletShield = false;
        isItemEffectDoubled = false;
        isIsolator = false;
        isDebuffImmnity = false;
        isFireWalker = false;

        isAfterDashAddSpd = false;
        isLuckySeven = false;
        isDealMoreDamageFullHp = false;
        isChestItemAddDamge = false;
        isKillEnemyDropFood = false;

        isGetDamageAddAttack = false;
        isDashPushBackEnemy = false;
        isBerserker = false;

        isPetGetStronger = false;
        isLuckAddCrit = false;
        isGetCoinAddCrit = false;
        isKillEnemeyBigger = false;

        isLonelyMan = false;
        isELementalist = false;
        isMoveSpeedAddAttack = false;

        isStrawMan = false;
        isGiftAngel = false;
        isElementResonance = false;
        isBloodPrice = false;
        isPlayerGetBigger = false;
        isPlayerGetSmaller = false;
        isAddCoinGain = false;

        damageAdd = 0f;
        cooldownAdd = 0f; //....etc
        durationAdd = 0f;
        speedAdd = 0f;
        sizeAdd = 0f;
        projectilNumAdd = 0;
        gobalDamageAdd = 0f;
        gobalCooldownAdd = 0f;
        gobalDurationAdd = 0f;
        gobalSpeedAdd = 0f;
        gobalSizeAdd = 0f;
        gobalProjectilNumAdd = 0;
        tempDamageAdd = 0f;
        tempCooldownAdd = 0f;
        tempDurationAdd = 0f;
        tempSpeedAdd = 0f;
        tempSizeAdd = 0f;
        tempProjectilNumAdd = 0;

        pickUpRangeAdd = 0f;
        expGainAdd = 0f;
        hpRegenAdd = 0f;
        hpMaxAdd = 0f;
        skillMaxLevelAdd = 0;



        dashCooldownAdd = 0f;

        critChanceAdd = 0f;

        gobalPlayerDefenceAdd = 0f;
        gobalLuckAdd = 0f;
        gobalMoveSpeedAdd = 0f;

        gobalGoldGainAdd = 0f;

        playerMaxHpAdd = 0f;



    }


}

