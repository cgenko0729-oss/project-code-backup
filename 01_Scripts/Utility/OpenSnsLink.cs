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
using System;

public class OpenSnsLink : MonoBehaviour
{
    //[SerializeField] private string url = "https://x.com/FuwaSurvivor";

    //public void OpenLink()
    //{
    //    Application.OpenURL(url);
    //}

    public enum LinkType { Web, DiscordInvite, Email, SteamStore }
    [SerializeField] private LinkType linkType = LinkType.Web;

    // Generic web / Discord
    [SerializeField] private string url = "https://example.com";

    // Email
    [SerializeField] private string emailAddress = "fuwa@gmail.com";
    [SerializeField] private string emailSubject = "FuwaFuwa Survivors Support";
    [SerializeField] private string emailBody =
        "Please describe your issue here:%0D%0A- Platform:%0D%0A- Version:%0D%0A- Steps to reproduce:";

    // Steam
    [SerializeField] private uint steamAppId = 123456u; // Å© replace with your real AppID

    public void Open()
    {
        switch (linkType)
        {
            case LinkType.Web:
                OpenURL(url);
                break;
            case LinkType.DiscordInvite:
                OpenURL(url);
                break;

            case LinkType.Email:
                OpenEmail(emailAddress, emailSubject, emailBody);
                break;

            case LinkType.SteamStore:
                OpenSteamStore(steamAppId);
                break;
        }
    }

    private void OpenURL(string targetUrl)
    {
        if (string.IsNullOrWhiteSpace(targetUrl)) return;
        Application.OpenURL(targetUrl);
    }

    private void OpenEmail(string to, string subject, string body)
    {
        if (string.IsNullOrWhiteSpace(to)) return;
        // URL-encode subject/body
        string s = Uri.EscapeDataString(subject ?? "");
        string b = Uri.EscapeDataString(body ?? "");
        string mailto = $"mailto:{to}?subject={s}&body={b}";
        Application.OpenURL(mailto);
    }

    private void OpenSteamStore(uint appId)
    {
        // If your game runs under Steam + you use Steamworks.NET, open the Steam overlay store page.
        #if STEAMWORKS_NET
        //if (SteamManager.Initialized)
        //{
        //    SteamFriends.ActivateGameOverlayToStore((AppId_t)appId, EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
        //    return;
        //}
        #endif

        // Fallback: open the web store page (works for non-Steam builds too)
        OpenURL($"https://store.steampowered.com/app/4043830/FuwaFuwa_Survivors/?curator_clanid=4777282");
    }


}

