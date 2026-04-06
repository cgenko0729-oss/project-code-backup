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

public class UiStatusMenuTraitUiObj : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;

    public void SetUp(TraitType type)
    {
        nameText.text = L.TraitName(type);
        descriptionText.text = L.TraitDesc(type);

    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

