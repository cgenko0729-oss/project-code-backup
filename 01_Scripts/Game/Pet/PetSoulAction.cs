using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TigerForge;               //EventManager
using UnityEngine;

public class PetSoulAction : MonoBehaviour, IHittable
{
    [Header("アニメーション設定")]
    [Tooltip("最初にフワッと浮き上がる高さ")]
    [SerializeField] private float floatHeight = 1.5f;

    [Tooltip("浮き上がるのにかかる時間")]
    [SerializeField] private float floatDuration = 0.8f;

    [Tooltip("ホーミング移動速度(加減速)")]
    [SerializeField] private float homingMoveSpeed = 0.2f;

    [Tooltip("魂のトレイル")]
    [SerializeField] private GameObject soulTrailObj;

    // 移動速度
    private float movespeed = 0f;

    // ホーミング移動フラグ
    private bool soulHoming =false;

    // プレイヤーのTransformを保持する変数
    private Transform playerTransform;

    // トレイルパーティクルのインスタンス
    private GameObject soulTrailInstance;

    public PetType petType;

    void Start()
    {
        float ScaleDuration = 1.0f;

        //一応初期化
        movespeed = 0f;
        soulHoming = false;

        //スケールアップアニメーション
        transform.localScale = Vector3.zero;

        Vector3 Scale = new Vector3(1.0f, 1.0f, 1.0f);

        transform.DOScale(Scale, ScaleDuration).SetEase(Ease.OutBack);

        SoundEffect.Instance.Play(SoundList.SoulDropSe);

        if (soulTrailObj != null)
        {   
            soulTrailInstance = Instantiate(soulTrailObj, transform.position, Quaternion.identity);
        }

        // プレイヤーのTransformを一度だけ探して、記憶しておく
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Destroy(this.gameObject, floatDuration);
            return;
        }
 
        PlaySoulAnimation();
    }

    private void Update()
    {
        // トレイルパーティクルの位置と回転を魂に追従させる
        if (soulTrailInstance != null)
        {
            soulTrailInstance.transform.position = transform.position;
            soulTrailInstance.transform.rotation = transform.rotation;
        }

        if (!soulHoming) return;

        // 移動速度を徐々に上げる
        movespeed += homingMoveSpeed * Time.deltaTime;

        // ホーミング動作を実行
        Homing(playerTransform, movespeed);
    }

    private void PlaySoulAnimation()
    {
        //DOTweenのシーケンスを作成
        Sequence mySequence = DOTween.Sequence();

        //最初に少し上に浮かび上がるアニメーション
        mySequence.Append(transform.DOMoveY(floatHeight, floatDuration).SetRelative().SetEase(Ease.OutQuad));

        // アニメーション完了時のコールバックを設定
        mySequence.OnComplete(() =>
        {
            // ホーミング移動を開始
            soulHoming = true;
        });
    }

    //Dotweenアニメーションを確実に停止させる
    private void OnDestroy()
    {
        transform.DOKill();    
    }

    private void Homing(Transform target, float moveSpeed)
    {
        // プレイヤーのTransformがnullでないことを確認
        if (target == null) return;

        Vector3 targetPosition = Vector3.zero;

       
        if (target == playerTransform)
        {
            // フォーメーションオフセットを考慮した目標位置を計算
            targetPosition = target.position;
        }

        // ターゲットとの「地面だけの距離」を計算する
        Vector3 targetPositionOnGround = targetPosition;
        Vector3 myPositionOnGround = transform.position;
        targetPositionOnGround.y = 0;
        myPositionOnGround.y = 0;
        float groundDistance = Vector3.Distance(myPositionOnGround, targetPositionOnGround);

        //プレイヤーの方向を計算
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0;

        direction = direction.normalized;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

        //回転を一度取得して修正後に再設定
        Vector3 fixedEulerAngles = transform.rotation.eulerAngles;
        fixedEulerAngles.x = 0; // X軸の回転を固定
        fixedEulerAngles.z = 0; // Z軸の回転を固定
        transform.rotation = Quaternion.Euler(fixedEulerAngles);

        // プレイヤーの方向に向かって移動
        transform.position += direction * Time.deltaTime * moveSpeed; // 移動速度は調整可能
    }

    public void OnHit()
    {
        if (!soulHoming) return;

        SoundEffect.Instance.Play(SoundList.SoulGetSe);

        if (soulTrailInstance != null)
        {
            StartCoroutine(LifetimeCoroutine());
        }

        // イベントを発行してペット獲得を通知
        EventManager.EmitEvent(GameEvent.PetGet);


        //煙エフェクト再生
        ItemManager.Instance.PlayTimeParticleInPlayer(ItemManager.Instance.itemKeepTimePs.PetGetPs,
                                                      ItemManager.Instance.defaultEffectPos+new Vector3(0,0.5f,0));

        //消える
        Destroy(this.gameObject);
        PetGetMessageSpawner.Instance.SpawnPetMessagObj(petType);
    }

    private IEnumerator LifetimeCoroutine()
    {
        float lifetime = 1.5f; // トレイルパーティクルの寿命（秒）

        // 指定された寿命の時間だけ待機する
        yield return new WaitForSeconds(lifetime);

        // トレイルパーティクルを消す
        Destroy(soulTrailInstance);     
    }
}
