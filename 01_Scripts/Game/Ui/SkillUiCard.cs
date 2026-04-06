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
using System.Security.Cryptography;
using UnityEngine.Localization.Settings;

public class SkillUiCard : MonoBehaviour
{

    public SkillIdType skillIdType = SkillIdType.None;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI skillDescription;
    public Image skillIcon;
    public bool isUnlockedInDemo = true;

    public TextMeshProUGUI skillEvolDesc;

    private void OnEnable()
    {
        SetUpSkill();
        EventManager.StartListening("LanguageChanged", SetUpSkill);
    }

    private void OnDisable()
    {
        EventManager.StopListening("LanguageChanged", SetUpSkill);
    }

    void Start()
    {
        skillName.text = L.SkillName(skillIdType);
        if (isUnlockedInDemo)
        {
            skillDescription.text = L.SkillDesc(skillIdType);
            skillEvolDesc.text =L.SkillName(skillIdType,true) + ":" +  L.SkillDesc(skillIdType, true);
        }
        else
        {
            skillDescription.text = LocalizationSettings.StringDatabase.GetLocalizedString("TextTable", "TraitNotUnlock");
            skillEvolDesc.text = "";
        }

    }

    void Update()
    {
        
    }

    public void SetUpSkill()
    {
        skillName.text = L.SkillName(skillIdType);
        if(isUnlockedInDemo)skillDescription.text = L.SkillDesc(skillIdType);
        else skillDescription.text = LocalizationSettings.StringDatabase.GetLocalizedString("TextTable", "TraitNotUnlock");
        Debug.Log("SkillUiCard SetUpSkill called" + skillIdType.ToString());
    }


}

