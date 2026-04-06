using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the Power-Up / Shop window.
/// – keeps the list of slots in sync with ScriptableObject data
/// – fills the info panel when a slot is clicked
/// – spends coins, applies buffs, updates UI on BUY
/// </summary>
public class ShopWindow : MonoBehaviour
{
    // ────────────────────────── 1. Scene references ──────────────────────────
    [Header("Slots already placed in the scene (drag each one here)")]
    [SerializeField] private List<ShopItemSlot> slots = new List<ShopItemSlot>();

    [Header("Info Panel")]
    [SerializeField] private Image           infoIcon;
    [SerializeField] private TextMeshProUGUI infoHeader;
    [SerializeField] private TextMeshProUGUI infoDesc;
    [SerializeField] private TextMeshProUGUI infoCost;
    [SerializeField] private Button          buyButton;

    private GameObject previousUiObj;

    // ────────────────────────── 2. Data ──────────────────────────
    [Header("ScriptableObjects")]
    [SerializeField] private List<ShopItemData> itemDatas = new List<ShopItemData>();

    // ────────────────────────── 3. Runtime state ──────────────────────────
    private ShopItemSlot selected;

    Dictionary<int,int> levelsCache = new();   // filled on load, updated on every buy

    [Header("Utility Buttons")]
[SerializeField] private Button saveNowButton;
[SerializeField] private Button resetButton;

    public CanvasGroup shopWindowParentCg;


    public GameObject maxTextObj;

    // ────────────────────────── 4. MonoBehaviour lifecycle ──────────────────
    private void Awake()
    {
       


    }

    public void RefreshShopPanelLanguage()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Setup(itemDatas[i], this);
        }
    }

    /// total cost of the first `level` tiers of a given item
    private int SumCost(ShopItemData data, int level)
    {
        int sum = 0;
        for (int i = 0; i < level && i < data.levels.Count; i++)
            sum += data.levels[i].needMoney;
        return sum;
    }

    public void ManualSave()
    {
        SaveProgress();
        CloseShopWindow();
        CurrencyManager.Instance.CloseCoinTextDisplay();
    }
    
    public void ResetShop()
    {
        if (CurrencyManager.Instance == null || BuffManager.Instance == null) return;
    
        int refund = 0;
    
        for (int i = 0; i < slots.Count; i++)
        {
            var slot   = slots[i];
            
            //int spent  = SumCost(itemDatas[i], slot.CurrentLevel);
            var data   = itemDatas.Find(x => x.itemType == slot.ItemType);  // Match by type, not index
            int spent  = SumCost(data, slot.CurrentLevel);
            
            refund    += spent;
    
            slot.InitLevel(0);
            levelsCache[(int)slot.ItemType] = 0;

            Debug.Log($"Slot{i}: Type={slot.ItemType}, Data Type={itemDatas[i].itemType}, Level={slot.CurrentLevel}, Refund={spent}");
        }

        Debug.Log($"Total refund: {refund}");
        int currentMoney = CurrencyManager.Instance.Coins;
        Debug.Log($"Current Money before refund: {currentMoney}");

        BuffManager.Instance.ResetAll();
        CurrencyManager.Instance.Add(refund, isBuffApplicable:false);
        Debug.Log($"Current Money after refund: {CurrencyManager.Instance.Coins}");

        SaveProgress();
        ClearInfoPanel();

        //BuffManager.Instance.ResetAll();
        //BuffManager.Instance.ApplyBuff();
        //for (int i = 0; i < slots.Count; i++)
        //{
        //    var slot = slots[i];
        //    var id   = (int)slot.ItemType;

        //    if (levelsCache.TryGetValue(id, out int level))
        //    {
        //        slot.InitLevel(level);
        //        BuffManager.Instance.ApplyLevel(itemDatas[i], level);
        //    }
        //    else
        //    {
        //        levelsCache[id] = 0;                 // brand-new SO added
        //    }
        //}
    
        SoundEffect.Instance.Play(SoundList.ShopRefundSe); 
    }

    // ────────────────────────── 5. Slot selection ──────────────────────────
    public void SelectSlot(ShopItemSlot slot)
    {
        selected = slot;
        ShopItemData d = itemDatas.Find(x => x.itemType == slot.ItemType);

        // Icon & title
        //infoIcon.sprite  = d.icon;
        //infoHeader.text  = d.displayName;
        infoHeader.text = L.ItemName(d.itemType);

        // Rich description (next-level bonus + total after purchase)
        float totalSoFar = 0f;
        for (int i = 0; i < slot.CurrentLevel; i++)
            totalSoFar += d.levels[i].addAmount;

        //infoDesc.text = string.Format(
        //    d.descriptionFormat,
        //    slot.AddNext,                  // {0} – next-level bonus
        //    totalSoFar + slot.AddNext);    // {1} – total after purchase

       
        infoDesc.text = L.ItemDesc(
        d.itemType,
        slot.AddNext,
        totalSoFar + slot.AddNext);

        if (slot.ReachedMax)
        {
            //infoDesc.text += "\n<color=#FF0000>MAX</color>";

            maxTextObj.SetActive(true);
        }
        else
        {
            maxTextObj.SetActive(false);
        }


            // Cost & button state
            infoCost.text = slot.ReachedMax ? "—" : $"{slot.PriceNext}";
        buyButton.interactable = !slot.ReachedMax &&
                                 CurrencyManager.Instance.CanAfford(slot.PriceNext);
    }

    void Start()          // do this *after* Awake so slots exist
    {

         // Guard: lists must match 1:1
        if (slots.Count != itemDatas.Count)
        {
            //Debug.LogError($"[ShopWindow] Slots ({slots.Count}) and ItemDatas ({itemDatas.Count}) counts differ!");
            enabled = false;
            return;
        }

        // Pair each slot with its data and give it a reference back to us
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].Setup(itemDatas[i], this);
        }

        //buyButton.onClick.AddListener(BuySelected);
        ClearInfoPanel();

        //saveNowButton.onClick.AddListener(ManualSave);
        //resetButton.  onClick.AddListener(ResetShop);




         int coins;                                       // -------- LOAD --------
    if (ShopSaveSystem.Load(out coins, out levelsCache))
    {

            Debug.Log("Shop Progress loaded successfully.");

        //CurrencyManager.Instance.LoadCoins(coins);

        // Clean slate
        BuffManager.Instance.ResetAll();
　　　　
        // Restore each slot and rebuild buffs
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            var id   = (int)slot.ItemType;

            if (levelsCache.TryGetValue(id, out int level))
            {
                slot.InitLevel(level);
                BuffManager.Instance.ApplyLevel(itemDatas[i], level);
            }
            else
            {
                levelsCache[id] = 0;                 // brand-new SO added
            }
        }
        }
        else
        {
            CurrencyManager.Instance.LoadCoins(1000);    // first run default
            foreach (var s in slots) levelsCache[(int)s.ItemType] = 0;
        }　
    }

    // ────────────────────────── 6. BUY logic ────────────────────────────────
    public void BuySelected()
    {
        if (selected == null || selected.ReachedMax) return;
        if(ShopManager.Instance.buyCooltime>0f) return;

        SoundEffect.Instance.Play(SoundList.ShopBuySe); 

        int price = selected.PriceNext;
        if (!CurrencyManager.Instance.Spend(price)) return;   // auto-save inside

        BuffManager.Instance.ApplyBuff(selected.ItemType, selected.AddNext);

        selected.LevelUp();
        levelsCache[(int)selected.ItemType] = selected.CurrentLevel;
        SaveProgress();                                       // saves LEVELS

        SelectSlot(selected); 
    }

    void SaveProgress()
    {
        if (CurrencyManager.Instance == null) return;
        ShopSaveSystem.Save(CurrencyManager.Instance.Coins, levelsCache);
    }

    void OnApplicationQuit() => SaveProgress();

    private void ClearInfoPanel()
    {
        //infoIcon.sprite   = null;
        infoHeader.text   = string.Empty;
        infoDesc.text     = string.Empty;
        infoCost.text     = string.Empty;
        buyButton.interactable = false;
    }

    public void CloseShopWindow()
    {
        //shopWindowParentCg.alpha = 0f;
        //shopWindowParentCg.blocksRaycasts = false;
        //shopWindowParentCg.gameObject.SetActive(false);
        ShopManager.Instance.CloseShopWindow();
    }

}