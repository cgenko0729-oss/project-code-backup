using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using System.Data;

public class BossHpBar : MonoBehaviour
{
    //public Slider Hp;
    public GameObject Boss;
    public Image BossHeart;


    float BossMaxHp;
    float BossHp;

    UIFXController uiFx;

    public void SetBossTargetObj(GameObject bossObj)
    {
        Boss = bossObj;
    }

    void Start()
    {
        uiFx = GetComponent<UIFXController>();

        if (uiFx)
        {
            uiFx.TriggerShine();
            Debug.Log("BossHpBar Start: UIFXController found and shine triggered.");
        }

        



    }

    void Update()
    {
        if(Boss)BossHp = Boss.GetComponent<EnemyStatusBase>().enemyHp;
        if(Boss)BossMaxHp = Boss.GetComponent<EnemyStatusBase>().enemyMaxHp;


        BossHeart.fillAmount = (float)(BossHp / BossMaxHp);
    }
}



