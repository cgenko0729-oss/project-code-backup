using DG.Tweening;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TigerForge; // your EventManager
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class AchievementManager : SingletonA<AchievementManager>
{
    //public static AchievementManager Instance;

    [Searchable][PreviewSprite]public List<TraitData> traitToLockList;
    [Searchable][PreviewSprite]public List<TraitData> traitToUnlockList;

    [Searchable][PreviewSprite]public List<Achievement> allAchievements; // Assign all your Achievement ScriptableObjects in the Inspector
    public Dictionary<string, Achievement> achievementDictionary;

    //public EasyFileSave efs;
    private const string SAVE_FILE_NAME = "achievement_progress";

    public AchievementSaveData progressData;

    public Dictionary<MapType, bool> mapUnlockStatus;
    public Dictionary<JobId, bool> characterUnlockStatus;
    public Dictionary<SkillIdType, bool> skillUnlockStatus;

    public HashSet<TraitType> UnlockedTraits { get; private set; }

    public long totalDamageDealt;
    public int totalEnemiesKilled;

    public int totalQuestsCompleted;
    public int totalHealedAmount;
    public int totalItemsCollected;
    public int totalCoinGetted;
    public float totalPlayTime;

    public int totalGameClears;
    public int totalDeaths;

    public int totalSkillEvoled;
    public int totalSkillEnchant;



    public bool isStageSpiderForestUnlock = true;
    public bool isStageAncientForestUnlocked = false;
    public bool isStageCastleUnlocked = false;
    public bool isStageDesertUnlocked = false;
    public bool isStageTempleUnlocked = false;


    public bool isStage1NormalCleared;
    public bool isStage1HardCleared;
    public bool isStage1NightmareCleared;
    public bool isStage1HellCleared;

    public bool isStage2NormalCleared;
    public bool isStage2HardCleared;
    public bool isStage2NightmareCleared;
    public bool isStage2HellCleared;

    public bool isStage3NormalCleared;
    public bool isStage3HardCleared;
    public bool isStage3NightmareCleared;
    public bool isStage3HellCleared;

    public bool isStage4NormalCleared;
    public bool isStage4HardCleared;
    public bool isStage4NightmareCleared;
    public bool isStage4HellCleared;



    public bool isCharacterDogKnightUnlocked = true;
    public bool isCharacterWizardUnlocked = false;
    public bool isCharacterArcherUnlocked = true;
    public bool isCharacterWarriorUnlocked = false;

    public bool isSkillSlashUnlocked = true;
    public bool isSkillArrowUnlocked = true;
    public bool isSkillStarUnlocked = true;
    public bool isSkillHammerUnlocked = true;
    public bool isSkillCircleballUnlocked = true;
    public bool isSkillPoisonFieldUnlocked = true;
    public bool isSkillThunderUnlocked = true;
    public bool isSkillTornadoUnlocked = true;
    public bool isSkillCircleShieldUnlocked = true;
    public bool isSkillBouncerUnlocked = true;
    public bool isSkillBoomerangeUnlocked = true;
    public bool isSkillIceUnlocked = true;
    public bool isSkillBombUnlocked = true;
    public bool isSkillShurikenUnlocked = true;
    public bool isSkillMonsterFlowerUnlocked = true;
    public bool isSkillBirdUnlocked = true;

    public bool isLive30MinEndless = false;


    //==Unlock Window==//
    public bool refindTitleCanvas = false;
    public Transform titleCanvasTrans;
    public AchievementUiMessageWindow achievementUnlockObj; //messaege window will pop up when a new achievement is unlocked
    public float popupDisplayDuration = 4.0f;

    private Queue<string> popupQueue = new Queue<string>();
    private bool isDisplayingPopup = false;
    public int popWindowCount = 0;

    public float popupSpawnDelay = 0.5f; // Time in seconds between each popup spawning
    private bool isProcessingQueue = false;

    public AudioClip unlockAchSe;

    public int achTotalNum = 0; //total number of achievements
    public int achUnlockedNum = 0; //number of unlocked achievements
    public float achFinishRate = 0f; //achievement finish rate

    public List<GameObject> currentAcquiredAchList = new List<GameObject>();


    protected Callback<UserStatsReceived_t> m_UserStatsReceived;

    [ContextMenu("DebugResetTutorData")]
    public void DebugResetTutorData()
    {
        progressData.isTutorialPetFinished = false;
        progressData.isTutorialSkillFinished = false;
    }

    private void OnUserStatsReceived(UserStatsReceived_t pCallback)
    {
        // Check if the download was successful
        if (pCallback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Steam: Failed to receive user stats.");
            return;
        }
    
        // Success! Now we can safely sync our achievements
        Debug.Log("Steam: Stats received successfully. Syncing achievements...");
        
        // Call your sync function here
        SyncWithSteam();
    }

    public void UpdateAchFinishStatus()
    {
        achTotalNum = allAchievements.Count;
        achUnlockedNum = progressData.unlockedAchievementIds.Count;
        if (achTotalNum > 0)
        {
            achFinishRate = (float)achUnlockedNum / achTotalNum * 100f;
        }
        else
        {
            achFinishRate = 0f;
        }

    }

    public void TestUnlockAch()
    {
        //if (Input.GetKeyDown(KeyCode.H))
        //{
        //    //ManualTestUnlockCertainAchievement("killed_10000_enemies");
        //    ManualTestUnlockCertainAchievement("Stage_SpiderForest_Normal_Cleared");
        //    //ManualTestUnlockCertainAchievement("Stage_AncientForest_Normal_Cleared");
        //    //ManualTestUnlockCertainAchievement("Stage_Desert_Normal_Cleared");
        //    //ManualTestUnlockCertainAchievement("Stage_AncientForest_AnyDifficulty_Cleared");
        //}
    }

    public Sprite GetImageByAchievementString(string id)
    {
        if (!achievementDictionary.ContainsKey(id))
        {
            Debug.LogWarning($"Achievement with ID '{id}' not found!");
            return null;
        }

        return achievementDictionary[id].icon;
    }

    public bool isSkillUnlocked(SkillIdType skill)
    {
        
        if (skillUnlockStatus.TryGetValue(skill, out bool isUnlocked))
        {
            return isUnlocked;
        }

        Debug.Log($"SkillType '{skill}' not found in the unlock dictionary.");
        return false;
    }

    public bool IsCharacterUnlocked(JobId job)
    {
        // Use TryGetValue for a safe and fast lookup. This is the standard way.
        // It checks if the key (job) exists, and if it does, it returns its value (isUnlocked).
        if (characterUnlockStatus.TryGetValue(job, out bool isUnlocked))
        {
            //Debug.Log($"Character '{job}' unlock status: {isUnlocked}");
            return isUnlocked;
        }

        // If the job somehow isn't in the dictionary, it's safer to consider it locked.
        //Debug.LogWarning($"JobId '{job}' not found in the unlock dictionary.");
        return false;
    }
    public bool IsMapUnlocked(MapType map)
    {
        // Same logic as the character check: fast, safe, and reliable.
        if (mapUnlockStatus.TryGetValue(map, out bool isUnlocked))
        {
            return isUnlocked;
        }

        // If the map isn't in the dictionary, it's safer to consider it locked.
        Debug.LogWarning($"MapType '{map}' not found in the unlock dictionary.");
        return false;
    }

    public void OnEnable()
    {
        EventManager.StartListening(GameEvent.SceneChanged, ResetAcquireAchList);

    }

    public void OnDisable()
    {
        EventManager.StopListening(GameEvent.SceneChanged, ResetAcquireAchList);

    }

    protected override void OnAwake()
    {
        //if (Instance != null)
        //{
        //    Destroy(gameObject);
        //    return;
        //}
        //Instance = this;
        //DontDestroyOnLoad(gameObject);

        InitializeManager();

        titleCanvasTrans = GameObject.FindWithTag("TitleCanvasObj").transform;

        UpdateAchFinishStatus();


        

        //unlock all toUnlock:
        for (int i = 0; i < traitToUnlockList.Count; i++)
        {
            TraitData trait = traitToUnlockList[i];
            if (trait != null)
            {
                UnlockTrait(trait.traitType);
            }

        }

        //lock all traitToLockList
        //for (int i = 0; i < traitToLockList.Count; i++)
        //{
        //    TraitData trait = traitToLockList[i];
        //    if (trait != null)
        //    {
        //        LockOneTrait(trait.traitType);
        //    }
        //}

    }

    public void RetrieveCharaMapUnlockData()
    {
         mapUnlockStatus = new Dictionary<MapType, bool> {
            [MapType.SpiderForest] = isStageSpiderForestUnlock,
            [MapType.AncientForest] = isStageAncientForestUnlocked,
            [MapType.Castle] = isStageCastleUnlocked,
            [MapType.Desert] = isStageDesertUnlocked,
            [MapType.Temple] = isStageTempleUnlocked,
        };

        characterUnlockStatus = new Dictionary<JobId, bool> {
            [JobId.DogKnight] = isCharacterDogKnightUnlocked,
            [JobId.Wizard] = isCharacterWizardUnlocked,
            [JobId.Archer] = isCharacterArcherUnlocked,
            [JobId.Warrior] = isCharacterWarriorUnlocked,
        };

    }

    public void ReFindTitleCanvasTransform()
    {
        if(!refindTitleCanvas) return;
        refindTitleCanvas = false;
        titleCanvasTrans = GameObject.FindWithTag("TitleCanvasObj").transform;
    }

    private void Update()
    {

        TestUnlockAch();

        UpdateAcquiredAchList();


        //if (Input.GetKeyDown(KeyCode.G)){
        //    TestUnlockTraits();
        //}

        // 全プレイヤーをアンロックする
        //if (Input.GetKeyDown(KeyCode.B))
        //{
        //    progressData.isCharacter2Unlocked = true;
        //    progressData.isCharacter3Unlocked = true;
        //    progressData.isCharacter4Unlocked = true;
        //    SaveProgress();
        //}
    }

    [ContextMenu("TestUnlockThunduck")]
    public void TestUnlockThunduck()
    {
        PetType targetType = PetType.MidBoss_Duck;
        PetData targetPetData = PetGetChanceManager.Instance.masterPetDataBase.allPetData.Find(p => p.petType == targetType);

        if (!PetGetChanceManager.Instance.playerPetDataBase.allPetData.Contains(targetPetData))
        {
            PetGetChanceManager.Instance.playerPetDataBase.allPetData.Add(targetPetData);
            targetPetData.isUnlocked = true;
            PetGetChanceManager.Instance.SaveGetPets();
        }
        
    }

    public void UnlockPetByType(PetType targetType) { 
        PetData targetPetData = PetGetChanceManager.Instance.masterPetDataBase.allPetData.Find(p => p.petType == targetType);
        if (targetPetData != null)
        {
            if (!PetGetChanceManager.Instance.playerPetDataBase.allPetData.Contains(targetPetData))
            {
                PetGetChanceManager.Instance.playerPetDataBase.allPetData.Add(targetPetData);
                targetPetData.isUnlocked = true;
                PetGetChanceManager.Instance.SaveGetPets();
            }
              
        }       
    }

    public void UnlockDlcMusclePet()
    {
        UnlockPetByType(PetType.MidBoss_Duck);
        UnlockPetByType(PetType.MidBoss_Rhino);
        UnlockPetByType(PetType.MidBoss_Frog);
        UnlockPetByType(PetType.MidBoss_Chicken);

        CurrencyManager.Instance.Add(10000, false);

    }

    public void UnlockDlcElitePet()
    {
        UnlockPetByType(PetType.MidBoss_Bee);
        UnlockPetByType(PetType.MidBoss_Golem);
        UnlockPetByType(PetType.MidBoss_MigicBook);
        UnlockPetByType(PetType.MidBoss_Wolf);

        CurrencyManager.Instance.Add(10000, false);
    }


    [ContextMenu("ResetAllTrait")]
    public void ResetAllTraitProgress()
    {
        UnlockedTraits.Clear();
        SaveProgress();
        LoadProgress();
        Debug.Log("All trait progress has been reset.");

    }

    [ContextMenu("DebugUnlockAllAchievement")]
    public void DebugUnlockAllAchievement()
    {
        foreach (var ach in allAchievements)
        {
            UnlockAchievement(ach.id);
        }
    }

        [ContextMenu("ResetAllAchievement")]
    public void ResetAllAchievementProgress()
    {
        progressData = new AchievementSaveData();
        SaveProgress();
        LoadProgress();
        //SyncWithSteam();
        Debug.Log("All achievement progress has been reset.");

    }

    [ContextMenu("ResetSteamStats")]
    public void ResetSteamStats()
    {
        if(SteamManager.Initialized)
        {
            // This resets achievements AND stats for the current user
            SteamUserStats.ResetAllStats(true);
            SteamUserStats.StoreStats();
            Debug.Log("Steam Stats have been reset.");
        }
    }

    private void InitializeManager()
    {




        //efs = new EasyFileSave(SAVE_FILE_NAME);

        // Convert the list to a dictionary for fast lookups
        achievementDictionary = new Dictionary<string, Achievement>();
        foreach (var ach in allAchievements)
        {
            achievementDictionary[ach.id] = ach;
        }

        //if (SteamManager.Initialized)
        //{
        //    // Setup a callback to know when we have received stats from Steam
        //    m_UserStatsReceived = Callback<UserStatsReceived_t>.Create(OnUserStatsReceived);
            
        //    SyncWithSteam();
        //}

        LoadProgress();
        //SyncWithSteam();


        mapUnlockStatus = new Dictionary<MapType, bool> {
            [MapType.SpiderForest] = isStageSpiderForestUnlock,
            [MapType.AncientForest] = isStageAncientForestUnlocked,
            [MapType.Castle] = isStageCastleUnlocked,
            [MapType.Desert] = isStageDesertUnlocked,
            [MapType.Temple] = isStageTempleUnlocked,
        };

        characterUnlockStatus = new Dictionary<JobId, bool> {
            [JobId.DogKnight] = isCharacterDogKnightUnlocked,
            [JobId.Wizard] = isCharacterWizardUnlocked,
            [JobId.Archer] = isCharacterArcherUnlocked,
            [JobId.Warrior] = isCharacterWarriorUnlocked,
        };

        skillUnlockStatus = new Dictionary<SkillIdType, bool> {
            [SkillIdType.Slash] = isSkillSlashUnlocked,
            [SkillIdType.arrow] = isSkillArrowUnlocked,
            [SkillIdType.starMagic] = isSkillStarUnlocked,
            [SkillIdType.Hammer] = isSkillHammerUnlocked,
            [SkillIdType.CircleBall] = isSkillCircleballUnlocked,
            [SkillIdType.PoisonField] = isSkillPoisonFieldUnlocked,
            [SkillIdType.Thunder] = isSkillThunderUnlocked,
            [SkillIdType.Tornado] = isSkillTornadoUnlocked,
            [SkillIdType.circleShield] = isSkillCircleShieldUnlocked,
            [SkillIdType.Bounce] = isSkillBouncerUnlocked,
            [SkillIdType.boomerang] = isSkillBoomerangeUnlocked,
            [SkillIdType.Ice] = isSkillIceUnlocked,
            [SkillIdType.bombShark] = isSkillBombUnlocked,
            [SkillIdType.Suriken] = isSkillShurikenUnlocked,
            [SkillIdType.MonsterFlower] = isSkillMonsterFlowerUnlocked,
            [SkillIdType.Bird] = isSkillBirdUnlocked,
        };



        //runtimeStatus = new Dictionary<string, AchievementRuntimeStatus>();
        //foreach (var ach in allAchievements)
        //{
        //    // 2. Combine the SO (definition) with the saved data (status)
        //    bool isUnlocked = progressData.unlockedAchievementIds.Contains(ach.id);

        //    runtimeStatus[ach.id] = new AchievementRuntimeStatus
        //    {
        //        Definition = ach,
        //        IsUnlocked = isUnlocked,
        //        // You can also add current progress here if needed
        //    };
        //}

    }

    public void NotifyGameTimePass(int time)
    {

        progressData.totalPlayTime += time;
        CheckAchievementProgress("GameTime_60min", (int)progressData.totalPlayTime/60);
        CheckAchievementProgress("GameTime_120min", (int)progressData.totalPlayTime/60);
        CheckAchievementProgress("GameTime_300min", (int)progressData.totalPlayTime/60);

    }

    public void NotifyItemGet(int itemNum)
    {
        progressData.totalItemsCollected += itemNum;
        CheckAchievementProgress("ItemCollect_20", progressData.totalItemsCollected);
        CheckAchievementProgress("ItemCollect_50", progressData.totalItemsCollected);
        CheckAchievementProgress("ItemCollect_100", progressData.totalItemsCollected);
        SaveProgress();

    }

    public void NotifyQuestFinished(int num)
    {
        progressData.totalQuestsCompleted += num;
        CheckAchievementProgress("Quest_FInish_5", progressData.totalQuestsCompleted);
        CheckAchievementProgress("Quest_FInish_10", progressData.totalQuestsCompleted);
        CheckAchievementProgress("Quest_FInish_20", progressData.totalQuestsCompleted);
        SaveProgress();

    }

    public void NotifyGameCLear(int num)
    {
        progressData.totalGameClears += num;
        CheckAchievementProgress("GameClear_1", progressData.totalGameClears);
        CheckAchievementProgress("GameClear_5", progressData.totalGameClears);
        CheckAchievementProgress("GameClear_10", progressData.totalGameClears);
        SaveProgress();

    }

    public void NotifyGameOver(int num)
    {
        progressData.totalDeaths += num;
        CheckAchievementProgress("GameDeath_1", progressData.totalDeaths);
        CheckAchievementProgress("GameDeath_10", progressData.totalDeaths);
        SaveProgress();
    }

    public void NotifyTimeSurvived(int num)
    {
        progressData.totalPlayTime += num;
        CheckAchievementProgress("PlayTime_60min", (int)progressData.totalPlayTime);
        CheckAchievementProgress("PlayTime_120min", (int)progressData.totalPlayTime);
        CheckAchievementProgress("PlayTime_300min", (int)progressData.totalPlayTime);
        SaveProgress();
    }

    public void NotifyCoinCollect(int num)
    {
        progressData.totalCoinGetted += num;
        CheckAchievementProgress("CoinCollect_10000", (int)progressData.totalCoinGetted);
        CheckAchievementProgress("CoinCollect_50000", (int)progressData.totalCoinGetted);
        CheckAchievementProgress("CoinCollect_100000", (int)progressData.totalCoinGetted);
        SaveProgress();

    }

    public void NotifyEnemyKilled(int num)
    {
        progressData.totalEnemiesKilled += num;
        CheckAchievementProgress("killed_10000_enemies", progressData.totalEnemiesKilled);
        CheckAchievementProgress("killed_50000_enemies", progressData.totalEnemiesKilled);
        CheckAchievementProgress("killed_100000_enemies", progressData.totalEnemiesKilled);
        SaveProgress();
    }

    public void NotifyDamageDealt(long amount)
    {
        progressData.totalDamageDealt += amount;
        CheckAchievementProgress("DamageDeal_500000", (int)progressData.totalDamageDealt);
        CheckAchievementProgress("DamageDeal_1000000", (int)progressData.totalDamageDealt);
        CheckAchievementProgress("DamageDeal_5000000", (int)progressData.totalDamageDealt);
        SaveProgress();
    }

    public void NotifyStageCleared(MapType stageId, DifficultyType difficulty)
    {

        string mapName = "0";
        string diffName = "0";

        mapName = stageId.ToString();
        diffName = difficulty.ToString();

        //UnlockAchievement(stageId + "_cleared");
        
        UnlockAchievement("Stage_" + mapName + "_" + diffName + "_Cleared");
        UnlockAchievement("Stage_" + mapName + "_AnyDifficulty_Cleared");

        //Debug all name map name diffname:
        Debug.Log("Stage Cleared: " + mapName + " Difficulty: " + diffName);

        if (difficulty == DifficultyType.Normal)
        {
            switch (stageId)
            {
                case MapType.None:
                    break;
                case MapType.SpiderForest:
                    progressData.isStage1NormalCleared = true;
                    AchievementManager.Instance.progressData.isCharacter2Unlocked = true;
                    AchievementManager.Instance.progressData.isStageAncientForestUnlocked = true;
                    break;
                case MapType.AncientForest:
                    progressData.isStage2NormalCleared = true;
                    AchievementManager.Instance.progressData.isCharacter2Unlocked = true;
                    AchievementManager.Instance.progressData.isCharacter3Unlocked = true;
                    AchievementManager.Instance.progressData.isStageDesertUnlocked = true;
                    break;
                case MapType.Castle:
                    break;
                case MapType.Desert:
                    progressData.isStage3NormalCleared = true;
                    AchievementManager.Instance.progressData.isCharacter4Unlocked = true;
                    AchievementManager.Instance.progressData.isStageTempleUnlocked = true;
                    break;
                case MapType.Temple:
                    progressData.isStage4NormalCleared = true;
                    break;
                case MapType.Max:
                    break;
                default:
                    break;
            }

        }
        else if (difficulty == DifficultyType.Hard)
        {
            switch (stageId)
            {
                case MapType.None:
                    break;
                case MapType.SpiderForest:
                    AchievementManager.Instance.progressData.isCharacter2Unlocked = true;
                    AchievementManager.Instance.progressData.isStageAncientForestUnlocked = true;
                    progressData.isStage1HardCleared = true;
                    break;
                case MapType.AncientForest:
                    AchievementManager.Instance.progressData.isCharacter3Unlocked = true;
                    AchievementManager.Instance.progressData.isCharacter2Unlocked = true;
                    AchievementManager.Instance.progressData.isStageDesertUnlocked = true;
                    progressData.isStage2HardCleared = true;
                    break;
                case MapType.Castle:
                    break;
                case MapType.Desert:
                    AchievementManager.Instance.progressData.isCharacter4Unlocked = true;
                    AchievementManager.Instance.progressData.isStageTempleUnlocked = true;
                    progressData.isStage3HardCleared = true;
                    break;
                case MapType.Temple:
                    progressData.isStage4HardCleared = true;
                    break;
                case MapType.Max:
                    break;
                default:
                    break;
            }
        }
        else if (difficulty == DifficultyType.Nightmare)
        {
            switch (stageId)
            {
                case MapType.None:
                    break;
                case MapType.SpiderForest:
                    progressData.isStage1NightmareCleared = true;
                    AchievementManager.Instance.progressData.isCharacter2Unlocked = true;
                     AchievementManager.Instance.progressData.isStageAncientForestUnlocked = true;
                    UnlockPetByType(PetType.MidBoss_Wolf);
                    break;
                case MapType.AncientForest:
                    progressData.isStage2NightmareCleared = true;
                    AchievementManager.Instance.progressData.isStageDesertUnlocked = true;
                    AchievementManager.Instance.progressData.isCharacter3Unlocked = true;
                    UnlockPetByType(PetType.MidBoss_Bee);
                    break;
                case MapType.Castle:
                    break;
                case MapType.Desert:
                    progressData.isStage3NightmareCleared = true;
                    AchievementManager.Instance.progressData.isStageTempleUnlocked = true;
                    AchievementManager.Instance.progressData.isCharacter4Unlocked = true;
                    UnlockPetByType(PetType.MidBoss_Golem);
                    break;
                case MapType.Temple:
                    progressData.isStage4NightmareCleared = true;
                    UnlockPetByType(PetType.MidBoss_MigicBook);
                    break;
                case MapType.Max:
                    break;
                default:
                    break;
            }
        }
        else if (difficulty == DifficultyType.Hell)
        {
            switch (stageId)
            {
                case MapType.None:
                    break;
                case MapType.SpiderForest:
                    progressData.isStage1HellCleared = true;
                     AchievementManager.Instance.progressData.isStageAncientForestUnlocked = true;
                    UnlockPetByType(PetType.MidBoss_Chicken);
                    break;
                case MapType.AncientForest:
                    progressData.isStage2HellCleared = true;
                    AchievementManager.Instance.progressData.isStageDesertUnlocked = true;
                    UnlockPetByType(PetType.MidBoss_Duck);
                    break;
                case MapType.Castle:
                    break;
                case MapType.Desert:
                    progressData.isStage3HellCleared = true;
                    UnlockPetByType(PetType.MidBoss_Rhino);
                    AchievementManager.Instance.progressData.isStageTempleUnlocked = true;
                    break;
                case MapType.Temple:
                    progressData.isStage4HellCleared = true;
                    UnlockPetByType(PetType.MidBoss_Frog);
                    break;
                case MapType.Max:
                    break;
                default:
                    break;
            }
        }

        SaveProgress();




    }



    public void UnlockAchievement(string id)
    {
        //return; //demo early return



        Debug.Log($"Attempting to unlock achievement: {id}");

        if (!achievementDictionary.ContainsKey(id))
        {
            Debug.Log($"Achievement with ID '{id}' not found!");
            return;
        }

        if (progressData.unlockedAchievementIds.Contains(id))
        {
            Debug.Log($"Achievement '{achievementDictionary[id].title}' is already unlocked.");
            return;

        }
    

        progressData.unlockedAchievementIds.Add(id);         // 1. Unlock Locally
        Debug.Log($"Locally unlocked achievement: {achievementDictionary[id].title}");

        SaveProgress();

       
        if (achievementDictionary[id].rewardCoin > 0)  //add the reward coin from that achievement
        {
            CurrencyManager.Instance.Add((int)achievementDictionary[id].rewardCoin, isBuffApplicable:false);
        }

        //PopAchMessageWindow(id);
        popupQueue.Enqueue(id);
        ProcessPopupQueue();


        UnlockSteamAchievement(achievementDictionary[id].steamApiName);  //Unlock on Steam

        UpdateAchFinishStatus();
    }

    //public void PopAchMessageWindow(string id)
    //{
    //    titleCanvasTrans = GameObject.FindWithTag("TitleCanvasObj").transform;
    //    AchievementUiMessageWindow window = Instantiate(achievementUnlockObj, titleCanvasTrans).GetComponent<AchievementUiMessageWindow>();
    //    window.SetUp(id);

    //}

    private void ProcessPopupQueue()
    {
        //if (isDisplayingPopup || popupQueue.Count == 0) return;

        //popWindowCount++;
        
        ////isDisplayingPopup = true;

        //Vector2 windowPos = new Vector2(0, 420 - 170 * popWindowCount);
        //string achievementIdToShow = popupQueue.Dequeue();
        //AchievementUiMessageWindow windowInstance = Instantiate(achievementUnlockObj, titleCanvasTrans);     
        //windowInstance.SetUp(achievementIdToShow,windowPos);


        //StartCoroutine(PopupLifecycleCoroutine(windowInstance.gameObject));


        if (!isProcessingQueue && popupQueue.Count > 0)
        {
            StartCoroutine(ProcessPopupQueueCoroutine());
        }

    }

    private IEnumerator ProcessPopupQueueCoroutine()
    {
        isProcessingQueue = true;
        ReFindTitleCanvasTransform();
        
        // Continue as long as there are achievements in the queue
        while (popupQueue.Count > 0)
        {
            popWindowCount++;

            // Calculate the position for the next window
            
            //Vector2 windowPos = new Vector2(0, 359 - 149 * (popWindowCount - 0));
            int newIndex = currentAcquiredAchList.Count;
            Vector2 windowPos = new Vector2(0, POPUP_START_Y - POPUP_SPACING_Y * newIndex);
            

            // Get the achievement ID from the queue
            string achievementIdToShow = popupQueue.Dequeue();
    
            // Instantiate and set up the window
            AchievementUiMessageWindow windowInstance = Instantiate(achievementUnlockObj, titleCanvasTrans);
            windowInstance.SetUp(achievementIdToShow, windowPos, GetImageByAchievementString(achievementIdToShow));
            SoundEffect.Instance.PlayUnlockAchSe();

            currentAcquiredAchList.Add(windowInstance.gameObject);
            windowInstance.SetIndexInAcquiredAchList(newIndex);

            // Wait for the specified delay before spawning the next one
            yield return new WaitForSeconds(popupSpawnDelay);
        }
    
        // Once the queue is empty, reset the flags for the next time
        isProcessingQueue = false;
        popWindowCount = 0;
    }

    public const float POPUP_START_Y = 210f;
    public const float POPUP_SPACING_Y = 149f;

    public void RemoveItemInAcquireAchListByIndex(int index)
    {
        if (index >= 0 && index < currentAcquiredAchList.Count)
        {
            GameObject objectToRemove = currentAcquiredAchList[index];
            currentAcquiredAchList.RemoveAt(index);
            for (int i = index; i < currentAcquiredAchList.Count; i++)
            {
                GameObject itemToUpdate = currentAcquiredAchList[i];
                if (itemToUpdate != null)
                {
                    AchievementUiMessageWindow uiWindow = itemToUpdate.GetComponent<AchievementUiMessageWindow>();
                    uiWindow.SetIndexInAcquiredAchList(i);
                    Vector2 newPosition = new Vector2(0, POPUP_START_Y - POPUP_SPACING_Y * i);
                    itemToUpdate.GetComponent<RectTransform>().DOAnchorPos(newPosition, 0.3f);
                }
            }

        }
    }

    public void ResetAcquireAchList()
    {
        refindTitleCanvas = true;
        currentAcquiredAchList.Clear();

    }

    public void UpdateAcquiredAchList()
    {
        if(Gamepad.current != null)
        {
            if (Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                 if (currentAcquiredAchList.Count > 0)
                {
                    GameObject firstItem = currentAcquiredAchList[0];
                    Button button = firstItem.GetComponentInChildren<Button>();
                    button.onClick.Invoke();

                    //currentAcquiredAchList.RemoveAt(0);
                }
            }
        }

        

        if (Input.GetKeyDown(KeyCode.E))
        {
            GetAndUnlockAchievement();

        }

        if(Gamepad.current != null)
        {
            if (Gamepad.current.dpad.up.wasPressedThisFrame)
            {
                GetAndUnlockAchievement();
            }
        }

    }

    void GetAndUnlockAchievement()
    {
         //get the button of first item in the list and click it,then remove it from the list
            if (currentAcquiredAchList.Count > 0)
            {
                GameObject firstItem = currentAcquiredAchList[0];
                Button button = firstItem.GetComponentInChildren<Button>();
                button.onClick.Invoke();

                //currentAcquiredAchList.RemoveAt(0);
            }
    }

    private void CheckAchievementProgress(string id, int currentValue)
    {
        if (!achievementDictionary.ContainsKey(id) || progressData.unlockedAchievementIds.Contains(id))
        {
            return;
        }

        Achievement ach = achievementDictionary[id];

        //progressData.incrementalAchievementProgress[id] = currentValue;
        var progressEntry = progressData.incrementalAchievementProgress.FirstOrDefault(p => p.id == id);

        if (progressEntry != null)
        {
            // If it exists, update its value.
            progressEntry.value = currentValue;
        }
        else
        {
            // If it doesn't exist, create a new entry and add it to the list.
            progressData.incrementalAchievementProgress.Add(new AchievementProgressEntry(id, currentValue));
        }

        if (currentValue >= ach.valueToUnlock)
        {
            UnlockAchievement(id);
        }
    }

    public void ManualTestUnlockCertainAchievement(string id)
    {
        UnlockAchievement(id);
    }

    // --- Steamworks Integration ---

    private void UnlockSteamAchievement(string steamApiName)
    {
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam Manager not initialized. Cannot unlock achievement on Steam.");
            return;
        }

        // 2. Validation: Does the ID exist?
        if (string.IsNullOrEmpty(steamApiName))
        {
            Debug.Log("Steam API Name is empty. Check your ScriptableObject.");
            return;
        }

        // 3. Check if already unlocked on Steam (to prevent spamming the API)
        bool isUnlockedOnSteam;
        bool ret = SteamUserStats.GetAchievement(steamApiName, out isUnlockedOnSteam);

        if (ret && !isUnlockedOnSteam)
        {
            // 4. Set the achievement
            SteamUserStats.SetAchievement(steamApiName);
            
            // 5. UPLOAD to Steam. 
            // NOTE: StoreStats is network intensive. In a heavy loop, don't call this. 
            // But for single achievement unlocks, it is fine.
            bool success = SteamUserStats.StoreStats();
            
            if(success)
                Debug.Log($"[Steam] Unlocked achievement: {steamApiName}");
            else
                Debug.Log($"[Steam] Failed to store stats for: {steamApiName}");
        }
    }

    private void SyncWithSteam()
    {
        if (!SteamManager.Initialized) return;

         bool anyChangeMade = false;

         // Loop through everything we have unlocked LOCALLY
         foreach (string unlockedId in progressData.unlockedAchievementIds)
         {
             if (achievementDictionary.TryGetValue(unlockedId, out Achievement ach))
             {
                 if (!string.IsNullOrEmpty(ach.steamApiName))
                 {
                     // Check status on Steam
                     SteamUserStats.GetAchievement(ach.steamApiName, out bool isUnlockedOnSteam);

                     // If unlocked locally but LOCKED on Steam -> Unlock it on Steam
                     if (!isUnlockedOnSteam)
                     {
                         SteamUserStats.SetAchievement(ach.steamApiName);
                         anyChangeMade = true;
                         Debug.Log($"[Steam Sync] Synced {ach.steamApiName} from Local Save.");
                     }
                 }
             }
         }

         // Only upload if we actually changed something to save bandwidth/performance
         if (anyChangeMade)
         {
             SteamUserStats.StoreStats();
         }
    }

    public void SaveProgress()
    {

        EasyFileSave efs = new EasyFileSave(SAVE_FILE_NAME);

       progressData.unlockedTraitTypes = UnlockedTraits.ToList(); // Convert the runtime HashSet back to a List for serialization.

        // Using AddSerialized to save the whole class instance.
        efs.AddSerialized("progress", progressData);
        if (!efs.Save())
        {
            Debug.Log($"Failed to save achievement data: {efs.Error}");
        }
    }

    private void LoadProgress()
    {
        EasyFileSave efs = new EasyFileSave(SAVE_FILE_NAME);

        if (efs.Load())
        {
            // The GetDeserialized method requires the type of the object.
            progressData = (AchievementSaveData)efs.GetDeserialized("progress", typeof(AchievementSaveData));
            if (progressData == null)
            {
                // Handle cases where the file exists but data is corrupt or of an old format
                progressData = new AchievementSaveData();
            }
        }
        else
        {
            // If no save file exists, create a new one.
            Debug.Log("No achievement save file found. Creating a new one.");
            progressData = new AchievementSaveData();
        }
        efs.Dispose(); // Clear memory after loading

        UnlockedTraits = new HashSet<TraitType>(); // Initialize the runtime HashSet.
        if (progressData.unlockedTraitTypes != null) // If the save data has a list of unlocked traits, add them all to our runtime set.
        {
            foreach (TraitType traitType in progressData.unlockedTraitTypes)
            {
                UnlockedTraits.Add(traitType);
            }
        }

        totalDamageDealt = progressData.totalDamageDealt;
        totalEnemiesKilled = progressData.totalEnemiesKilled;
        totalQuestsCompleted = progressData.totalQuestsCompleted;
        totalHealedAmount = progressData.totalHealedAmount;
        totalItemsCollected = progressData.totalItemsCollected;
        totalCoinGetted = progressData.totalCoinGetted;

        totalGameClears = progressData.totalGameClears;
        totalDeaths = progressData.totalDeaths;

        totalPlayTime = progressData.totalPlayTime;

        isStage1NormalCleared = progressData.isStage1NormalCleared;
        isStage1HardCleared = progressData.isStage1HardCleared;
        isStage1NightmareCleared = progressData.isStage1NightmareCleared;
        isStage1HellCleared = progressData.isStage1HellCleared;
        isStage2NormalCleared = progressData.isStage2NormalCleared;
        isStage2HardCleared = progressData.isStage2HardCleared;
        isStage2NightmareCleared = progressData.isStage2NightmareCleared;
        isStage2HellCleared = progressData.isStage2HellCleared;
        isStage3NormalCleared = progressData.isStage3NormalCleared;
        isStage3HardCleared = progressData.isStage3HardCleared;
        isStage3NightmareCleared = progressData.isStage3NightmareCleared;
        isStage3HellCleared = progressData.isStage3HellCleared;
        isStage4NormalCleared = progressData.isStage4NormalCleared;
        isStage4HardCleared = progressData.isStage4HardCleared;
        isStage4NightmareCleared = progressData.isStage4NightmareCleared;
        isStage4HellCleared = progressData.isStage4HellCleared;
        isCharacterDogKnightUnlocked = progressData.isCharacter1Unlocked;
        isCharacterWizardUnlocked = progressData.isCharacter4Unlocked;
        isCharacterArcherUnlocked = progressData.isCharacter2Unlocked;
        isCharacterWarriorUnlocked = progressData.isCharacter3Unlocked;

        //Debug print isCharacter1 to 4Unlocked:
        Debug.Log($"Character Unlock Status - DogKnight: {progressData.isCharacter1Unlocked}, Archer: {progressData.isCharacter2Unlocked}, Warrior: {progressData.isCharacter3Unlocked}, Wizard: {progressData.isCharacter4Unlocked}");

        isStageSpiderForestUnlock = progressData.isStageSpiderForestUnlock;
        isStageAncientForestUnlocked = progressData.isStageAncientForestUnlocked;
        isStageCastleUnlocked = progressData.isStageCastleUnlocked;
        isStageDesertUnlocked = progressData.isStageDesertUnlocked;
        isStageTempleUnlocked = progressData.isStageTempleUnlocked;

        isSkillSlashUnlocked = progressData.isSkillSlashUnlocked;
        isSkillArrowUnlocked = progressData.isSkillArrowUnlocked;
        isSkillStarUnlocked = progressData.isSkillStarUnlocked;
        isSkillHammerUnlocked = progressData.isSkillHammerUnlocked;
        isSkillCircleballUnlocked = progressData.isSkillCircleballUnlocked;
        isSkillPoisonFieldUnlocked = progressData.isSkillPoisonFieldUnlocked;
        isSkillThunderUnlocked = progressData.isSkillThunderUnlocked;
        isSkillTornadoUnlocked = progressData.isSkillTornadoUnlocked;
        isSkillCircleShieldUnlocked = progressData.isSkillCircleShieldUnlocked;
        isSkillBouncerUnlocked = progressData.isSkillBouncerUnlocked;
        isSkillBoomerangeUnlocked = progressData.isSkillBoomerangeUnlocked;
        isSkillIceUnlocked = progressData.isSkillIceUnlocked;
        isSkillBombUnlocked = progressData.isSkillBombUnlocked;
        isSkillShurikenUnlocked = progressData.isSkillShurikenUnlocked;
        isSkillMonsterFlowerUnlocked = progressData.isSkillMonsterFlowerUnlocked;
        isSkillBirdUnlocked = progressData.isSkillBirdUnlocked;

        isLive30MinEndless = progressData.isLive30MinEndless;



    }

    public bool IsTraitUnlocked(TraitType traitType)
    {
        if (UnlockedTraits == null)
        {
            Debug.LogError("UnlockedTraits set is not initialized! Make sure LoadProgress has run.");
            return false;
        }
        return UnlockedTraits.Contains(traitType);
    }

    public void UnlockTrait(TraitType traitType)
    {
        if (IsTraitUnlocked(traitType))
        {
            //Debug.Log($"Trait '{traitType}' is already unlocked.");
            return; // Already unlocked, do nothing.
        }
    
        UnlockedTraits.Add(traitType);    
        //Debug.Log($"Unlocked Trait: {traitType}");    
        SaveProgress();
    }

    public void LockOneTrait(TraitType trait)
    {
        if (UnlockedTraits.Contains(trait))
        {
            UnlockedTraits.Remove(trait);
            //Debug.Log($"Locked Trait: {trait}");
            SaveProgress();
        }
        else
        {
            Debug.Log($"Trait '{trait}' is not unlocked, cannot lock.");
        }
    }

        public void TestUnlockTraits()
    {
        UnlockTrait(TraitType.DoubleDash);
        UnlockTrait(TraitType.HealDouble);
        UnlockTrait(TraitType.EnchantFire);
        UnlockTrait(TraitType.EnchantIce);
        SaveProgress();

    }

    /*
    Achievement list : 
    (id) (jpTranslation) (englishTranslation) (chinese simplified translation) (chinese traditional translation)


    

    ach.Stage_SpiderForeset_AnyDifficulty_Cleared.name 森の冒険者 / Forest Adventurer / 森林探险者 / 森林探險者
    ach.Stage_SpiderForeset_AnyDifficulty_Cleared.desc ステージ1を任意の難易度で初めてクリアする / Clear Stage 1 on any difficulty for the first time / 首次以任意难度通关第1关 / 首次以任意難度通關第1關
    ach.Stage_SpiderForeset_AnyDifficulty_Cleared.reward 新しいステージ、キャラクターアンロック / Unlock new stage and character / 解锁新关卡和角色 / 解鎖新關卡和角色
    
    ach.Stage_AncientForest_AnyDifficulty_Cleared.name 遺跡の冒険者 / Remnant Adventurer / 遗迹探险者 / 遺跡探險者
    ach.Stage_AncientForest_AnyDifficulty_Cleared.desc ステージ2を任意の難易度で初めてクリアする / Clear Stage 2 on any difficulty for the first time / 首次以任意难度通关第2关 / 首次以任意難度通關第2關
    ach.Stage_AncientForest_AnyDifficulty_Cleared.reward 新しいステージアンロック / Unlock new stage / 解锁新关卡 / 解鎖新關卡

    ach.Stage_Desert_AnyDifficulty_Cleared.name 砂漠の冒険者 / Desert Adventurer / 沙漠探险者 / 沙漠探險者
    ach.Stage_Desert_AnyDifficulty_Cleared.desc ステージ3を任意の難易度で初めてクリアする / Clear Stage 3 on any difficulty for the first time / 首次以任意难度通关第3关 / 首次以任意難度通關第3關
    ach.Stage_Desert_AnyDifficulty_Cleared.reward 新しいステージアンロック / Unlock new stage / 解锁新关卡 / 解鎖新關卡

    ach.Stage_Temple_AnyDifficulty_Cleared.name 神殿の冒険者 / Temple Adventurer / 神殿探险者 / 神殿探險者
    ach.Stage_Temple_AnyDifficulty_Cleared.desc ステージ4を任意の難易度で初めてクリアする / Clear Stage 4 on any difficulty for the first time / 首次以任意难度通关第4关 / 首次以任意難度通關第4關
    ach.Stage_Temple_AnyDifficulty_Cleared.reward 新しいステージアンロック / Unlock new stage / 解锁新关卡 / 解鎖新關卡
    

    
    ach.Stage_SpiderForest_Normal_Cleared.name 森の征服者
    ach.Stage_SpiderForest_Normal_Cleared.desc ステージ1のノーマル難易度を初めてクリアする
    ach.Stage_SpiderForest_Normal_Cleared.reward 新しいステージ、キャラクターアンロック

    ach.Stage_AncientForest_Normal_Cleared.name 遺跡の征服者
    ach.Stage_AncientForest_Normal_Cleared.desc　ステージ2のノーマル難易度を初めてクリアする
    ach.Stage_AncientForest_Normal_Cleared.reward　新しいステージアンロック

    ach.Stage_Desert_Normal_Cleared.name 砂漠の征服者
    ach.Stage_Desert_Normal_Cleared.desc ステージ3のノーマル難易度を初めてクリアする
    ach.Stage_Desert_Normal_Cleared.reward 新しいステージアンロック

    ach.Stage_Temple_Normal_Cleared.name 神殿の征服者
    ach.Stage_Temple_Normal_Cleared.desc ステージ4のノーマル難易度を初めてクリアする
    ach.Stage_Temple_Normal_Cleared.reward 新しいステージアンロック
     
    //killed_10000_enemies
    ach.killed_10000_enemies.name モンスタースレイヤー1
    ach.killed_10000_enemies.desc 敵を10000体倒す
    ach.killed_10000_enemies.reward コイン300枚

    ach.killed_50000_enemies.name モンスタースレイヤー2
    ach.killed_50000_enemies.desc 敵を50000体倒す
    ach.killed_50000_enemies.reward コイン500枚

    ach.killed_100000_enemies.name モンスタースレイヤー3
    ach.killed_100000_enemies.desc 敵を100000体倒す
    ach.killed_100000_enemies.reward コイン1000枚

    ach.Quest_FInish_5.name クエストマスター1
    ach.Quest_FInish_5.desc クエストを5回クリアする
    ach.Quest_FInish_5.reward コイン200枚

    ach.Quest_FInish_10.name クエストマスター2
    ach.Quest_FInish_10.desc クエストを10回クリアする
    ach.Quest_FInish_10.reward コイン500枚

    ach.Quest_FInish_20.name クエストマスター3
    ach.Quest_FInish_20.desc クエストを20回クリアする
    ach.Quest_FInish_20.reward コイン700枚

    ach.GameClear_1.name 初クリア
    ach.GameClear_1.desc 初めてゲームをクリアする
    ach.GameClear_1.reward コイン200枚

    ach.GameClear_5.name ゲームマスター1
    ach.GameClear_5.desc ゲームを5回クリアする
    ach.GameClear_5.reward コイン500枚

    ach.GameClear_10.name ゲームマスター2
    ach.GameClear_10.desc ゲームを10回クリアする
    ach.GameClear_10.reward コイン1000枚

    ach.GameDeath_1.name 初ゲームオーバー
    ach.GameDeath_1.desc 初めてゲームオーバーになる
    ach.GameDeath_1.reward コイン200枚

    ach.GameDeath_10.name 不屈の挑戦者1
    ach.GameDeath_10.desc ゲームオーバーを10回迎える
    ach.GameDeath_10.reward コイン500枚

    ach.GameDeath_50.name 不屈の挑戦者2
    ach.GameDeath_50.desc ゲームオーバーを50回迎える
    ach.GameDeath_50.reward コイン1000枚

    ach.ItemCollect_20.name アイテムコレクター1
    ach.ItemCollect_20.desc アイテムを20個集める
    ach.ItemCollect_20.reward コイン200枚

    ach.ItemCollect_.50.name アイテムコレクター2
    ach.ItemCollect_50.desc アイテムを50個集める
    ach.ItemCollect_50.reward コイン500枚

    ach.ItemCollect_100.name アイテムコレクター3
    ach.ItemCollect_100.desc アイテムを100個集める
    ach.ItemCollect_100.reward コイン1000枚

    ach.Quest_FInish_5.name クエストハンター1
    ach.Quest_FInish_5.desc クエストを5回クリアする
    ach.Quest_FInish_5.reward コイン200枚

    ach.Quest_FInish_10.name クエストマスター2
    ach.Quest_FInish_10.desc クエストを10回クリアする
    ach.Quest_FInish_10.reward コイン500枚

    ach.Quest_FInish_20.name クエストマスター3
    ach.Quest_FInish_20.desc クエストを20回クリアする
    ach.Quest_FInish_20.reward コイン700枚

    ach.DamageDeal_500000.name ダメージディーラー1
    ach.DamageDeal_500000.desc 総ダメージ量が500,000を超える
    ach.DamageDeal_500000.reward コイン300枚

    ach.DamageDeal_1000000.name ダメージディーラー2
    ach.DamageDeal_1000000.desc 総ダメージ量が1,000,000を超える
    ach.DamageDeal_1000000.reward コイン500枚

    ach.DamageDeal_5000000.name ダメージディーラー3
    ach.DamageDeal_5000000.desc 総ダメージ量が5,000,000を超える
    ach.DamageDeal_5000000.reward コイン1000枚

    ach.GameTime_60min.name プレイタイム1
    ach.GameTime_60min.desc 総プレイ時間が60分を超える
    ach.GameTime_60min.reward コイン200枚

    ach.GameTime_120min.name プレイタイム2
    ach.GameTime_120min.desc 総プレイ時間が120分を超える
    ach.GameTime_120min.reward コイン500枚

    ach.GameTime_300min.name プレイタイム3
    ach.GameTime_300min.desc 総プレイ時間が300分を超える
    ach.GameTime_300min.reward コイン1000枚

    ach.CoinCollect_3000.name コインコレクター1
    ach.CoinCollect_3000.desc ゲーム内でコインを3,000枚集める
    ach.CoinCollect_3000.reward コイン200枚

    ach.CoinCollect_5000.name コインコレクター2
    ach.CoinCollect_5000.desc ゲーム内でコインを5,000枚集める
    ach.CoinCollect_5000.reward コイン500枚

    ach.CoinCollect_10000.name コインコレクター3
    ach.CoinCollect_10000.desc ゲーム内でコインを10,000枚集める
    ach.CoinCollect_10000.reward コイン1000枚

    ach.KillBoss_10.name ボスハンター1
    ach.KillBoss_10.desc ボスを10体倒す(中ボスを含めて)
    ach.KillBoss_10.reward コイン200枚

    ach.KillBoss_50.name ボスハンター2
    ach.KillBoss_50.desc ボスを30体倒す(中ボスを含めて)

    ach.PetUnlock_3.name ペットコレクター
    ach.PetUnlock_3.desc ペットを3体アンロックする
    ach.PetUnlock_3.reward コイン200枚

    ach.PetUnlock_10.name ペットマスター
    ach.PetUnlock_10.desc ペットを10体アンロックする
    ach.PetUnlock_10.reward コイン500枚

    ach.PetUnlock_15.name ペットグランドマスター
    ach.PetUnlock_15.desc ペットを15体アンロックする
    ach.PetUnlock_15.reward コイン700枚

    ach.PlayerGetHeal_500.name ヒーラーマスター1
    ach.PlayerGetHeal_500.desc プレイヤーの総回復量が500を超える
    ach.PlayerGetHeal_500.reward コイン300枚

    ach.PlayerGetHeal_2500.name ヒーラーマスター2
    ach.PlayerGetHeal_2500.desc プレイヤーの総回復量が2,500を超える
    ach.PlayerGetHeal_2500.reward コイン500枚

    ach.PlayerGetHeal_5000.name ヒーラーマスター3
    ach.PlayerGetHeal_5000.desc プレイヤーの総回復量が5,000を超える
    ach.PlayerGetHeal_5000.reward コイン1000枚

    ach.PlayerDash_50.name ダッシュマスター1
    ach.PlayerDash_50.desc プレイヤーの総ダッシュ回数が50回を超える
    ach.PlayerDash_50.reward コイン200枚

    ach.PlayerDash_200.name ダッシュマスター2
    ach.PlayerDash_200.desc プレイヤーの総ダッシュ回数が200回を超える
    ach.PlayerDash_200.reward コイン500枚

    ach.PlayerDash_500.name ダッシュマスター3
    ach.PlayerDash_500.desc プレイヤーの総ダッシュ回数が500回を超える
    ach.PlayerDash_500.reward コイン1000枚

    ach.Endless_20min.name エンドレスチャレンジャー1
    ach.Endless_20min.desc エンドレスモードで20分間生き延びる
    ach.Endless_20min.reward コイン300枚

    ach.Endless_40min.name エンドレスチャレンジャー2
    ach.Endless_40min.desc エンドレスモードで30分間生き延びる
    ach.Endless_40min.reward コイン500枚

    ach.Endless_60min.name エンドレスチャレンジャー3
    ach.Endless_60min.desc エンドレスモードで40分間生き延びる
    ach.Endless_60min.reward コイン700枚


     エンドレスチャレンジャー1
     エンドレスモードで20分間生き延びる
     コイン300枚
    
     エンドレスチャレンジャー2
     エンドレスモードで30分間生き延びる
     コイン500枚
    
     エンドレスチャレンジャー3
     エンドレスモードで40分間生き延びる
    コイン700枚

     */






}

