using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager

public class PoisonFieldHItController : MonoBehaviour
{


    private void OnTriggerEnter(Collider col) // 敵にはダメージを与え、破壊可能オブジェクトにはOnHitを呼び出す
    {
        if (col.CompareTag("Enemy"))
        {
            //SkillModelBase sm = GetComponent<SkillModelBase>();
            //if (!sm.isFinalSkill) return;

            //EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();
            //EnemyType type = enemyStat.enemyType;
            //if(enemyStat.enemyType == EnemyType.Mover)
            //{
            //    EnemyActionBase enemyAct = col.GetComponent<EnemyActionBase>();
            //    if(enemyAct)enemyAct.ApplySpeedDownDebuff(0.49f,1f);
            //}
            


        }
    }


}

