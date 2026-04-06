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
using HighlightPlus;

public abstract class BossActionBase : MonoBehaviour
{

    public bool hasHpBar = true;
    public GameObject bossHpBar; // ボスのHPバーオブジェクト
    public Animator animator;
    public HighlightEffect highlightEffect;
    public EnemyStatusBase enemyStatus;
    
    public Transform playerTrans; // プレイヤーのTransform
    public float distToPlayer; // プレイヤーとの距離
    public Vector3 dirToPlayer;
    public Vector3 playerForwardVector;

    public float moveSpeed = 2.8f; // 移動速度
    public float moveSpeedNormal = 2.8f;

    public Vector3 originalScale = Vector3.one;

    public int currentAtttackPattern = 0;
    public float nextMoveCnt = 3.5f;

    public bool isDead = false;
    public bool isPhrase2 = false;

    //AttackTween//
    public Vector3 punchRotationAmount = new Vector3(35f, 0f, 0f); //30 about X
    public float punchDuration        = 0.4f;
    public int   punchVibrato         = 4;   
    public float punchElasticity      = 0.2f;

    public int bossPhaseId = 1; //ボスのフェーズ識別用
    public bool isDynamicBossHp = false; //ボスのHPバーが動的に変化するかどうか midBoss1 = 1, midBoss2 = 2, finalBoss = 3

    protected void InitBoss()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform; 
        animator = GetComponent<Animator>();
        highlightEffect = GetComponent<HighlightEffect>();
        enemyStatus = GetComponent<EnemyStatusBase>();
        
        originalScale = transform.localScale;

        if(isDynamicBossHp)
        {
            enemyStatus.enemyMaxHp = EnemyManager.Instance.GetMaxHpForEnemy(EnemyType.Boss, bossPhaseId);
            enemyStatus.enemyHp = enemyStatus.enemyMaxHp;
        }


        if (hasHpBar)
        {
            BossHpBar hpbar = bossHpBar.GetComponentInChildren<BossHpBar>();
            hpbar.SetBossTargetObj(this.gameObject);
            bossHpBar.SetActive(true);
        }
        

    
    }

    protected void UpdatePlayerInfo()
    {
        dirToPlayer = (playerTrans.position - transform.position).normalized; // プレイヤーの方向を計算
        distToPlayer = Vector3.Distance(transform.position, playerTrans.position); // プレイヤーとの距離を計算
        playerForwardVector = playerTrans.forward; // プレイヤーの前方ベクトルを取得
    }

     public void HomingPlayer()
    {
        Vector3 direction = (playerTrans.position - transform.position).normalized;
        transform.position += direction * Time.deltaTime * moveSpeed; // 移動速度は調整可能
        
    }

    public void RotateTowardTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - transform.position;
        direction.y = 0; // 水平方向のみに回転
        if (direction == Vector3.zero) return; // 方向がゼロベクトルの場合は回転しない
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f); // 回転速度は調整可能

    }

    public void RotateTowardPlayer(float rotateSpeed = 5f)
    {
        if (playerTrans == null) return;
        Vector3 direction = playerTrans.position - transform.position;
        direction.y = 0;
        if (direction == Vector3.zero) return;
        Quaternion targetLookRotation = Quaternion.LookRotation(-direction);
        float targetYAngle = targetLookRotation.eulerAngles.y;
        Quaternion finalTargetRotation = Quaternion.Euler(-180f, targetYAngle, 180f);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalTargetRotation, Time.deltaTime * rotateSpeed);

    }

    public void FacePlayerNow()
    {
        if (playerTrans == null) return;
        Vector3 direction = (playerTrans.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation; // プレイヤーの方向を向く

    }

    public void FaceTargetNow(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation; 

    }

    void DoAttackAniTween()
    {
       transform.DOPunchRotation(punchRotationAmount,punchDuration,vibrato: punchVibrato,elasticity: punchElasticity).SetEase(Ease.OutQuad);
    }

    public void SetInnerGlow(float targetVal = 1.4f, float duration =0.28f, int loopTimes = 6)
    {
        highlightEffect.innerGlow = 0.1f; // 初期値を設定
        DOTween.To(() => highlightEffect.innerGlow, x => highlightEffect.innerGlow = x, targetVal, duration)
            .SetLoops(loopTimes, LoopType.Yoyo) // 指定回数往復
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                highlightEffect.innerGlow = 0.1f;
            });
 
    }


}

