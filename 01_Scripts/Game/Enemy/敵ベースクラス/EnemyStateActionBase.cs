using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class EnemyStateActionBase : EnemyActionBase　// 敵キャラの状態遷移や共通処理をまとめた基底クラス
{

    public enum EnemyState　// 敵の各状態を表す列挙型
    {
        Idle,
        Move,
        Wander,
        Chase,
        RunAway,
        Attack,
        SkillAttack,
        GetHit,
        Dead,
    }

    public float stateCnt  = 0f;                    // 各状態の継続時間などをカウントするタイマー
    public StateMachine<EnemyState> stateMachine;　　// 状態遷移を管理するステートマシン
    public int stateInfo = 0;                       // 現在の状態番号（enum を int に変換したもの）

    //public Animator animator;                       // Animator コンポーネント（アニメーション制御用）
    public Vector3 playerPos;                       // プレイヤーの座標（Y固定版などで使用）
    public Vector3 faceVector;                      // 向きを決める基準点（例：次に顔を向けたいポイント）

    protected virtual void Awake()
    {
        
    }

    protected virtual void Start()
    {
        
    }

    protected virtual void Update()
    {
        

    }

    public void GetAnimator()　// Animator コンポーネントを取得し
    {
        animator = GetComponent<Animator>();
        if (animator == null) Debug.LogError("Animatorない" + gameObject.name);
        
    }
    
    public void GetStateInfo()　// ステートマシンの現在状態を int 型で取得して stateInfo に代入
    {
        stateInfo = (int)stateMachine.State;
    }

    public void UpdateStateCnt()　// stateCnt を経過時間分だけ減算（タイマー処理）
    {
        stateCnt -= Time.deltaTime * enemyStatus.iceDebuffFactor * enemyStatus.poisonSpeedDownFactor;
    }

    public void CalDistToPlayer()　// プレイヤーまでの距離を計算し、親クラスの distToPlayer に代入
    {
        distToPlayer = Vector3.Distance(playerTrans.transform.position, transform.position);
    }

    public void CalPlayerPos()　// プレイヤーの座標を取得し、Y座標を固定して playerPos に格納
    {
        playerPos = new Vector3(playerTrans.position.x, fixedY, playerTrans.position.z);

    }

     public void EnemyFaceAwayPlayer()　// プレイヤーから背を向けるように敵を回転させる
    {

        Vector3 dir = (new Vector2(playerTrans.position.x, playerTrans.position.z) - new Vector2(transform.position.x, transform.position.z)).normalized;　// XZ平面のみで方向ベクトルを計算
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(-dir.x, 0, -dir.y));　　// 反対向きの回転を作成
        if (dir != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime);　 // dir がゼロベクトルでない場合、滑らかに回転
    
    }

    public void EnemyFaceForward(bool isOpposite = true)　// faceVector の方向に顔を向ける（isOpposite=false で正面向き、true で背面向き）
    {
        
        Vector3 dir = new Vector3(faceVector.x - transform.position.x, 0f,faceVector.z - transform.position.z).normalized;    　 // 目標位置との方向ベクトルを計算（Y成分は無視）    
        Quaternion lookRot = Quaternion.identity;
        if (!isOpposite) lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y));
        else lookRot = Quaternion.LookRotation(new Vector3(-dir.x, 0, -dir.y));
        if (dir != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime);　// dir がゼロベクトルでない場合、滑らかに回転



    }

    public void EnemyFaceTargetPoint(Vector3 targetPoint)
    {
        Vector3 dir = new Vector3(targetPoint.x - transform.position.x, 0f, targetPoint.z - transform.position.z).normalized; // 目標位置との方向ベクトルを計算（Y成分は無視）
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y));
        if (dir != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime); // dir がゼロベクトルでない場合、滑らかに回転

    }


}

