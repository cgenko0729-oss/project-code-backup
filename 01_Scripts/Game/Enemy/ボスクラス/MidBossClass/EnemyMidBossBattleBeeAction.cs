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
using UnityEditor;
using System.Threading;
using System;

public class EnemyMidBossBattleBeeAction : MonoBehaviour
{
   
     public enum BossState
    {
        Idle,
        Move,
        Dizzy,
        Death,  
        Attack,
        ReturnCenter,
    }

    public float stateCnt;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<BossState> stateMachine;　　// 状態遷移を管理するステートマシン
    public string stateInfoText; // 状態番号を文字列に変換するためのテキスト

    public GameObject bossHpBar; // ボスのHPバーオブジェクト
    private Animator animator;
    HighlightEffect highlightEffect;
    EnemyStatusBase enemyStatus;

    Transform playerTrans; // プレイヤーのTransform
    public float distToPlayer; // プレイヤーとの距離
    public Vector3 dirToPlayer; 

    public bool isDead = false;
    public bool isPhrase2 = false;

    public float moveSpeed = 2.8f; // 移動速度
    public float moveSpeedNormal = 2.8f;
    public float moveCntNext = 3.5f;

    public Vector3 originalScale = Vector3.one;

    //AttackTween//
    public Vector3 punchRotationAmount = new Vector3(-35f, 0f, 0f); //30 about X
    public float punchDuration        = 0.4f;
    public int   punchVibrato         = 4;   
    public float punchElasticity      = 0.2f;

    public int currentAtttackPattern = 0;
    public Vector3 centerPos = Vector3.zero;
    public float distToCenter = 0f; // ボスの中心位置からの距離
    public float distNeedToReturnCenter = 17.7f;
    public bool isReactivate = false;
    public float reactivateCnt = 4f;

    private Tween animTween;

    public GameObject beeBoneObject;

    public AudioClip attackSe;



    private void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform; 

        animator = GetComponent<Animator>();
        highlightEffect = GetComponent<HighlightEffect>();
        enemyStatus = GetComponent<EnemyStatusBase>();

        stateMachine = StateMachine<BossState>.Initialize(this);
        stateMachine.ChangeState(BossState.Move);

        originalScale = transform.localScale;
        if(bossHpBar)bossHpBar.SetActive(true);

        centerPos = transform.position; // ボスの中心位置を初期化
    }

    private void Update()
    {
        UpdateStateInfo();
        UpdatePlayerInfo();
        distToCenter = Vector3.Distance(transform.position, centerPos); // ボスの中心位置からの距離を計算

        ////if press F
        //if(Input.GetKeyDown(KeyCode.F)) {
        //    EnemyAnimUtil.PunchRotation(transform, punchRotationAmount, punchDuration, punchVibrato, punchElasticity).Play();
        //}

        ////if press g
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    EnemyAnimUtil.LookAround(transform, lookAngle: 50f, timeToEdge: 0.25f, rotations: 2).Play();
        //}

        ////if press h
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    EnemyAnimUtil.BounceUpDown(transform, 0.1f, 2.0f).Play();
        //}

    }


    void Move_Enter()
    {
        stateCnt = 7f;

    }

    void Move_Update()
    {
        RotateTowardPlayer();
        HomingPlayer();

        if(stateCnt <= 0 && distToPlayer < 11f)
        {
            //if (distToCenter > distNeedToReturnCenter) ChangeToState(BossState.ReturnCenter);
            stateMachine.ChangeState(BossState.Attack);
        }

    }

    void ReturnCenter_Enter()
    {
        stateCnt = 10f;
    }

    void ReturnCenter_Update()
    {
                 
        Vector3 direction = (centerPos - transform.position).normalized;           
        transform.position += direction * moveSpeed * Time.deltaTime * 2.1f; // 中心に向かって移動

        RotateTowardTarget(centerPos);

        if(distToCenter < 2.1f)
        {
            ChangeToState(BossState.Move);
        }

        enemyStatus.enemyHp += Time.deltaTime * 15f; // 中心に戻る間にHPを回復


    }

    void Attack_Enter()
    {
        //stateCnt = 4f;
        SetInnerGlow();
        AttackSequenceAsync();
        
    }

    private async void AttackSequenceAsync()
    {

        try
        {
            FacePlayerNow(); 
            await UniTask.Delay(TimeSpan.FromSeconds(2.8f), cancellationToken: this.GetCancellationTokenOnDestroy());

            for (int i = 0; i < 3; i++)
            {
                FacePlayerNow();
                DoAttackAniTween(); 
                ShotBeeBone();     
                //EazySoundManager.PlaySound(someShootSound);
              
                if (i < 2) 
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(1.5f), cancellationToken: this.GetCancellationTokenOnDestroy()); // 3. Wait for 1 second between shots, but NOT after the last shot.
                }
            }

                await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: this.GetCancellationTokenOnDestroy());
                stateMachine.ChangeState(BossState.Move);
        }
        catch(OperationCanceledException)
        {

        }
        
    }

    public void ShotBeeBone()
    {
        Quaternion beeBoneRotation = Quaternion.LookRotation(playerTrans.position - transform.position);
        GameObject bone = Instantiate(beeBoneObject, transform.position, beeBoneRotation);
        SkillForwardMove boneMove = bone.GetComponent<SkillForwardMove>();
        boneMove.moveVec = (playerTrans.position - transform.position).normalized * 7.9f;

        SoundEffect.Instance.PlayOneSound(attackSe, 0.77f);
    }

    void Attack_Update()
    {


        RotateTowardPlayer();
        


        //if(stateCnt <= 0)
        //{
        //    stateMachine.ChangeState(BossState.Move);
        //}

    }

    void Dizzy_Enter()
    {

    }

    void Dizzy_Update()
    {

    }

    void Death_Enter()
    {

    }

    void Death_Update()
    {
       
    }


    //==========================Utility&Helper===================================//
    public void ChangeToState(BossState newState)
    {
        stateMachine.ChangeState(newState);
        stateInfoText = stateMachine.State.ToString(); // 状態名を文字列に変換
    }

    public void ChangeToMoveState() => stateMachine.ChangeState(BossState.Move);

    public void UpdateStateInfo()
    {
        stateInfoText = stateMachine.State.ToString();
        stateCnt -= Time.deltaTime;
    }

    public void UpdatePlayerInfo() 
    {
        dirToPlayer = (playerTrans.position - transform.position).normalized; // プレイヤーの方向を計算
        distToPlayer = Vector3.Distance(transform.position, playerTrans.position); // プレイヤーとの距離を計算

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

    public void RotateTowardPlayer()
    {
        if (playerTrans == null) return;
        Vector3 direction = playerTrans.position - transform.position;
        direction.y = 0;
        if (direction == Vector3.zero) return;
        Quaternion targetLookRotation = Quaternion.LookRotation(-direction);
        float targetYAngle = targetLookRotation.eulerAngles.y;
        Quaternion finalTargetRotation = Quaternion.Euler(-180f, targetYAngle, 180f);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalTargetRotation, Time.deltaTime * 5f);

    }

    public void FacePlayerNow()
    {
        if (playerTrans == null) return;
        Vector3 direction = (playerTrans.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation; // プレイヤーの方向を向く

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

