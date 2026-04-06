using Cysharp.Threading.Tasks;
using DG.Tweening;
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;    
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;     
using static SteamLeaderboardManager;

public class LeaderboardUIManager : Singleton<LeaderboardUIManager>
{
    [Header("UI References")]
    [SerializeField] private GameObject _leaderboardWindow; // The main panel
    [SerializeField] private Transform _contentContainer;   // The Content object inside the Scroll View
    [SerializeField] private LeaderboardRowUI _rowPrefab;   // The prefab you created in Step 2
    [SerializeField] private GameObject _loadingSpinner;    // Optional: A simple rotating icon

    [SerializeField] private Transform _contentContainerNormalScroll; // The Content object inside the Scroll View for Normal mode

    public CanvasGroup messageGroup;
    public CanvasGroup leaderboardGroup;

    public  RectTransform messageScrollContent;
    public RectTransform leaderboardScrollContent;
    public RectTransform leaderboardScrollContentNormal;
    public RectTransform emptyTrans;

    private Sequence _flipSeq;
    private const float FlipDuration = 0.5f;

    private float autoFlipPageCnt = 7.5f;

    public float messageScrollContentFlipTargetPosX = 181f;
    private float leaderboardScrollContentFlipTargetPosX = 7f;

    public float messageScrollContentFlipStartPosX = 597f;
    public float leaderboardScrollContentFlipStartPosX = 490f;

    public float messageScrollContentFlipEndPosX = -249f;
    public float leaderboardScrollContentFlipEndPosX = -512f;

    public bool isInMessagePage = true;

    public int flipTime = 0;

    public List<GameObject> anouncementObjlist;
    public List<GameObject> rankingObjList;

    public TextMeshProUGUI yourHighestScoreText;
    public TextMeshProUGUI yourHighestScoreTextNormal;

    public GameObject joinLeaderBoardToggleObj;
    public GameObject leaveLeaderBoardToggleObj;
    public Image toggleImage;
    public Color toggleActiveColor = new Color(1f,1f,1f,1f);

    public Image deleteDataConfirmFillImg;
    public GameObject deleteDataConfirmMenu;
    public bool isInDeleteDataConfirmMenu = false;
    public float confirmDeleteInterval = 2f;

     [Header("Refresh Settings")]
    [SerializeField] private float _refreshCooldown = 5.0f; // Prevent spamming
    private float _currentRefreshTimer = 0f;

    public List<GameObject> rankNormalList;
    public List<GameObject> rankHardList;
    public bool isNormalModeRank = true;

    public float refreshOnceCnt = 10f;

    private void OnEnable()
    {
        // Subscribe to the event
        SteamLeaderboardManager.OnLeaderboardFetched += RefreshUI;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        SteamLeaderboardManager.OnLeaderboardFetched -= RefreshUI;
    }

    public void SwithToNormalRank()
    {
        for(int i=0;i< rankNormalList.Count;i++)
        {
            rankNormalList[i].SetActive(true);
        }
        for(int j=0;j< rankHardList.Count;j++)
        {
            rankHardList[j].SetActive(false);
        }

    }

    public void SwithToHardRank()
    {
        for(int i=0;i< rankNormalList.Count;i++)
        {
            rankNormalList[i].SetActive(false);
            
        }
        for(int j=0;j< rankHardList.Count;j++)
        {
            rankHardList[j].SetActive(true);
        }
    }

    public void ChangeToNextRankMode()
    {
        if(isNormalModeRank)
        {
            isNormalModeRank = false;
            SwithToHardRank();
        }
        else
        {
            isNormalModeRank = true;
            SwithToNormalRank();
        }

    }

    void UpdateRankingMenu()
    {
        confirmDeleteInterval -= Time.deltaTime;

        if (_currentRefreshTimer > 0f)
        {
            _currentRefreshTimer -= Time.deltaTime;
        }

        if (isInMessagePage) return;

        bool refreshInput = Input.GetKeyDown(KeyCode.R);
        if(Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                refreshInput = true;
            }

        if (refreshInput)
            {
                ManualRefreshLeaderboard();
            }

        //if press X key:
        if (Input.GetKeyDown(KeyCode.X))
        {
            if(isInDeleteDataConfirmMenu) return;
            if(confirmDeleteInterval > 0f) return;

            OpenDeleteDataConfirmMenu();
            SoundEffect.Instance.Play(SoundList.DebugkeySe);
            Debug.Log("Open Delete Data Confirm Menu");
        }

        if(Gamepad.current != null)
        {
            if (Gamepad.current.startButton.wasPressedThisFrame)
            {
                if(isInDeleteDataConfirmMenu) return;
                 if(confirmDeleteInterval > 0f) return;

                OpenDeleteDataConfirmMenu();
                SoundEffect.Instance.Play(SoundList.DebugkeySe);
            }
        }

        if (isInDeleteDataConfirmMenu)
        {
            if (Gamepad.current != null && Gamepad.current.buttonSouth.isPressed)
            {
                CloseDeleteDataConfirmMenu();
            }


            //if press and hold X key:
            if (Input.GetKey(KeyCode.X))
            {
                deleteDataConfirmFillImg.fillAmount += Time.deltaTime / 4f; // 2 seconds to fill
                if (deleteDataConfirmFillImg.fillAmount >= 1f)
                {
                    // Confirm deletion
                    SteamLeaderboardManager.Instance.ResetScoreToZero();
                    SoundEffect.Instance.Play(SoundList.DebugkeySe);
                    CloseDeleteDataConfirmMenu();
                    deleteDataConfirmFillImg.fillAmount = 0f;
                    confirmDeleteInterval = 2f;
                }
            }
            else if (Gamepad.current != null && Gamepad.current.startButton.isPressed)
            {
                deleteDataConfirmFillImg.fillAmount += Time.deltaTime / 4f; // 2 seconds to fill
                if (deleteDataConfirmFillImg.fillAmount >= 1f)
                {
                    // Confirm deletion
                    SteamLeaderboardManager.Instance.ResetScoreToZero();
                    SoundEffect.Instance.Play(SoundList.DebugkeySe);
                    CloseDeleteDataConfirmMenu();
                    deleteDataConfirmFillImg.fillAmount = 0f;
                    confirmDeleteInterval = 2f;
                }
            }
            else
            {
                // Not holding, reset fill
                deleteDataConfirmFillImg.fillAmount = 0f;
            }


        }


    }

    public void ManualRefreshLeaderboard()
    {
        // 1. Check Cooldown
        if (_currentRefreshTimer > 0f)
        {
            Debug.Log($"Please wait {_currentRefreshTimer:F1}s before refreshing again.");
            // Optional: Play a "buzzer/error" sound here
            return;
        }

        // 2. Visual Feedback
        Debug.Log("Refeshing Leaderboard...");
        SoundEffect.Instance.Play(SoundList.DebugkeySe); // Play a sound
        
        //if (_loadingSpinner != null) _loadingSpinner.SetActive(true);

        // 3. Clear UI immediately so user sees it wiping
        ClearExistingRows(_contentContainer);
        ClearExistingRows(_contentContainerNormalScroll);

        // 4. Request new data from Steam
        // Note: Steamworks usually caches data slightly, but calling DownloadEntries 
        // usually forces a check or returns the latest known state.
        SteamLeaderboardManager.Instance.FetchTop100(isNormalMode: false);
        SteamLeaderboardManager.Instance.FetchTop100(isNormalMode: true);

        // 5. Set Cooldown
        _currentRefreshTimer = _refreshCooldown;

        yourHighestScoreText.text = Mathf.FloorToInt(AchievementManager.Instance.progressData.highestHistoryDamage).ToString();
        yourHighestScoreTextNormal.text = Mathf.FloorToInt(AchievementManager.Instance.progressData.highestHistoryDamageNormalMode).ToString();
    }

    //context menu test
    [ContextMenu("Test Open Delete Data Confirm Menu")]
    public void OpenDeleteDataConfirmMenu()
    {
        autoFlipPageCnt = 28f;
        deleteDataConfirmMenu.SetActive(true);
        MenuOpenAnimator menuAni  = deleteDataConfirmMenu.GetComponent<MenuOpenAnimator>();
        menuAni.PlayeMenuAni(show:true);
        isInDeleteDataConfirmMenu = true;

    }

    public void CloseDeleteDataConfirmMenu()
    {
        MenuOpenAnimator menuAni  = deleteDataConfirmMenu.GetComponent<MenuOpenAnimator>();
        menuAni.PlayeMenuAni(show:false);
        isInDeleteDataConfirmMenu = false;

    }

    public void FlipPage()
    {

        autoFlipPageCnt = 21.9f;

        // Toggle target
        SetPage(!isInMessagePage);
    }

    public void SetPage(bool goToMessagePage)
{
    // If already there, ignore
    if (goToMessagePage == isInMessagePage) return;

    // Stop previous animation cleanly
    if (_flipSeq != null && _flipSeq.IsActive()) _flipSeq.Kill();

    // Optional: also kill any direct tweens on targets (extra safe)
    messageScrollContent.DOKill();
    leaderboardScrollContent.DOKill();


    messageGroup.DOKill();
    leaderboardGroup.DOKill();

    // Decide directions
    RectTransform outRect,outRect2, inRect, inRect2;
    CanvasGroup outGroup, inGroup;

    float outFromX, outToX;
    float inFromX, inToX;

    if (isInMessagePage)
    {
        // Message -> Leaderboard
        outRect = messageScrollContent;
        outRect2 = emptyTrans;

        outGroup = messageGroup;
        outFromX = messageScrollContentFlipTargetPosX;
        outToX   = messageScrollContentFlipEndPosX;

        inRect = leaderboardScrollContent;
        inRect2 = leaderboardScrollContentNormal;
        inGroup = leaderboardGroup;

        inFromX = leaderboardScrollContentFlipStartPosX;
        inToX   = leaderboardScrollContentFlipTargetPosX;

        for(int i=0;i< anouncementObjlist.Count;i++)
        {
            anouncementObjlist[i].SetActive(false);
        }
        for(int j=0;j< rankingObjList.Count;j++)
        {
            rankingObjList[j].SetActive(true);
        }

     }
    else
    {
        // Leaderboard -> Message
        outRect = leaderboardScrollContent;
        outGroup = leaderboardGroup;
        outRect2 = leaderboardScrollContentNormal;

        outFromX = leaderboardScrollContentFlipTargetPosX;
        outToX   = leaderboardScrollContentFlipEndPosX;

        inRect = messageScrollContent;
        inRect2 = emptyTrans;
            inGroup = messageGroup;
        inFromX = messageScrollContentFlipStartPosX;
        inToX   = messageScrollContentFlipTargetPosX;

        for(int i=0;i< anouncementObjlist.Count;i++) anouncementObjlist[i].SetActive(true);
        for(int j=0;j< rankingObjList.Count;j++) rankingObjList[j].SetActive(false);

            SwithToNormalRank();

        }

    // Ensure "in" page starts at correct position & invisible BEFORE tween starts
    SetAnchoredPosX(inRect, inFromX);
        SetAnchoredPosX(inRect2, inFromX);
        inGroup.alpha = 0f;

    // Optional: block interaction during animation
    outGroup.blocksRaycasts = false;
    outGroup.interactable = false;
    inGroup.blocksRaycasts = false;
    inGroup.interactable = false;
   
        // Build: move+fade out AND move+fade in at the same time
        _flipSeq = DOTween.Sequence();

    // Out page: move to end + fade to 0
    _flipSeq.Join(outRect.DOAnchorPosX(outToX, FlipDuration));
    _flipSeq.Join(outRect2.DOAnchorPosX(outToX, FlipDuration));
    
    _flipSeq.Join(outGroup.DOFade(0f, FlipDuration));

    // In page: move to target + fade to 1
    _flipSeq.Join(inRect.DOAnchorPosX(inToX, FlipDuration));
        _flipSeq.Join(inRect2.DOAnchorPosX(inToX, FlipDuration));

        _flipSeq.Join(inGroup.DOFade(1f, FlipDuration));



    _flipSeq.OnComplete(() =>
    {
        // Now we've switched
        isInMessagePage = goToMessagePage;

        // Re-enable interaction only on the visible page
        if (isInMessagePage)
        {
            messageGroup.blocksRaycasts = true;
            messageGroup.interactable = true;

            leaderboardGroup.blocksRaycasts = false;
            leaderboardGroup.interactable = false;
        }
        else
        {
            leaderboardGroup.blocksRaycasts = true;
            leaderboardGroup.interactable = true;

            messageGroup.blocksRaycasts = false;
            messageGroup.interactable = false;
        }
    });
}

// Helper to set anchoredPosition.x without touching y
private static void SetAnchoredPosX(RectTransform rt, float x)
{
    var p = rt.anchoredPosition;
    p.x = x;
    rt.anchoredPosition = p;
}

    private void ApplyPageInstant(bool messagePage)
{
    if (messagePage)
    {
        SetAnchoredPosX(messageScrollContent, messageScrollContentFlipTargetPosX);
        messageGroup.alpha = 1f;

        SetAnchoredPosX(leaderboardScrollContent, leaderboardScrollContentFlipEndPosX); // or StartPosX if you prefer
        //SetAnchoredPosX(leaderboardScrollContentNormal, leaderboardScrollContentFlipEndPosX);
            leaderboardGroup.alpha = 0f;

        messageGroup.blocksRaycasts = true;  messageGroup.interactable = true;
        leaderboardGroup.blocksRaycasts = false; leaderboardGroup.interactable = false;
    }
    else
    {
        SetAnchoredPosX(leaderboardScrollContent, leaderboardScrollContentFlipTargetPosX);
        leaderboardGroup.alpha = 1f;

        SetAnchoredPosX(messageScrollContent, messageScrollContentFlipEndPosX);
        messageGroup.alpha = 0f;

        leaderboardGroup.blocksRaycasts = true; leaderboardGroup.interactable = true;
        messageGroup.blocksRaycasts = false; messageGroup.interactable = false;
    }
}


    // Call this method from your "Leaderboard" Button in the Main Menu
    public void OpenLeaderboard()
    {
        _leaderboardWindow.SetActive(true);
        
        // Show loading state
        if(_loadingSpinner != null) _loadingSpinner.SetActive(true);
        ClearExistingRows(_contentContainer);
        ClearExistingRows(_contentContainerNormalScroll);

        // Ask Steam Manager for the top 100
        SteamLeaderboardManager.Instance.FetchTop100(isNormalMode: false);
        SteamLeaderboardManager.Instance.FetchTop100(isNormalMode: true);
    }

    //context menu test
    [ContextMenu("Test Open Leaderboard")]
    void TestOpenLeardboard()
    {
        if(SteamManager.Initialized)
        {
           OpenLeaderboard();
        }
    }
    public void CloseLeaderboard()
    {
        _leaderboardWindow.SetActive(false);
    }

    private void Start()
    {
        //OpenLeaderboard();

        Invoke(nameof(SwithToNormalRank), 2.8f);

        ApplyPageInstant(isInMessagePage);

        DOVirtual.DelayedCall(2.1f, TestOpenLeardboard);

        float highestScore = AchievementManager.Instance.progressData.highestHistoryDamage;
        yourHighestScoreText.text = Mathf.FloorToInt(highestScore).ToString();

        float highestScoreNormal = AchievementManager.Instance.progressData.highestHistoryDamageNormalMode;
        yourHighestScoreTextNormal.text = Mathf.FloorToInt(highestScoreNormal).ToString();


        for (int i= 0; i < rankingObjList.Count; i++)
        {
            rankingObjList[i].SetActive(false);
        }


        //Init Toggle Leaderboard Join State
        bool isJoinedLeaderboard = AchievementManager.Instance.progressData.isParticipatedInSteamLeaderboard;
        if(isJoinedLeaderboard)
        {
            joinLeaderBoardToggleObj.SetActive(true);
            leaveLeaderBoardToggleObj.SetActive(false);
            toggleImage.color = toggleActiveColor;
        }
        else
        {
            joinLeaderBoardToggleObj.SetActive(false);
            leaveLeaderBoardToggleObj.SetActive(true);
            toggleImage.color = Color.white;
        }

    }



    private void Update()
    {
        autoFlipPageCnt -= Time.deltaTime;
        if (autoFlipPageCnt <= 0f)
        {
            if(flipTime >=1)  return;
            flipTime++;
            
            
            FlipPage();
            autoFlipPageCnt = 11f;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleJoinLeaderBoard();
        }

        if(Gamepad.current != null)
        {
            if (Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                ToggleJoinLeaderBoard();
            }
        }

        UpdateRankingMenu();


        refreshOnceCnt -= Time.deltaTime;
        if(refreshOnceCnt <= 0f)
        {
            ManualRefreshLeaderboard();
            refreshOnceCnt = 999f;
        }

    }

    public void ToggleJoinLeaderBoard()
    {
        if (isInMessagePage) return;
        
            bool isJoinedLeaderboard = AchievementManager.Instance.progressData.isParticipatedInSteamLeaderboard;
            if(isJoinedLeaderboard)
            {
                AchievementManager.Instance.progressData.isParticipatedInSteamLeaderboard = false;
                joinLeaderBoardToggleObj.SetActive(false);
                leaveLeaderBoardToggleObj.SetActive(true);
                AchievementManager.Instance.SaveProgress();
                SoundEffect.Instance.Play(SoundList.DebugkeySe);
                    toggleImage.color = Color.white;

                Debug.Log("Left Steam Leaderboard");
                

            }
            else
            {
                AchievementManager.Instance.progressData.isParticipatedInSteamLeaderboard = true;
                joinLeaderBoardToggleObj.SetActive(true);
                leaveLeaderBoardToggleObj.SetActive(false);
                AchievementManager.Instance.SaveProgress();
                SoundEffect.Instance.Play(SoundList.DebugkeySe);
                    toggleImage.color = toggleActiveColor;

                Debug.Log("Joined Steam Leaderboard");

            }

        
    }

    private void RefreshUI(List<SteamLeaderboardManager.LeaderboardEntry> entries, LeaderboardType type)
    {
        // Hide loading spinner if we have received at least one result
        // (You might want better logic here to wait for both, but this works for now)
        if(_loadingSpinner != null) _loadingSpinner.SetActive(false);

        // Determine which container to use based on the Type
        Transform targetContainer;

        if (type == LeaderboardType.Hard)
        {
            targetContainer = _contentContainer;
            Debug.Log("Refreshing Hard Mode Leaderboard UI");
        }
        else
        {
            targetContainer = _contentContainerNormalScroll;
            Debug.Log("Refreshing Normal Mode Leaderboard UI");
        }

        // Clear only the specific container
        ClearExistingRows(targetContainer);

        // Loop through data and spawn rows in the correct container
        foreach (var entry in entries)
        {
            if(entry.score <= 1) continue; // Skip entries with score 0 or 1 
            LeaderboardRowUI newRow = Instantiate(_rowPrefab, targetContainer);
            newRow.SetData(entry.rank, entry.username, entry.score, entry.steamID);
        }
    }

    private void ClearExistingRows(Transform container)
    {
        if (container == null) return;

        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}

