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

public class AchievementUiMessageWindow : MonoBehaviour
{
    public TextMeshProUGUI acName;
    public TextMeshProUGUI acDesc;
    public TextMeshProUGUI acReward;
    public Image acIcon;

    public int indexInAcquiredAchList = -1;

    public void SetIndexInAcquiredAchList(int _index)
    {
        indexInAcquiredAchList = _index;
    }

    public void SetUp(string name, Vector3 pos, Sprite _achIcon)
    {
        acName.text = L.AchName(name);
        acDesc.text = L.AchDesc(name);
        acReward.text = L.AchReward(name);

        acIcon.sprite = _achIcon;

        RectTransform rt = GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
    }

    public void RemoveInAcquireAchList()
    {
        AchievementManager.Instance.RemoveItemInAcquireAchListByIndex(indexInAcquiredAchList);
    }

    void Start()
    {
        MenuOpenAnimator uiAni  = GetComponent<MenuOpenAnimator>();
        uiAni.PlayeMenuAni(true);

    }

    void Update()
    {
        
    }

    public void CloseWindow()
    {
        EventManager.EmitEvent("CloseOneAchievementMessageWindow");
        MenuOpenAnimator uiAni = GetComponent<MenuOpenAnimator>();
        uiAni.PlayeMenuAni(false);
    }

}

