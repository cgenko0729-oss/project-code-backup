using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class CharacterBuffManager : Singleton<CharacterBuffManager>
{

    public float damageBoost;
    public float moveSpeedBoost;
    public float hpBoost;
    public float attackRangeBoost;

}

