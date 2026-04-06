using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using TigerForge;
using UnityEngine;

public class PetAnubisAction : PetNagaAction
{
    [Header("場に出せる召喚数の最大")]
    [SerializeField] private int maxSummonCount = 6;

    private List<GameObject> summonedPets = new List<GameObject>();

    private GameObject deadPrefab;
    private Vector3 spawnPos;

    #region --ステート上書き----------------------------------

    protected override void ActiveSkillMotion_Update()
    {
        //アニメーション変更
        AnimationEndChange("ActiveSkillMotion");
    }

    #endregion --ステート上書き終了--

    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening(GameEvent.LastAttack_Anubis, SkillAction);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening(GameEvent.LastAttack_Anubis, SkillAction);
    }

    protected override void Update()
    {
        base.Update();

        ChangeCoolDown();

        ChangeActiveSkill();
    }

    protected override void SetupProjectile(PetSkillData proj)
    {
        proj.SetPool(skillObjPool);
        // 元々の初期化処理
        proj.Initialize(takeDamages, this.gameObject,LastAttackType.Anubis);
        proj.SetDirection(skillRot);
        proj.speed = petSkillSpeed;
    }

    public void SkillAction()
    {
        //アクティブスキルのクールタイムがリセットされているか確認
        if (petData.activeSkillRemainingCooldown == ResetCoolTime)
        {
            //スキルを使用中にする
            skillActive = true;

            //死体のデータを貰う
            var data = EventManager.GetData(GameEvent.LastAttack_Anubis) as Dictionary<string, object>;

            //データが無ければ終了
            if (data == null || !data.ContainsKey("prefab")) return;

            //死体のオブジェクトと位置を取得
            deadPrefab = data["prefab"] as GameObject;
            spawnPos = (Vector3)data["position"];

            //アクティブスキルの効果発動
            sm.ChangeState(PetActionStates.ActiveSkillMotion);

            //クールタイムリセット
            ResetCoolDown();
        }
        else
        {
            return;
        }
    }

    //エネミー復活時の処理
    public void EnemyRevive()
    {
        //追加で召喚できる数を取得
        int extraCount = ActivePetManager.Instance.PetCloneCount;
        int totalSummonCount = 1 + extraCount;

        //スキル持続時間を設定
        float finalSkillDuration = petData.activeSkillDuration * ActivePetManager.Instance.PetSkillDuration;

        int currentLimit = maxSummonCount + extraCount;

        for (int i = 0; i < totalSummonCount; i++)
        {
            if (summonedPets.Count >= currentLimit)
            {               
                GameObject oldPet = summonedPets[0];
                summonedPets.RemoveAt(0);

                if (oldPet != null)
                {                 
                    ActivePetManager.Instance.RemoveActivePet(oldPet);                
                    Destroy(oldPet);
                }
            }

            Vector3 offset = Vector3.zero;
            if (i > 0)
            {
                offset = new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            }

            //編成位置を計算
            Vector3 finalOffset = (spawnPos + offset) - playerTransform.position;

            //蘇生する
            GameObject dead = Instantiate(deadPrefab, spawnPos, this.transform.localRotation);

            //復活エフェクト再生
            ItemManager.Instance.PlayTimeParticleInObject(dead,
                                                          ItemManager.Instance.itemKeepTimePs.EnemyRevivePs,
                                                          Vector3.zero);

            ActivePetActionBase deadStatus = dead.GetComponent<ActivePetActionBase>();
            if (deadStatus != null)
            {
                //編成位置を設定
                deadStatus.SetFormationOffset(finalOffset);

                //君はクローンですと告げる
                deadStatus.Doppelganger();

                //アクティブスキルの持続時間を設定
                deadStatus.SetSelfTimer(finalSkillDuration);

                //強制移行
                deadStatus.ForceChangeState(PetActionStates.Walk);
            }
            ActivePetManager.Instance.RegisterActivePetList(dead);
        }
    }
}