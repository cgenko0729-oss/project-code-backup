using Cysharp.Threading.Tasks;
using DG.Tweening;
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using Steamworks;
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;    
using UnityEngine;
using UnityEngine.UI;     

public class SteamDLCManager : Singleton<SteamDLCManager>
{

   // 2. PASTE YOUR NEW DLC APP ID HERE (Replacing 0000000)
    // You get this number from the Steamworks dashboard after clicking "Add New DLC"
    public uint dlcMusAppId = 4390330; 
    public uint dlcEliteAppId = 4413670;


    void Start()
    {
        // 3. Wait a frame or check immediately if SteamManager is ready
        //CheckDLCA();
    }

    [ContextMenu("UnlockDLCA")]
    public void TestActivateDLCA()
    {
         AchievementManager.Instance.UnlockDlcMusclePet();
    }

    public void CheckDLCA()
    {
        // Safety check: Make sure Steam API is actually running
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam Manager not initialized. Cannot check DLC.");
            //isDLC_A_Installed = false;
            return;
        }

        // 4. Create the AppId_t struct required by Steamworks.NET
        AppId_t dlcID = new AppId_t(dlcMusAppId);

        // 5. Ask Steam if the user owns AND has installed this DLC
        // BIsDlcInstalled returns true if the user owns the DLC and it is currently installed.
        CurrencyManager.Instance.isDLCMusIsPurchased = SteamApps.BIsDlcInstalled(dlcID);

        if (CurrencyManager.Instance.isDLCMusIsPurchased)
        {
            Debug.Log($"[SteamDLCManager] DLC {dlcMusAppId} is INSTALLED. Unlocking content.");

            AchievementManager.Instance.UnlockDlcMusclePet();
            CurrencyManager.Instance.SetDLCMusUiActive();

        }
        else
        {
            Debug.Log($"[SteamDLCManager] DLC {dlcMusAppId} is NOT installed.");
        }
    }

    public void CheckDlcB()
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam Manager not initialized. Cannot check DLC.");
            return;
        }
        AppId_t dlcID = new AppId_t(dlcEliteAppId);
        CurrencyManager.Instance.isDLCEliteIsPurchased = SteamApps.BIsDlcInstalled(dlcID);
        if (CurrencyManager.Instance.isDLCEliteIsPurchased)
        {
            Debug.Log($"[SteamDLCManager] DLC {dlcEliteAppId} is INSTALLED. Unlocking content.");
            AchievementManager.Instance.UnlockDlcElitePet();
            CurrencyManager.Instance.SetDLCEliteUiActive();
        }
        else
        {
            Debug.Log($"[SteamDLCManager] DLC {dlcEliteAppId} is NOT installed.");
        }

    }

}

