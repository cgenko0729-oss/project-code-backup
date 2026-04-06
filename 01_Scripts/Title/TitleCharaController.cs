using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class TitleCharaController : MonoBehaviour
{
    [SerializeField]public PlayerData playerData;
    public List<GameObject> charaList;

    private int nowJobId = 0;
    private int prevJobId = 0;

    void Start()
    {
        foreach(var charaState in GetComponentsInChildren<TitleCharaState>())
        {
            var chara = charaState.gameObject;
            charaList.Add(chara);
            chara.gameObject.SetActive(false);
        }

        nowJobId = (int)playerData.jobId;
        prevJobId = nowJobId;
        ChangePlayer();
    }

    void Update()
    {
        nowJobId = (int)playerData.jobId;
        if(nowJobId != prevJobId)
        {
            ChangePlayer();
            prevJobId = nowJobId;
        }
    }

    private void ChangePlayer()
    {
        // 現在のキャラ
        var nowChara = charaList[prevJobId];

        // 変更したジョブIDのキャラに変更する
        var changeChara = charaList[nowJobId];
        if (changeChara == null)
        {
            Debug.Log("Not found change player");
            return;
        }

        // キャラの交代を行う
        nowChara.gameObject.SetActive(false);
        changeChara.gameObject.SetActive(true);
        playerData.jobId = (JobId)nowJobId;
    }
}

