using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;




public class TimeManager : Singleton<TimeManager>
{
    public float GameTotalTime; //一回のゲームの総プレー時間、指定できます。ゲーム初期化する時、残り時間はこれに初期化(単位は秒)
    public float gameTimeLeft; //残り時間
    public float gameTimePassed; //経過時間
    public int gamePhrase = 1; //ゲーム進行段階
    
    public int endlessExtraPhrase = 0; //無限モードの追加段階
    public float endlessExtraTimeCnt = 0;
    public bool isEndlessTimeStarted = false;

    public float gamePlayUnscaledTimePassed = 0; //ポーズ中もカウントされる経過時間

    public float gameFps;
    public bool isTimeDebug = false;
    //[Range(0f, 10f)]
    public float gameTimeScale = 1;

    public RectTransform minuteUiObj;
    public RectTransform commonUiObj;
    public RectTransform secondUiObj;

    public TextMeshProUGUI bossNameTextUi;
    public Color bossNameColor = Color.red;

    public Image newEnemyWaveMessageImage;
    public List<GameObject> skullImageList;
    public TextMeshProUGUI waveNumText;
    public AudioClip nextWaveSe;

    public GameObject mapEndlessModeReturnPortalObj;
    public bool hasSpawnedPortal = false;
    public bool isEndlessFinalPhaseStarted = false;

    void Start()
    {
        gameTimeLeft = GameTotalTime;
        gameTimePassed = 0;
        endlessExtraPhrase = 0;

        hasSpawnedPortal = false;
    }

    void Update()
    {

        gamePlayUnscaledTimePassed += Time.unscaledDeltaTime; //for achievement that need total play time

        if (GameManager.Instance.stateMachine.State != GameState.GameOver ||GameManager.Instance.stateMachine.State != GameState.GameClear )
        {
            gameTimePassed += Time.deltaTime;
            gameTimeLeft -= Time.deltaTime;
        }
            
        if (isTimeDebug && !SkillManager.Instance.waitingForPlayer) Time.timeScale = gameTimeScale;
     

        ChangeColorOfMinuteSecond(Color.red);

        UpdateTimePhrase();


        if (waveNumText) waveNumText.text = gamePhrase.ToString(); //

        
        if (StageManager.Instance.isEndlessMode) //there will be text hint in front of portal
        {
            if(gameTimePassed >= 60 * 11 && !hasSpawnedPortal)
            {
                isEndlessFinalPhaseStarted = true;
                hasSpawnedPortal = true;
               Vector3 spawnPos = new Vector3(0f, 0.21f, 0f);
               Quaternion spawnRot = Quaternion.identity;
               GameObject obj =  Instantiate(mapEndlessModeReturnPortalObj, spawnPos, spawnRot);
               obj.SetActive(true);


            }
        }

    }

    public void UpdateTimePhrase()
    {
        //1.5 , 3, 5,7,8.5
        //if(gameTimePassed > 60 * 8) gamePhrase = 6;      //8.5分経過で6にする
        //else if(gameTimePassed > 60 * 7) gamePhrase = 5;   //7分経過で5にする
        //else if (gameTimePassed > 60 * 5) gamePhrase = 4;  //5分経過で4にする
        //else if (gameTimePassed > 60 * 3) gamePhrase = 3;  //3分経過で3にする
        //else if (gameTimePassed > 60 * 1.5) gamePhrase = 2;//1.5分経過で2にする
        //else gamePhrase = 1;                                //それ以外は1にする

        //if press R key
        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    DisplayerWaveMessage();
        //    Debug.Log("Wave Message Displayed");
        //}

        if(QuickRunModeManager.Instance.isQuickRunMode) return;

        int targetPhrase = 1; // Start with the default lowest phrase.

    if (gameTimePassed > 60 * 8.5)
    {
        targetPhrase = 7;
    }
    else if (gameTimePassed > 60 * 7)
    {
        targetPhrase = 6;
    }
    else if (gameTimePassed > 60 * 5.6)
    {
        targetPhrase = 5;
    }
    else if (gameTimePassed > 60 * 4.2)
    {
        targetPhrase = 4;
    }
    else if (gameTimePassed > 60 * 2.8)
    {
        targetPhrase = 3;
    }
    else if (gameTimePassed > 60 * 1.5)
    {
        targetPhrase = 2;

        hasSpawnedPortal = true;
    }

    // Now, compare the target phrase to the current phrase.
    // If they are different, it means we have just crossed a time boundary.
    if (targetPhrase != gamePhrase)
    {
        gamePhrase = targetPhrase; // Update to the new phrase.
        DisplayerWaveMessage();    // Call the message function ONLY ONCE upon changing.
    }

    if (StageManager.Instance.isEndlessMode && gameTimePassed > 60 * 10)
        {
            if(!isEndlessTimeStarted) isEndlessTimeStarted = true;

            endlessExtraTimeCnt += Time.deltaTime;
            if (endlessExtraTimeCnt >= 60f) //毎分
            {
                endlessExtraPhrase += 1;
                endlessExtraTimeCnt = 0;
                DisplayerWaveMessage();
            }
        }

    }

    public void ChangeColorOfMinuteSecond(Color color)
    {
        if (StageManager.Instance.isEndlessMode) return;

        if (gameTimeLeft <= 60)
        {
            minuteUiObj.GetComponent<TextMeshProUGUI>().color = color;
            secondUiObj.GetComponent<TextMeshProUGUI>().color = color;
            commonUiObj.GetComponent<TextMeshProUGUI>().color = color;

            //also shake the text 's rectTransform and loop forever 
            //minuteUiObj.DOShakeAnchorPos(0.5f, 3f, 3,70, false, true).SetLoops(-1);
            //secondUiObj.DOShakeAnchorPos(0.5f, 3f, 3,70, false, true).SetLoops(-1);


        }
   

        if (gameTimeLeft <= 0)
        {
            minuteUiObj.GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, 0);
            secondUiObj.GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, 0);
            commonUiObj.GetComponent<TextMeshProUGUI>().color = new Color(color.r, color.g, color.b, 0);
            Color purple = new Color(0.5f, 0, 0.5f, 1);
            commonUiObj.GetComponent<TextMeshProUGUI>().color = purple;
            commonUiObj.GetComponent<TextMeshProUGUI>().text = "";

            //if (MapManager.Instance.nowMap == MapType.SpiderForest)bossNameTextUi.text = L.UI("ui.bossSpider");
            //else if (MapManager.Instance.nowMap == MapType.AncientForest) bossNameTextUi.text = L.UI("ui.bossTurnipa");
            //else if(MapManager.Instance.nowMap == MapType.Castle) bossNameTextUi.text = L.UI("ui.bossCastle");
            //else if (MapManager.Instance.nowMap == MapType.Desert) bossNameTextUi.text = L.UI("ui.bossDragon");
            //else bossNameTextUi.text = "";
            //bossNameTextUi.color = bossNameColor;


            //turn it into switch loop 
            switch (MapManager.Instance.nowMap) 
            {
                case MapType.None:
                    bossNameTextUi.text = "";
                    break;
                case MapType.SpiderForest:
                    bossNameTextUi.text = L.UI("ui.bossSpider");
                    bossNameColor = purple;
                    break;
                case MapType.AncientForest:
                    bossNameTextUi.text = L.UI("ui.bossTurnipa");
                    bossNameColor = Color.green;
                    break;
                case MapType.Temple:
                    bossNameTextUi.text = L.UI("ui.bossTemple");
                    bossNameColor = purple;
                    break;
                case MapType.Desert:
                    bossNameTextUi.text = L.UI("ui.bossDragon");
                    bossNameColor = Color.red;
                    break;
                default:
                    bossNameTextUi.text = "";
                    break;
            }

            bossNameTextUi.color = bossNameColor;

        }
    }



    public void DisplayerWaveMessage()
    {
        SoundEffect.Instance.PlayOneSound(nextWaveSe, 0.7f);

        newEnemyWaveMessageImage.transform.localScale = new Vector3(4.2f, 0f, 1f);
        Color originalColor = newEnemyWaveMessageImage.color;
        newEnemyWaveMessageImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.21f);

        for (int i = 0; i < skullImageList.Count; i++)
        {
            if (i < gamePhrase)
            {
                skullImageList[i].SetActive(true);
            }
            else
            {
                skullImageList[i].SetActive(false);
            }
        }


        newEnemyWaveMessageImage.DOFade(1f, 0.21f).SetEase(Ease.InOutSine).OnComplete(() => {
           
        });


        newEnemyWaveMessageImage.transform.DOScaleY(2.1f, 0.49f).SetEase(Ease.OutBack).OnComplete(() => {
            DOVirtual.DelayedCall(2.1f, () => {
                newEnemyWaveMessageImage.transform.DOScaleY(0f, 0.49f).SetEase(Ease.InBack);
            });
        });
    }


    public void ResetNormalTime()
    {
        Time.timeScale = 1f;
    }

    public void StopTimeNow()
    {
        Time.timeScale = 0f;
    }


}

