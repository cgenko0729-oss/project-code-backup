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

public class SKillAfterMathHitController : MonoBehaviour
{

    private float effectSize = 1f;
    private float effectDamage = 50f;
    public Vector3 effectStartSize ; // エフェクトの初期サイズ


    public void SetEffectStatus(float damage ,float size)
    {
        effectDamage = damage; // エフェクトのダメージを設定
        effectSize = size; // エフェクトのサイズを設定
        transform.localScale = effectStartSize * effectSize; // エフェクトのサイズを更新

    }

    private void Awake()
    {
        effectStartSize = transform.localScale; // 初期サイズを取得
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider col) // 敵にはダメージを与え、破壊可能オブジェクトにはOnHitを呼び出す
    {
        if (col.CompareTag("Enemy"))
        {
            EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();
            if (enemyStat != null) enemyStat.TakeDamage(effectDamage, true, SkillIdType.starMagic);
            //DpsManager.Instance.ReportDamage(skillIdType, skillDamage);

           

        }

    }


}

