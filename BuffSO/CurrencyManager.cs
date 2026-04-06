using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


//execution order = Currencymanager > ShopWindow

[DefaultExecutionOrder(-100)]              // runs before almost everything
public class CurrencyManager : SingletonA<CurrencyManager>
{
    public int Coins = 1000;
    public TextMeshProUGUI moneyText;

    public bool isDLCMusIsPurchased = false;
    public bool isDLCEliteIsPurchased = false;

    private const string FILE = "r";       // EasyFileSave ID

    public Image dlcMusTickImage;
    public Image dlcEliteTickImage;

    public float dlcCheckTimer = 4f;
    public int dlcCheckCount = 0;

    /// <remarks>
    /// SingletonA calls <c>OnAwake()</c> exactly once on the real instance.
    /// Do *all* loading here so every other script sees the correct value.
    /// </remarks>
    protected override void OnAwake()
    {
        LoadCoinFromSaveFile();            // ‡@ read disk before anyone else
    }


    public bool CanAfford(int price) => Coins >= price;

    public void Add(int amount, bool isBuffApplicable = true)
    {
        if (amount <= 0) return;

        // Šl“¾—Ê‚Ìã¸—¦(%)
        float getCoinMultiplier = 0;
        if (StageManager.Instance.mapData.stageDifficulty != DifficultyType.None)
        {
            if (isBuffApplicable)
            {
                 getCoinMultiplier = BuffManager.Instance.gobalGoldGain +       // BuffManager‚Ìã¸—¦
                                ((int)StageManager.Instance.mapData.stageDifficulty - 1) * 25.0f;   // “ïˆÕ“x‚É‚æ‚éã¸—¦(25%‚¸‚Âã¸‚·‚é)
            }
            else
            {
                getCoinMultiplier = 0;
            }
           
        }

        Coins += (int)(amount * (1 + getCoinMultiplier / 100));
        SaveCoinToFile();                  // ‡A auto-save
        RefreshUI();
    }

    /// <returns>true if the purchase succeeded</returns>
    public bool Spend(int price)
    {
        if (price > Coins) return false;
        Coins -= price;
        SaveCoinToFile();                  // ‡A auto-save
        RefreshUI();
        return true;
    }

    /// Used only by Shop reset / debug
    public void LoadCoins(int value)
    {
        Coins = value;
        SaveCoinToFile();
        RefreshUI();
    }

    private void Start()
    {
        CloseCoinTextDisplay();  // loading already done


        DLCCheckOnce();
       

    }

    public void DLCCheckOnce()
    {
        dlcCheckCount++;
        EasyFileSave file = new EasyFileSave(FILE);
        if (file.Load())
        {
            isDLCMusIsPurchased = file.GetBool("isDLCMusIsPurchased", isDLCMusIsPurchased);
            isDLCEliteIsPurchased = file.GetBool("isDLCEliteIsPurchased", isDLCEliteIsPurchased);

            if (!isDLCMusIsPurchased)
            {
                if(SteamDLCManager.Instance != null) SteamDLCManager.Instance.CheckDLCA();   
            }

            if (!isDLCEliteIsPurchased)
            {
                if(SteamDLCManager.Instance != null) SteamDLCManager.Instance.CheckDlcB();
            }

            if(isDLCEliteIsPurchased)
            {
                SetDLCEliteUiActive();
            }
            if (isDLCMusIsPurchased)
            {
                SetDLCMusUiActive();
            }

          }
    }

    public void SetDLCEliteUiActive()
    {
        if (dlcEliteTickImage) dlcEliteTickImage.gameObject.SetActive(true);
    }

    public void SetDLCMusUiActive()
    {
        if (dlcMusTickImage) dlcMusTickImage.gameObject.SetActive(true);
    }



    public void SaveDLCStatusToFile()
    {
               EasyFileSave file = new EasyFileSave(FILE);
        file.Add("isDLCMusIsPurchased", isDLCMusIsPurchased);
        file.Add("isDLCEliteIsPurchased", isDLCEliteIsPurchased);
        file.Save();
    }

    private void Update()
    {
        RefreshUI();

        dlcCheckTimer -= Time.deltaTime;
        if(dlcCheckTimer <= 0f && dlcCheckCount < 3)
        {
            DLCCheckOnce();
            dlcCheckTimer = 4f; // reset timer for next check
        }

    }

    public void OpenCoinTextDisplay()
    {
        if (moneyText == null) return;
        moneyText.gameObject.SetActive(true);
        moneyText.text = Coins.ToString();
    }

    public void CloseCoinTextDisplay()
    {
        if (moneyText == null) return;
        moneyText.gameObject.SetActive(false);
    }

    private void RefreshUI()
    {
        if (moneyText != null && moneyText.gameObject.activeInHierarchy)
            moneyText.text = Coins.ToString();
    }

    private void LoadCoinFromSaveFile()
    {
        EasyFileSave file = new EasyFileSave(FILE);
        if (file.Load())
        {
            Coins = file.GetInt("coins", Coins);

           
        }
    }

    public void SaveCoinToFile()
    {
        EasyFileSave file = new EasyFileSave(FILE);
        file.Add("isDLCMusIsPurchased", isDLCMusIsPurchased);
        file.Add("isDLCEliteIsPurchased", isDLCEliteIsPurchased);
        file.Add("coins", Coins);
        file.Save();
    }

    [ContextMenu("Test Add 10000 Coin")]    
    public void TestAdd10000Coin()
    {
        Add(10000);


    }

}

