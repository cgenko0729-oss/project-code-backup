using QFSW.MOP2;                //Object Pool
using System.Collections;
using System.Collections.Generic;
using TigerForge;               //EventManager
using UnityEngine;  

public class PetSkillData : MonoBehaviour
{ 
    [Header("スキルのパーティクル")]
    public ParticleSystem petSkillPS;

    [Header("ダメージを与えた時のオブジェクト")]
    public GameObject petSkillDamageObj;

    [Header("弾の寿命（秒）")]
    [SerializeField] private float lifetime = 5.0f;

    [Header("当たった時に消えるか")]
    [Tooltip("Trueなら当たった時に消える")]
    [SerializeField] private bool isHitDestroyFlg = true;

    [Header("このオブジェクトのダメージを消す")]
    [SerializeField] private bool isDamageClearFlg = false;

    [Header("スキルのオブジェクトプール")]
    public ObjectPool skillObjPool;

    private enum SkillType
    {
        Other,
        DragonBreath,
    }

    [Header("スキルの種類")]
    [SerializeField] private SkillType skillType = SkillType.Other;

    [Header("角度を戻す時間(DragonFlameのみ)")]
    [SerializeField] private float returnAngleTime = 0.5f;

    public float speed = 10.0f;

    //与えるダメージ
    private float damages = 0f;

    //与えるダメージを保持する変数
    private float finalDamages = 0f;

    private Vector3 moveDirection;
    private Quaternion lookRot;
    private bool hasHit = false; // すでにヒットしたかどうかのフラグ

    // もし、一定時間で消したい場合は以下を使用
    private Coroutine lifetimeCoroutine;
    private Coroutine FlameRoutine;

    private ObjectPool myPool; // オブジェクトプール

    private GameObject originalAttacker;

    private LastAttackType lastAttacker = LastAttackType.Other;

    private float hpDamageRate = 0f;

    private float bonusDmg = 0f;

    //ペットからダメージ値を受け取るための専用の公開メソッド
    public void Initialize(float damageFromPet, GameObject attacker,
                           LastAttackType lastAttack = LastAttackType.Other, 
                           float _hpDamageRate = 0f)
    {
        if (isDamageClearFlg)
        {
            //ペットからのダメージを保持
            this.finalDamages = damageFromPet;
            //このオブジェクト自身のダメージは0にする
            this.damages = 0f;
        }
        else
        {
            //このオブジェクト自身がダメージを持つ
            this.damages = damageFromPet;
            //爆発用のダメージは0にしておく
            this.finalDamages = 0f;
        }
        this.originalAttacker = attacker;
        this.lastAttacker = lastAttack;
        this.hpDamageRate = _hpDamageRate;
    }
    public void SetPool(ObjectPool pool)
    {
        myPool = pool;
    }
    private void OnEnable()
    {
        // ヒットフラグを初期状態に戻す
        hasHit = false;

        lookRot = transform.rotation;

        lifetimeCoroutine = StartCoroutine(LifetimeCoroutine());

        if (skillType == SkillType.DragonBreath)
        {
            FlameRoutine = StartCoroutine(FlameCoroutine());
        }
    }
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        lookRot = Quaternion.LookRotation(moveDirection);
        transform.rotation = lookRot; // 初期回転も設定しておくと一瞬で向く
    }

    private void Start()
    {
        if (petSkillPS != null)
        {
            ParticleSystem ps = Instantiate(petSkillPS, transform.position, Quaternion.identity, transform);
            ps.Play();
        }
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;

        if (skillType == SkillType.Other)
        {
            if (speed > 0f && moveDirection != Vector3.zero)
            {
                // 徐々に回転を向ける
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);
            }
        }
        if (skillType == SkillType.DragonBreath)
        {
            // 常にlookRotの方向を向く
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
        }
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

    private IEnumerator FlameCoroutine()
    {
        // 指定された寿命の時間だけ待機する
        yield return new WaitForSeconds(returnAngleTime);

        Vector3 targetEulerAngles = lookRot.eulerAngles;
        targetEulerAngles.x = 0f;
        lookRot = Quaternion.Euler(targetEulerAngles);
        moveDirection = lookRot * Vector3.forward;
    }

    private void OnTriggerEnter(Collider col) // 敵にはダメージを与え、破壊可能オブジェクトにはOnHitを呼び出す
    {
        if (hasHit) return; // すでにヒット済みなら無視

        if (col.CompareTag("Enemy"))
        {
            if (isHitDestroyFlg)
            {
                hasHit = true; // ヒットフラグを立てる
            }

            EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();

            if (enemyStat != null)
            {
                if (this.damages > 0)
                {
                    float baseDmg = this.damages;
                   
                    //敵のHP割合ダメージ計算
                    if (this.hpDamageRate > 0f)
                    {
                        float rate = this.hpDamageRate;

                        bonusDmg = enemyStat.enemyMaxHp * rate;
                    }

                    float finalDamage = baseDmg + bonusDmg;

                    if (this.lastAttacker != LastAttackType.Other)
                    {
                        FinalDanages(enemyStat, finalDamage, this.lastAttacker);
                    }
                    else
                    {
                        FinalDanages(enemyStat, finalDamage);
                    }
                }
            }

            if (petSkillDamageObj != null)
            {
                //エフェクト生成
                GameObject hitObj = skillObjPool.GetObject();

                hitObj.transform.position = transform.position;

                //スクリプトを取得
                PetSkillHitEffectData hitEffectData = hitObj.GetComponent<PetSkillHitEffectData>();

                //保持していたダメージ値を渡す
                if (hitEffectData != null)
                {
                    //当たった時用のダメージを初期化
                    hitEffectData.Initialize(this.finalDamages,this.originalAttacker);
                    hitEffectData.SetPool(skillObjPool);
                }
            }

            if (myPool != null)
            {
                if (isHitDestroyFlg)
                {
                    myPool.Release(gameObject); // オブジェクトプールに返す
                }
            }

            SetPetAttackEvent(enemyStat);
        }
    }

    //最終ダメージ
    public void FinalDanages(EnemyStatusBase enemyStat, float finalDamages, LastAttackType lastAttack = LastAttackType.Other)
    {
        bool isPetDamageDouble = SkillEffectManager.Instance.universalTrait.isPetGetStronger;

        if (isPetDamageDouble)
        {
            enemyStat.TakeDamage(finalDamages * 2, isShowDamageNumber: true, skillType: SkillIdType.Pet, LastAttack: lastAttack);
        }
        else enemyStat.TakeDamage(finalDamages, isShowDamageNumber: true, skillType: SkillIdType.Pet, LastAttack: lastAttack);

        ItemManager.Instance.takedmgTotal += finalDamages;
    }

    public void SetPetAttackEvent(EnemyStatusBase enemyStat)
    {
        var eventData = new Dictionary<string, object>();

        eventData["attacker"] = this.originalAttacker;
        eventData["target"] = enemyStat.transform;

        //攻撃の号令を発信する(他ペットが攻撃したときの条件を取れる)
        EventManager.SetData(GameEvent.PetAttack, eventData);
        EventManager.EmitEvent(GameEvent.PetAttack);
    }

    //敵のHP割合ダメージ取得
    public float GetHpDamageRate()
    {
        return hpDamageRate;
    }
}

