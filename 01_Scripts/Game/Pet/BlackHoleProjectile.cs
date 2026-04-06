using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class BlackHoleProjectile : MonoBehaviour
{
    [Header("弾の性能")]
    [SerializeField] private float speed = 1f;
    [SerializeField] private float lifetime = 5f; // 寿命


    [Header("爆発エフェクト")]
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("爆発エフェクトのサウンド")]
    [SerializeField] protected SoundList explosionSound;

    private float damage = 0f;
    private Vector3 moveDirection;
    private Quaternion lookRot;

    private Coroutine lifetimeCoroutine;

    // 親からデータを受け取るための初期化メソッド
    public void Initialize(float totalDamage)
    {
        this.damage = totalDamage;
    }

    private void Start()
    {
        lifetimeCoroutine = StartCoroutine(LifetimeCoroutine());
    }

    void Update()
    {
        // まっすぐ前に進む
        transform.position += moveDirection * speed * Time.deltaTime;
    }
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
        lookRot = Quaternion.LookRotation(moveDirection);
        transform.rotation = lookRot; // 初期回転も設定しておくと一瞬で向く
    }


    private void OnTriggerEnter(Collider other)
    {
        // 敵に当たった時の処理
        if (other.CompareTag("Enemy"))
        {
            EnemyStatusBase enemyStat = other.GetComponent<EnemyStatusBase>();
            if (enemyStat != null)
            {
                FinalDanages(enemyStat, damage);
            }     
        }
    }

    private IEnumerator LifetimeCoroutine()
    {
        // 指定された寿命の時間だけ待機する
        yield return new WaitForSeconds(lifetime);

        if(explosionEffectPrefab != null)
        {
            // 弾を生成
            GameObject projectileObj = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

            projectileObj.transform.localScale = explosionEffectPrefab.transform.lossyScale * 2f;

            CameraShake.Instance.StartShake(0.4f, 0.5f, 0.5f, 30f);

            // 弾のスクリプトに溜めたパワーを渡す
            BlackHoleProjectile projectileScript = projectileObj.GetComponent<BlackHoleProjectile>();
            if (projectileScript != null)
            {
               projectileScript.Initialize(damage);
            }
            if(!SoundEffect.Instance.IsPlaying(explosionSound))
            {
               SoundEffect.Instance.Play(explosionSound);
            }
        }

        // プールが設定されていない場合の保険
        Destroy(gameObject);
    }

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
}