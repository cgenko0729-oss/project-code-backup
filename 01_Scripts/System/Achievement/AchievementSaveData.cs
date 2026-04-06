using System;
using System.Collections.Generic;

[Serializable]
public class AchievementProgressEntry
{
    public string id;
    public int value;

    // A parameterless constructor is needed for serialization
    public AchievementProgressEntry() { }

    public AchievementProgressEntry(string id, int value)
    {
        this.id = id;
        this.value = value;
    }
}

[Serializable]
public class AchievementSaveData
{
    //// A list of IDs for achievements that have been unlocked permanently.
    //public HashSet<string> unlockedAchievementIds;
    
    //// A dictionary to track progress for incremental achievements.
    //// Key: The achievement ID (e.g., "killed_100_enemies")
    //// Value: The current progress (e.g., 57)
    //public Dictionary<string, int> incrementalAchievementProgress;

    //// A place to store general player stats that can trigger achievements.
    //public long totalDamageDealt;
    //public int totalEnemiesKilled;
    //// Add any other stats you need to track here...

    //// Constructor to initialize with default values
    //public AchievementSaveData()
    //{
    //    unlockedAchievementIds = new HashSet<string>();
    //    incrementalAchievementProgress = new Dictionary<string, int>();
    //    totalDamageDealt = 0;
    //    totalEnemiesKilled = 0;
    //}



    public List<TraitType> unlockedTraitTypes;

    // A list of IDs for achievements that have been unlocked permanently.
    // Changed from HashSet to List for better serialization compatibility.
    public List<string> unlockedAchievementIds;
    
    // A list to track progress for incremental achievements.
    // This replaces the Dictionary.
    public List<AchievementProgressEntry> incrementalAchievementProgress;

    // A place to store general player stats that can trigger achievements.
    public long totalDamageDealt;
    public int totalEnemiesKilled;

    public int totalQuestsCompleted;
    public int totalHealedAmount;
    public int totalItemsCollected;
    public int totalCoinGetted;
    public int totalPlayTime; //in seconds

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

    public bool isCharacter1Unlocked = true;
    public bool isCharacter2Unlocked = false;
    public bool isCharacter3Unlocked = false;
    public bool isCharacter4Unlocked = false;

    public bool isLive30MinEndless = false;

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

    //getter for isCharacterUnlocked:
    public bool IsCharacter1Unlocked() { return isCharacter1Unlocked; }
    public bool IsCharacter2Unlocked() { return isCharacter2Unlocked; }
    public bool IsCharacter3Unlocked() { return isCharacter3Unlocked; }
    public bool IsCharacter4Unlocked() { return isCharacter4Unlocked; }


    public bool isStage1Unlocked = true;
    public bool isStage2Unlocked = false;
    public bool isStage3Unlocked = false;
    public bool isStage4Unlocked = false;
    public bool isStage5Unlocked = false;


    public float highestHistoryDamage;
    public float highestHistoryDamageNormalMode;
    public bool isParticipatedInSteamLeaderboard = true;

    public bool isTutorialPetFinished = false;
    public bool isTutorialSkillFinished = false;
    public bool isTutorEndlessModeFinished = false;
    public bool isTutorSkillTreeFinished = false;

    public bool isSkillTreeUiInited = false;


    // Constructor to initialize with default values
    public AchievementSaveData()
    {
        unlockedTraitTypes = new List<TraitType>();

        unlockedAchievementIds = new List<string>();
        incrementalAchievementProgress = new List<AchievementProgressEntry>();
        totalDamageDealt = 0;
        totalEnemiesKilled = 0;

        totalQuestsCompleted = 0;
        totalHealedAmount = 0;
        totalItemsCollected = 0;
        totalCoinGetted = 0;
        totalGameClears = 0;
        totalDeaths = 0;

        isStage1NormalCleared = false;
        isStage1HardCleared = false;
        isStage1NightmareCleared = false;
        isStage1HellCleared = false;

        isStage2NormalCleared = false;
        isStage2HardCleared = false;
        isStage2NightmareCleared = false;
        isStage2HellCleared = false;

        isStage3NormalCleared = false;
        isStage3HardCleared = false;
        isStage3NightmareCleared = false;
        isStage3HellCleared = false;

        isStage4NormalCleared = false;
        isStage4HardCleared = false;
        isStage4NightmareCleared = false;
        isStage4HellCleared = false;

        isStageAncientForestUnlocked = false;
        isStageDesertUnlocked = false;
        isStageTempleUnlocked = false;


        highestHistoryDamage = 0f;
        highestHistoryDamageNormalMode = 0f;
        isParticipatedInSteamLeaderboard = true;

        isTutorialPetFinished = false;
        isTutorialSkillFinished = false;
        isTutorEndlessModeFinished = false;
        isTutorSkillTreeFinished = false;

        isSkillTreeUiInited = false;









    }





}

