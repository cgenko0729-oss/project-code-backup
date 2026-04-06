using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using UnityEngine.EventSystems;

public class ShopItemSlot : MonoBehaviour,IPointerClickHandler,ISubmitHandler,ISelectHandler
{
    [SerializeField] private Image           iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;

    // these were private before → keep them private!
    [SerializeField]private ShopItemData data;
    private ShopWindow   owner;

    public  int  CurrentLevel { get; private set; }
    public  bool ReachedMax   => CurrentLevel >= data.levels.Count;
    public  int  PriceNext    => ReachedMax ? 0 : data.levels[CurrentLevel].needMoney;
    public  float AddNext     => ReachedMax ? 0 : data.levels[CurrentLevel].addAmount;
    public  ShopItemType ItemType => data.itemType;

    public bool isUnlocked = false;

    // ---------- NEW ----------
    public void Setup(ShopItemData d, ShopWindow o)
    {
        data     = d;
        owner    = o;
        iconImage.sprite = d.icon;
        //nameText.text    = d.displayName;
        nameText.text = L.ItemName(d.itemType);

        
        
        Refresh();
    }

    public void OnPointerClick(PointerEventData _)
    {
        if (isUnlocked) owner.SelectSlot(this);
    }

    public void OnSubmit(BaseEventData _)
    {
        //OnPointerClick(null);


    }

    public void OnSelect(BaseEventData eventData)
    {
        if (isUnlocked) owner.SelectSlot(this);
    }

    public void LevelUp()
    {
        CurrentLevel++;
        Refresh();
    }

    private void Refresh()
    {
        levelText.text = ReachedMax ? "MAX" : $"Lv {CurrentLevel}/{data.levels.Count}";
    }

    public void InitLevel(int levelFromSave)
    {
        CurrentLevel = Mathf.Clamp(levelFromSave, 0, data.levels.Count);
        Refresh();
    }


}

