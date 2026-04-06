using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using Steamworks;
using System;


public class SteamLeaderboardManager : SingletonA<SteamLeaderboardManager>
{

    //private const string LEADERBOARD_NAME = "TotalDamage";
    //private SteamLeaderboard_t _currentLeaderboard;
    //private bool _initialized = false;
    private const string LEADERBOARD_HARD_NAME = "TotalDamage";
    private const string LEADERBOARD_NORMAL_NAME = "NormalTotalDamage";
    private SteamLeaderboard_t _hardLeaderboard;   // For "TotalDamage"
    private SteamLeaderboard_t _normalLeaderboard; // For "NormalTotalDamage"
    private bool _hardBoardInitialized = false;
    private bool _normalBoardInitialized = false;
    

    // Callbacks required by Steamworks.NET to handle async responses
    //private CallResult<LeaderboardFindResult_t> _findResult;
     private CallResult<LeaderboardFindResult_t> _findResultHard;
    private CallResult<LeaderboardFindResult_t> _findResultNormal;


    private CallResult<LeaderboardScoreUploaded_t> _uploadResult;
    
    //private CallResult<LeaderboardScoresDownloaded_t> _downloadResult;
    private CallResult<LeaderboardScoresDownloaded_t> _downloadResultHard;
    private CallResult<LeaderboardScoresDownloaded_t> _downloadResultNormal;

    public List<LeaderboardEntry> CurrentLeaderboardEntries = new List<LeaderboardEntry>();

    public enum LeaderboardType
    {
        Hard,
        Normal
    }

    public static event Action<List<LeaderboardEntry>, LeaderboardType> OnLeaderboardFetched;

    [System.Serializable]
    public struct LeaderboardEntry // simple struct to hold data for your UI
    {
        public string username;
        public int rank;
        public int score;
        public CSteamID steamID;
    }


    void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam Manager not initialized!");
            return;
        }

        uint currentID = SteamUtils.GetAppID().m_AppId;
    Debug.Log($"<color=cyan>Checking Steam Connection...</color>");
    Debug.Log($"<color=yellow>Connected to App ID: {currentID}</color>");
        

        // Initialize CallResults
        //_findResult = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFindResult);
        _uploadResult = CallResult<LeaderboardScoreUploaded_t>.Create(OnLeaderboardUploadResult);
        //_downloadResult = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);

         _findResultHard = CallResult<LeaderboardFindResult_t>.Create(OnHardLeaderboardFound);
        _findResultNormal = CallResult<LeaderboardFindResult_t>.Create(OnNormalLeaderboardFound);

         _downloadResultHard = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);
        _downloadResultNormal = CallResult<LeaderboardScoresDownloaded_t>.Create(OnLeaderboardScoresDownloaded);

        // Step 1: Find the leaderboard ID from Steam
        //FindLeaderboard(LEADERBOARD_HARD_NAME);
        
        FindHardLeaderboard();



        if(SteamManager.Initialized) {
        string name = SteamFriends.GetPersonaName();
        Debug.Log("Steam is connected! Logged in as: " + name);
        
        // Now try to upload a fake score to test
        //SteamLeaderboardManager.Instance.UpdateScore(777); 
    } else {
        Debug.Log("SteamManager did not initialize. Is Steam running?");
    }

    }

    void Update()
    {


        //TestDebugUpdateLearderBoard();

    }

    private void FindHardLeaderboard()
    {
        Debug.Log($"Attempting to find HARD leaderboard: {LEADERBOARD_HARD_NAME}...");
        SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(LEADERBOARD_HARD_NAME);
        _findResultHard.Set(hSteamAPICall);
    }

    private void OnHardLeaderboardFound(LeaderboardFindResult_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_bLeaderboardFound == 0 || bIOFailure)
        {
            Debug.LogError("HARD Leaderboard not found!");
        }
        else
        {
            _hardLeaderboard = pCallback.m_hSteamLeaderboard;
            _hardBoardInitialized = true;
            Debug.Log($"HARD Leaderboard found! ID: {_hardLeaderboard}");

            // CHAINING: Now that Hard is done, find Normal
            // We use a separate function and separate CallResult variable
            FindNormalLeaderboard();
        }
    }

    private void FindNormalLeaderboard()
    {
        Debug.Log($"Attempting to find NORMAL leaderboard: {LEADERBOARD_NORMAL_NAME}...");
        SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(LEADERBOARD_NORMAL_NAME);
        _findResultNormal.Set(hSteamAPICall);
    }

    private void OnNormalLeaderboardFound(LeaderboardFindResult_t pCallback, bool bIOFailure)
    {
        if (pCallback.m_bLeaderboardFound == 0 || bIOFailure)
        {
            Debug.Log("NORMAL Leaderboard not found!");
        }
        else
        {
            _normalLeaderboard = pCallback.m_hSteamLeaderboard;
            _normalBoardInitialized = true;
            Debug.Log($"NORMAL Leaderboard found! ID: {_normalLeaderboard}");
        }
    }

    [ContextMenu("Test Update Leaderboard")]
    void TestDebugUpdateLearderBoard()
    {
        //if (Input.GetKeyDown(KeyCode.B))
        {
            UpdateScore(10001);
            UpdateScoreNormalMode(700);
        }
    }



    //private void FindLeaderboard(string name)
    //{
    //    Debug.Log($"Attempting to find leaderboard: {name}...");
    //    SteamAPICall_t hSteamAPICall = SteamUserStats.FindLeaderboard(name);
    //    _findResult.Set(hSteamAPICall);
    //}

    //private void OnLeaderboardFindResult(LeaderboardFindResult_t pCallback, bool bIOFailure)
    //{
    //    if (pCallback.m_bLeaderboardFound == 0 || bIOFailure)
    //    {
    //        Debug.LogError("Steam Leaderboard not found or IO Failure.");
    //    }
    //    else
    //    {
    //        // 4. IMPORTANT: Identify WHICH leaderboard we just found
    //        SteamLeaderboard_t foundBoard = pCallback.m_hSteamLeaderboard;
    //        string foundName = SteamUserStats.GetLeaderboardName(foundBoard);

    //        Debug.Log($"Leaderboard found! Name: {foundName} | ID: {foundBoard}");

    //        if (foundName == LEADERBOARD_HARD_NAME)
    //        {
    //            _hardLeaderboard = foundBoard;
    //            _hardBoardInitialized = true;
                
    //            // CHAINING: Now that we found Hard, let's go find Normal
    //            FindLeaderboard(LEADERBOARD_NORMAL_NAME);
    //        }
    //        else if (foundName == LEADERBOARD_NORMAL_NAME)
    //        {
    //            _normalLeaderboard = foundBoard;
    //            _normalBoardInitialized = true;
    //            Debug.Log("Both Leaderboards initialized successfully.");
    //        }
    //    }
    //}

    public void ResetScoreToZero()
    {
        if (!_hardBoardInitialized) return;

        // Note: Ideally you should also split _uploadResult if you want to upload two things at once reliably,
        // but for a reset, executing them sequentially or hoping for the best is usually okay. 
        // For perfect safety, duplicate _uploadResult too.
        
        SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(
            _hardLeaderboard, 
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, 
            0, 
            null, 
            0
        );

        SteamAPICall_t hSteamAPICall2 = SteamUserStats.UploadLeaderboardScore(
            _normalLeaderboard, 
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodForceUpdate, 
            0, 
            null, 
            0
        );

        // This might still cause a race condition, but it's acceptable for a reset function.
        _uploadResult.Set(hSteamAPICall);
        _uploadResult.Set(hSteamAPICall2);
        
        AchievementManager.Instance.progressData.highestHistoryDamage = 0;
        AchievementManager.Instance.progressData.highestHistoryDamageNormalMode = 0;
        AchievementManager.Instance.SaveProgress();
        Debug.Log($"Resetting Scores to 0");

    }

    public void UpdateScoreNormalMode(int totalDamage)
    {
        if (!_normalBoardInitialized)
        {
            Debug.LogWarning("Normal Leaderboard not initialized yet.");
            return;
        }

        Debug.Log($"Uploading to NORMAL: {totalDamage}");

        // Use _normalLeaderboard handle
        SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(
            _normalLeaderboard, 
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, 
            totalDamage, 
            null, 
            0
        );

        _uploadResult.Set(hSteamAPICall);
        
    }

    public void UpdateScore(int totalDamage)
    {
        if (!_hardBoardInitialized)
        {
            Debug.LogWarning("Hard/Total Leaderboard not initialized yet.");
            return;
        }

        Debug.Log($"Uploading to HARD/TOTAL: {totalDamage}");

        // Use _hardLeaderboard handle
        SteamAPICall_t hSteamAPICall = SteamUserStats.UploadLeaderboardScore(
            _hardLeaderboard, 
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest, 
            totalDamage, 
            null, 
            0
        );

        _uploadResult.Set(hSteamAPICall);
    }

    private void OnLeaderboardUploadResult(LeaderboardScoreUploaded_t pCallback, bool bIOFailure)
    {
        //if (pCallback.m_bSuccess == 0 || bIOFailure)
        //{
        //    Debug.LogError("Score upload failed.");
        //}
        //else
        //{
        //    Debug.Log($"Score uploaded! Rank: {pCallback.m_nGlobalRankNew} Score: {pCallback.m_nScore} Changed: {pCallback.m_bScoreChanged}");
        //}

        // Check 1: Did the network packet fail?
    if (bIOFailure)
    {
        Debug.LogError("Score upload failed: IO Failure (Connection to Steam lost?)");
        return;
    }

    // Check 2: Did Steam reject the data?
    if (pCallback.m_bSuccess == 0)
    {
        Debug.LogError("Score upload failed: Steam rejected the upload. Check Leaderboard 'Writes' permission in Steamworks (Must be 'Trusted').");
        return;
    }

    // Success
    Debug.Log($"Score uploaded! Rank: {pCallback.m_nGlobalRankNew} Score: {pCallback.m_nScore} Changed: {pCallback.m_bScoreChanged}");

    }

    // =========================================================
    // DOWNLOADING SCORES (For UI)
    // =========================================================

 

    //private void GetLeaderboardData(SteamLeaderboard_t boardHandle, int rangeStart, int rangeEnd)
    //{
    //    // Safety check to ensure the handle is valid
    //    if (boardHandle.m_SteamLeaderboard == 0) return;

    //    SteamAPICall_t hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(
    //        boardHandle, 
    //        ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 
    //        rangeStart, 
    //        rangeEnd
    //    );

    //    //_downloadResult.Set(hSteamAPICall);

    //}



    private void OnLeaderboardScoresDownloaded(LeaderboardScoresDownloaded_t pCallback, bool bIOFailure)
    {
         if (bIOFailure) { Debug.LogError("Download failed."); return; }

        List<LeaderboardEntry> entriesForThisBatch = new List<LeaderboardEntry>();
        LeaderboardType type = LeaderboardType.Hard;

        // Accurate checking of which board this data belongs to
        if (pCallback.m_hSteamLeaderboard == _hardLeaderboard)
        {
            type = LeaderboardType.Hard;
        }
        else if (pCallback.m_hSteamLeaderboard == _normalLeaderboard)
        {
            type = LeaderboardType.Normal;
        }
        else 
        {
            Debug.LogWarning("Received download callback for unknown leaderboard handle.");
        }

        for (int i = 0; i < pCallback.m_cEntryCount; i++)
        {
            LeaderboardEntry_t steamEntry;
            SteamUserStats.GetDownloadedLeaderboardEntry(pCallback.m_hSteamLeaderboardEntries, i, out steamEntry, null, 0);

            entriesForThisBatch.Add(new LeaderboardEntry
            {
                rank = steamEntry.m_nGlobalRank,
                score = steamEntry.m_nScore,
                username = SteamFriends.GetFriendPersonaName(steamEntry.m_steamIDUser),
                steamID = steamEntry.m_steamIDUser
            });
        }

        Debug.Log($"Downloaded {entriesForThisBatch.Count} entries for {type} Mode.");

        OnLeaderboardFetched?.Invoke(entriesForThisBatch, type);
    
    }

    //public void FetchTop100()
    //{
    //     GetLeaderboardData(_hardLeaderboard, 1, 100);
    //}

    // Optional: You can overload this to specify which board to view
    // Usage: FetchTop100(true) for Normal, FetchTop100(false) for Hard
    public void FetchTop100(bool isNormalMode = false)
    {
         if (isNormalMode)
        {
            if(!_normalBoardInitialized) return;

            // Request NORMAL data
            SteamAPICall_t hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(
                _normalLeaderboard, 
                ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 
                1, 
                100
            );

            // Assign to the NORMAL CallResult
            _downloadResultNormal.Set(hSteamAPICall);
        }
        else
        {
            if(!_hardBoardInitialized) return;

            // Request HARD data
            SteamAPICall_t hSteamAPICall = SteamUserStats.DownloadLeaderboardEntries(
                _hardLeaderboard, 
                ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 
                1, 
                100
            );

            // Assign to the HARD CallResult
            _downloadResultHard.Set(hSteamAPICall);
        }
    }

}

