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

public class ApplyBuffPetScalingEffect : UpgradeEffectBase
{
    [Header("連れてきたペット数によるバフ効果量")]
    [SerializeField, Tooltip("ダメージ増加量")]
    private float increaseCritAmount = 0;
    [SerializeField, Tooltip("サイズ増加量")]
    private float increaseSizeAmount;

    [Header("ペットの空き枠の数によるバフ効果量")]
    [SerializeField, Tooltip("ダメージ増加量")]
    private float increaseDamageAmount = 0;
    [SerializeField, Tooltip("スキルクールダウン減少量")]
    private float decreaseSkillCDAmount = 0;

    public override void CanEnableBuff()
    {
        isEnable = BuffManager.Instance.isApplyBuffPetScalingEnabled;

        // isEnableフラグがTrueならバフを適用する
        if (isEnable == true)
        {
            int selectedPetNum = PetSelectDataManager.Instance.SelectedPets.Count;
            int selectPetMaxNum = PetSelectDataManager.Instance.MaxPets;
            int nullPetNum = selectPetMaxNum - selectedPetNum;

            // 連れてきたペット数によるバフを適用する
            float finalCritAmount = increaseCritAmount * selectedPetNum; 
            float finalSizeAmount = increaseSizeAmount * selectedPetNum;
            BuffManager.Instance.gobalCritChanceAdd += finalCritAmount;
            BuffManager.Instance.gobalSizeAdd += finalSizeAmount;

            // ペットの空き枠の数によるバフを適用する
            float finalDamageAmount = increaseDamageAmount * nullPetNum;
            float finalSkillCDAmount = decreaseSkillCDAmount * nullPetNum;
            BuffManager.Instance.gobalDamageAdd += finalDamageAmount;
            BuffManager.Instance.gobalCooldownAdd += finalSkillCDAmount;

            //DOVirtual.DelayedCall(0.7f, () =>
            //{
            //     ActiveBuffManager.Instance.AddStack(TraitType.LionSkill3_LonelyLion);
            //Debug.Log($"ApplyBuffPetScalingEffect::CanEnableBuff -> Apply Buff Pet Scaling Effect: SelectedPetNum={selectedPetNum}, NullPetNum={nullPetNum}, FinalCritAmount={finalCritAmount}, FinalSizeAmount={finalSizeAmount}, FinalDamageAmount={finalDamageAmount}, FinalSkillCDAmount={finalSkillCDAmount}");
            //ActiveBuffManager.Instance.SetStacks(TraitType.LionSkill3_LonelyLion, selectedPetNum);
            //});
            ActiveBuffManager.Instance.SetStacks(
                TraitType.LionSkill3_LonelyLion, 0);
        }
    }
}

