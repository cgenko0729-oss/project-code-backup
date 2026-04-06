using Cysharp.Threading.Tasks;
using DG.Tweening;
using Hellmade.Sound; //SoundManager
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;    
using UnityEngine;   

public class AddAttackPerFinalSkillEffect : UpgradeEffectBase
{
    [SerializeField,Header("追加攻撃のPrefabオブジェクト")]
    private GameObject addAttackPrefab;

    [Header("追加攻撃の発生回数の計算")]
    [SerializeField,Tooltip("増加に必要な敵撃破数の基礎値")]
    private int addEnemyKillNum = 0;
    [SerializeField,Tooltip("最大増加数")]
    private int addAttackMaxNum = 0;

    [Header("計算途中の値の確認用")]
    [SerializeField]private int nextAddEnemyKillNum = 0;    // 次の回数増加に必要な敵撃破数
    [SerializeField]private int addAttackNum = 0;           // 追加攻撃の回数

    public float skillDamage = 0;
    private EnemyManager enemyManagerInst;  // EnemyManagerのインスタンス

    public override void EffectUpdate()
    {
        if(addEnemyKillNum == 0) { return; }
        if (enemyManagerInst == null)
        {
#if UNITY_EDITOR
            Debug.Log("EnemyManagerのインスタンスがNULLです");
#endif
            return;
        }

        // 追加攻撃の発生回数が最大数に到達しているなら計算を行わない
        if(addAttackNum >= addAttackMaxNum) { return; }

        if(enemyManagerInst.allEnemyKillNum >= nextAddEnemyKillNum)
        {
            addAttackNum++;

            nextAddEnemyKillNum +=
                addEnemyKillNum * addAttackNum;
        }
    }

    public override void CanEnableBuff()
    {
        isEnable = BuffManager.Instance.isAddAttackPerFinalSkillEnabled;

        if(isEnable)
        {
            ActiveBuffManager.Instance.SetStacks(
                TraitType.OwlSkill3_TenshionUp, 0);
        }

        nextAddEnemyKillNum = 0;
        addAttackNum = 0;
        enemyManagerInst = EnemyManager.Instance;   
    }

    public override bool ActiveBuff(Vector3 effectPos,float damageAmout)
    {
        if(addAttackPrefab == null) { return false; }

        // 追加攻撃を発生させる
        var effect = Instantiate<GameObject>
            (addAttackPrefab,effectPos,Quaternion.identity);
        var controller = effect.GetComponent<AddAttackEffectStarController>();
        controller.SpawnEffect(damageAmout / 2, addAttackNum * 0.4f);

        ActiveBuffManager.Instance.SetStacks(
                TraitType.OwlSkill3_TenshionUp, addAttackNum);

        return false;
    }
}