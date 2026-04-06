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

public class OpenChest : MonoBehaviour, IHittable
{
    private bool isOpen = false; // チェストが開いているかどうかのフラグ
    public float rotationDuration = 1.0f; // 回転にかかる時間（秒）

    public ParticleSystem chestShineEffect;

    public bool isEnchantChest = false;

    void Start()
    {
        if (isEnchantChest)
        {
            SoundEffect.Instance.Play(SoundList.QuestFinishSe);
        }
    }

    public void OnHit()
    {

        if (isOpen) return; // すでに開いている場合は何もしない

        isOpen = true; // チェストが開いたことを記録
        
        // 目標となる角度を(X:-180, Y:0, Z:-180)に設定
        transform.DORotate(new Vector3(-90f, 0f, -180f), rotationDuration).OnComplete(() => {
            if (isEnchantChest)
            {
                //SkillManager.Instance.isNotLvUp = true;
                //EventManager.EmitEvent("PlayerLevelUp");


                //SkillManager.Instance.GetTraitLevelUp();
                Invoke("DelayChestReward", 0.21f);
                SoundEffect.Instance.Play(SoundList.QuestFinishSe);
            }
        });

        if (!isEnchantChest)
        {
            CoinSpawner.Instance.SpawnWithDelay(transform.position); // コインをスポーン
            CoinSpawner.Instance.SpawnChestRewardItem(transform.position,false);
        }


        GameQuestManager.Instance.ClearAllEnemyQuest(transform.position);

        Invoke("StopShineEffect", 2.1f);

    }

    public void StopShineEffect()
    {
        chestShineEffect.Stop();
    }

    public void DelayChestReward()
    {
        CoinSpawner.Instance.SpawnWithDelay(transform.position); // コインをスポーン
            CoinSpawner.Instance.SpawnChestRewardItem(transform.position);
    }

}

