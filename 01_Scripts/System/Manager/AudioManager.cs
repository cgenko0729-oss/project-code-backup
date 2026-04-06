using DG.Tweening;
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//BGMの種類
public enum BGMType
{
    Title,     //タイトル画面の曲
    PauseMenu, //ポーズメニュー曲
    Boss,      //ボス戦曲(ファンタジーフォレスト用)
    FF_Field,  //フィールド曲(ファンタジーフォレスト用)
    AF_Field,  //フィールド曲(エンシェントフォレスト用)
    DS_Field,  //フィールド曲(砂漠用)
    CS_Field,  //フィールド曲(城用)
    TP_Field   //フィールド曲(寺院用)
}

public class AudioManager : Singleton<AudioManager>
{
    private float baseVolume = 0.5f;   //BGM本体の音量を変える

    [Header("マップデータ")]
    public MapData nowMap; 

    [SerializeField]private float fadeDuration = 2.0f;   //BGMをフェードアウトさせる変数

    [Header("全体の音量調節(BGM,SE)")]
    [Range(0, 1)]
    public float globalVolume = 1.0f;

    [Header("BGM全体の音量調節")]
    [Range(0, 1)]
    public float allBGMVolume = 1.0f;

    [Header("タイトル曲の本体音量")]
    [Range(0, 1)]
    public float titleBGMVolume = 0.5f;

    [Header("フィールド曲の本体音量")]
    [Range(0, 1)]
    public float fieldBGMVolume = 0.5f;

    [Header("ボス戦の本体音量")]
    [Range(0, 1)]
    public float bossBGMVolume = 0.25f;

    [Header("その他の本体音量")]
    [Range(0, 1)]
    public float defaultBGMVolume = 0.0f;

    //BGMのリスト
    public AudioSource[] bgmList;

    private AudioSource audioSource = null;
    private bool  isPause = false;
    private bool  isOptionMenu = false;
    private float optionBGMVol = 0.5f; // オプション画面を開いたときのBGM音量
    private BGMType nowBGMType; // 現在のBGMを記録する変数

    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーン名によってBGM切り替え
        switch (scene.name)
        {
            case "TitleScene":
                //フラグリセット
                isPause = false;
                isOptionMenu = false;
                FadeInBGM(BGMType.Title);
                break;
            case "GameScene":
                //フラグリセット
                isPause      = false;
                isOptionMenu = false;
                switch (nowMap.mapType)
                {
                    case MapType.SpiderForest:
                        FadeInBGM(BGMType.FF_Field);
                        break;
                    case MapType.AncientForest:
                        FadeInBGM(BGMType.AF_Field);
                        break;
                    case MapType.Desert:
                        FadeInBGM(BGMType.DS_Field);
                        break;
                    case MapType.Castle:
                        FadeInBGM(BGMType.CS_Field);
                        break;
                    case MapType.Temple:
                        FadeInBGM(BGMType.TP_Field);
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }

    void Start()
    {
    }

    public void OnEnable()
    {
        EventManager.StartListening(GameEvent.pushTitlebtn,   FadeOutNowBGM);
        EventManager.StartListening(GameEvent.CutSceneStart , FadeOutNowBGM);
        EventManager.StartListening(GameEvent.StartBossFight, PlayBossBGM);
        EventManager.StartListening(GameEvent.isPauseMenu,    TogglePauseBGM);
        EventManager.StartListening(GameEvent.isOptionMenu,   ToggleOptionBGM);
        EventManager.StartListening("isGameClear",            FadeOutNowBGM);
        EventManager.StartListening("isGameOver",             FadeOutNowBGM);
    }

    public void OnDisable()
    {
        EventManager.StopListening(GameEvent.pushTitlebtn,   FadeOutNowBGM);
        EventManager.StopListening(GameEvent.CutSceneStart , FadeOutNowBGM);
        EventManager.StopListening(GameEvent.StartBossFight, PlayBossBGM);
        EventManager.StopListening(GameEvent.isPauseMenu,    TogglePauseBGM);
        EventManager.StartListening(GameEvent.isOptionMenu,  ToggleOptionBGM);
        EventManager.StopListening("isGameClear",            FadeOutNowBGM);
        EventManager.StopListening("isGameOver",             FadeOutNowBGM);
    }

    void Update()
    {
        if (audioSource == null) return;

        if (DOTween.IsTweening(audioSource)) return;

        if (isOptionMenu)
        { 
            // オプション画面を開いている間は、音量を指定の倍率まで下げる
            audioSource.volume = baseVolume * allBGMVolume * globalVolume * optionBGMVol;
        }
        else
        {
            // 通常の音量計算
            audioSource.volume = baseVolume * allBGMVolume * globalVolume;
        }
    }

    public void PlayBossBGM()
    {
        PlayBGM(BGMType.Boss);
    }

    //BGMをフェードアウトさせる関数
    public void FadeOutNowBGM()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                audioSource.Stop();
                // フェード後、volume を元に戻す（次に再生する時用）
                NowBGMOption(nowBGMType);
            });
        }
    }

    
    public void FadeInBGM(BGMType _bgm)
    {
        audioSource = bgmList[(int)_bgm];
        audioSource.volume = 0f;
        audioSource.Play();

        nowBGMType = _bgm;

        //NowBGMOptionを使うとボリュームが設定されるので使わない
        switch (_bgm)
        {
            case BGMType.Title:
                baseVolume = titleBGMVolume;
                break;
            case BGMType.Boss:
                baseVolume = bossBGMVolume;
                break;
            case BGMType.FF_Field:
            case BGMType.AF_Field:
            case BGMType.DS_Field:
            case BGMType.CS_Field:
            case BGMType.TP_Field:
                baseVolume = fieldBGMVolume;
                break;
            default:
                baseVolume = defaultBGMVolume;
                break;
        }

        // フェードイン処理
        audioSource.DOFade(baseVolume * allBGMVolume* globalVolume, fadeDuration * 2);
    }

    //BGMを再生する関数
    //_bgm...再生したいBGMの引数
    public void PlayBGM(BGMType _bgm)
    {
        audioSource = bgmList[(int)_bgm];
        audioSource.Play();

        nowBGMType = _bgm;
        NowBGMOption(_bgm);
    }

    //現在再生されているBGMを止める関数
    //_nowBGM...今再生中のBGMの引数
    public void StopNowBGM(BGMType _nowBGM)
    {
        audioSource = bgmList[(int)_nowBGM];
        audioSource.Stop();
    }

    //BGMを変更する関数
    //_nowBGM...今再生中のBGMの引数
    //_nextBGM...再生したいBGMの引数
    public void ChangeNextBGM(BGMType _nowBGM, BGMType _nextBGM)
    {
        //現在の曲を止め、次の曲を再生する
        bgmList[(int)_nowBGM].Stop();
        audioSource = bgmList[(int)_nextBGM];
        audioSource.Play();

        nowBGMType = _nextBGM;
        NowBGMOption(_nextBGM);
    }

    public void PlayPauseBGM()
    {
        float pauseBGMVol = 0.5f;
        AudioSource pauseBGM = bgmList[(int)BGMType.PauseMenu];

        audioSource.Pause();
        pauseBGM.volume = pauseBGMVol * allBGMVolume * globalVolume;
        pauseBGM.Play();
        isPause = true;

    }

    public void StopPauseBGM()
    {
        AudioSource pauseBGM = bgmList[(int)BGMType.PauseMenu];

        if (pauseBGM.isPlaying)
        {
            pauseBGM.Stop();
        }

        audioSource.UnPause();

        NowBGMOption(nowBGMType);
        
        isPause = false;
    }

    //ポーズ画面を押したとき、専用BGMが再生される関数
    private void TogglePauseBGM()
    {
        if (audioSource == null) return;

        float pauseBGMVol = 0.5f;
        AudioSource pauseBGM = bgmList[(int)BGMType.PauseMenu];

        if (!isPause)
        {
            audioSource.Pause();
            pauseBGM.volume = pauseBGMVol * allBGMVolume * globalVolume;
            pauseBGM.Play();
            isPause = true;
        }
        else
        {
            if (pauseBGM.isPlaying)
            {
                pauseBGM.Stop();
            }

            audioSource.UnPause();

            NowBGMOption(nowBGMType);
           
            isPause = false;
        }
    }

    //オプション画面を押したとき、BGMの音量が下がる関数
    private void ToggleOptionBGM()
    {
        if (audioSource == null) return;

        StopPauseBGM(); // ポーズBGMが流れている場合は停止する

        isOptionMenu = !isOptionMenu;
    }

    //現在のBGMに対してのオプション
    private void NowBGMOption(BGMType type)
    {
        switch (type)
        {
            case BGMType.Title:
                baseVolume = titleBGMVolume;
                break;
            case BGMType.Boss:
                baseVolume = bossBGMVolume;
                break;
            case BGMType.FF_Field:
            case BGMType.AF_Field:
            case BGMType.DS_Field:
            case BGMType.CS_Field:
            case BGMType.TP_Field:
                baseVolume = fieldBGMVolume;
                break;
            default:
                baseVolume = defaultBGMVolume;
                break;
        }

        audioSource.volume = baseVolume * allBGMVolume * globalVolume;

    }

    //セッター(GlobalVolume)
    public void GlobalVolumeOption(float vol)
    {
        globalVolume=Mathf.Clamp(vol, 0, 1);
    }

    //セッター(AllBGMVolume)
    public void AllBGMVolumeOption(float vol)
    {
        allBGMVolume = Mathf.Clamp(vol, 0, 1);
    }
}

