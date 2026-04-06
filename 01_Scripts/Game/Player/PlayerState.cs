using DG.Tweening;
using MonsterLove.StateMachine;
using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using System.ComponentModel; //StateMachine
using TigerForge;               //EventManager
using TMPro;    
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class PlayerState : MonoBehaviour
{
    [Header("基本的なステータス")]
    private float maxHp = 0;        // 最大HP
    private float moveSpeed = 0;    // 移動スピード
    private float dashSpeed = 0;    // ダッシュスピード
    private float dashDuration = 0; // ダッシュ継続時間(秒)
    private float dashCoolDown = 0; // ダッシュクールダウン(秒)

    [Header("現在のキャラのステータス")]
    [Tooltip("現在のHP")]
    [SerializeField]private float nowHp = 0;
    [Tooltip("生存フラグ")]
    [SerializeField] private bool isAliveFlg = true;
    [Tooltip("現在のレベル")]
    [SerializeField] private float nowLv = 1;
    [Tooltip("現在の経験値")]
    public float nowExp = 0;
    [Tooltip("次のレベルに必要な経験値量")]
    public float nextLvExp = 0;
    [Tooltip("無敵時間の残り時間(秒)")]
    public float invincibleCnt = 0;
    [Tooltip("無敵状態中かどうかのフラグ")]
    public bool isInvincible = false;
    [Tooltip("ダッシュを発動できるまでの残り時間(秒)")]
    private float dashCoolDownCnt = 0;

    [Header("エフェクト関連")]
    [Tooltip("無敵時に有効化するハイライトエフェクトのコンポーネント")]
    [SerializeField] private Behaviour highlightEffect;

    [Header("計算などで使用される情報")]
    [Tooltip("回転速度")]
    [SerializeField] private float rotationSpeed = 10.0f;
    [Tooltip("レベルアップに必要な経験値の基礎量")]
    [SerializeField]private float baseNextLvExp = 2f;
    [Tooltip("攻撃された後の無敵時間(秒)")]
    [SerializeField] private float invincibleTime = 3;

    /// <summary> マテリアルの加算色パラメータのID </summary>
    private static readonly int PROPERTY_ADDITIVE_COLOR = Shader.PropertyToID("_AdditiveColor");
    /// <summary> モデルのRenderer </summary>
    [Header("その他の情報")]
    [SerializeField]
    private Renderer _renderer;

    /// <summary> モデルのマテリアルの複製 </summary>
    private Material _material;

    private Sequence _seq;

    // ――――― 外部参照用関数 ―――――
    public float MoveSpeed => moveSpeed;
    public float FinalMoveSpeed => MoveSpeed * BuffManager.Instance.gobalMoveSpeed;
    public float DashSpeed => dashSpeed;
    public float DashDuration => dashDuration;
    public float RotationSpeed => rotationSpeed;
    public float NowHp 
    {
        get => nowHp;
        set => nowHp = value;
    }
    public float MaxHp
    {
        get => maxHp;
        set => maxHp = value;

    }
    public bool IsAliveFlg => isAliveFlg;
    public float NowLv => nowLv;
    public float NextLvExp => nextLvExp;
    public float NowExp => nowExp;
    public float DashCoolDownTime => dashCoolDown;
    public float DashCoolDownCnt => dashCoolDownCnt;
    public float InvincibleTime => invincibleTime;
    // ――――― 外部参照用関数 ―――――

    public GameObject playerClone;

    // 選ばれたプレイヤーキャラにデータを渡す
    public void SetStatusData(PlayerData.StatusData data)
    {
        maxHp = data.maxHp + (int)BuffManager.Instance.gobalHealthAdd;
        moveSpeed = data.moveSpeed;
        dashSpeed = data.dashSpeed;
        dashDuration = data.dashDuration;
        dashCoolDown = data.dashCoolDown;
        PlayerController controller = GetComponent<PlayerController>();
        controller.dashRechargeTime = dashCoolDown;

        // 現在のステータスの値を初期化
        nowHp = MaxHp;

        //HPの値を貰う
        ItemManager.Instance.getPlayerNowHP = nowHp;

        nextLvExp = baseNextLvExp;
        Debug.Log("PlayerStateのSetStatusData関数の実行");
    }

    public void EnableDoubleCastModel()
    {
        playerClone.SetActive(true);
    }

    private void OnEnable()
    {
        EventManager.StartListening("ChangePlayerHp", ApplyPlayerHp);
        EventManager.StartListening("PlayerDash", ResetDashCoolDownCnt);
    }

    private void OnDisable()
    {
        EventManager.StopListening("ChangePlayerHp",ApplyPlayerHp);
        EventManager.StopListening("PlayerDash",ResetDashCoolDownCnt);
    }

    public void ApplyHpChange(float hpVal)
    {
        EventManager.EmitEventData("ChangePlayerHp", hpVal);
        //Debug.Log($"Player HP Changed: {hpVal} (Current HP: {NowHp})");
    }

    private void ApplyPlayerHp()
    {
        if(isAliveFlg == false) { return; }

        //無敵状態に触れた場合はダメージを受けない
        ItemManager.Instance.InvinsbleOnTouchEnemy();

        if (ActivePetManager.Instance.isMuscleDuckInGame)
        {
            var muscleDuck = ActivePetManager.Instance.GetMuscleDuckScript();

            if (muscleDuck != null)
            {
                var enemyData = EventManager.GetData(GameEvent.CounterPetAttack);
               
                if (enemyData != null && enemyData is Transform targetTrans)
                {                 
                    bool isSuccess = muscleDuck.TryPriorityCounter(targetTrans);

                    if (isSuccess)
                    {                       
                        return;
                    }
                }
            }
        }

        var amount = EventManager.GetFloat("ChangePlayerHp");
        float healAmount = amount;
        amount *= (1 - BuffManager.Instance.gobalPlayerDefenceAdd / 100);

        // 渡された値が負の場合はダメージを受けた
        if (amount < 0)
        {
            if (invincibleCnt > 0) 
            {
                return;
            }

            // ダメージを受けた後の無敵時間の残りカウントをセットする
            invincibleCnt = invincibleTime;

            GetComponent<PlayerMaterialController>().Flash();            

            if (!SkillEffectManager.Instance.isPlayerShieldActive)
            {
                GetComponent<Animator>().SetTrigger("GetHit");
                NowHp += amount;
                SoundEffect.Instance.Play(SoundList.PlayerGetDamage);
                CameraShake.Instance.StartShake();
                EffectManager.Instance.SpawnDamagePlayerMoji(amount);
            }
            else
            {
                SkillEffectManager.Instance.shieldObj.BreakShield();
                SoundEffect.Instance.PlayOneSound(SkillEffectManager.Instance.shieldBreakSe,0.14f);
            }

            EventManager.EmitEvent(GameEvent.PlayerGetDamage);

            // 受けたダメージの総量に今回のダメージを追加する
            PlayerDataManager.Instance.totalDamage += Math.Abs(amount);

            // ダメージを受けて体力が減ったのでバフの獲得を行う
            UpgradeEffectManager.Instance.Active(BuffType.ApplyBuffPerDamage);
            // 体力が1/3以下になっているのならバフを発動させる
            // 合計のダメージ量からバフ獲得量を計算する
            UpgradeEffectManager.Instance.Active(BuffType.ApplyPinchBuff);
        }
        else
        {
            if(SkillEffectManager.Instance.universalTrait.isHealDouble) { 
                healAmount *= 2;
                ActiveBuffManager.Instance.AddStack(TraitType.HealDouble);
            }
            NowHp += healAmount;
            EventManager.EmitEvent(GameEvent.PlayerGetHeal);

            //カブキングはその回復量を受け取る
            if(ActivePetManager.Instance.isTurnipaKingInGame)
            {
                EventManager.EmitEventData(GameEvent.turnipaKingSkillPowUp, healAmount);
            }
        }

        
        EventManager.EmitEvent(GameEvent.PlayerHpChanged);

        // 体力が0より下になったらゲームオーバー処理を実行する
        if(nowHp <= 0)
        {
            if (SkillEffectManager.Instance.universalTrait.isReviveOnce)
            {
                SkillEffectManager.Instance.universalTrait.isReviveOnce = false;
                EventManager.EmitEventData("ChangePlayerHp", maxHp * 0.5f);
                SkillEffectManager.Instance.SpawnReviveEffect();
                ActiveBuffManager.Instance.ReduceStack(TraitType.Reviver);
            }
            else
            {
                nowHp = 0;
                isAliveFlg = false;
                //　ゲームオーバー処理を実行する
                EventManager.EmitEvent("isGameOver");
            }
            

            
        }
        
        // 体力が最大値を超えないようにする
        if(nowHp >= maxHp)
        {
            nowHp = maxHp;
        }

        // 体力の変動が起こったので残りの割合でバフの計算を行う
        UpgradeEffectManager.Instance.Active(BuffType.ApplyBuffHealthScaling);
    }

    private void ResetDashCoolDownCnt()
    {
        //dashCoolDownCnt = dashCoolDownTime;
        dashCoolDownCnt = dashCoolDown;
    }

    private void GetItemInvincble()
    {
        if (!ItemManager.Instance.pickUpInvincble) return;

        if (ItemManager.Instance.nowPickUpInvincble) return;

        invincibleCnt = ItemManager.Instance.invincbleTimeMax;

        ItemManager.Instance.nowPickUpInvincble = true;

        //ハイライトエフェクトを有効化する
        if (highlightEffect != null)
        {
            highlightEffect.enabled = true;
        }
    }

    public void AddExp(float exp)
    {
        nowExp += exp;
    }

    public void LevelMax()
    {
        nowLv = 30;
    }

    private void Awake()
    {
        _material = _renderer.material;
    }

    void Start()
    {
        // 現在のステータスの値を初期化
        nowHp = MaxHp;
        nextLvExp = baseNextLvExp;

        Debug.Log("PlayerStateのStart関数の実行");


        if (QuickRunModeManager.Instance.isQuickRunMode)
        {
            nowLv = 2;
        }

        //ハイライトエフェクトを無効化しておく
        if (highlightEffect != null)
        {
            highlightEffect.enabled = false;
        }
    }

    void Update()
    {
        // レベルアップ処理
        if(nowExp >= nextLvExp)
        {
            nowExp = 0;
            nowLv++;
            EventManager.EmitEvent("PlayerLevelUp");

            // 次のレベルに必要な経験値を計算する
            nextLvExp = baseNextLvExp * nowLv;
            if (QuickRunModeManager.Instance.isQuickRunMode)
            {
                if (nowLv >= 28) nextLvExp += 140;
            }
            else
            {
                if (nowLv >= 35) nextLvExp += 140;
            }
            
        }

        //無敵アイテムを取得したら無敵にする
        if (ItemManager.Instance.pickUpInvincble||!ItemManager.Instance.nowPickUpInvincble)
        {
            GetItemInvincble();
        }

        // 無敵時間の更新
        if (invincibleCnt > 0)
        {
            invincibleCnt -= 1.0f * Time.deltaTime;

            // カウントが0以下にはならないようにする
            if(invincibleCnt <= 0)
            {
                invincibleCnt = 0;

                //無敵アイテムのフラグを消す(外部のフラグだが動かしてOK)
                ItemManager.Instance.pickUpInvincble = false;

                ItemManager.Instance.nowPickUpInvincble = false;

                //ハイライトエフェクトを無効化しておく
                if (highlightEffect != null)
                {
                    highlightEffect.enabled = false;
                }

                //無敵が終わったら攻撃アップとスピードアップの効果をリセットする
                if (!ItemManager.Instance.pickUpInvincble)
                {
                    float defaultAmount = 1.0f;

                    //無敵時間の間に攻撃ポーション等を取っているのならリセットしない
                    if (!ItemManager.Instance.pickUpPowUpPotion)
                    {
                        ItemManager.Instance.powUpAmount = defaultAmount;
                    }
                    if (!ItemManager.Instance.pickUpSpdUpWing)
                    {
                        ItemManager.Instance.spdUpAmount = defaultAmount;
                    }
                }
            }
        }

        // ダッシュのクールダウンの更新
        if(dashCoolDownCnt > 0)
        {
            dashCoolDownCnt -= 1.0f * Time.deltaTime;

            // カウントが０以下にならないようにする
            if(dashCoolDownCnt <= 0)
            {
                dashCoolDownCnt = 0;
            }
        }
    }
}

