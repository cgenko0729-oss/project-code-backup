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

public class LoadingCharaSpanwer : MonoBehaviour
{
    public PlayerData playerData;
    public List<GameObject> charaModelPrefabs;

    void Start()
    {   
        if(playerData == null)
        {
            Debug.Log("PlayerDataÇéQè∆Ç≈Ç´Ç‹ÇπÇÒÇ≈ÇµÇΩ");
        }
        else
        {
            JobId jobId = playerData.jobId;
            var chara = Instantiate<GameObject>(charaModelPrefabs[(int)jobId], transform);
        }
    }
}

