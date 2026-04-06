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
using Cysharp.Threading.Tasks.Triggers;

public class PetSkillIconData : MonoBehaviour
{
    [Header("アイコン表示に必要なデータ")]
    [SerializeField] private Image petIconImage;
    [SerializeField] private Image numberIconImage;
    [SerializeField] private Image dirButtonIconImage;
    [SerializeField] private TextMeshProUGUI autoText;
    [SerializeField] private Image skillIconImage;
    [Tooltip("スキルクールダウンの表示")]
    [SerializeField] private TextMeshProUGUI cooldownNumText;
    [SerializeField] private GameObject cooldownMask;
    private PetData petData;
    private float activeSkillTotalCooldown = 0;

    // ペットのアイコン画像やスキル情報をセット
    public void SetPetSkillData(PetData _petData,
        Sprite numberKeySprite = null, Sprite dirbuttonSprite = null)
    {
        // アイコン画像等の設定
        petIconImage.sprite = _petData.petIcon;
        skillIconImage.sprite = _petData.skills[0].skillicon;
        numberIconImage.sprite = numberKeySprite;
        dirButtonIconImage.sprite = dirbuttonSprite;

        // アクティブスキルの表示
        bool hasActiveSkill = _petData.hasActiveSkill;
        numberIconImage.gameObject.SetActive(hasActiveSkill);
        dirButtonIconImage.gameObject.SetActive(hasActiveSkill);
        cooldownMask.gameObject.SetActive(hasActiveSkill);
        cooldownNumText.gameObject.SetActive(hasActiveSkill);

        // パッシブスキルの表示
        bool hasPassiveSkill = _petData.hasPassiveSkill;
        autoText.gameObject.SetActive(hasPassiveSkill);

        petData = _petData;
        activeSkillTotalCooldown = _petData.activeSkillTotalCooldown;
    }

    void Update()
    {
        if (petData == null || petData.hasActiveSkill == false) { return; }

        float remainingCooldown = petData.activeSkillRemainingCooldown;
        float progress = remainingCooldown / activeSkillTotalCooldown;
        cooldownMask.transform.localScale = new Vector3(1, progress, 1);

        Color color = Color.white;
        if(remainingCooldown <= 0)
        {
            color.a = 0;
        }
        cooldownNumText.text = ((int)remainingCooldown).ToString();
        cooldownNumText.color = color;
    }
}

