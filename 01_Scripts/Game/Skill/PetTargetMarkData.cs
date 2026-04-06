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

public class PetTargetMarkData : MonoBehaviour
{
    private Transform targetToFollow;

    private Vector3 offset = new Vector3(0, 1f, 0);

    private ObjectPool myPool;

    //ó^Ç¶ÇÈÉ_ÉÅÅ[ÉWÇï€éùÇ∑ÇÈïœêî
    private float finalDamages = 0f;

    public void Initialize(Transform target, ObjectPool pool)
    {
        this.targetToFollow = target;
        this.myPool = pool;
    }

    void LateUpdate()
    {
        if (targetToFollow == null)
        {
            ReturnToPool();
            return;
        }
        if(ActivePetManager.Instance.spellBomb)
        {
            ReturnToPool();
            return;
        }
        transform.position = targetToFollow.position + offset;
    }


    public void ReturnToPool()
    {
        if (!this.gameObject.activeInHierarchy) return;

        if (myPool != null)
        {
            myPool.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

