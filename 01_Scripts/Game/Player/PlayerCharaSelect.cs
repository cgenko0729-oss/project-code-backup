using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class PlayerCharaSelect : MonoBehaviour
{
    public PlayerData playerData;
    public List<GameObject> charaList;

    private void Awake()
    {
        if (playerData == null)
        {
            Debug.Log("playerData is NULL");
        }

        playerData = PlayerDataManager.Instance.playerData;

        foreach (var chara in charaList)
        {
            int selectJobId = (int)playerData.jobId;
            var selectChara = charaList[selectJobId];
           
            if(chara != selectChara)
            {
                chara.gameObject.SetActive(false);
                continue;
            }

            if (selectChara != null)
            {
                selectChara.gameObject.SetActive(true);

                var statusData = playerData.StatusDataList[(int)playerData.jobId];
                var playerStateComp = selectChara.gameObject.GetComponent<PlayerState>();
                if (playerStateComp != null)
                {
                    playerStateComp.SetStatusData(statusData);
                }

                // 選択したジョブの攻撃アニメーションを行うようにする
                var animator = selectChara.gameObject.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetInteger("JobId", selectJobId);
                }
            }
        }
    }

    void Update()
    {
    }
}

