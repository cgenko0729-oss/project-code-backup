using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class AoeCircle : MonoBehaviour
{
    public GameObject AoeEffectObj;　　　// AOE発生後に再生するエフェクトのプレハブ
    public GameObject modelObj;

    public Transform outterCicleTran;   // 外側の円のTransform
    public Transform innerCircleTrans;  // 内側の円のTransform

    public float startScale = 0.1f;     // AOE開始時の円の大きさ
    public float endScale = 4.0f;       // AOE最大時の円の大きさ
    public float duration = 3f;         // 円が成長するまでの時間（秒）

    public SphereCollider col;        //当たり判定用のSphereCollider or OverlapSphereを使う

    public float aoeDamage;　　       // AOEが当たったときに与えるダメージ量

    public float controlScale = 1.0f;
    public bool isControlScale = false;

    public bool isDamageEnemy = true;
    public bool isDamagePlayer = true;  // プレイヤーにダメージを与えるかどうか
    public bool isSpawnEffect = true;  // AOEエフェクトを生成するかどうか

    public bool isFixedEffectRotation = false;
    public Vector3 effectRotation = Vector3.zero; // エフェクトの回転を固定する場合の回転値

    public bool hasModelObj = false;
    public bool isProjectileSignal = false;
    public bool noEffectModel = false;

    public bool hasSoundEffect = false;
    
    public SoundList finishSE;

    public bool isExpandNow = true;

    public ObjectPool aoeCirclePool;
    public bool isPooled = false;

    public bool isOnHitCamShake = false;

    public bool isBlockMove = false;
    public bool isBlockDash = false;
    public bool isBlockCast = false;


    private void OnEnable()
    {
        if (isPooled)
        {
            innerCircleTrans.DOKill();
            innerCircleTrans.localScale = new Vector3(startScale, startScale, startScale);　
            if(isExpandNow)innerCircleTrans.DOScale(endScale, duration).OnComplete(SpawnEffectCompleteObject);
            outterCicleTran.localScale = new Vector3(endScale, endScale, endScale);
            col = GetComponent<SphereCollider>();
            col.enabled = false;
        }
    }


    void Start()
    {
        　
        innerCircleTrans.localScale = new Vector3(startScale, startScale, startScale);　　　　　// 内側の円を startScale の大きさにリセット
        if(isExpandNow)innerCircleTrans.DOScale(endScale, duration).OnComplete(SpawnEffectCompleteObject);　　// DOTweenで内側の円を endScale まで duration 秒かけて拡大し、完了時にSpawnEffectCompleteObjectを呼ぶ

        outterCicleTran.localScale = new Vector3(endScale, endScale, endScale);　　　          // 外側の円は最初から最大サイズにしておく

        col = GetComponent<SphereCollider>();
        col.enabled = false;　　　　　　　　　　　// 初期状態では当たり判定を無効にしておく

    }

    void Update()
    {
       

    }

    public void StartExpandCircle()
    {
        innerCircleTrans.DOScale(endScale, duration).OnComplete(SpawnEffectCompleteObject);
    }

    public void AddOuterCircleSize(float addSize)
    {
        endScale += addSize;
        outterCicleTran.localScale = new Vector3(endScale, endScale, endScale);
    }

    public void InitCircle(float endScale, float duration, float aoeDamage)
    {
        isBlockMove = false;
        isBlockDash = false;
        isBlockCast = false;

        this.endScale = endScale;　　　　// AOE最大時の円の大きさを設定
        this.duration = duration;　　　　// 円が成長するまでの時間を設定
        this.aoeDamage = aoeDamage;　　　// AOEが当たったときに与えるダメージ量を設定

        innerCircleTrans.localScale = new Vector3(startScale, startScale, startScale);　　　　　// 内側の円を startScale の大きさにリセット
        innerCircleTrans.DOScale(endScale, duration).OnComplete(SpawnEffectCompleteObject);　　// DOTweenで内側の円を endScale まで duration 秒かけて拡大し、完了時にSpawnEffectCompleteObjectを呼ぶ

        outterCicleTran.localScale = new Vector3(endScale, endScale, endScale);　　　          // 外側の円は最初から最大サイズにしておく

        col = GetComponent<SphereCollider>();
        col.enabled = false;　　　　　　　　　　　// 初期状態では当たり判定を無効にしておく
    }

    public void InitCircle(float endScale, float duration, float aoeDamage, float controlScale)
    {
        this.endScale = endScale;　　　　// AOE最大時の円の大きさを設定
        this.duration = duration;　　　　// 円が成長するまでの時間を設定
        this.aoeDamage = aoeDamage;　　　// AOEが当たったときに与えるダメージ量を設定

        innerCircleTrans.localScale = new Vector3(startScale, startScale, startScale);　　　　　// 内側の円を startScale の大きさにリセット
        innerCircleTrans.DOScale(endScale, duration).OnComplete(SpawnEffectCompleteObject);　　// DOTweenで内側の円を endScale まで duration 秒かけて拡大し、完了時にSpawnEffectCompleteObjectを呼ぶ

        outterCicleTran.localScale = new Vector3(endScale, endScale, endScale);　　　          // 外側の円は最初から最大サイズにしておく

        col = GetComponent<SphereCollider>();
        col.enabled = false;　　　　　　　　　　　// 初期状態では当たり判定を無効にしておく

        this.controlScale = controlScale;  // コントロールスケールを設定
        isControlScale = true;

    }



    public void SpawnEffectCompleteObject()
    {

        col.enabled = true;　　　　　// 当たり判定を有効化
        col.radius = endScale/2;　　// 当たり判定範囲を円の半径に合わせる
        CheckHitWithPlayer();　　　　// プレイヤーや敵へのヒット判定を実行

        if (isSpawnEffect)
        {

            if(hasSoundEffect)SoundEffect.Instance.Play(finishSE);

            if (isProjectileSignal)
            {
                ProjectileData signalData = new ProjectileData();
                signalData.projectilePos = transform.position;    
                signalData.projectileSize = endScale;
                EventManager.EmitEventData("BossProjectileSignal", signalData);               
            }

            if (!noEffectModel)
            {

                GameObject effect = null;
                GameObject model = null;

                if (!isFixedEffectRotation)
                {
                    effect = Instantiate(AoeEffectObj, innerCircleTrans.position, Quaternion.identity);  // AOEエフェクトのプレハブを生成
                    Debug.Log("Effect Rotation: " + effect.transform.rotation.eulerAngles); // エフェクトの回転をログに出力
                }
                else
                {
                    effect = Instantiate(AoeEffectObj, innerCircleTrans.position, Quaternion.Euler(effectRotation));　 // AOEエフェクトのプレハブを生成（回転を固定する場合）
                    Debug.Log("Effect Rotation: " + effect.transform.rotation.eulerAngles); // エフェクトの回転をログに出力
                }
                if (hasModelObj)
                {
                    model = Instantiate(modelObj, innerCircleTrans.position, Quaternion.identity); // モデルオブジェクトを生成
                    ObjectMoverUtil modelMove = model.GetComponent<ObjectMoverUtil>(); // モデルオブジェクトの移動スクリプトを取得
                    modelMove.movePosStart = new Vector3(innerCircleTrans.position.x, -5.6f, innerCircleTrans.position.z);
                    modelMove.movePosEnd = new Vector3(innerCircleTrans.position.x, 0.0f, innerCircleTrans.position.z);
                }

                if (isControlScale)
                {
                    EffectScaleControlloer es = effect.GetComponent<EffectScaleControlloer>();　// エフェクトのスケールコントローラーを取得
                    es.effectScale = controlScale;
                }

            }
            
        }





        if (isPooled)
        {
            innerCircleTrans.localScale = new Vector3(0, 0, 0);　
            aoeCirclePool.Release(this.gameObject);
        }
        else Destroy(gameObject, 0.35f);　// 自分自身のオブジェクトを少し遅らせて破棄
    }



    public void UpdateTransform(Vector3 newTransform)
    {
        transform.position = newTransform;

    }

    //public void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player"))
    //    {
    //        Debug.Log("Hit Player");
    //        EventManager.EmitEventData("ChangePlayerHp", -10f);
    //    }
    //}

    public void CheckHitWithPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, endScale/2);　// 現在の位置を中心に、endScale/2 の半径で当たり判定
        foreach (var hitCollider in hitColliders)
        {

            if (hitCollider.CompareTag("Player") && isDamagePlayer)　// プレイヤーにヒットした場合
            {
                EventManager.EmitEventData("ChangePlayerHp", -aoeDamage); // イベントマネージャー経由でプレイヤーHPを aoeDamage だけ減らす

                if(isOnHitCamShake)CameraShake.Instance.StartShake();
            
                if (isBlockMove)
                {
                    EnemyManager.Instance.ApplyCannotMove();
                }
                else if (isBlockDash)
                {
                    EnemyManager.Instance.ApplyCannotDash();
                }
                else if (isBlockCast)
                {
                    EnemyManager.Instance.ApplyCannotCast();
                }

            }

            if (hitCollider.CompareTag("Enemy") && isDamageEnemy)　// 敵にヒットした場合
            {
                IDamageable damageable = hitCollider.GetComponent<IDamageable>();　// IDamageableインターフェイスを持っているかチェック
                if (damageable != null)
                {
                    damageable.TakeDamage(999);　// ダメージを与える
                }
            }


        }

    }

}

