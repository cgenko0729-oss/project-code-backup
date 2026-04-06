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

public enum PetShopItemType
{
    //each pet have two slot

    None = 0,
    PickRangeBuff,
    MoveSpeedBuff,
    AttackDamageBuff,
    CooldownBuff,
    AttackRangeBuff,
    HpHealBuff,
    

}

public class ShopMakingNote : MonoBehaviour
{

    //1. define shopItemType to buy (what kind of status and data) (for pet like pickrange, moveSpd, atk, cooldown, atkRange, hpheal, )

    //2.when pet init , load the data and bind to itself's petclass from .dat file , and apply the buff to itself (moveSpeed + moveSpdBuff)

    //during the shop , define what kind it like ,  

    //3.ShopItemData as atom each shop item is a SO, , contain icon,type,name,desc,level struct(needMoney,AddAmount), List Levels)

    //4.ShopItemSlot as Visual ui, hold ShopItemData, currentLv, isMax, priceNext,addAmountNext, itemType, isUnlocked ,

    //5.ShopWindow as manager , hold list of all shopItemSlot, a buy ui panel, 

    //6.ShopSaveSystem to save/Load all buy data 

}

