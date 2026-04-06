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

public class DragonFlameFireSapwner : MonoBehaviour
{

     public float spawnFireCnt = 3f;
    public bool isSpawnedFire = false;

    private void OnEnable()
    {
        isSpawnedFire = false;
        spawnFireCnt = Random.Range(0.7f, 2.1f);
    }

    private void Start()
    {
        //rand spawnFireCnt between 2 and 5
        spawnFireCnt = Random.Range(0.7f, 2.1f);
        isSpawnedFire = false;
    }

    void Update()
    {
         spawnFireCnt -= Time.deltaTime;
        if (spawnFireCnt <= 0f && !isSpawnedFire)
        {
            isSpawnedFire = true;
            SkillEffectManager.Instance.SpawnSkillFireObj(transform.position,SkillIdType.Pet);
        }
    }
}

