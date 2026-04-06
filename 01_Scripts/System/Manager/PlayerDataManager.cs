using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class PlayerDataManager : SingletonA<PlayerDataManager>
{
    public PlayerData playerData;

    [Header("受けたダメージの総量")]
    public float totalDamage;

    void Start()
    {
        if (StageManager.Instance.currentScene == SceneType.Title)
        {
            playerData.jobId = JobId.DogKnight;
            //Debug.Log("Titleシーンなのでデフォルト職業を犬に設定: " + playerData.jobId);
        }
    }

    void Update()
    {
        
    }
}

