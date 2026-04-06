using QFSW.MOP2;
using System.Collections;
using TigerForge;
using UnityEngine;

public class PetBossFlameKingDragonAction : ActivePetActionBase, IPetActiveSkill
{
    [Header("ブレスのオブジェクトプール")]
    [SerializeField] private ObjectPool breathObjPool;

    [Header("ブレスを吐く場所")]
    [SerializeField] private GameObject breathPointObject;

    [Header("ブレス(当たり判定)のオブジェクトプール")]
    [SerializeField] private ObjectPool breathColObjPool;

    [Header("ブレスの当たり判定を出す場所")]
    [SerializeField] private GameObject brethColPoint;

    [Header("ブレスの攻撃力")]
    [SerializeField] private int breathAttackPower;

    [Header("ブレスの速度")]
    [SerializeField] private float breathSpeed;

    [Header("ブレスの持続時間")]
    [SerializeField] private float breathDuration;

    [Header("扇形の範囲設定")]
    [SerializeField] private int projectilesPerVolley = 5;  // 1回の斉射で発射する弾の数
    [SerializeField] private float spreadAngle = 60f;     // 扇形の合計角度
    [SerializeField] private float volleyInterval = 0.2f;      // 斉射と斉射の間隔（秒）

    [Header("ブレスのラグ")]
    [SerializeField] private float breathLagTime = 0.5f; // ブレスを吐くまでのラグ時間

    [Header("ブレス(当たり判定)の攻撃間隔")]
    [SerializeField] private float breathColAttackInterval = 0.75f; // ブレスの当たり判定の攻撃間隔

    [Header("クールタイム減少の割合")]
    [SerializeField] 
    private float cooldownReductionPercentage = 10f;

    private float seCooldown = 0.3f;

    private enum DragonStates
    {
        Idle,
        Flying,
        GetOff,
    }

    private DragonStates dragonStates=DragonStates.Idle;
    private Coroutine activeBreathCoroutine = null;
    private Coroutine BreathColCoroutine = null;


    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening(GameEvent.LastAttack_FlameDragonKing, CooldownReduction);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening(GameEvent.LastAttack_FlameDragonKing, CooldownReduction);
    }

    protected override void Start()
    {
        base.Start();

        SetCoolDown();
    }
    public void PetActiveSkill()
    {
        //もしpetdataのアクティブスキルのクールタイムが変更されていたら、スキルを発動できないようにする
        if (petData.activeSkillRemainingCooldown == ResetCoolTime && !skillActive)
        {
            //アクティブスキルを使用中にする
            skillActive = true;

            //アクティブスキルの効果発動
            ActiveSkillAction();

            //一斉攻撃を開始
            EventManager.EmitEvent(GameEvent.AllAttackStart);

            SoundEffect.Instance.Play(SoundList.DebugkeySe);

            //クールタイムリセット
            ResetCoolDown();
        }
        else
        {
            SoundEffect.Instance.Play(SoundList.ButtonCancelSe);

            return;
        }
    }

    protected override void Update()
    {
        base.Update();

        seCooldown -= Time.deltaTime;

        ChangeCoolDown();

        if (dragonStates == DragonStates.Flying)
        {
            FlyingMotion();
        }
        else if (dragonStates == DragonStates.GetOff)
        {
            GetOffMotion();
        }
    }
    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    protected override void ActiveSkillAction()
    {
        sm.ChangeState(PetActionStates.ActiveSkillMotion);
    }

    private void Flying()
    {
        dragonStates = DragonStates.Flying;
    }
    private void GetOff()
    {
        dragonStates = DragonStates.GetOff;
    }

    private void IdleMotion()
    {
        skillActive = false;
        sm.ChangeState(PetActionStates.Idle);
    }

    private void FlyingMotion()
    {
        //徐々にY軸方向に上昇させる
        Vector3 upMovement = new Vector3(0, 5f * Time.deltaTime, 0);
        transform.position += upMovement;

        //最高到達点に達したら、止まる
        if (transform.position.y >= 5f)
        {
            Vector3 pos = transform.position;
            pos.y = 5f;
            transform.position = pos;
            dragonStates = DragonStates.Idle;
        }
    }

    private void GetOffMotion()
    {
        //徐々にY軸方向に下降させる
        Vector3 downMovement = new Vector3(0, -5f * Time.deltaTime, 0);
        transform.position += downMovement;
        //地面に到達したら、Idle状態に戻る
        if (transform.position.y <= 0f)
        {
            Vector3 pos = transform.position;
            pos.y = 0f;
            transform.position = pos;
        }
    }

    private void FlameBreath()
    {
        //ブレス(演出)のコルーチン
        if (activeBreathCoroutine != null)
        {
            StopCoroutine(activeBreathCoroutine);
        }
        activeBreathCoroutine = StartCoroutine(FlameBreathCoroutine());

        //ブレスの当たり判定を出す
        if (BreathColCoroutine != null)
        {
            StopCoroutine(BreathColCoroutine);
        }
        BreathColCoroutine = StartCoroutine(BreathColliderCoroutine());
    }

    private IEnumerator FlameBreathCoroutine()
    {
        float elapsedTime = 0f; // 経過時間を記録するタイマー
        CameraShake.Instance.StartShake(0.2f, 1f, 0.5f, 50f);

        while (elapsedTime < breathDuration)
        {
            CameraShake.Instance.StartShake(0.4f, 0.1f, 0.5f, 30f);

            Vector3 spawnPos = breathPointObject.transform.position;
            Vector3 forwardVec = breathPointObject.transform.forward;

            for (int i = 0; i < projectilesPerVolley; i++)
            {
                float angle = 0f;
                if (projectilesPerVolley > 1)
                {
                    float startAngle = -spreadAngle * 0.5f;
                    float angleStep = spreadAngle / (projectilesPerVolley - 1);
                    angle = startAngle + angleStep * i;
                }

                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
                Vector3 direction = rotation * forwardVec;
                Quaternion spawnRotation = Quaternion.LookRotation(direction);

                PetSkillData breath = breathObjPool.GetObjectComponent<PetSkillData>(spawnPos, spawnRotation);

                if (breath != null)
                {
                    breath.SetPool(breathObjPool);
                    breath.SetDirection(direction);
                    breath.speed = breathSpeed;
                }
            }

            yield return new WaitForSeconds(volleyInterval);

            elapsedTime += volleyInterval;
        }
        activeBreathCoroutine = null;
    }

    private IEnumerator BreathColliderCoroutine()
    {
        //開幕のラグ時間を待つ
        yield return new WaitForSeconds(breathLagTime);

        float elapsedTime = 0f; // 経過時間を記録するタイマー

        while (elapsedTime < breathDuration)
        {         
            //ブレスの当たり判定を出す位置を調べる
            Vector3 spawnPos = brethColPoint.transform.position;
            Vector3 forwardVec = brethColPoint.transform.forward;

            GameObject breathCol = breathColObjPool.GetObject();

            breathCol.transform.position = brethColPoint.transform.position; //スキルの位置を設定

            //回転を一度取得して修正後に再設定
            Vector3 fixedEulerAngles = transform.rotation.eulerAngles;
            fixedEulerAngles.x = 0;
            fixedEulerAngles.z = 0; 
            transform.rotation = Quaternion.Euler(fixedEulerAngles);

            breathCol.transform.rotation = Quaternion.Euler(fixedEulerAngles);

            PetSkillData breath = breathCol.GetComponent<PetSkillData>();

            if (breath != null)
            {
                breath.SetPool(breathColObjPool);
                if(whoIam == PetCloneType.Original)
                {
                    breath.Initialize(breathAttackPower, this.gameObject,LastAttackType.FlameDragonKing);                  
                }
                else
                {
                    breath.Initialize(breathAttackPower, this.gameObject);
                }
                breath.SetDirection(forwardVec);
                breath.speed = breathSpeed;
            }
            
            yield return new WaitForSeconds(breathColAttackInterval);

            elapsedTime += breathColAttackInterval;
        }
        BreathColCoroutine = null;
    }

    private void CooldownReduction()
    {
        //クールタイムを減らす
        if (petData.activeSkillRemainingCooldown > 10f)
        {
            if(seCooldown <= 0)
            {
                seCooldown = 0.2f;
                SoundEffect.Instance.Play(SoundList.HealSE);
            }
            

            float reductionAmount = 0f; //クールタイム減少量（秒）
            reductionAmount = petData.activeSkillTotalCooldown * (cooldownReductionPercentage / 100); //クールタイムの10%を減らす場合

            petData.activeSkillRemainingCooldown -= reductionAmount;
            if (petData.activeSkillRemainingCooldown < 0f)
            {
                petData.activeSkillRemainingCooldown = 0f;
            }
        }
    }
}
