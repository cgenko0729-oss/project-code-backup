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
using Steamworks;

public class LeaderboardRowUI : MonoBehaviour
{
   [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _usernameText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private RawImage _avatarImage; 

    public void SetData(int rank, string username, int score, CSteamID steamID)
    {
        _rankText.text = $"#{rank}";
        _usernameText.text = username;
        _scoreText.text = score.ToString("N0"); // N0 formats numbers with commas (e.g. 1,000)
    
        GetAvatar(steamID);
    
    }

    private void GetAvatar(CSteamID steamID)
    {
        // Get the handle for the medium or large avatar
        int imageId = SteamFriends.GetLargeFriendAvatar(steamID);

        // If the imageId is -1, the avatar isn't loaded yet. 
        // Usually, for leaderboards, Steam caches them when downloading the entry, 
        // so it should be ready immediately.
        if (imageId != -1) 
        {
            _avatarImage.texture = SteamAvatarHelper.GetSteamImageAsTexture2D(imageId);
        }
        else 
        {
            // Optional: Set a default "Loading" or "No Avatar" image here
            // _avatarImage.texture = _defaultIcon;
        }
    }

//    private void OnDestroy() {
//    if(_avatarImage.texture != null) {
//        Destroy(_avatarImage.texture);
//    }
//}

}

