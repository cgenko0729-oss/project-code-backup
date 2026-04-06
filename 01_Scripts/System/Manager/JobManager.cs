using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class JobManager : Singleton<JobManager>
{

    //list that hold all TraitData
    public List<TraitData> traitDataList = new List<TraitData>();

    public TraitData[] targetTraitDataOptions = new TraitData[3];

    public TraitData targetTraitData;


    public TraitType traitType = TraitType.None;
    public TraitJobType traitJobType = TraitJobType.Universal;

    public string traitName = "ñºëO";
    public string traitDescription = "ê‡ñæ";
    public Sprite icon;

    public bool isAfterDashCast = false;
    public bool isAfterDashEnhanced = false;

    public bool isHpChangeCast = false; 
    public bool isHpChangeEnhanced = false;

    public bool isAfterCastExplosion = false;

    public bool isSkillMovingDropFire = false;
    public bool isSkillMovingDropSpike = false;

    public bool isDoubleCast = false;

    public float damageAdd = 0f;
    public float cooldownAdd = 0f;
    public float durationAdd = 0f;
    public float speedAdd = 0f;
    public float sizeAdd = 0f;

    public float dashCooldownAdd = 0f;
    public float dashMaxTimeAdd = 0f;

    public float pickUpRangeAdd = 0f;
    public float expGainAdd = 0f;
    public float hpRegenAdd = 0f;
    public float hpMaxAdd = 0f;

    void Start()
    {
        
    }

    void Update()
    {
        //if press Z , RandomlyPickOneTraitFromListToTargetTraitData
        //if (Input.GetKeyDown(KeyCode.Z))
        //{
        //    RandomlyPickOneTraitFromListToTargetTraitData();

        //    traitName = targetTraitData.traitName;
        //    traitDescription = targetTraitData.traitDescription;
        //    traitType = targetTraitData.traitType;
        //    traitJobType = targetTraitData.traitJobType;
        //    icon = targetTraitData.icon;

        //    isAfterDashCast = targetTraitData.isAfterDashCast;
        //    isAfterDashEnhanced = targetTraitData.isAfterDashEnhanced;
        //    isHpChangeCast = targetTraitData.isHpChangeCast;

        //    isHpChangeEnhanced = targetTraitData.isHpChangeEnhanced;
        //    isAfterCastExplosion = targetTraitData.isFinishCastExplosion;
        //    isSkillMovingDropFire = targetTraitData.isSkillMovingDropFire;
        //    isSkillMovingDropSpike = targetTraitData.isSkillMovingDropSpike;
        //    isDoubleCast = targetTraitData.isDoubleCast;
        //    damageAdd = targetTraitData.damageAdd;
        //    cooldownAdd = targetTraitData.cooldownAdd;
        //    durationAdd = targetTraitData.durationAdd;
        //    speedAdd = targetTraitData.speedAdd;
        //    sizeAdd = targetTraitData.sizeAdd;
        //    dashCooldownAdd = targetTraitData.dashCooldownAdd;
        //    dashMaxTimeAdd = targetTraitData.dashMaxTimeAdd;
        //    pickUpRangeAdd = targetTraitData.pickUpRangeAdd;
        //    expGainAdd = targetTraitData.expGainAdd;
        //    hpRegenAdd = targetTraitData.hpRegenAdd;
        //    hpMaxAdd = targetTraitData.hpMaxAdd;
                


        //}

        //if press X , RandomlyPickThreeTraitFromListToTargetTraitData
        //if (Input.GetKeyDown(KeyCode.X))
        //{
        //    RandomlyPickThreeTraitFromListToTargetTraitData();
        //}

    }

    public void RandomlyPickOneTraitFromListToTargetTraitData()
    {
        //if (traitDataList.Count == 0) return;

        //targetTraitData = null;

        //int randomIndex = Random.Range(0, traitDataList.Count);
        //targetTraitData = traitDataList[randomIndex];

        ////remove that option from the list
        //traitDataList.RemoveAt(randomIndex);

        //SkillManager.Instance.activeSkillCastersHolder[0].SetSkillTrait(targetTraitData);

        //Debug.Log("Randomly Picked Trait: " + targetTraitData.traitName);

    }


    public void RandomlyPickThreeTraitFromListToTargetTraitData()
    {
        //if (traitDataList.Count == 0) return;

        //for (int i = 0; i < targetTraitDataOptions.Length; i++)
        //{
        //    targetTraitDataOptions[i] = null;
        //}

        //List<TraitData> tempTraitDataList = new List<TraitData>(traitDataList);
        //for (int i = 0; i < targetTraitDataOptions.Length; i++)
        //{
        //    if (tempTraitDataList.Count == 0) break;

        //    int randomIndex = Random.Range(0, tempTraitDataList.Count);
        //    targetTraitDataOptions[i] = tempTraitDataList[randomIndex];
        //    tempTraitDataList.RemoveAt(randomIndex);
        //    //traitDataList.Remove(targetTraitDataOptions[i]);

        //    Debug.Log("Randomly Picked Trait Option " + (i + 1) + ": " + targetTraitDataOptions[i].traitName);

        //}




    }

}

