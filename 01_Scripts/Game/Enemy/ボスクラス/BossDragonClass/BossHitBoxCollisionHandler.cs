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

public class BossHitBoxCollisionHandler : MonoBehaviour
{
    public float hitDamage = 15f;

    public float hitBoxLastTime = 0.7f; // ヒットボックスが有効な時間
    public bool isHitEnabled = false;

    void Start()
    {
        
    }

    private void Update()
    {
        if(isHitEnabled)
        {
            hitBoxLastTime -= Time.deltaTime;
            if (hitBoxLastTime <= 0f)
            {
                isHitEnabled = false;
                hitBoxLastTime = 0.7f; // リセット
            }
        }
    }

    public void EnableHitBox(float hitTime = 0.7f)
    {
        isHitEnabled = true;
        hitBoxLastTime = hitTime;

    }

    //public void OnCollisionEnter(Collision collision)
    //{
    //    if(!isHitEnabled) return;   

    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        EventManager.EmitEventData("ChangePlayerHp",-hitDamage);
    //        //isHitEnabled = false;

    //        //debug log
    //        Debug.Log("BossHitBoxCollisionHandler: OnCollisionEnter: Playerに当たった");
    //    }

    //}

     public void OnCollisionStay(Collision collision)
    {
        if(!isHitEnabled) return;   

        if (collision.gameObject.CompareTag("Player"))
        {
            EventManager.EmitEventData("ChangePlayerHp",-hitDamage);

            //debug log
            Debug.Log("BossHitBoxCollisionHandler: OnCollisionStay: Playerに当たった");

            //isHitEnabled = false;
        }

    }



}

