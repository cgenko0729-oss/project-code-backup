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
using System.Linq;
using System;
using UnityEngine.Assertions.Must;

public class MenuPlayerEnchantController : MonoBehaviour
{
    [Header("取得済みエンチャントの表示用")]
    public GameObject[] enchantsList;

    public MenuSkillStateControl control;

    private void OnEnable()
    {
        var selectedEnchants = SkillEffectManager.Instance.playerTraitList;
        int num = 0;
        foreach (var enchant in selectedEnchants)
        {
            if (enchant != null)
            {
                if(num > selectedEnchants.Count()) { return; }
                if(selectedEnchants.Count() > 4) { return; }

                enchantsList[num].GetComponent<Selectable>().interactable = true;
                var enchantIcon = enchantsList[num].GetComponent<PlayerEnchantIconController>();
                enchantIcon.iconImage.sprite = enchant.icon;
                enchantIcon.iconMask.gameObject.SetActive(true);
                
                if (enchantIcon != null)
                {
                    enchantIcon.enchantNameStr = L.TraitName(enchant.traitType);
                    enchantIcon.enchantDescStr = L.TraitDesc(enchant.traitType);
                }

                num++;
            }
        }

        if(control != null)
        {
            GameObject firstObj = control.firstListGroup;
            if(firstObj == null) { return; }

            Selectable firstUI = firstObj?.GetComponent<Selectable>();
            if(firstUI == null) { return; }
            Selectable last = enchantsList.Last<GameObject>().GetComponent<Selectable>();
            Navigation lastNavi = last.navigation;
            lastNavi.selectOnRight = firstUI;
            last.navigation = lastNavi;
        }
    }

    void Awake()
    {

        Selectable first = enchantsList[0].GetComponent<Selectable>();
        Selectable last = enchantsList.Last<GameObject>().GetComponent<Selectable>();
        Navigation firstNavi = first.navigation;
        firstNavi.selectOnLeft = last;
        Navigation lastNavi = last.navigation;
        lastNavi.selectOnRight = first;

        first.navigation = firstNavi;
        last.navigation = lastNavi;
        
        
        foreach (var enchant in enchantsList)
        {
            var enchantIcon = enchant.GetComponent<PlayerEnchantIconController>();
            enchantIcon.iconMask.gameObject.SetActive(false);
            enchant.GetComponent<Selectable>().interactable = false;
        }
    }
}

