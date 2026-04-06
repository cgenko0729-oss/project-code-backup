using DG.Tweening;
using QFSW.MOP2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonPillarAction : MonoBehaviour
{
    [Header("弾の寿命（秒）")]
    [SerializeField] private float lifetime = 15.0f;

    [Header("落下設定")]
    [SerializeField] private float fallDuration = 0.5f;

    [Header("スキルの撃つ位置")]
    public GameObject skillPoint;

    [Header("煙エフェクト")]
    public GameObject dastObject;

    [Header("魔法刃のプレハブプール")]
    [SerializeField] private ObjectPool magicBladePrefabPool;

    [Header("魔法刃の攻撃設定")]
    [SerializeField] private int projectilesPerVolley = 8;   //1回の斉射で発射する弾の数
    [SerializeField] private float spreadAngle =　45f;       //扇形の合計角度
    [SerializeField] private float interval = 3f;
    [SerializeField] private float magicBladeSpeed = 8f;

    [Header("魔法刃攻撃のSE")]
    [SerializeField] private SoundList magicBladeSound;

    private float damage = 0f;

    private ObjectPool myPool; // オブジェクトプール

    private Coroutine lifetimeCoroutine;
    private Coroutine SpawnMagicBladeCoroutine;

    public void Initialize(float totalDamage)
    {
        this.damage = totalDamage;
    }

    public void SetPool(ObjectPool pool)
    {
        myPool = pool;
    }

    private IEnumerator LifetimeCoroutine()
    {
        float finalLifetime = lifetime * ActivePetManager.Instance.PetSkillDuration;

        // 指定された寿命の時間だけ待機する
        yield return new WaitForSeconds(finalLifetime);

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

    private void OnEnable()
    {
        lifetimeCoroutine = StartCoroutine(LifetimeCoroutine());

        //落下アニメーションを開始する
        transform.DOMoveY(0, fallDuration).SetEase(Ease.InQuad) .OnComplete(() => 
        {
           StartCoroutine(SpawnMagicBlade());
        });
    }

    private void OnDisable()
    {
        dastObject.SetActive(false); //煙エフェクトを無効にする
        this.transform.DOKill();
        this.StopAllCoroutines();
    }

    private IEnumerator SpawnMagicBlade()
    {
        dastObject.SetActive(true); //煙エフェクトを有効にする

        while (true)
        {
            Vector3 baseDirection = skillPoint.transform.position.normalized;
            baseDirection.y = 0;

            for (int i = 0; i < projectilesPerVolley; i++)
            {
                float angle = 0f;
                if (projectilesPerVolley > 1)
                {
                    float angleStep = spreadAngle * (projectilesPerVolley -1);
                    angle = angleStep * i;
                }

                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                Vector3 fireDirection = rotation * baseDirection;

                GameObject skill = magicBladePrefabPool.GetObject();
                skill.transform.position = skillPoint.transform.position;
                skill.transform.rotation = Quaternion.LookRotation(fireDirection);

                PetSkillData proj = skill.GetComponent<PetSkillData>();
                if (proj != null)
                {
                    proj.SetPool(magicBladePrefabPool);
                    // 元々の初期化処理
                    proj.Initialize(this.damage, this.gameObject);
                    proj.SetDirection(fireDirection);
                    proj.speed = magicBladeSpeed;
                }
            }
            // SEを再生する
            if (!SoundEffect.Instance.IsPlaying(magicBladeSound))
            {
                SoundEffect.Instance.Play(magicBladeSound);
            }
            yield return new WaitForSeconds(interval);
        }
    }
}
