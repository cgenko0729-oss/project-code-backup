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

[System.Serializable]
[CreateAssetMenu(fileName = "StageQuestConfig", menuName = "Quest/Stage Quest Config")]
public class StageQuestConfig : ScriptableObject
{
    public MapType stageType;
    public int stageId;
    public string stageName;

    public float questTriggerInterval = 180f;
    public int maxConcurrentQuests = 2;

    public List<QuestData> availableQuests = new List<QuestData>();
    public List<float> questWeights = new List<float>();

}

