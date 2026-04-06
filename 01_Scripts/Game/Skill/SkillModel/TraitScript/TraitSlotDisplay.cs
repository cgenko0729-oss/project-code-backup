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
using UnityEngine.EventSystems;

public class TraitSlotDisplay : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public int casterSlotId = -1;
    public int traitSlotId = -1;

    public Image traitIcon;
    public string traitName;
    public string traitDescription;

    public bool isPlayerTrait = false;

    public bool isAssigned = false;

    public bool isOffsetApply = false;
    public float displayXOffset = -140f;
    public float displayYOffset = 0f;


    private void OnEnable()
    {
        EventManager.StartListening("UpdateNewTriat", UpdateTraitIcon);
    }

    private void OnDisable()
    {
        
    }

   

    void UpdateTraitIcon()
    {
        if (!isPlayerTrait)
        {
             if (SkillManager.Instance.activeSkillCastersHolder.Count <= casterSlotId) return;  //if SkillManager.Instance.activeSkillCastersHolder 's length is less than or equal to casterSlotId, return      
            if (SkillManager.Instance.activeSkillCastersHolder[casterSlotId].traitDataHoldingList.Count <= traitSlotId) return; //if SkillManager.Instance.activeSkillCastersHolder[casterSlotId] 's traitDataHoldingList length is less than or equal to traitSlotId, return
     
            TraitType type = SkillManager.Instance.activeSkillCastersHolder[casterSlotId].traitDataHoldingList[traitSlotId].traitType;
            traitName = L.TraitName(type);
            traitDescription = L.TraitDesc(type);
            traitIcon.sprite = SkillManager.Instance.activeSkillCastersHolder[casterSlotId].traitDataHoldingList[traitSlotId].icon;

            Vector3 spawnPos = new Vector3(2.21f,2.42f,5f);
            float xSpace = casterSlotId * 1.28f;
            float ySpace = traitSlotId * 0.777f;
            spawnPos.y -= xSpace;
            spawnPos.x += ySpace;

            if (!isAssigned)
            {
                EffectManager.Instance.SpawnGetEnchantEffectObj(spawnPos);
                Debug.Log("Spawn Get Enchant Effect Obj at " + spawnPos);
            }
           

            isAssigned = true;
        }
        else
        {
            if (SkillEffectManager.Instance.playerTraitList.Count <= traitSlotId) return; //if SkillEffectManager.Instance.playerTraitList 's length is less than or equal to traitSlotId, return


            TraitType type = SkillEffectManager.Instance.playerTraitList[traitSlotId].traitType;
            traitName = L.TraitName(type);
            traitDescription = L.TraitDesc(type);
            traitIcon.sprite = SkillEffectManager.Instance.playerTraitList[traitSlotId].icon;


        }
       
       


    }

    private void Start()
    {
        traitIcon = GetComponent<Image>();
    }

    private void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        RectTransform parentRt = GetComponentInParent<RectTransform>();
        RectTransform rt = traitIcon.GetComponent<RectTransform>();
        Vector2 worldPos = rt.anchoredPosition;
        Vector2 parentPos = parentRt.anchoredPosition;
        float posY = 280 - casterSlotId * 210;
        Vector2 displayPos = new Vector2(parentPos.x, posY);

        if(isOffsetApply) displayPos = new Vector2(parentPos.x + displayXOffset, posY + displayYOffset);

        if(!string.IsNullOrEmpty(traitName) && !string.IsNullOrEmpty(traitDescription)){

        }
        SkillEffectManager.Instance.ShowCurrentTraitName(traitName,traitDescription,displayPos);


    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SkillEffectManager.Instance.HideCurrentTraitName();

    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }


}

