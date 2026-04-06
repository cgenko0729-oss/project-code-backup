using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;


public class expMove : MonoBehaviour
{


    // ────────────────[Tunables]────────────────
    public ObjectPool expPool;      
    public float groundPosY = 0.49f;
    public float moveSpeedNormal = 5f;
    public float moveSpeedMagnet = 14f;
    public float distNeedForHoming = 0.5f;
    public float distNeededForHomingToPet = 2.1f;
    public float kickBackDuration = 1f;
    public float kickBackSpeed = 4f;

    //Magnet Debuff
    public float magnetDebuff = 0.7f;
    public float magnetDebuffDuration = 7f;
    public bool isMagnetDebuffActive = false;

    [Header("Jump Settings")]
    public float jumpHeight = 1f;
    public float jumpDuration = .6f;
    public float landingRadius = .4f;

    // ────────────────[Runtime State]────────────────
    public float moveSpeed;
    public bool  isHomingActive, isKickingBack, isJumping;
    float kickTimer;
    Vector3 kickDir;
    Vector3 selfPos;
    
    Transform selfTran;
    static Transform playerTrans;
    static float  distNeedPlayerSqr;
    static float  distNeedPetSqr;
    
    Transform petTrans;      // ★ Inspector または動的探索
    Transform homeTarget;    // ★ 現在追尾中ターゲット

    PlayerState playerState;
    public float expAmount = 1f;

    public bool isDetoryable = false;

    public float rotSpeed = 140f;

    public bool isGetEnchant = false;

    private UpgradeEffectBase upgradeEffect;

    // ────────────────[MonoBehaviour]────────────────
    void Awake()
    {
        selfTran = transform;

        // プレイヤー
        var playerGO = GameObject.FindWithTag("Player");
        playerTrans = playerGO.transform;
        playerState = playerGO.GetComponent<PlayerState>();

        // ★ ペット (Scene に無い or 複数ある場合は適宜変更)
        var petGO = GameObject.FindWithTag("PetCollector");
        if (petGO) petTrans = petGO.transform;

        // pickup バフ込みで閾値を再計算
        float distNeedWithBuff = distNeedForHoming * (1 + BuffManager.Instance.gobalPickUpRange / 100f);
        distNeedPlayerSqr = distNeedWithBuff         * distNeedWithBuff;
        distNeedPetSqr    = distNeededForHomingToPet * distNeededForHomingToPet;

        moveSpeed   = moveSpeedNormal;
        //expAmount  *= 1 + (BuffManager.Instance.gobalExpGain / 100f);

        magnetDebuff = 1.0f;
    }

    private void Start() => upgradeEffect =
        UpgradeEffectManager.Instance.upgradeEffectList.Find(d => d.BuffType == BuffType.ApplyBuffPerExp);
    void OnEnable()
    {
        //a bit heavier 
        float distNeedWithBuff = distNeedForHoming * (1 + BuffManager.Instance.gobalPickUpRange / 100f);
        distNeedPlayerSqr = distNeedWithBuff         * distNeedWithBuff;

        // 位置リセット
        selfPos           = selfTran.position;
        selfPos.y         = groundPosY;
        selfTran.position = selfPos;

        // 状態リセット
        isHomingActive = isKickingBack = false;
        moveSpeed      = moveSpeedNormal;
        isJumping      = true;
        homeTarget     = null;             // ★

        // DOJump でランダム着地
        transform.DOKill();
        Vector3 target = transform.position;
        target.x += Random.Range(-landingRadius, landingRadius);
        target.z += Random.Range(-landingRadius, landingRadius);
        DOTween.Sequence()
            .Append(transform.DOJump(target, jumpHeight, 1, jumpDuration).SetEase(Ease.OutQuad))
            .Append(transform.DOPunchPosition(Vector3.up * 0.14f, 0.28f, 1, 0).SetEase(Ease.OutQuad))
            .OnComplete(() => isJumping = false);
    }

    void OnDisable() => transform.DOKill();

    void Update()
    {
        if (isJumping) return;

        //magnetDebuffDuration -= Time.deltaTime;

        //if(isMagnetDebuffActive && magnetDebuffDuration <= 0f)
        //{
        //    isMagnetDebuffActive = false;
        //    magnetDebuff = 1.0f;
        //}

        selfPos = selfTran.position;
        selfTran.Rotate(Vector3.up, rotSpeed * Time.deltaTime); // 回転演出

        // ───── マグネット処理（プレイヤー固定で強制ホーミング）─────
        if (ItemManager.Instance.pickUpMagnet)
        {
            ActivateHoming(playerTrans);             // ★まとめ関数
            moveSpeed = moveSpeedMagnet;
        }

        // ───── ホーミング開始判定 ─────
        if (!isHomingActive)
        {
            // ①プレイヤーまでの距離²
            float dx = playerTrans.position.x - selfPos.x;
            float dz = playerTrans.position.z - selfPos.z;
            float distPlayer2D = dx * dx + dz * dz;

            // ②ペットまでの距離²
            float distPet2D = float.MaxValue;        // ★デフォ大
            if (petTrans)
            {
                float pdx = petTrans.position.x - selfPos.x;
                float pdz = petTrans.position.z - selfPos.z;
                distPet2D = pdx * pdx + pdz * pdz;
            }

            // ③判定：先に閾値を満たした方
            if (distPlayer2D <= distNeedPlayerSqr)
            {
                ActivateHoming(playerTrans);         // ★
            }
            else if (distPet2D <= distNeedPetSqr)
            {
                ActivateHoming(petTrans);            // ★
            }
            else
            {
                return; // まだ誰も範囲外 → Update 終了
            }
        }

        // ───── キックバック中 ─────
        if (isKickingBack)
        {
            selfPos += kickDir * kickBackSpeed * Time.deltaTime;
            kickTimer -= Time.deltaTime;
            if (kickTimer <= 0f) isKickingBack = false;
            selfTran.position = selfPos;
            return;
        }

        // ───── 追尾移動 ─────
        if (!homeTarget) return; // 念のため

        float dxH = homeTarget.position.x - selfPos.x;
        float dzH = homeTarget.position.z - selfPos.z;
        float magSqr = dxH * dxH + dzH * dzH;

        // 到達チェック
        if (magSqr < 0.01f)
        {
            if (!isDetoryable) expPool.Release(gameObject);
            else Destroy(gameObject);
            SoundEffect.Instance.Play(SoundList.GetExp);

            GiveExpToCollector(homeTarget);          // ★共通化
            //playerState.AddExp(expAmount);

            if(SoundEffect.Instance.getExpSoundFrequency <=0)
            {
                SoundEffect.Instance.getExpSoundFrequency = 0.035f; 
                SoundEffect.Instance.Play(SoundList.GetExp);
            }
            
            return;
        }

        float invMag = 1f / Mathf.Sqrt(magSqr);
        selfPos.x += dxH * invMag * moveSpeed * Time.deltaTime;
        selfPos.z += dzH * invMag * moveSpeed * Time.deltaTime;
        selfTran.position = selfPos;
    }

    // ────────────────[Helper]────────────────
    /// <summary>指定ターゲットへのホーミングを開始</summary>
    void ActivateHoming(Transform target)
    {
        if (isHomingActive) return;
        homeTarget   = target;                         // ★ 記憶
        isHomingActive = true;

        // KickBack
        kickDir   = -(target.position - selfPos).normalized;
        kickTimer = kickBackDuration;
        isKickingBack = true;

        isMagnetDebuffActive = true;
        magnetDebuffDuration = 7.7f;
        magnetDebuff = 0.56f; // マグネットデバフ適用
    }

    /// <summary>誰が取得したかで処理分岐</summary>
    void GiveExpToCollector(Transform collector)
    {
        if (collector == playerTrans)
        {
            playerState.AddExp(expAmount * 1 + (BuffManager.Instance.gobalExpGain / 100f));
            MapManager.Instance.ReduceMapExpCount();

            if (isGetEnchant)
            {
                 SkillManager.Instance.GetTraitLevelUp();
                 //Invoke("DelayChestReward", 0.21f);
                 //SoundEffect.Instance.Play(SoundList.QuestFinishSe);
            }

            // 経験値取得でのバフを発動する
            if(upgradeEffect != null)
            {
                if(upgradeEffect.isEnable == true)
                {
                    upgradeEffect.ActiveBuff();
                }
            }
        }
        else if (collector == petTrans)
        {
            playerState.AddExp(expAmount * 1 + (BuffManager.Instance.gobalExpGain / 100f)); // とりあえず同じにしておく
            MapManager.Instance.ReduceMapExpCount();
            petTrans.GetComponent<PetKappaAction>()?.ChangeToFollowState();

            if (isGetEnchant)
            {
                 SkillManager.Instance.GetTraitLevelUp();
                 //Invoke("DelayChestReward", 0.21f);
                 //SoundEffect.Instance.Play(SoundList.QuestFinishSe);
            }

        }
    }


}

