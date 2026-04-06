using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class BossWebScaleController : MonoBehaviour
{
    public float startScale = 0.1f; // 初期スケール
    public float endScale = 1.0f;   // 最終スケール
    public float scaleSpeed = 1.5f; // スケール変化の速度

    public float lifeTime = 7f;

    public Vector3 startRotation;
    public float randRotY;

    void Start()
    {
        // 初期スケールを設定
        transform.localScale = new Vector3(startScale, startScale, startScale);

        // 初期回転を設定
        randRotY = Random.Range(0f, 360f);
        Vector3 tempRot = startRotation;
        tempRot.y += randRotY;
        startRotation = tempRot;

        transform.localRotation = Quaternion.Euler(startRotation);



    }

    void Update()
    {
        if (transform.localScale.x < endScale)
        {
            // スケールを徐々に大きくする
            transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(endScale, endScale, endScale), scaleSpeed * Time.deltaTime);

        }

        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f)
        {
            Destroy(gameObject);
        }

    }

    public void OnCollisionEnter(Collision collision)
    {
        // 何かに衝突したときの処理
        if (collision.gameObject.CompareTag("Player"))
        {
            //EventManager.EmitEvent("PlayerHitWeb");
            ItemManager.Instance.pickUpSpdUpWing = true;
            ItemManager.Instance.spdUpAmount = 0.4f;
            ItemManager.Instance.spdUpTime = 3f;
            EffectManager.Instance.CreatePlayerWebSlowEffect();
        }
    }

}

