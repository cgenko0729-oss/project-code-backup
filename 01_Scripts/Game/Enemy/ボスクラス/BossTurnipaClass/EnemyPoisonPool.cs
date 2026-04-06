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

public class EnemyPoisonPool : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        
    }


    public void OnCollisionEnter(Collision collision)  // プレイヤーとの衝突時にダメージを与える
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            EventManager.EmitEventData("PlayerSlowDown",3f); // プレイヤーの移動速度を下げるイベントを発行
            EventManager.EmitEventData("ChangePlayerHp",-10); // プレイヤーにダメージを与える
            //PlayAttackTween();                                            // 攻撃アニメーションを再生
        }
    }



}

