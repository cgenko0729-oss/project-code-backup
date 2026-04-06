using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound;
using UnityEngine.EventSystems; //SoundManager
using UnityEngine.InputSystem;

public class ResultMenuController : Singleton<ResultMenuController>
{

    public CanvasGroup menuBackgorund;
    public GameObject uiBackground;
    public GameObject retryButton;
    public TextMeshProUGUI clearTimeText;
    public TextMeshProUGUI playerLvText;
    public TextMeshProUGUI enemyKillText;
    public TextMeshProUGUI TotalDamageText;

    public TextMeshProUGUI rankMojiText;
    public TextMeshProUGUI rankValText;

    public TextMeshProUGUI spiderKillText;
    public TextMeshProUGUI mushroomKillText;
    public TextMeshProUGUI batKillText;
    public TextMeshProUGUI spiderEliteKillText;
    public TextMeshProUGUI lizardKillText;
    public TextMeshProUGUI dragonKillText;

    public TextMeshProUGUI spiderKillNumText;
    public TextMeshProUGUI mushroomKillNumText;
    public TextMeshProUGUI batKillNumText;
    public TextMeshProUGUI spiderEliteKillNumText;
    public TextMeshProUGUI lizardKillNumText;
    public TextMeshProUGUI dragonKillNumText;

    public TextMeshProUGUI slashdmgText;
    public TextMeshProUGUI thunderdmgText;
    public TextMeshProUGUI circleBalldmgText;
    public TextMeshProUGUI poisonFieldDmgText;

    public Image slashImg;
    public Image thunderImg;
    public Image circleBallImg;
    public Image poisonFieldImg;

    public TextMeshProUGUI tornadoDmgText;
    public TextMeshProUGUI shieldDmgText;

    public TextMeshProUGUI cointToAddText;

    EnemyManager em;
    DpsManager dm;
    SkillManager sm;

    public float clearTime;
    public int playerLv;
    public int playerHp;
    public int enemyKillCount;
    public int totalDamage;

    public float clearTimeTextAnimator = 0;
    public float playerLvTextAnimator = 0;
    public float enemyKillTextAnimator = 0;
    public float TotalDamageTextAnimator = 0;

    PlayerState playerStatus;

    public bool isResultMenuOpen = false;
    public bool hasPlayedTextAnimation = false;

    public float startFade = 0.5f;

    public Vector3 backgroundScale = new Vector3(9f, 10f, 7f);

    public int coinToAdd;
    public float turnCoinGet = 0;

    public int turnQuestComplete = 0;

    public List<SkillCasterBase> skillCasterBases = new();

    public float currentGameRunTime = 0f;
    public int totalGameItemGet = 0;

    public GameObject detailStatMenuObj;

    public TextMeshProUGUI playerDamageText;
    public TextMeshProUGUI playerDefenceText;
    public TextMeshProUGUI playerCriticalText;
    public TextMeshProUGUI playerLuckText;
    public TextMeshProUGUI playerHealthText;

    public bool isShowResultFinished = false;

    public ResultSkillDisplayDataHolder dataHolder;

    public GameObject padIconRetry;
    public GameObject padIconNextPage;
    public GameObject padIconPrevPage;

    public CanvasGroup retryButtonCg;

    public float highestHistoryDamageNormalModePrevious = 1000000f;

    public float highestHistoryDamagePrevious = 1000000f;
    public float highestHistoryDamageCurrent = 0f;
    public TextMeshProUGUI highestScoreMsgText;

    [ContextMenu("Reset Highest Score")]
    public void ResetHighestScore()
    {
        AchievementManager.Instance.progressData.highestHistoryDamage = 0;
        AchievementManager.Instance.progressData.highestHistoryDamageNormalMode = 0;
        AchievementManager.Instance.SaveProgress();
        highestHistoryDamagePrevious = 0;
        highestHistoryDamageNormalModePrevious = 0;
    }
    public void AddInSkillCasterBase(SkillCasterBase casterToAdd)
    {      
        skillCasterBases.Add(casterToAdd);
    }

    private void OnEnable()
    {
        EventManager.StartListening("FinishQuest", () => { turnQuestComplete++; });
    }

    private void OnDisable()
    {
        EventManager.StopListening("FinishQuest", () => { turnQuestComplete++; });


    }

    void Start()
    {
        playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();

        rankValText.transform.localScale = Vector3.zero;

        em = EnemyManager.Instance;
        dm = DpsManager.Instance;
        sm = SkillManager.Instance;

        highestHistoryDamagePrevious = AchievementManager.Instance.progressData.highestHistoryDamage;
        highestHistoryDamageNormalModePrevious = AchievementManager.Instance.progressData.highestHistoryDamageNormalMode;

    }

    void Update()
    {
        UpdateControllerInput();

        if (!isResultMenuOpen)
        {
            clearTime = TimeManager.Instance.gameTimePassed;
            playerHp = (int)playerStatus.NowHp;
            playerLv = (int)playerStatus.NowLv;
            enemyKillCount = EnemyManager.Instance.allEnemyKillNum;
            totalDamage = (int)DpsManager.Instance.allSkillLifeTimeTotalDamage;
        }
       
    }

    void UpdateControllerInput()
    {
        
        if(Gamepad.current == null) return;



        if (isShowResultFinished)
        {

            //if press controller confirm key
            if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                TransitionController.Instance.ChangeToTitleScene();
                Debug.Log("Return to Title Scene");
            }

            //if controller submit press: 

            if (Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                dataHolder.ToNextDetailPage(false);
                Debug.Log("Next Detail Page");
            }
            if (Gamepad.current.leftShoulder.wasPressedThisFrame)
            {
                dataHolder.ToNextDetailPage(true);
                Debug.Log("Previous Detail Page");
            }
        }

        
            

    }


    string BuildClearTime(float seconds)
    {
        int m = Mathf.FloorToInt(seconds / 60);
        int s = Mathf.FloorToInt(seconds % 60);
        //return $"生存時間:{m}分{s:D2}秒";
        //return L.UI("ui.clearTime") + $":{m}m{s:D2}s";
        return $"{L.UI("ui.clearTime")}:{m}{L.UI("ui.Result_Minute")}{s:D2}{L.UI("ui.Result_Second")}";
    }

    public void ShowResultMenu()
    {

        Cursor.lockState = CursorLockMode.None;

        float damagePercent = BuffManager.Instance.gobalDamageAdd;

        playerDamageText.text = $":{(int)damagePercent}" + "%";
        playerDefenceText.text = $":{(int)BuffManager.Instance.gobalPlayerDefenceAdd}"+ "%";
        playerCriticalText.text = $":{(int)BuffManager.Instance.gobalCritChanceAdd}%";
        playerLuckText.text = $":{(int)SkillManager.Instance.luck}"+ "%";
        playerHealthText.text = $":{(int)playerStatus.NowHp}" + " /" + $"{(int)playerStatus.MaxHp}";


        clearTime      = TimeManager.Instance.gameTimePassed;
        playerHp       = (int)playerStatus.NowHp;
        playerLv       = (int)playerStatus.NowLv;
        enemyKillCount = EnemyManager.Instance.allEnemyKillNum;
        totalDamage    = (int)DpsManager.Instance.allSkillLifeTimeTotalDamage;
        //float petDamage = DpsManager.Instance.GetDamageBySkillType(SkillIdType.Pet);
        
        coinToAdd = (int)((int)(enemyKillCount/ 28) + (int)(clearTime/2.1) + (int)(playerLv*7.7)) ;
        
        DifficultyType diff = StageManager.Instance.mapData.stageDifficulty;
        if (diff == DifficultyType.Hard) coinToAdd = (int)(coinToAdd * 1.25f);
        else if (diff == DifficultyType.Nightmare) coinToAdd = (int)(coinToAdd * 1.5f);
        else if (diff == DifficultyType.Hell) coinToAdd = (int)(coinToAdd * 2f);

        //quick mode coin cut to half

        coinToAdd += (int)turnCoinGet;
        CurrencyManager.Instance.Add(coinToAdd); 

        //(EnemyManager.Instance.allEnemyKillNum/70) + (TimeManager.Instance.gameTimePassed/2.1) + (playerStatus.NowLv* 2)

        currentGameRunTime = TimeManager.Instance.gamePlayUnscaledTimePassed;

        highestHistoryDamageCurrent = totalDamage;

        if (StageManager.Instance.isEndlessMode)
        {
            if (highestHistoryDamageCurrent > highestHistoryDamagePrevious)
            {
                AchievementManager.Instance.progressData.highestHistoryDamage = (long)highestHistoryDamageCurrent;
                AchievementManager.Instance.SaveProgress();
                highestScoreMsgText.gameObject.SetActive(true);
                if (SteamLeaderboardManager.Instance != null && AchievementManager.Instance.progressData.isParticipatedInSteamLeaderboard) SteamLeaderboardManager.Instance.UpdateScore((int)highestHistoryDamageCurrent);
                Debug.Log("New Highest History Damage Achieved: " + highestHistoryDamageCurrent);
                //Update Leaderboard, Display in ui
            }
        }
        else
        {
            if (highestHistoryDamageCurrent > highestHistoryDamageNormalModePrevious)
            {
                AchievementManager.Instance.progressData.highestHistoryDamageNormalMode = (long)highestHistoryDamageCurrent;
                AchievementManager.Instance.SaveProgress();
                highestScoreMsgText.gameObject.SetActive(true);
                if (SteamLeaderboardManager.Instance != null && AchievementManager.Instance.progressData.isParticipatedInSteamLeaderboard) SteamLeaderboardManager.Instance.UpdateScoreNormalMode((int)highestHistoryDamageCurrent);
                Debug.Log("New Highest History Damage (Normal Mode) Achieved: " + highestHistoryDamageCurrent);
                //Update Leaderboard, Display in ui
            }

        }

        

        uiBackground.SetActive(true);
        uiBackground.transform.localScale = Vector3.zero;     
        menuBackgorund.alpha = 0;                             
        menuBackgorund.blocksRaycasts = true;

        isResultMenuOpen              = true;
        hasPlayedTextAnimation = false;

        if(GameManager.Instance.isGameOver)
        {
            if(clearTime >= 300f)
            {
                rankValText.text = "C";
                rankValText.color = Color.white;
            }
            else if (clearTime >= 300f)
            {
                rankValText.text = "D";
                rankValText.color = Color.grey;
            }
            else if (clearTime >= 120f)
            {
                rankValText.text = "E";
                rankValText.color = Color.red;
            }else if(clearTime < 120f)
            {
                rankValText.text = "F";
                rankValText.color = Color.red;
            }

            
        }
        else
        {
            if (coinToAdd >= 1000 || playerLv >= 42 || playerHp >= 95)
            {
                rankValText.text = "S";
                rankValText.color = Color.yellow;
            }
            else if (coinToAdd >= 700 || playerLv >= 35 || playerHp >= 70)
            {
                rankValText.text = "A";
                rankValText.color = Color.yellow;
            }
            else 
            {
                rankValText.text = "B";
                rankValText.color = Color.green;
            }
            

        }

        Sequence openSeq = DOTween.Sequence();                
        openSeq.Append(uiBackground.transform.DOScale(backgroundScale, 0.4f).SetEase(Ease.OutBack));
        openSeq.Join(menuBackgorund.DOFade(1f, 0.3f).SetEase(Ease.OutQuad));
        openSeq.AppendCallback(PlayStatTweens);

        spiderKillText.text = L.UI("ui.MoverEnemy");
        mushroomKillText.text =L.UI("ui.SurroundingEnemy") ;
        batKillText.text =L.UI("ui.FlyingEnemy");
        spiderEliteKillText.text =L.UI("ui.EliteEnemy");
        lizardKillText.text =L.UI("ui.MageEnemy");
        dragonKillText.text =L.UI("ui.BomberEnemy");

        spiderKillNumText.text = $":{em.moverKillNum}";
        mushroomKillNumText.text = $":{em.SurrounderKillNum}";
        batKillNumText.text = $":{em.FlyerKillNum}";
        spiderEliteKillNumText.text = $":{em.EliteMoverKillNum}";
        lizardKillNumText.text = $":{em.CasterKillNum}";
        dragonKillNumText.text = $":{em.bomberKillNum}";

        //slashdmgText.text = $"矢:{(int)dm.arrowLifeTimeTotalDamage}";
        //thunderdmgText.text = $"ブーメラン:{(int)dm.boomerangLifeTimeTotalDamage}";
        //circleBalldmgText.text = $"騎士の盾:{(int)dm.shieldLifeTimeTotalDamage}";
        //poisonFieldDmgText.text = $"バウンドナイフ :{(int)dm.bounceLifeTimeTotalDamage}";

        slashdmgText.text = ""; //初期化
        thunderdmgText.text = "";
        circleBalldmgText.text = "";
        poisonFieldDmgText.text = "";
        tornadoDmgText.text = "";
        shieldDmgText.text = "";

        //slashdmgText.text = L.SkillName(sm.activeSkillCastersHolder[0].casterIdType) + $":{(int)dm.GetDamageBySkillType(sm.activeSkillCastersHolder[0].casterIdType)}";
        //if(sm.activeSkillCastersHolder.Count >= 2) thunderdmgText.text = L.SkillName(sm.activeSkillCastersHolder[1].casterIdType) + $":{(int)dm.GetDamageBySkillType(sm.activeSkillCastersHolder[1].casterIdType)}";
        //if (sm.activeSkillCastersHolder.Count >= 3) circleBalldmgText.text = L.SkillName(sm.activeSkillCastersHolder[2].casterIdType) + $":{(int)dm.GetDamageBySkillType(sm.activeSkillCastersHolder[2].casterIdType)}";
        //if (sm.activeSkillCastersHolder.Count >= 4) poisonFieldDmgText.text = L.SkillName(sm.activeSkillCastersHolder[3].casterIdType) + $":{(int)dm.GetDamageBySkillType(sm.activeSkillCastersHolder[3].casterIdType)}";

        if (sm.activeSkillCastersHolder.Count >= 1)
        {
            var skillType = sm.activeSkillCastersHolder[0].casterIdType;
            var damage = (int)dm.GetLifeTimeTotalDamageBySkillType(skillType);
            //slashdmgText.text = L.SkillName(skillType) + $":{damage}";
            slashdmgText.text = $"{damage}";
            slashImg.sprite = sm.activeSkillCastersHolder[0].casterSpriteImage;
        }

        if (sm.activeSkillCastersHolder.Count >= 2)
        {
            var skillType = sm.activeSkillCastersHolder[1].casterIdType;
            var damage = (int)dm.GetLifeTimeTotalDamageBySkillType(skillType);
            //thunderdmgText.text = L.SkillName(skillType) + $":{damage}";
            thunderdmgText.text = $"{damage}";
            thunderImg.sprite = sm.activeSkillCastersHolder[1].casterSpriteImage;
        }

        if (sm.activeSkillCastersHolder.Count >= 3)
        {
            var skillType = sm.activeSkillCastersHolder[2].casterIdType;
            var damage = (int)dm.GetLifeTimeTotalDamageBySkillType(skillType);
            //circleBalldmgText.text = L.SkillName(skillType) + $":{damage}";
            circleBalldmgText.text = $"{damage}";
            circleBallImg.sprite = sm.activeSkillCastersHolder[2].casterSpriteImage;
        }

        if (sm.activeSkillCastersHolder.Count >= 4)
        {
            var skillType = sm.activeSkillCastersHolder[3].casterIdType;
            var damage = (int)dm.GetLifeTimeTotalDamageBySkillType(skillType);
            //poisonFieldDmgText.text = L.SkillName(skillType) + $":{damage}";
            poisonFieldDmgText.text = $"{damage}";
            poisonFieldImg.sprite = sm.activeSkillCastersHolder[3].casterSpriteImage;
        }


        //tornadoDmgText.text = $"竜巻:{(int)dm.tornadoLifeTimeTotalDamage}";    
        //shieldDmgText.text = $"爆裂魔法:{(int)dm.starMagicLifeTimeTotalDamage}";


        // 選択中UIをRetryButtonに設定する
        if (retryButton != null)
        {
            EventSystem.current.SetSelectedGameObject(retryButton);
        }
    }

    void PlayStatTweens()                                     
    {
        if (hasPlayedTextAnimation) return;
        hasPlayedTextAnimation = true;

        if (Gamepad.current != null)
        {
            padIconRetry.SetActive(true);
            padIconNextPage.SetActive(true);
            padIconPrevPage.SetActive(true);
        }

        isShowResultFinished = true;

        if (!GameManager.Instance.isGameOver)
        {
            //AchievementManager.Instance.NotifyGameCLear(1);
            AchievementManager.Instance.NotifyStageCleared(StageManager.Instance.mapData.mapType, StageManager.Instance.mapData.stageDifficulty);
        }
        else if(GameManager.Instance.isGameOver)
        {
            AchievementManager.Instance.NotifyGameOver(1);
        }

        if(StageManager.Instance.isEndlessMode && clearTime > 60 * 20)
        {
            AchievementManager.Instance.UnlockAchievement("Endless_20min");
        }

        if(StageManager.Instance.isEndlessMode && clearTime > 60 * 30)
        {
            AchievementManager.Instance.UnlockAchievement("Endless_30min");
        }

        if(StageManager.Instance.isEndlessMode && clearTime > 60 * 40)
        {
            AchievementManager.Instance.UnlockAchievement("Endless_40min");
        }

        AchievementManager.Instance.NotifyDamageDealt(totalDamage);
        AchievementManager.Instance.NotifyEnemyKilled(enemyKillCount);
        AchievementManager.Instance.NotifyCoinCollect(coinToAdd);
        AchievementManager.Instance.NotifyGameTimePass((int)currentGameRunTime);
        AchievementManager.Instance.NotifyItemGet(totalGameItemGet);
        AchievementManager.Instance.NotifyQuestFinished(turnQuestComplete);
        

        const float dur  = 0.5f;
        Ease        ease = Ease.OutQuad;

        Sequence seq = DOTween.Sequence();

        PlayCountDownSound();

        clearTimeTextAnimator = 0;
        seq.Append(
            DOTween.To(() => clearTimeTextAnimator,
                       x => {
                           clearTimeTextAnimator = x;
                           clearTimeText.text    = BuildClearTime(x);
                       },
                       clearTime, dur).SetEase(ease)
        );

        seq.AppendCallback(PlayCountDownSound);

        seq.Append(
            DOVirtual.Int(0, playerLv, dur,
                v => playerLvText.text = $"Lv     :{v}")
            .SetEase(ease)
        );

        seq.AppendCallback(PlayCountDownSound);

        seq.Append(
            DOVirtual.Int(0, enemyKillCount, dur,
                v => enemyKillText.text = L.UI("ui.kills") + $":{v}")
            .SetEase(ease)
        );

        seq.AppendCallback(PlayCountDownSound);

        seq.Append(
            DOVirtual.Int(0, totalDamage, dur,
                v => TotalDamageText.text = L.UI("ui.totalDamage") + $":{v}")
            .SetEase(ease)
        );

        if (highestScoreMsgText.gameObject.activeSelf)
        {
            seq.Append(highestScoreMsgText.DOFade(1f, 0.5f));

            //seq.Append(highestScoreMsgText.transform.DOShakePosition(1f, new Vector3(5f, 0f, 0f), 10, 90f, false, true).SetLoops(-1));
            seq.AppendCallback(() => 
            {
                highestScoreMsgText.transform
                    .DOShakePosition(1f, new Vector3(5f, 0f, 0f), 10, 90f, false, true)
                    .SetLoops(-1); // Infinite loop
            });

        }

        //seq.Append(retryButtonCg.DOFade(1f, 0.5f).SetEase(Ease.OutQuad));

        seq.AppendCallback(PlayCountDownSound);

        //cointToAddText.text = $"+{coinToAdd}枚"; .append
        seq.Append(
            DOVirtual.Int(0, coinToAdd, dur,
                v => cointToAddText.text = $"+{v}")
            .SetEase(ease)
        );

        
        //CurrencyManager.Instance.SaveCoinToFile();


        seq.Append(
            rankValText.transform.DOScale(new Vector3(0.177f,0.177f,0.177f), dur)
                .SetEase(ease)
        ).OnComplete(() => {
            Invoke(nameof(StopTimeWithDelay), 14.9f);
            PlayerState player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();
            CapsuleCollider playerCol = player.GetComponent<CapsuleCollider>();
            playerCol.enabled = false;

            retryButtonCg.DOFade(1f, 0.5f).SetEase(Ease.OutQuad);

             detailStatMenuObj.gameObject.SetActive(true);
        ResultSkillDisplayDataHolder.Instance.SetAllSkillDisplays();
            MenuOpenAnimator menuAni = detailStatMenuObj.GetComponent<MenuOpenAnimator>();
            menuAni.PlayeMenuAni(true);
            EventManager.EmitEvent("OpenDetailStatMenu");

            

            isShowResultFinished = true;

        });

        //seq.AppendCallback(PlayCountDownSound);

       
    }

    public void StopTimeWithDelay()
    {
        if(StageManager.Instance.isGameScene && GameManager.Instance.isGameOver) Time.timeScale  = 0f;
    }

    public void PlayCountDownSound()
    {
        SoundEffect.Instance.Play(SoundList.ScoreCountSe); 
    }


}

