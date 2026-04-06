using QFSW.MOP2;                //Object Pool
using System.Collections;
using System.Collections.Generic;
using TigerForge;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class PetSkillHitEffectData : MonoBehaviour
{
    [Header("※スキルの後のエフェクトにも\nダメージ判定をしたいときに使うスクリプト")]

    [Header("弾の寿命（秒）")]
    [SerializeField] private float lifetime = 5.0f;

    [Header("判定")]
    [SerializeField] private Collider attackCol;

    [Header("攻撃SE")]
    [SerializeField] protected SoundList skillSound=SoundList.None;

    enum AttackType
    {
        SingleHit, //一回だけダメージを与える
        MultiHit,   //複数回ダメージを与える
        AllHit,      //当たったらずっとダメージを与え続ける
        None,
    }

    [Header("攻撃タイプ")]
    [SerializeField] private AttackType attackType = AttackType.SingleHit;

    [Header("ダメージカウンター")]
    [SerializeField] private int attackCounter = 3; //複数回攻撃可能な場合の回数制限

    //与えるダメージ
    private float damages = 1.0f;

    // もし、一定時間で消したい場合は以下を使用
    private Coroutine lifetimeCoroutine;

    private ObjectPool myPool; // オブジェクトプール

    private int attackHitCount = 0; // 攻撃がヒットした回数のカウンター

    private GameObject ownerPet;

    private void OnEnable()
    {
        if(attackCol != null)
        {
            attackCol.isTrigger = true;
        }

        PlayAttackSound();

        lifetimeCoroutine = StartCoroutine(LifetimeCoroutine());
    }

    public void SetPool(ObjectPool pool)
    {
        myPool = pool;
    }

    //ペットからダメージ値を受け取るための専用の公開メソッド
    public void Initialize(float damageFromPet, GameObject owner)
    {
        this.damages = damageFromPet;
        this.ownerPet = owner;
    }

    private void OnTriggerEnter(Collider col) // 敵にはダメージを与え、破壊可能オブジェクトにはOnHitを呼び出す
    {
        
        if (col.CompareTag("Enemy"))
        {
          
            EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();
            if (enemyStat != null) 
            {
                SetPetAttackEvent(enemyStat);

                FinalDanages(enemyStat, this.damages);
            }
        }

        if (attackType == AttackType.AllHit) return;

        if (attackType == AttackType.SingleHit)
        {
            //一回攻撃したら当たり判定を無効にする
            if (attackCol != null)
            {
                attackCol.isTrigger = false;
            }          
        }
        else if (attackType == AttackType.MultiHit)
        {
            attackHitCount = attackCounter;

            attackHitCount--;
            if (attackHitCount <= 0)
            {
                //複数回攻撃可能だが、回数制限に達したら当たり判定を無効にする
                if (attackCol != null)
                {
                    attackCol.isTrigger = false;
                }              
            }
        } 
    }

    public void FinalDanages(EnemyStatusBase enemyStat, float finalDamages)
    {
        bool isPetDamageDouble = SkillEffectManager.Instance.universalTrait.isPetGetStronger;

        float doubleDamages = 2f;

        if (isPetDamageDouble)
        {
            enemyStat.TakeDamage(finalDamages * doubleDamages);
        }
        else enemyStat.TakeDamage(finalDamages);

        ItemManager.Instance.takedmgTotal += finalDamages;
    }

    private IEnumerator LifetimeCoroutine()
    {
        // 指定された寿命の時間だけ待機する
        yield return new WaitForSeconds(lifetime);

        // 時間が来たら、プールに返却する
        if (myPool != null)
        {
            myPool.Release(gameObject);
        }
        else
        {
            // プールが設定されていない場合の保険
            Destroy(gameObject);
        }
    }

    private void PlayAttackSound()
    {
        ActivePetManager.Instance.PetSoundHitCount++;

        if (ActivePetManager.Instance.PetSoundHitCount >= 3)
        {
            SoundEffect.Instance.Play(skillSound);
            ActivePetManager.Instance.PetSoundHitCount = 0;
        }
    }

    public void SetPetAttackEvent(EnemyStatusBase enemyStat)
    {
        var eventData = new Dictionary<string, object>();

        eventData["attacker"] = this.ownerPet;
        eventData["target"] = enemyStat.transform;

        //攻撃の号令を発信する(他ペットが攻撃したときの条件を取れる)
        EventManager.SetData(GameEvent.PetAttack, eventData);
        EventManager.EmitEvent(GameEvent.PetAttack);
    }
}

