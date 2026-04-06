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

public class CharaUpgradeShopItemData : MonoBehaviour,
    ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, ISelectHandler
{
    public enum ShopItemState
    {
        DisableUnlock,  // 解放不可能
        EnableUnlock,   // 解放可能
        IsUnlocked,     // 解放済み
    }

    public CharaUpgradeDataSO upgradeDataSO;
    public UIFXController uiFx;
    public ShopItemState shopItemState = ShopItemState.DisableUnlock;

    [Header("アイテム(自分自身)につながる線")]
    public UIFXController nodeLineUiFx;
    [SerializeField] private Color defaultTintColor;
    [SerializeField] private Color isUnlokcedTintColor;

    [Header("シルエットアイコン情報")]
    public List<Image> silhouetteIcons = new();

    void Start()
    {
        if(uiFx == null)
        {
            uiFx = GetComponent<UIFXController>();
        }

        if (upgradeDataSO == null)
        {
            GetComponent<Selectable>().interactable = false;
        }

        GetComponentsInChildren<Image>(silhouetteIcons);
        silhouetteIcons.RemoveAt(0);
    }

    private void LateUpdate()
    {
        if (upgradeDataSO == null) { return; }

        // 現在のShopItemStateを状態ごとに判定する
        GetComponent<Selectable>().interactable = upgradeDataSO.enableUnlock;
        uiFx.SetOutline(shopItemState == ShopItemState.IsUnlocked, isUnlokcedTintColor, 1.0f, 2.5f);
    }

    // 解放できるようになったかを調べる
    public void UpdateEnableUnlock()
    {
        if (upgradeDataSO == null) { return; }

        // 解放できるようになるための前提条件を満たしていない場合
        var requiredUpgrade = upgradeDataSO.requiredUpgrade;
        upgradeDataSO.enableUnlock = false;
        if (requiredUpgrade == null || requiredUpgrade?.isUnlocked == true)
        {
            upgradeDataSO.enableUnlock = true;
        }
    }

    // 解放できなくなする強化項目を持っている場合にロック処理を行う
    public void UpdateExclusion()
    {
        if (upgradeDataSO == null) { return; }

        if (upgradeDataSO.isUnlocked == true && upgradeDataSO.exclusionUpgrades.Count != 0)
        {
            for (int i = 0; i < upgradeDataSO.exclusionUpgrades.Count; i++)
            {
                var upgrade = upgradeDataSO.exclusionUpgrades[i];
                upgrade.enableUnlock = false;
                upgradeDataSO.exclusionUpgrades[i] = upgrade;
            }
        }


        bool grayscaleFlg = false;
        float iluminosity = 0f;
        Color tintColor = Color.white;
        // 現在のShopItemStateを調べる
        if (upgradeDataSO.enableUnlock == true)
        {
            shopItemState = ShopItemState.EnableUnlock;
            grayscaleFlg = false;
            if (upgradeDataSO.isUnlocked == true)
            {
                shopItemState = ShopItemState.IsUnlocked;
                grayscaleFlg = true;
                iluminosity = 0f;
                tintColor = isUnlokcedTintColor;
            }
        }
        else
        {
            shopItemState = ShopItemState.DisableUnlock;
            grayscaleFlg = true;
            iluminosity = -0.8f;
        }

        nodeLineUiFx?.SetGreyscale(grayscaleFlg, 1, iluminosity, tintColor);

        foreach(var icon in silhouetteIcons)
        {
            icon.color = defaultTintColor;
            if(shopItemState == ShopItemState.IsUnlocked)
            {
                icon.color = Color.white;
            }
        }
    }


    public void OnSelect(BaseEventData eventData)
    {
        // 選択中の強化項目の情報を渡す
        CharaUpgradeShopManager.Instance.nowSelectedName =
            L.UpgradeName(upgradeDataSO.jobId, upgradeDataSO.dataId);
        CharaUpgradeShopManager.Instance.nowSelectedDesc =
            L.UpgradeDesc(upgradeDataSO.jobId, upgradeDataSO.dataId);
        CharaUpgradeShopManager.Instance.nowSelectedUnlockCoin =
            upgradeDataSO.unlockCoin;

        // 選択されたときのアニメーション再生

    }

    public void OnPointerEnter(PointerEventData eventData) => OnSelect(eventData);
    
    public void OnSubmit(BaseEventData eventData)
    {
        if(upgradeDataSO == null) { return; }
        if(upgradeDataSO.enableUnlock == false) { return; }
        if(upgradeDataSO.isUnlocked == true) { return; }

        // 所持コイン枚数が足りなければ解放しない
        if(CurrencyManager.Instance.Coins < upgradeDataSO.unlockCoin) { return; }

        CurrencyManager.Instance.Spend(upgradeDataSO.unlockCoin);
        CharaUpgradeShopManager.Instance.AddUseTotalCoin(
            upgradeDataSO.jobId, upgradeDataSO.unlockCoin);
        upgradeDataSO.isUnlocked = true;

        SoundEffect.Instance.PlayBuySkillTreeItemSe();

        EventManager.EmitEvent("UpdateAllShopItems");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSubmit(eventData);
    }
}

