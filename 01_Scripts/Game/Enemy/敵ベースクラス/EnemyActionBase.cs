using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;
using System.Security.Cryptography;            //Object Pool



/// <summary>  
/// 敵の基本的な行動を定義するクラス  
/// </summary>  
public class EnemyActionBase : MonoBehaviour
{
    [Header("Separation")]
    public float separateRadius = 0.5f; // 分離の半径  
    public float seperatePower = 14f;  // 分離の力  
    public LayerMask sperateLayer;     // 分離対象のレイヤー  
    public bool isSeperateEnabled = true; // 分離機能が有効かどうか  

    public const int maxNeighbors = 14;  // 最大隣接数
    Collider[] neighborsCols = new Collider[maxNeighbors]; // 隣接オブジェクトのCollider配列

    public int frameCounter; // フレームカウンター
    [Range(1, 5)] public int seperatePerN = 1; // 分離を計算するフレーム数の間隔

    public float fixedY;
    public float r2, invR2;

    [Header("Homing")]
    public float moveSpd = 3f; // 移動速度  
    public Transform playerTrans; // プレイヤーのTransform  
    public bool isSimpleHoming = true; // シンプルなホーミングが有効かどうか  

    public Vector2 move2D;
    public Vector2 self;
    public float distToPlayer; // プレイヤーとの距離  
    public float distNeedToHoming = 28f; // ホーミングが必要な距離  
    public float deltaX;
    public float deltaZ;
    public float distDelta;

    public Vector3 targetPoint = Vector3.zero;

    public Vector3 dirToPlayer= Vector3.zero;

    protected EnemyStatusBase enemyStatus;

    public float distTooFarToHoming = 42f;

    //public bool isPoisonDebuff = false; // スピードダウンのフラグ
    //public float poisonSpeedDownFactor = 1f; // スピードダウンの倍率
    //public float poisonSpeedDownDuration = 5f; // スピードダウンの持続時間

    //public bool isIceDebuff = false; // 氷のデバフフラグ
    //public float iceDebuffFactor = 1f; // 氷のデバフ倍率
    //public float iceDebuffDuration = 5f; // 氷のデバフ持続時間

    public Animation anim;
    public AnimationState currentAnimState;
    public Animator animator;
    public bool isUsingAnimationData = true;
    public bool isCustomAnimSpeed = false;
    public float customAnimSpeed = 0.35f;

    public bool isResetRotation = true;

    private void Awake()
    {
        
        enemyStatus = GetComponent<EnemyStatusBase>(); // EnemyStatusBaseコンポーネントを取得

        if (isUsingAnimationData)
        {
            anim = GetComponent<Animation>();
            currentAnimState = anim[anim.clip.name];
        }
        else
        {
            animator = GetComponent<Animator>();
        }
        

        if (isCustomAnimSpeed)
        {
            //isCustomAnimSpeed = false;
            currentAnimState.speed = customAnimSpeed;
        }

    
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
       
    }

    protected void InitSeperationInfo()
    {
        fixedY = transform.position.y; // 固定Y座標を設定  
        r2 = separateRadius * separateRadius; // 半径の二乗を計算  
        invR2 = 1f / r2; // 半径二乗の逆数を計算  
    }

    protected void GetPlayerInfo()
    {
        var p = GameObject.FindWithTag("Player"); // プレイヤーをタグで検索  
        if (p) playerTrans = p.transform; // プレイヤーのTransformを取得  
    }

    public void CalDirToPlayer()
    {
        

    }

    public void UpdateDistToPlayerInXZ()
    {
        Vector2 player = new Vector2(playerTrans.position.x, playerTrans.position.z); // プレイヤーのXZ座標  
        self = new Vector2(transform.position.x, transform.position.z); // 自分のXZ座標  

        //distToPlayer = Vector2.Distance(self, player); // プレイヤーとの距離を計算
        
        distToPlayer = Mathf.Sqrt(distDelta);

        if (distToPlayer > distTooFarToHoming) enemyStatus.DeadNoExp(false);

    }

    public void EnemySimpleHoming()
    {      
        transform.position = Vector3.MoveTowards(transform.position, playerTrans.position, enemyStatus.enemyMoveSpd * Time.deltaTime * enemyStatus.poisonSpeedDownFactor * enemyStatus.iceDebuffFactor); // プレイヤーの位置に向かって移動
   
        // self = new Vector2(transform.position.x, transform.position.z);
        //Vector2 target = new Vector2(playerTrans.position.x, playerTrans.position.z); // ターゲットのXZ座標
        //Vector2 toTarget = (target - self).normalized; // ターゲットへの方向を計算
        //move2D = toTarget * enemyStatus.enemyMoveSpd * Time.deltaTime * speedDownFactor; // 移動ベクトルを計算

    }

    public void EnemyExplodeHoming()
    {      
        transform.position = Vector3.MoveTowards(transform.position, playerTrans.position, enemyStatus.enemyMoveSpd * 1.42f * Time.deltaTime * enemyStatus.poisonSpeedDownFactor * enemyStatus.iceDebuffFactor); // プレイヤーの位置に向かって移動
   
    }

    public void EnemyFollowTargetPoint()
    {
        self = new Vector2(transform.position.x, transform.position.z);
        Vector2 target = new Vector2(targetPoint.x, targetPoint.z); // ターゲットのXZ座標
        Vector2 toTarget = (target - self).normalized; // ターゲットへの方向を計算
        
        //distToPlayer = Vector2.Distance(self, target); // ターゲットとの距離を計算
        move2D = toTarget * enemyStatus.enemyMoveSpd * Time.deltaTime * enemyStatus.iceDebuffFactor * enemyStatus.poisonSpeedDownFactor*enemyStatus.SpdDownRate; // 移動ベクトルを計算

    }

    //public void UpdateSpeedDebuff()
    //{
    //    poisonSpeedDownDuration -= Time.deltaTime; // スピードダウンの持続時間を減少

    //    if (isPoisonDebuff && poisonSpeedDownDuration <=0)
    //    {
    //        isPoisonDebuff = false; // スピードダウンを無効化
    //        poisonSpeedDownFactor = 1f; // スピードダウンの倍率をリセット
    //    }
    //}

    //public void UpdateIceDebuff()
    //{
    //    if (isIceDebuff)
    //    {
    //        iceDebuffDuration -= Time.deltaTime; // 氷のデバフの持続時間を減少
    //        if (iceDebuffDuration <= 0f)
    //        {
    //            isIceDebuff = false; // 氷のデバフを無効化
    //            iceDebuffFactor = 1f; // 氷のデバフ倍率をリセット
    //            isSeperateEnabled = true;
    //            enemyStatus.ResetColor();
    //            ResetAnimSpeed();
    //        }
    //    }
    //}

    public void ResetAnimSpeed()
    {
        if (isUsingAnimationData)
        {
            if (isCustomAnimSpeed)
            {
                //isCustomAnimSpeed = false;
                currentAnimState.speed = customAnimSpeed;
            }
            else
            {
                currentAnimState.speed = 1f;
            }
        }
        else
        {
            animator.speed = 1f;
        }
        
        
    }

    public void SetAnimSpeed(float spdVal)
    {
        if(isUsingAnimationData)currentAnimState.speed = spdVal;
        else animator.speed = spdVal;

    }

    //public void ApplySpeedDownDebuff(float speedDownVal,float speedDowDuration)
    //{
    //    isPoisonDebuff = true;
    //    poisonSpeedDownDuration = speedDowDuration; // スピードダウンの持続時間をリセット
    //    if(poisonSpeedDownFactor > speedDownVal) poisonSpeedDownFactor = speedDownVal; // スピードダウンの倍率を設定（現在の値より小さい場合のみ適用）
    //}

    //public void ApplyIceDebuff()
    //{
    //    isIceDebuff = true;
    //    iceDebuffDuration = 3f;
    //    iceDebuffFactor = 0f; // 氷のデバフ倍率を0に設定（移動不可状態）
    //    isSeperateEnabled = false;
    //    enemyStatus.SetIceDebuffColor();
    //    SetAnimSpeed(0);
    //}

    public void EnemyFollow()
    {
        if (!playerTrans) return; // プレイヤーがいない場合は終了  
        if (!isSimpleHoming) return; // ホーミングが無効なら終了  

        self = new Vector2(transform.position.x, transform.position.z); // 自分のXZ座標  
        Vector2 player = new Vector2(playerTrans.position.x, playerTrans.position.z); // プレイヤーのXZ座標  
        Vector2 toPlayer = (player - self).normalized; // プレイヤーへの方向を計算  
        move2D = toPlayer * enemyStatus.enemyMoveSpd * enemyStatus.poisonSpeedDownFactor * enemyStatus.iceDebuffFactor * enemyStatus.SpdDownRate * Time.deltaTime; // 移動ベクトルを計算  
        


    }

    public void EnemyRotation()
    {
        //if (enemyStatus.isIceDebuff) return; // 氷のデバフ中は回転しない

        //Vector3 dir = (new Vector2(playerTrans.position.x, playerTrans.position.z) - new Vector2(transform.position.x, transform.position.z)).normalized; // プレイヤーへの方向  
        //Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y)); // 回転方向を計算  
        //if (dir != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime); // スムーズに回転  

        if (!enemyStatus.canEnemyRotate) return;

        deltaX = playerTrans.position.x - transform.position.x;
        deltaZ = playerTrans.position.z - transform.position.z;
        distDelta = deltaX * deltaX + deltaZ * deltaZ;
        
        // Early exit for very close distances (avoid jittering)
        if (distDelta < 0.01f) return;
        
        // Direct Y rotation calculation (much faster than LookRotation)
        float targetY = Mathf.Atan2(deltaX, deltaZ) * Mathf.Rad2Deg;
        float currentY = transform.eulerAngles.y;
        
        // Skip tiny rotations (avoid unnecessary updates)
        float angleDifference = Mathf.DeltaAngle(currentY, targetY);
        if (Mathf.Abs(angleDifference) < 0.1f) return;
        
        // Y-only interpolation (much faster than Quaternion.Slerp)
        float newY = Mathf.LerpAngle(currentY, targetY, enemyStatus.poisonSpeedDownFactor * enemyStatus.iceDebuffFactor * 3.5f * Time.deltaTime);

        //transform.eulerAngles = new Vector3(0f, newY, 0f); // Y軸のみ回転を更新 caches it to improve performance
        transform.rotation = Quaternion.Euler(0f, newY, 0f);

    }

    public void EnemyRotationFaceTarget(Vector3 faceTarget)
    {
        Vector3 flatDiff = new Vector3(faceTarget.x - transform.position.x, 0f, faceTarget.z - transform.position.z); // ターゲットとの平面上の差分  
        if (flatDiff.sqrMagnitude < Mathf.Epsilon) return; // 差分が小さい場合は終了  
        Quaternion lookRot = Quaternion.LookRotation(flatDiff.normalized); // 回転方向を計算  
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime); // スムーズに回転  

    }

    public void EnemyRotationTargetPoint()
    {
        Vector3 flatDiff = new Vector3(targetPoint.x - transform.position.x,0f,targetPoint.z - transform.position.z);
        if (flatDiff.sqrMagnitude < Mathf.Epsilon) return;
        Quaternion lookRot = Quaternion.LookRotation(flatDiff.normalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime);

    }

    public void FaceTargetPointNow()
    {
        Vector3 direction = (targetPoint - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation;

    }

    public void EnemySeparation()
    {
        if (!isSeperateEnabled) return; // 分離が無効なら終了  

        frameCounter = (frameCounter + 1) % seperatePerN; // フレームカウンターを更新  
        if (frameCounter == 0) move2D += ComputeSeparationXZ(self) * Time.deltaTime; // 分離ベクトルを加算  
        Vector3 next = new Vector3(self.x + move2D.x, fixedY, self.y + move2D.y); // 次の位置を計算  
        transform.position = next; // 位置を更新  
    }

    public Vector2 ComputeSeparationXZ(Vector2 me)
    {
        Vector2 sep = Vector2.zero; // 分離ベクトルを初期化  
        int count = Physics.OverlapSphereNonAlloc(new Vector3(me.x, fixedY, me.y), separateRadius, neighborsCols, sperateLayer, QueryTriggerInteraction.Ignore); // 周囲のオブジェクトを取得  

        for (int i = 0; i < count; i++) {
            var c = neighborsCols[i];
            if (c.gameObject == gameObject) continue; // 自分自身を無視  
            Vector3 cp = c.transform.position;
            Vector2 other = new Vector2(cp.x, cp.z);
            Vector2 off = me - other; // 他のオブジェクトとのオフセット  
            float d2 = off.sqrMagnitude; // 距離の二乗を計算  
            if (d2 > 0f && d2 < r2) {
                float factor = (r2 - d2) * invR2; // 距離に応じた係数を計算  
                sep += off * (factor * seperatePower * 0.5f); // 分離ベクトルを加算  
            }
        }

        return sep; // 分離ベクトルを返す  
    }
}

