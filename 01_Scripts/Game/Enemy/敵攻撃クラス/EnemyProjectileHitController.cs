using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class EnemyProjectileHitController : MonoBehaviour
{
    public bool isDestroyOnFinish = true; // 当たったときに弾を破壊するかどうか
    public bool isReleaseEffectOnFinish = false; // 当たったときにエフェクトを再生するかどうか
    public GameObject onFinishEffect; // 当たったときに再生するエフェクトのプレハブ 

    public float projectileDamage = 10f; // 弾のダメージ量

    public bool isDestroyOnHitMapObjOrShield = true;

    public bool isStandStillShieldBlock = false; // 立ち止まっているシールドに当たったときにブロックされるかどうか

    public bool isObjectPooled = false; // オブジェクトプールを使用するかどうか
    public ObjectPool pool;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            
            if (isReleaseEffectOnFinish && onFinishEffect != null)
            {
                GameObject effect = Instantiate(onFinishEffect, transform.position, Quaternion.identity);
            }

            if (isStandStillShieldBlock && SkillEffectManager.Instance.universalTrait.isStandStillGetShield)
            {
                
                    SkillEffectManager.Instance.SpawnStandStillShieldObj();
                    EventManager.EmitEventData("ChangePlayerHp", -1.4);
                    EventManager.EmitEvent(GameEvent.PlayerGetDamage);
                    ActiveBuffManager.Instance.AddStack(TraitType.StandStillGetShield);
            }
            else
            {
                EventManager.EmitEventData("ChangePlayerHp", -projectileDamage);
                CameraShake.Instance.StartShake();
                
            }

            if (isDestroyOnFinish)
            {
                if(isObjectPooled && pool != null)
                {
                    pool.Release(gameObject);
                }
                else Destroy(gameObject);

            }
            

        }



        if (collision.gameObject.CompareTag("MapObj")) //EnemyProjectileBlockObj
        {
            if(!isDestroyOnHitMapObjOrShield) return;

            //Debug.Log("EnemyProjectileHitController: OnCollisionEnter: MapObjに当たった");
            // 環境オブジェクトに当たった場合の処理
            if (isDestroyOnFinish)
            {
                if(isObjectPooled && pool != null)
                {
                    pool.Release(gameObject);
                }
                else Destroy(gameObject);

            }

             if (isReleaseEffectOnFinish && onFinishEffect != null)
            {
                GameObject effect = Instantiate(onFinishEffect, transform.position, Quaternion.identity);
            }

        }
    }

}

