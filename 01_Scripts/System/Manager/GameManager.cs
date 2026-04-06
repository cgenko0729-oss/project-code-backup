using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               
using QFSW.MOP2;                
using MonsterLove.StateMachine;
using Cysharp.Threading.Tasks;
using EasyTransition;

public enum GameState
{
    Start,
    Playing,
    Pause,
    BossFight,
    GameOver,
    GameClear
}

public class GameManager : Singleton<GameManager>
{
    public StateMachine<GameState> stateMachine;

    public bool isGameStart;
    public bool isGameOver;
    public bool isGamePause;
    public bool isGameClear;
    public bool isBossFight = false;
    public bool isBattling = true;

    public TransitionSettings tSetting;
    float transitionDelayTime = 0.5f;

    public float gameFps;
    private int frameCount;
    private float elapsedTime;
    private float updateInterval = 0.5f;

    public TextMeshProUGUI fpsDisplayText;

    public bool isDebugMode = true;

    public PlayerData playerData;

    public float gameVol = 1f;

    private void OnEnable()
    {
        EventManager.StartListening("isGameClear", ChangeGameClearState);
        EventManager.StartListening("isGameOver", ChangeGameOverState);
    }

    private void OnDisable()
    {
        EventManager.StopListening("isGameClear", ChangeGameClearState);
        EventManager.StopListening("isGameClear", ChangeGameOverState);
    }

  
    public void DoGameOverNow()
    {
        //find player with tag

        PlayerState player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();

        CapsuleCollider playerCol = player.GetComponent<CapsuleCollider>();
        playerCol.enabled = false;

        player.NowHp = -1;
            
        //player.isAliveFlg = false;

            //　ゲームオーバー処理を実行する
            EventManager.EmitEvent("isGameOver");

        


    }


    private void ChangeGameClearState()
    {
        if(stateMachine.State != GameState.GameClear)
        {
            stateMachine.ChangeState(GameState.GameClear);
        }

        isGameClear = true;

        HIdeManager.Instance.HideStageClearObject();

        CameraViewManager.Instance.ShowAndUnlockCursor();

    }

    private void ChangeGameOverState()
    {
        if(stateMachine.State != GameState.GameOver)
        {
            stateMachine.ChangeState(GameState.GameOver);
        }

        HIdeManager.Instance.HideStageClearObject();

        isGameClear = true;
        isGameOver = true;

        SoundEffect.Instance.Play(SoundList.GameOverSe);

        CameraViewManager.Instance.ShowAndUnlockCursor();

    }

    void Start()
    {
        stateMachine = StateMachine<GameState>.Initialize(this);
        stateMachine.ChangeState(GameState.Start);
       
    }

    void Update()
    {
        GameFPSCalculation();

        gameVol = 1 * AudioManager.Instance.globalVolume * SoundEffect.Instance.allSoundVolume;


    }

    public void PauseGame()
    {
        if(Time.timeScale != 0)Time.timeScale = 0;
        isGamePause = true;
    }

    public void UnPauseGame()
    {
        Time.timeScale = 1;
        isGamePause = false;
    }


    public void GameFPSCalculation()
    {

        frameCount++;
        elapsedTime += Time.unscaledDeltaTime;

 
        if (elapsedTime > updateInterval)
        {
           
            gameFps = frameCount / elapsedTime;
             frameCount = 0;
            elapsedTime = 0f;
        }

        if (fpsDisplayText != null)
        {
            if(isDebugMode)fpsDisplayText.text = "Fps: " + gameFps.ToString("F1");
            else fpsDisplayText.text = "";
            
        }

    }



}

