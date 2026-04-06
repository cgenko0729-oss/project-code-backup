using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine;
using System.Linq; //StateMachine
using Hellmade.Sound;





public class SoundEffect : Singleton<SoundEffect>
{
    public float seVolAdjust = 1.4f; //SE音量調整用の倍率
    public float allSoundVolume = 1.0f; //全体のSE音量
    //public float allBgmVolume = 1.0f; //全体のBGM音量
    //public float allUiSoundVolume = 1.0f; //全体のUI音量

    [SerializeField] private List<SoundData> soundLibrary;
    private Dictionary<SoundList,SoundData> map;

    public bool HasSound = false;

    public float getExpSoundFrequency = 0.1f; //経験値取得時のサウンド再生間隔

    public float getITESoundFrequency = 0.0f; //無敵状態で敵に触れたときのサウンド再生間隔
    public float getITESoundFrequencyMax = 1.0f; //↑の最大時間

    public AudioSource source;

    public AudioClip spawnFlyerEnemySound;

    public AudioClip unlockAchSe;

    public AudioClip clickButtonSe;

    public int controllerVibrationType = 0;

    private List<PlayingSoundInfo> currentlyPlayingSounds = new List<PlayingSoundInfo>();

    public float bombExplosionInterval = 0.21f; //爆弾系スキルの爆発SE再生間隔
    public float bombExplosionTimer = 0f;

    public AudioClip buySkillTreeItemSe;

    // 再生中のサウンド情報を保持するための小さなクラス（構造体でも可）
    private class PlayingSoundInfo
    {
        public SoundList id;
        public float endTime;
    }

    public void SetSoundEffectVolume(float volume)
    {
        allSoundVolume = volume;
    }
    void Awake()
    {
       //Debug.Log($"[SoundEffect] Awake called. soundLibrary has {soundLibrary.Count} items.");

        map = soundLibrary.ToDictionary(x => x.id);

       //Debug.Log($"[SoundEffect] Map initialized with {map.Count} sounds.");

        //if (!source) source = gameObject.AddComponent<AudioSource>();

    }

    public void PlayBuySkillTreeItemSe()
    {
        PlayOneSound(buySkillTreeItemSe, 0.49f);
    }

    public void PlayRefundSKillTreeSe()
    {
        Play(SoundList.ShopRefundSe);
    }

    private void Start()
    {
        ApplySoundSeLoadFromSaveFile();
    }

     void ApplySoundSeLoadFromSaveFile()
    {
        bool loaded = SettingsSaveSystem.Load(
        out int w,
        out int h,
        out FullScreenMode mode,
        out int vSync,
        out int fps,
        out int quality,
        out int lang,
        out float globalVolume,
        out float bgmVolume,
        out float seVolume,
        out int vibrateType
    );


        SoundEffect.Instance.allSoundVolume = seVolume;

    }

    public void Play(SoundList id)
    {
        if (!HasSound) return; //音を鳴らさない設定の場合は何もしない
        
        //Debug.Log($"[SoundEffect] Attempting to play sound: {id}. Map contains key: {map.ContainsKey(id)}");

        if (map.ContainsKey(id))
        {
            var soundData   = map[id];
            if (soundData.clip == null)
            {
                Debug.LogError($"[SoundEffect] SoundData for {id} is found, but its AudioClip is NULL!");
            }
            else
            {
                EazySoundManager.PlaySound(soundData.clip, soundData.volume);

                currentlyPlayingSounds.Add(new PlayingSoundInfo
                {
                    id = id,
                    endTime = Time.time + soundData.clip.length //現在時刻 + クリップの長さ = 終了時刻
                });
            }
        }
        else
        {
            //Debug.LogError($"[SoundEffect] Key '{id}' not found in the sound map! Playback failed.");
        }
       
        //var clip  = def.variants.GetRandom();       // extension 
    }

    public void OnEnable()
    {
        EventManager.StartListening(GameEvent.PlayerDash, PlayPlayerDashSe);
        EventManager.StartListening("isGameClear", ClearSeDelay);
        //EventManager.StartListening(GameEvent.ChangePlayerHp, PlayerGetDamgeSe);



    }
    public void OnDisable()
    {
        EventManager.StopListening(GameEvent.PlayerDash, PlayPlayerDashSe);
        EventManager.StopListening("isGameClear", ClearSeDelay);
        //EventManager.StopListening(GameEvent.ChangePlayerHp, PlayerGetDamgeSe);



    }

    public void Update()
    {
        EazySoundManager.GlobalSoundsVolume = allSoundVolume  * seVolAdjust * AudioManager.Instance.globalVolume * 1.4f;

        getExpSoundFrequency -= Time.deltaTime; //経験値取得時のサウンド再生間隔を減らす

        getITESoundFrequency-= Time.deltaTime; //無敵状態で敵に触れたときのサウンド再生間隔を減らす

        if(getITESoundFrequency < 0) getITESoundFrequency = 0f;
        //EazySoundManager.GlobalMusicVolume = allBgmVolume;
        //EazySoundManager.GlobalUISoundsVolume = allUiSoundVolume;

        //if preee y 

        // 現在再生中のサウンドリストを更新して、終了したサウンドを削除する
        for (int i = currentlyPlayingSounds.Count - 1; i >= 0; i--)
        {   
            if (currentlyPlayingSounds[i].endTime <= Time.time)
            {              
                currentlyPlayingSounds.RemoveAt(i);
            }
        }

        bombExplosionTimer -= Time.deltaTime;


    }

    public void PlayBombExplosionSound()
    {
        if(bombExplosionTimer >0) return;
        bombExplosionTimer = bombExplosionInterval;
        Play(SoundList.TraitAfterSkillExplosionSe);
    }

    public void PlayBuySe()
    {
        Play(SoundList.ShopBuySe);
    }

    public void PlayUnlockAchSe()
    {
        PlayOneSound(unlockAchSe, 0.29f);

    }

    public bool IsPlaying(SoundList id)
    {
        // 現在再生中のサウンドリストを更新して、終了したサウンドを削除する
        return currentlyPlayingSounds.Any(sound => sound.id == id);
    }

    public void GameClearSe()
    {
        Play(SoundList.GameClearSe);
    }

    public void PlayPlayerDashSe()
    {
        Play(SoundList.PlayerDash);
    }

    public void ClearSeDelay()
    {
        //BossBgm.volume = 0f;
        Invoke("GameClearSe", 1.1f);
    }

    public void PlayerGetDamgeSe()
    {
        float amount = EventManager.GetFloat("ChangePlayerHp");
        if(amount < 0)Play(SoundList.PlayerGetDamage);
    }

    public void PlayOneSound(AudioClip clip, float vol = 1f)
    {
        source.PlayOneShot(clip, AudioManager.Instance.globalVolume * SoundEffect.Instance.allSoundVolume * vol);

    }

    public void PlaySpawnFlyerSound()
    {
        PlayOneSound(spawnFlyerEnemySound, 1f);
    }

    public void PlayClickButtonSound()
    {
        PlayOneSound(clickButtonSe, 0.77f);

    }

}

