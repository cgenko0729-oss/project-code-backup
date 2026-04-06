using System.Collections.Generic;
using System.Linq;
using TigerForge;
using UnityEngine;

public class PetSnakeEliteAction : ActivePetActionBase
{
    #region 固有能力---------------------------------------

    [Space]

    [Header("固有の能力")]

    [Header("鈍足デバフの量")]
    public float debuffSpeedDownAmount = 0.8f;

    [Header("鈍足デバフの時間")]
    public float debuffSpeedDownTime = 3.0f;

    private GameObject leaderPetObject;

    #endregion --------------------------------------------

    public override void PerformAttack() => PetAttackAction();

    protected override void PetAttackAction() => base.PetAttackAction();

    // --- 初期設定 ---
    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.StartListening(GameEvent.PetAttack, OnLeaderAttacked);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.StopListening(GameEvent.PetAttack, OnLeaderAttacked);
    }

    protected override void Start()
    {
        base.Start();
       
        FindLeaderPet();
    }
 
    private void FindLeaderPet()
    {
        //リーダーペットを取得
        var allActivePets = ActivePetManager.Instance.activePets;
        if (allActivePets == null || allActivePets.Count == 0)
        {
            return; 
        }

        //リーダーペットはリストの最初のペットとする
        this.leaderPetObject = allActivePets.First();
        if (this.leaderPetObject == null)
        {
            return;
        }

        //リーダーペットが蛇族か確認
        ActivePetActionBase leaderStatus = leaderPetObject.GetComponent<ActivePetActionBase>();
        if (leaderStatus == null || leaderStatus.GetPetData() == null)
        {
            return;
        }

        //蛇族なら効果発動
        if (leaderStatus.GetPetData().race == PetRace.Snake)
        {
            ItemManager.Instance.PlayTimeParticleInObject(leaderPetObject,
                                                          ItemManager.Instance.itemKeepTimePs.RevengePs,
                                                          ItemManager.Instance.defaultEffectPos);
            float multiplier = 2f;
            debuffSpeedDownAmount *= multiplier;
            debuffSpeedDownTime *= multiplier;

            SoundEffect.Instance.Play(SoundList.NagaShout);
        }
        else
        {
            ItemManager.Instance.PlayTimeParticleInObject(leaderPetObject,
                                                          ItemManager.Instance.itemKeepTimePs.SoulEatPs,
                                                          ItemManager.Instance.defaultEffectPos);
        }
    }

    private void OnLeaderAttacked()
    {       
        if (leaderPetObject == null) return;

        var eventData = (Dictionary<string, object>)EventManager.GetData(GameEvent.PetAttack);

        if (eventData == null) return;

        GameObject attacker = (GameObject)eventData["attacker"];

        Transform targetTransform = (Transform)eventData["target"];
      
        if (attacker != leaderPetObject)
        {
            return;
        }

        if (targetTransform != null)
        {
            EnemyStatusBase enemyStat = targetTransform.GetComponent<EnemyStatusBase>();
            if (enemyStat != null)
            {
                enemyStat.ApplySpeedDownDebuff(debuffSpeedDownAmount, debuffSpeedDownTime);
            }
        }
    }
}
