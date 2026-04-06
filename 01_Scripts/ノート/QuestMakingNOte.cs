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

public class QuestMakingNote : MonoBehaviour
{

    /*
     
    Quest have 

    1.QusetConfig: 
    -stageId
    -QuestInterval(140s) 
    -Max quest 
    -AvailableQuestList
    -Weight?

    2.QuestDataSo:
    -QuestType (stay range , killBoss, killelite, traceRare, killGroupEnemy)
    -Quest Prefab
    -TimeLimit(180s~)
    -Quest Active Range(14~)
    -TargetAmount(100~)
    -ProgressRate(7~) (for stayRange)
    -RewardPrefab
    -CoinRewardNum?

    3.QuestObjPrefab:
    A.DestinationIndicator2D script
    B.GameQuestHandler script
    GameQuestHandler Config:
    -QuestText: ƒNƒGƒXƒg:“G‚ð‚P‚O‚O‘Ì“|‚·
    -DamageNumPrefab
    -questRange(14)
    C.RingCircle ChildObj (progressRateRing) with SphereCollider
    D.QuestObjModel for visual (mushRoom Model),Ancient Pillar, Fog Effect Obj...etc, 
    
    


    //Quest Flow 
    1.Make a quest gameObject,(in QuestObj folder)
    2., attach DestinationIndicator2D script and GameQuestHandler script 
    3.GameQuestHandler Config: 
    -tick 


     */





}

