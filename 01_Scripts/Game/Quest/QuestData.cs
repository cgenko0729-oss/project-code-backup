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
using NUnit.Framework;


[System.Serializable]
[CreateAssetMenu(fileName = "QuestData", menuName = "Quest/Quest Data" ,order = -776)]
public class QuestData : ScriptableObject
{
    [Header("Basic Info")]
    public QuestType questType;
    public string questName;
    public string description;
    public Vector3 spawnPos = Vector3.zero;
    public GameObject questPrefab;

    [Header("Parameters")]
    public float timeLimit = 180f;
    public float questRange = 7.7f;
    public int targetAmount = 100; // kills required, coins to collect, etc.
    public float progressRate = 7f; // for time-based quests like mushroom picking

    [Header("Rewards")]
    public GameObject treasureChestPrefab;
    public int coinReward = 100;


}

