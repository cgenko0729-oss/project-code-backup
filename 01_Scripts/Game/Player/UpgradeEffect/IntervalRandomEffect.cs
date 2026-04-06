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

public class IntervalRandomEffect : UpgradeEffectBase
{
    [Header("このバフ用の専用アイテムボックスPrefab")]
    public GameObject itemBoxPrefab;
    public GameObject spawnEffect;
    public float spawnDist = 1.0f;

    [Header("発動間隔")]
    public float activeInterval = 0;

    [SerializeField]private float intervalCounter = 0;
    private GameObject playerObj;
    private ParticleSystem activeSpawnEffect;

    public override void EffectUpdate()
    {
        if(isActived == true)
        {
            // 発動間隔の更新
            intervalCounter -= Time.deltaTime;
            if (intervalCounter <= 0) 
            {
                isActived = false;
            }
        }
        else
        {
            if (playerObj == null) { return; }

            // 出現させる場所を計算する
            var playerPos = new Vector3();
            playerPos = playerObj.transform.position;
            Vector3 spawnDir = new Vector3();
            spawnDir.x = Random.Range(0, 1);
            spawnDir.z = Random.Range(0, 1);
            playerPos += spawnDir * spawnDist;

            // 出現時のエフェクトを発生させる
            if (itemBoxPrefab == null) { return; }
            var effect = Instantiate(spawnEffect,
                                    playerPos,
                                    spawnEffect.transform.rotation);
            activeSpawnEffect = effect?.GetComponent<ParticleSystem>();

            // アイテムボックスを出現させる
            if (itemBoxPrefab == null) { return; }
            var item = Instantiate(itemBoxPrefab, 
                                    playerPos, 
                                    itemBoxPrefab.transform.rotation);

            Debug.Log($"アイテムを出現させました\n" +
                $"x:.{playerPos.x}.y:.{playerPos.y}.z:.{playerPos.z}.");

            // 発動間隔をリセット
            isActived = true;
            intervalCounter = activeInterval;
        }

        if(activeSpawnEffect != null)
        {
            activeSpawnEffect.GetComponent<ParticleSystem>();
            if(activeSpawnEffect.isPlaying == false)
            {
                Debug.Log("エフェクトの削除");
                Destroy(activeSpawnEffect.gameObject);
                activeSpawnEffect = null;
            }
        }
    }

    public override void CanEnableBuff()
    {
        isEnable = BuffManager.Instance.isIntervalRandomEffectEnabled;

        if(isEnable == true)
        {
            ActiveBuffManager.Instance.SetStacks(
                TraitType.OwlSkill2_WonderPocket, 0);
        }

        playerObj = GameObject.FindWithTag("Player");
        intervalCounter = activeInterval;
    }
}

