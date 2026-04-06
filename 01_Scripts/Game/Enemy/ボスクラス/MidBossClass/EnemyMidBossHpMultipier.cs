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

public class EnemyMidBossHpMultipier : MonoBehaviour
{

    public EnemyStatusBase status;
    public float hpMultipier = 1.0f;

    void Start()
    {
        status = GetComponent<EnemyStatusBase>();

        switch (StageManager.Instance.mapData.stageDifficulty)
        {
            case DifficultyType.None:
                hpMultipier = 1.0f;
                break;
            case DifficultyType.Normal:
                hpMultipier = 1.1f;
                break;
            case DifficultyType.Hard:
                hpMultipier = 1.28f;
                break;
            case DifficultyType.Nightmare:
                hpMultipier = 1.49f;
                break;
            case DifficultyType.Hell:
                hpMultipier = 1.7f;
                break;
            default:
                break;
        }

        DOVirtual.DelayedCall(1f, DelaySetHpByDifficulty);

        int endlessExtra = TimeManager.Instance.endlessExtraPhrase;
        if (endlessExtra > 0)
        {
            hpMultipier += 0.077f * endlessExtra;
        }

    }

    void DelaySetHpByDifficulty()
    {
        status.enemyMaxHp = Mathf.CeilToInt(status.enemyMaxHp * hpMultipier);
        status.enemyHp = status.enemyMaxHp;
    }

    void Update()
    {
        
    }
}

