using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using System.Linq;              //LINQ
using UnityEngine.InputSystem;


/*───────────────────────────────────────────────────────────
 *  SkillManager
 *───────────────────────────────────────────────────────────
 *  プレイヤーのレベルアップ時に表示される「スキル選択 UI」全般を管理。
 *  ・新規スキル取得 or 既存スキル強化 の判定
 *  ・ランダム候補の生成（レアリティ抽選／数値算出）
 *  ・UI への内容反映とボタン操作
 *  ・ブースト／スワップ（交換）などの特殊操作
 *──────────────────────────────────────────────────────────*/



/// <summary>レアリティごとの副作用テーブル</summary>
[System.Serializable]
public class SideEffectRaritySetting
{
    public OptionRarity rarity;      // Normal / Rare … Legendary
    public float basePercent;        // 基本値 (±%)
    public int   randMin;             // 乱数最小
    public int   randMax;             // 乱数最大
}

/// <summary>このステータスを上げたときに起こり得る副作用 1 件</summary>
[System.Serializable]
public class SideEffectCandidate
{
    [Range(0,100)] public float probability = 50;      // ← 50% なら表の 50 / 50 を再現
    public SideEffectType sideType = SideEffectType.None;
    public List<SideEffectRaritySetting> rarityTable = new();
}

/// <summary>
/// ガチャ的な確率テーブル：レアリティとその出現確率をひと組で保持
/// </summary>
[System.Serializable]                    
public class RarityChance
{
    public OptionRarity rarity;                  // レアリティの種類
    [Range(0,100)] public float probability = 0; // そのレアリティが選ばれる確率（%）
}

/// <summary>レアリティ別に“何％アップ・乱数範囲”を設定</summary>
[System.Serializable]                     
public class StatusRaritySetting
{
    public OptionRarity rarity;    // レアリティの種類
    public float basePercent;      // 基本増加率（%）     
    public int randMin;             // ランダム加算値の最小値     
    public int randMax;             // ランダム加算値の最大値     
}

/// <summary>
/// 1 スキルが持つ複数ステータス（攻撃力 / 速度 …）ごとの上限とレアリティ別テーブル
/// </summary>
[System.Serializable]                    
public class StatusSettings
{
    public SkillStatusType statusType;                      // ステータスの種類（攻撃力・速度など）
    public int maxLevel = 3;                                // 種類別の最大レベル(e.g 攻撃力Max = lv3, 速度Max = lv2)  
    public List<StatusRaritySetting> rarityTable = new();   // レアリティ別パラメータ設定
    public List<SideEffectCandidate>  sideEffects = new(); //この強化に紐づく副作用リスト

}

/// <summary>スキル固有の、ステータス設定リスト</summary>
[System.Serializable]                    
public class SkillSettings
{
    public SkillIdType skillId;                         // スキルのID
    public List<StatusSettings> statusList = new();     //各ステータス設定リスト 
}

public class SkillManager : Singleton<SkillManager>
{
   
    [Header("=====================================")]
    [Header("ボタン関連")]
   
    public int reRollChance = 10;           // 再ロール可能回数
    public int exchangeChance = 20;         // 交換可能回数
    public int boostChance = 5;             // 残りブースト回数
    public float rerollWaitCnt = 1f;        // 再ロール待機時間

    [Header("レベルアップメニューの状態確認")]
    public bool isLevelUpWindowOpen = false;
    public bool isExchangeMode = false;     // 交換モードかどうか
    public bool isBoostMode = false;        // ブーストモードかどうか
    public bool waitingForPlayer = false;   // プレイヤー選択待ち中フラグ
    public bool isGetNewSkill = true;       // 新規スキル習得フェーズ中かどうか
    public bool isGetNewTrait = false;
    public bool isSelectingTrait = false;

    public bool isCoinOrHp = false;          // 選択肢がないフラグ

    [Header("=====================================")]
    [Header("保有スキルリスト")]
    public List<SkillCasterBase> activeSkillCasterCollections = new();              // 現在アクティブ（習得済み）のスキルキャスター一覧
    
    [Header("全スキル候補リスト")]
    public List<SkillCasterBase> AllSkillCasterCollections = new();                 // 全スキルキャスター一覧

    public List<SkillCasterBase> activeSkillCastersHolder = new();

  
    [Header("=====================================")]
    [Header("スキル編集")] 
    public List<SkillSettings> skillSettings = new();                           // スキル設定データ一覧

    [Header("=====================================")]
    [Header("レアリティ別確率編集テーブル")]
    public List<RarityChance>  rarityChances = new()                            // レアリティ別確率テーブル
    {
        new RarityChance{ rarity = OptionRarity.Normal   , probability = 55 },  // 通常=55%
        new RarityChance{ rarity = OptionRarity.Rare     , probability = 25 },  // レア=25%
        new RarityChance{ rarity = OptionRarity.Epic     , probability = 15 },  // エピック=15%
        new RarityChance{ rarity = OptionRarity.Legendary, probability =  5 },  // レジェンダリー=5%
    };

    public List<SideEffectCandidate> sideEffects = new();
    public readonly SideEffectType[]   sideTypeToApply  = new SideEffectType[maxOptions];
    public readonly float[]            sideValueToApply = new float[maxOptions];

    [Range(0,100)] public float passiveLevelUpChance = 25f;                     // パッシブ効果が選ばれる確率（%）
   
    [Header("=====================================")]
    [Header("レベルアップメニューのUI関係")]
    public CanvasGroup[]       optionMenus;               // レベルアップ選択肢のキャンバスグループ
    public Button[]            optionButtons;             // 選択ボタン
    public TextMeshProUGUI[]   optionNames;               // スキル名表示テキスト
    public TextMeshProUGUI[]   optionTypes;               // ステータスタイプ表示テキスト
    public TextMeshProUGUI[]   optionStatus;              // 増加量表示テキスト
    public TextMeshProUGUI[]   optionSiderEffect;          // 副作用表示テキスト
    public TextMeshProUGUI[]   optionRarityTexts;         // レアリティ表示テキスト
    public Image[]             optionImages;              // スキルアイコン表示イメージ
    public TextMeshProUGUI     levelUpTitle;            // レベルアップタイトル表示テキスト
    public TextMeshProUGUI     levelUpTitleShaderEffect; // with ui text effect

    public bool[] isOptionEvolving;          // 進化可能かどうかのフラグ

    public Vector2[] optionMenusStartPos;         // レベルアップ選択肢の初期位置

    public Vector2 optionMenuEnchantMovePos = new Vector2(-100, 0);


    [Header("Level Stars")]
    public Transform[] optionStarRoots;
    public GameObject[] optionStarFrameObjs;

    public Button characterEnchantButton;
    public Button[] skillEnchantButtons;
    public TraitData traitToApply;
    public int casterObjToReleaseEnchant = 0;

    public Button[] traitOptionButtons;
    public TextMeshProUGUI[] traitOptionNameText;
    public TextMeshProUGUI[] traitOptionDescriptionText; // Traitの説明文


    public GameObject[] traitSlotToHide;
    public TextMeshProUGUI[] traitSlots1;
    public TextMeshProUGUI[] traitSlots2;

    //=======TraitPool========//
    [Searchable][PreviewSprite]public List<TraitData> availableTraitPool;
    [Searchable][PreviewSprite]public List<TraitData> availableTraitPool2;
    [Searchable][PreviewSprite]public List<TraitData> availableTraitPool3;
    [Searchable][PreviewSprite]public List<TraitData> availableTraitPoolWeapon;
    
    public bool isCharacterTrait = false; //職業固有のTraitを取得するかどうか
    public int getTraitTimeCount = 0; //何回目のTrait取得か

    public TraitData[] traitOptionToGet = new TraitData[maxOptions]; // Currently generated 3 trait options for the card UI
    public List<TraitData> ownedTraits = new(); // Which traits the player already owns (optional convenience list)

    

    public const int maxOptions = 3;                        // 同時に表示する選択肢数
    public readonly bool[]          isPassiveOption   = new bool[maxOptions];                                 // 各スロットがパッシブ効果かどうか
    public readonly float[]            valueToLevelUp   = new float[maxOptions];                              // レベルアップ量
    public readonly int[]              casterIdxToLevel = new int  [maxOptions];                              // アクティブスキルリスト上のインデックス
    public readonly SkillStatusType[]  typeToLevelUp    = new SkillStatusType[maxOptions];                    // ステータスタイプ
    public readonly SkillIdType[]      skillIdToLevelUp = new SkillIdType   [maxOptions];                    // 新規習得時のスキルID
    public readonly OptionRarity[]     rarityToLevelUp  = new OptionRarity  [maxOptions];                    // レアリティ
    public readonly PassiveSkillStatusType[] passiveTypeToLevelUp = new PassiveSkillStatusType[maxOptions];  // パッシブ効果タイプ
    public bool[] giftToGet = new bool[maxOptions]; //Coin or Hpを取得するかどうかのフラグ

    public  bool[] isEvolutionOption = new bool[maxOptions];　　　　　　　　　　　　// 各スロットが進化可能かどうか
    public readonly int[] newSkillIdxToAdd = new int[maxOptions];               // 新規習得スキルのインデックス
    

    static readonly Dictionary<OptionRarity, Color> rarityColorMap = new()
    {
        { OptionRarity.Normal   , Color.white                       },
        { OptionRarity.Rare     , Color.green                       },
        { OptionRarity.Epic     , new Color(0.6f, 0.1f, 0.6f)      }, 
        { OptionRarity.Legendary, new Color(1f,   0.84f,  0f)       },
    };

    public RectTransform levelupFrame;
    UIFXController frameUiFx;
    public Button RerollButton;
    public Button BoostButton;
    public Button ExchangeButton;
    public GameObject rerollButtonFrame;
    public GameObject boostButtonFrame;
    public GameObject exchangeButtonFrame;
    public TextMeshProUGUI rerollButtonText;
    public TextMeshProUGUI boostButtonText;
    public TextMeshProUGUI exchangeButtonText;

    public Button RerollAButton;
    public Button RerollBButton;
    public Button RerollCButton;
    public TextMeshProUGUI rerollAButtonText;
    public TextMeshProUGUI rerollBButtonText;
    public TextMeshProUGUI rerollCButtonText;

    public Color redFrameColor = new Color(1f, 0.1f, 0f, 1f); // ブーストモードのフレーム色

    private FieldPick currentPick;           // １つ目に選ばれたフィールド
   
    public enum ExField { Rarity, Stat, Skill }
    private struct FieldPick      
    {
        public int slot;          
        public ExField field;    
        public bool valid => slot >= 0;
    }

    [Header("=====================================")]
    [Header("レベルアップ中に非表示にするUIメニュー")]
    public GameObject[] uiMenuToHide; // レベルアップ中に非表示にするUIメニュー
    public Sprite HpRecoverSprite;　　　// HP回復アイコン
    public Sprite CoinGetSprite;

    public PlayerState playerStatus;

    public SkillCoolDown[] skillCoolDownUis;

    BuffManager bm;

    public GameObject boostMessage;
    public GameObject exchangeMessge;

    private float optionTypeTextPosY = -9.16f +0.77f;

    public int enchantCasterNum = -1;

    public float enchantWaitCdCnt = 1f;

    //public bool[] isHpOption = new bool[maxOptions];

    public bool isSkillWindowFirstTimeOpen = false;


    //=======Sound========//
    public AudioClip getSkillSe;
    public AudioClip getTraitSe;
    public AudioClip evolutionSe;
    public AudioClip pickTraitSe;
    public AudioClip pickNewSkillSe;

    public Texture normalColorTex;
    public Texture rareColorTex;
    public Texture epicColorTex;
    public Texture LegendColorTex;
    public Texture EnchantColorTex;
    public Texture EvolColorTex;
    public Texture NewSkillColorTex;
    
    public Color GetNewSkillColor;
    public Color GetNewTraitColor;
    public Color NormalColor;
    public Color RareColor;
    public Color EpicColor;
    public Color LegendColor;
    public Color NegativeValueColor;

    [Header("Rarity Visuals")]
    public TMP_ColorGradient  normalStyle;
    public TMP_ColorGradient  rareStyle;
    public TMP_ColorGradient epicStyle;
    public TMP_ColorGradient legendaryStyle;
    public TMP_ColorGradient newSkillStyle;
    public TMP_ColorGradient enchantStyle;
    public TMP_ColorGradient evolStyle;
    public TMP_ColorGradient debuffNegativeStyle;

    public TMP_ColorGradient titleLevelUpStyle;
    public TMP_ColorGradient titleGetEnchantStyle;

    Dictionary<OptionRarity, TMP_ColorGradient> _rarityGradientMap;

    public float luck = 0f;

    public AudioClip getLegendSound;


    bool Valid<T>(List<T> list, int idx) => idx >= 0 && idx < list.Count;

    public List<TraitData> traitAlreadySelected = new();

    public bool isNotLvUp = false;
    public bool isNotLvUpGetSkillTrait = false;

    public GameObject exchangeSignObject;

    public LevelUpCard[] levelUpCards;
    public int currentSelectedRow = 0;
    public  int _currentSelectedSlot = 0;
    public bool _canNavigate = true;
    public Image controllerPointingIcon;
    public Image controllerPointingIconTrait;

    public GameObject renderEffectCamera;
    public GameObject renderEffectObj;
    public GameObject renderRawImage;

    public GameObject renderFeatherEffectObj;
    public float featherSpawnPosY = 3.5f;

    public GameObject[] noBulletEffectObjList;

    //public GameObject modeCancelIcon;

    public float controllerMoveCdCnt = 0.1f;

    public RectTransform gameSceneButtonGroupRect;
    
    public RectTransform ButtonGroupRect;
    public RectTransform keyIconRect;

    public bool isAllSkillSlotFull = false;

    // Tracks all trait types that have appeared in the current level-up session
    public  HashSet<TraitType> _traitsSeenThisSession = new HashSet<TraitType>();

    void OnEnable() 
    { 
        EventManager.StartListening("PlayerLevelUp", DoLevelUp); // レベルアップイベント登録
    }
    void OnDisable() 
    { 
        EventManager.StopListening("PlayerLevelUp", DoLevelUp); 
    }

    public void Awake()
    {
        bm = BuffManager.Instance;
        reRollChance += bm.gobalRerollChanceAdd;
        boostChance += bm.gobalBoostChanceAdd;
        exchangeChance += bm.gobalSwapChanceAdd;

        _rarityGradientMap = new Dictionary<OptionRarity, TMP_ColorGradient>
        {
            { OptionRarity.Normal,    normalStyle    },
            { OptionRarity.Rare,      rareStyle      },
            { OptionRarity.Epic,      epicStyle      },
            { OptionRarity.Legendary, legendaryStyle },
        };

        
        foreach (var slot in traitSlotToHide)
        {
            slot.SetActive(false);
        }

        levelUpTitle.text = "";

        //get optionMenusStartPos
        for (int i = 0; i < optionMenus.Length; i++)
        {

            optionMenusStartPos[i] = optionMenus[i].GetComponent<RectTransform>().anchoredPosition;
        }

        
        for(int i = 0; i < availableTraitPool.Count; i++)
        {
            availableTraitPool[i].isSelected = false;
        }

        for(int i = 0; i < availableTraitPool2.Count; i++)
        {
            availableTraitPool2[i].isSelected = false;
        }
        for (int i = 0; i < availableTraitPool3.Count; i++)
        {
            availableTraitPool3[i].isSelected = false;
        }
        for (int i = 0; i < availableTraitPoolWeapon.Count; i++)
        {
            availableTraitPoolWeapon[i].isSelected = false;
        }


        rerollButtonText = RerollButton.GetComponentInChildren<TextMeshProUGUI>();
        boostButtonText = BoostButton.GetComponentInChildren<TextMeshProUGUI>();
        exchangeButtonText = ExchangeButton.GetComponentInChildren<TextMeshProUGUI>();

        rerollAButtonText = RerollAButton.GetComponentInChildren<TextMeshProUGUI>();
        rerollBButtonText = RerollBButton.GetComponentInChildren<TextMeshProUGUI>();
        rerollCButtonText = RerollCButton.GetComponentInChildren<TextMeshProUGUI>();


    }

    public void CheckSkillSlotAvailbility()
    {
        isAllSkillSlotFull = true; // Assume all are full initially
    
        for (int i = 0; i < activeSkillCasterCollections.Count; i++)
        {
            if (activeSkillCasterCollections[i].traitDataHoldingList.Count < 3)
            {
                isAllSkillSlotFull = false; // Found one that isn't full
                return; // No need to check further
            }
        }

    }

    void ToggleBoostMode()
    {
        if (!waitingForPlayer) return;
        if (boostChance <= 0) return;
        if (isExchangeMode) ExitExchangeMode();

        Color frameColorEnhance = new Color(1f, 0.1f, 0f, 1f); 
        frameUiFx.SetOverlayEnable(true,redFrameColor);
        frameUiFx.SetRoundWaveEnable(true);
       
        isBoostMode = !isBoostMode;
        CursorManager.Instance.SetCursorBoost();

        if (!isBoostMode)
        {
            frameUiFx.SetOverlayEnable(false,Color.black);
            boostMessage.SetActive(false);
            exchangeMessge.SetActive(false);
            //modeCancelIcon.SetActive(false);
            //frameUiFx.SetRoundWaveEnable(false);

            ExchangeButton.interactable = true;
            RerollButton.interactable = true;
        }
        else if (isBoostMode)
        {
            boostMessage.SetActive(true); 
            //modeCancelIcon.SetActive(true);

            ExchangeButton.interactable = false;
            RerollButton.interactable = false;
        }

        SoundEffect.Instance.Play(SoundList.ButtonClickSe);

    }

    void ToggleExchangeMode()
    {
        if (!waitingForPlayer) return;
        if (exchangeChance <= 0 || isGetNewSkill) return;
        if (isBoostMode) ToggleBoostMode(); 
        if (isExchangeMode) 
        { 
            ExitExchangeMode(); 
            SoundEffect.Instance.Play(SoundList.ButtonCancelSe);
            return; 
        }

        EventManager.EmitEvent("ShowSlotFrame");

        RerollButton.interactable = false;
        BoostButton.interactable = false;

        isExchangeMode  = true;
        currentPick     = new FieldPick { slot = -1 };  
        CursorManager.Instance.SetCursorExchange();

        Color frameColor = new Color(0.45f, 0.32f, 0.76f, 1f); 
        frameUiFx.SetOverlayEnable(true,frameColor);
        frameUiFx.SetRoundWaveEnable(true);

        exchangeMessge.SetActive(true); 
        boostMessage.SetActive(false); 
        //modeCancelIcon.SetActive(false);

        exchangeSignObject.SetActive(true);

        SoundEffect.Instance.Play(SoundList.ButtonClickSe);        
    }

    void ExitExchangeMode()
    {

        EventManager.EmitEvent("HideSlotFrame");

        isCurrentlyPickingFieldOption = false; 
        exchangeSignObject.SetActive(true);
        isExchangeMode = false;
        currentPick = new FieldPick { slot = -1 };
        ClearAllHighlights();
        CursorManager.Instance.SetCursorNormal();         

        frameUiFx.SetOverlayEnable(false,Color.blue);
        //frameUiFx.SetRoundWaveEnable(false);

        exchangeMessge.SetActive(false);

        BoostButton.interactable = true;
        RerollButton.interactable = true;

        

    }

    public void DoLevelUp()
    {

        if (!playerStatus.IsAliveFlg) return;
        if (GameManager.Instance.stateMachine.State == GameState.GameClear) return;

        ButtonGroupRect.anchoredPosition = new Vector2(0, -177);
        keyIconRect.anchoredPosition = new Vector2(770, 574);

        //Dotween ButtonGroupRect to 0,0 in 0.21s
        ButtonGroupRect.DOAnchorPos(new Vector2(0, 0), 0.42f).SetEase(Ease.OutCubic).SetUpdate(true);
        keyIconRect.DOAnchorPos(new Vector2(770, 450), 0.42f).SetEase(Ease.OutCubic).SetUpdate(true);

        isLevelUpWindowOpen = true;

        if (CameraViewManager.Instance.currentMode == CameraViewManager.CameraMode.CloseView)
        {
            CameraViewManager.Instance.ShowAndUnlockCursor();
        }

        GameManager.Instance.isBattling = false;

        levelupFrame.gameObject.SetActive(true);
        frameUiFx.SetRoundWaveEnable(true);
        renderEffectCamera.SetActive(true);
        renderRawImage.SetActive(true);

        waitingForPlayer = true;                        // プレイヤー選択待ち開始
        CheckNewSkillCondition();                       // 新規スキル取得条件チェック    
        if (isGetNewSkill) DoGetNewSkill();             // 新規スキル取得フェーズへ
        else if (isGetNewTrait)
        {
            DoGetNewTrait(isCharacterTrait);
        }
        else DoSkillLevelUp();             // 現スキルレベルアップフェーズへ
    }

    public void CheckNewSkillCondition()
    {

        if (isNotLvUpGetSkillTrait)
        {
            isNotLvUpGetSkillTrait = false;
            SoundEffect.Instance.PlayOneSound(getSkillSe, 0.28f);
            isGetNewSkill = false;
            isGetNewTrait = true;
            isCharacterTrait = false;

            return;
        }

         if (isNotLvUp)
        {
            SoundEffect.Instance.PlayOneSound(getTraitSe, 0.56f);
            isGetNewSkill = false;
            isGetNewTrait = true;
            isCharacterTrait = true;
            isNotLvUp = false;
            getTraitTimeCount = 1;
            return;
        }


        if (QuickRunModeManager.Instance.isQuickRunMode)
        {
            if (playerStatus.NowLv == 4 || playerStatus.NowLv == 9 || playerStatus.NowLv == 15)
        {
            SoundEffect.Instance.PlayOneSound(getSkillSe, 0.28f);
            isGetNewSkill = true;                    //プレイヤーがレベル4または7なら新規スキル取得可能
        }
        else if (playerStatus.NowLv == 7 || playerStatus.NowLv == 12 || playerStatus.NowLv == 21)
        {
            SoundEffect.Instance.PlayOneSound(getTraitSe, 0.56f);
            isGetNewSkill = false;
            isGetNewTrait = true;
            isCharacterTrait = true;

                if (playerStatus.NowLv == 7) getTraitTimeCount = 1;
                else if (playerStatus.NowLv == 12) getTraitTimeCount = 2;
                else if (playerStatus.NowLv == 21) getTraitTimeCount = 3;
                else getTraitTimeCount = 1;
                //getTraitTimeCount++;
            }
        else // それ以外はレベルアップのみ
        {
            isGetNewSkill = false;
            //isGetNewTrait = false;
            isCharacterTrait = false;
            SoundEffect.Instance.Play(SoundList.LevelUp);   // レベルアップ効果音再生
        }



            return;
        }

        


        if (playerStatus.NowLv == 4 || playerStatus.NowLv == 11 || playerStatus.NowLv == 17)
        {
            SoundEffect.Instance.PlayOneSound(getSkillSe, 0.28f);
            isGetNewSkill = true;                    //プレイヤーがレベル4または7なら新規スキル取得可能
        }
        else if (playerStatus.NowLv == 7 || playerStatus.NowLv == 14 || playerStatus.NowLv == 21)
        {
            SoundEffect.Instance.PlayOneSound(getTraitSe, 0.56f);
            isGetNewSkill = false;
            isGetNewTrait = true;
            isCharacterTrait = true;

            if (playerStatus.NowLv == 7) getTraitTimeCount = 1;
            else if (playerStatus.NowLv == 14) getTraitTimeCount = 2;
            else if (playerStatus.NowLv == 21) getTraitTimeCount = 3;
            else getTraitTimeCount = 1;
            //getTraitTimeCount++;
        }
        else // それ以外はレベルアップのみ
        {
            isGetNewSkill = false;
            //isGetNewTrait = false;
            isCharacterTrait = false;
            SoundEffect.Instance.Play(SoundList.LevelUp);   // レベルアップ効果音再生
        }

    }

    public void DoSkillLevelUp()
    {
        levelUpTitle.colorGradientPreset = titleLevelUpStyle;
        GenerateOptions();                              // レベルアップ候補生成
        ShowLevelUpWindow();                            // レベルアップ画面表示
    }

    void DoGetNewTrait(bool isCharacterTrait)
    {
        RerollButton.gameObject.SetActive(false);
        BoostButton.gameObject.SetActive(false);
        ExchangeButton.gameObject.SetActive(false);

        RerollAButton.gameObject.SetActive(true);
        RerollBButton.gameObject.SetActive(true);
        RerollCButton.gameObject.SetActive(true);


        levelUpTitle.colorGradientPreset = titleGetEnchantStyle;
        isSelectingTrait = true;
        GenerateNewTraitOption(isCharacterTrait);
        ShowLevelUpWindow();
    }

    public void DoGetNewSkill()
    {
        levelUpTitle.colorGradientPreset = newSkillStyle;

        foreach (var slot in traitSlotToHide)
        {
            slot.SetActive(true);
        }

        waitingForPlayer = true;         // プレイヤー選択待ち開始
        GenerateNewSkillOptions();       // 新規スキル候補生成
        ShowLevelUpWindow();             // レベルアップ画面表示
    }

    void GenerateNewTraitOption(bool isCharacterTrait)
    {
       _traitsSeenThisSession.Clear();

        List<TraitData> sourcePool;
        sourcePool = availableTraitPool;

        if (isCharacterTrait)
        {
            if(getTraitTimeCount == 1) sourcePool = availableTraitPool2;
            else if (getTraitTimeCount == 2) sourcePool = availableTraitPool;
            else if (getTraitTimeCount == 3) sourcePool = availableTraitPool3;

        }
        else
        {
            // Safety check to ensure the index is valid
            if (enchantCasterNum < 0 || enchantCasterNum >= activeSkillCasterCollections.Count)
            {
                Debug.LogError("GenerateNewTraitOption called for a skill trait with an invalid enchantCasterNum.");
                isGetNewTrait = false;
                DoSkillLevelUp();
                return;
            }

            //sourcePool = activeSkillCasterCollections[casterIdxToLevel[enchantCasterNum]].availableTraitList; //each weapon has its own trait pool
            sourcePool = availableTraitPoolWeapon;
        }

        // 3. Create a list of candidates from the source pool that have not been selected yet.
        var candidates = (sourcePool ?? new List<TraitData>()).Where(t => t != null && !t.isSelected && AchievementManager.Instance.IsTraitUnlocked(t.traitType)).ToList();

        if(DebugKeyManager.Instance.isInSchoolDemo) candidates =  (sourcePool ?? new List<TraitData>()).Where(t => t != null &&t.isDemoRecommendedTrait && !t.isSelected && AchievementManager.Instance.IsTraitUnlocked(t.traitType)).ToList();

        // 4. Handle the case where there are no available traits.
        //if (candidates.Count == 0)
        //{
        //    isGetNewTrait = false;
        //    DoSkillLevelUp(); // Fallback to a normal level up.
        //    return;
        //}

        // --- 5. The Shuffle and Pick Logic (Fisher-Yates) ---
        int n = candidates.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            // Swap the elements
            (candidates[k], candidates[n]) = (candidates[n], candidates[k]);
        }

        // 6. Now that the list is shuffled, simply take the first few items.
        int count = Mathf.Min(maxOptions, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            traitOptionToGet[i] = candidates[i];
            if (candidates[i] != null) _traitsSeenThisSession.Add(candidates[i].traitType);
        }
        
        waitingForPlayer = true;

        ControllerVibrationManager.Instance.Vibrate();

    }

    void GenerateNewSkillOptions()
    {
        for (int i = 0; i < maxOptions; ++i)            // 初期化
        {
            newSkillIdxToAdd[i] = -1;                   // インデックスなし
            skillIdToLevelUp[i] = SkillIdType.None;   　// スキルIDなし
        }
        
        var candidates = activeSkillCasterCollections.Where(c => !c.isActivated && !c.isPlayerBaseSkill && c.isUnlock).ToList(); // 未習得スキルを候補に

        if (candidates.Count == 0)
        {
            waitingForPlayer = false;      // 候補なしなら待機解除
            isGetNewSkill    = false;      // 新規習得フェーズ終了
            return;
        }

        int optionCount = Mathf.Min(maxOptions, candidates.Count); // 表示数を決定

        for (int i = 0; i < optionCount; ++i) // ランダムに選択
        {
            int pick  = Random.Range(0, candidates.Count);
            var skill = candidates[pick];
            newSkillIdxToAdd[i] = AllSkillCasterCollections.IndexOf(skill);   // 全体リストでの位置
            skillIdToLevelUp[i] = skill.casterIdType;                         // スキルID設定
            candidates.RemoveAt(pick);                                        // 重複防止で削除
        }
    }



    bool SupportsStat(int slot, SkillStatusType type)
    {
        var caster = activeSkillCasterCollections[casterIdxToLevel[slot]];
        var rule = skillSettings.Find(s => s.skillId == caster.casterIdType);
        return rule != null && rule.statusList.Exists(st => st.statusType == type);
        //return true;
    }

    public bool isCurrentlyPickingFieldOption = false;
    public int currentExchangeSlotIndex = 0;

    public void OnOptionFieldClicked(int slot, int fieldKind)   // fieldKind == (int)ExField  , What if there is no that many options? it will crash
    {
        if (!isExchangeMode || slot < 0 || slot >= maxOptions) return;
        var field = (ExField)fieldKind;
    
        isCurrentlyPickingFieldOption = false;

        if (!currentPick.valid)
        {
            isCurrentlyPickingFieldOption = true;
            
            currentPick = new FieldPick { slot = slot, field = field };
            HighlightField(slot, field, true);    
            SoundEffect.Instance.Play(SoundList.SwapButtonSe);
            currentExchangeSlotIndex = slot;
           int nextSlot = exSlot + 1;
            if (nextSlot >= maxOptions) nextSlot = 0;
            if (nextSlot == currentExchangeSlotIndex) 
            {
                nextSlot += 1;
                if (nextSlot >= maxOptions) nextSlot = 0;
            }
            SetSwapExchangeSlot(nextSlot, exKind);

            for (int i = 0; i < maxOptions; ++i)
            {
                if (!optionMenus[i].gameObject.activeSelf) continue; //skip any card slot that is not active.

                if (i == slot) continue;
                if (field != ExField.Stat || SupportsStat(i, typeToLevelUp[slot]))    
                {
                    HighlightField(i, field, false, true);  // green
                }
                else
                {
                    //isExchangeMode = false;
                    //CursorManager.Instance.SetCursorNormal();
                    //ClearAllHighlights();
                }

            }

          
            return;
        }
    
        // 2nd click rules
        if (slot == currentPick.slot)          
        {
            
            SoundEffect.Instance.Play(SoundList.ButtonCancelSe);
            HighlightField(slot, field, false);
            currentPick = new FieldPick { slot = -1 };
            isExchangeMode = false;
            exchangeMessge.SetActive(false); 
            CursorManager.Instance.SetCursorNormal();
            ClearAllHighlights();
            return;
        }
        if (field != currentPick.field)
        {
            //ClearAllHighlights();
            //isExchangeMode = false;
            //CursorManager.Instance.SetCursorNormal();
            SoundEffect.Instance.Play(SoundList.ButtonCancelSe);
            return;
        }                 

        if (field == ExField.Stat)
        {
            var srcType = typeToLevelUp[currentPick.slot];
            var dstType = typeToLevelUp[slot];

            if (!SupportsStat(slot, srcType) ||
                !SupportsStat(currentPick.slot, dstType))
            {
                SoundEffect.Instance.Play(SoundList.ButtonCancelSe);
                //HighlightField(currentPick.slot, field, false);
                //ClearAllHighlights();
                //currentPick = new FieldPick { slot = -1 };
                return;
            }
        }

        SwapFieldValues(currentPick.slot, slot, field);
        //ClearAllHighlights();
        SuccessClearAllHighlights();

        exchangeChance--;
        isExchangeMode = false; // 交換モード終了
        exchangeMessge.SetActive(false);

        if (exchangeChance <= 0)
        {
            isExchangeMode = false;
            ExchangeButton.image.color = Color.white;
        }

        HighlightField(currentPick.slot, field, false);
        currentPick = new FieldPick { slot = -1 };

        CursorManager.Instance.SetCursorNormal();
        ShowLevelUpWindow();                  
    }

    void SwapFieldValues(int a, int b, ExField field)
    {
        SoundEffect.Instance.Play(SoundList.SwapButtonSe);

        //if either is evolution option, do nothing and return
        if (isEvolutionOption[a] || isEvolutionOption[b])
        {
            Debug.Log("One of the selected options is an evolution option, cannot swap.");
            SoundEffect.Instance.Play(SoundList.ButtonCancelSe);
            return;
        }

        switch (field)
        {
            case ExField.Rarity:
                (rarityToLevelUp[a], rarityToLevelUp[b]) = (rarityToLevelUp[b], rarityToLevelUp[a]);

                valueToLevelUp[a] = ReRollValueForSlot(a);  
                valueToLevelUp[b] = ReRollValueForSlot(b);  
                break;
    
            case ExField.Stat:
                (typeToLevelUp[a],   typeToLevelUp[b])   = (typeToLevelUp[b],   typeToLevelUp[a]);
                (valueToLevelUp[a],  valueToLevelUp[b])  = (valueToLevelUp[b],  valueToLevelUp[a]);

                valueToLevelUp[a] = ReRollValueForSlot(a); 
                valueToLevelUp[b] = ReRollValueForSlot(b); 
                break;
    
            case ExField.Skill:
                (casterIdxToLevel[a], casterIdxToLevel[b]) =　(casterIdxToLevel[b], casterIdxToLevel[a]);  
                (skillIdToLevelUp[a],  skillIdToLevelUp[b]) =　(skillIdToLevelUp[b], skillIdToLevelUp[a]);

                //valueToLevelUp[a] = ReRollValueForSlot(a);  
                //valueToLevelUp[b] = ReRollValueForSlot(b);  
                break;
        }

        ReRollSideEffectForSlot(a);
        ReRollSideEffectForSlot(b);

    }

    void HighlightField(int slot, ExField field, bool on, bool candidate = false)
    {
        Color sel  = new Color(1f,1f,0.2f);     // yellow
        Color cand = new Color(0f,0.7f,1f);   　// blue
        Color c = candidate ? cand : (on ? sel : Color.white); // if candidate, use blue; else if on, use yellow; else white

        EventManager.EmitEvent("HideSlotFrame");
        

        switch (field)
        {
            case ExField.Rarity:
                //optionRarityTexts[slot].color = on || candidate ? c: rarityColorMap[rarityToLevelUp[slot]];
                if(candidate)optionRarityTexts[slot].GetComponent<LevelUpCursor>().ActivateCursor();
                break;
            case ExField.Stat:
                optionTypes [slot].color = c;
                //optionStatus[slot].color = c;
                if(candidate)optionStatus[slot].GetComponent<LevelUpCursor>().ActivateCursor();
                break;
            case ExField.Skill:
                //optionNames [slot].color = c;
                if(candidate)optionNames[slot].GetComponent<LevelUpCursor>().ActivateCursor();
                break;
        }

    }

   public void OnOptionButtonPressed(int slot)
    {
        if (isExchangeMode) return;
        //print the slot 
        //Debug.Log($"Option button pressed: Slot {slot}");

        SoundEffect.Instance.Play(SoundList.UiClick);
        EventManager.EmitEvent("OpenLevelUpWindow");

        

        if (isBoostMode && !isGetNewSkill)          // ブーストはレベルアップ時のみ
        {
            var current = rarityToLevelUp[slot];
            var next    = current.Next();

            if (next == null)
            {
                SoundEffect.Instance.Play(SoundList.ButtonCancelSe);
                return; // >=Lengend = これ以上アップできません
            }

            //if that slot is a evolution option, return and do nothing
            if (isEvolutionOption[slot])
            {
                Debug.Log("This option is an evolution option, cannot boost.");
                SoundEffect.Instance.Play(SoundList.ButtonCancelSe);
                return;

            }


            rarityToLevelUp[slot] = next.Value;

            if (isPassiveOption[slot]) valueToLevelUp[slot] = Random.Range(1f, 5f); // Passive：単純に値を再抽選
            else
            {
                var castertmp   = activeSkillCasterCollections[casterIdxToLevel[slot]];
                var ruleRoot = skillSettings.Find(s => s.skillId == castertmp.casterIdType);
                
                var stRule   = ruleRoot.statusList.Find(st => st.statusType == typeToLevelUp[slot]);
                var rarRule  = stRule.rarityTable.Find(r => r.rarity == next);
              
                float add = Random.Range(rarRule.randMin, rarRule.randMax + 1);
                valueToLevelUp[slot] = rarRule.basePercent + add;

                ReRollSideEffectForSlot(slot);

            }

            boostChance--;
            isBoostMode = false;
            boostMessage.SetActive(false); 
            frameUiFx.SetOverlayEnable(false,Color.black);
            //frameUiFx.SetRoundWaveEnable(false);
            SoundEffect.Instance.Play(SoundList.BoostSe);

            ShowLevelUpWindow();        //画面を即時リフレッシュ
            CursorManager.Instance.SetCursorNormal();
            return;                     //ここで通常処理を止める
        }

        if(isExchangeMode || isBoostMode) return; // 交換・ブースト中は無効

        waitingForPlayer = false;


        if (isEvolutionOption[slot]) // there is issue and possibility that isEvolutionOption is not set back to false after evolution
        {
            var casterEvo = activeSkillCasterCollections[casterIdxToLevel[slot]];
            casterEvo.isFinalSkill = true;      
            casterEvo.CalFinalStatus();        
            casterEvo.casterLevel++;

            //==進化End==//
            //ReactivateUiUponFinish();
            //EffectManager.Instance.CreatePlayerLevelUpEffect();
            //waitingForPlayer = false;

            //clear and reset all evo option

            enchantCasterNum = slot;

            for (int i = 0; i < maxOptions; i++)
            {
                isEvolutionOption[i] = false;
            }

            SoundEffect.Instance.PlayOneSound(getTraitSe, 0.56f);
            isGetNewTrait = true;
            DoGetNewTrait(isCharacterTrait);

            return;                          //レベルアップ終了
        }


        if (isGetNewTrait)  // Trait選択肢を選んだ場合, Traitを適用
        {
            EventManager.EmitEvent("SelectedTraitSlot");

            RerollAButton.gameObject.SetActive(false);
            RerollBButton.gameObject.SetActive(false);
            RerollCButton.gameObject.SetActive(false);

            traitToApply = traitOptionToGet[slot];

            
            //hide other 2 optionMenus
            for (int i = 0; i < maxOptions; i++)
            {
                if (i != slot)
                {
                    optionMenus[i].gameObject.SetActive(false);
                }
            }


            //if trait affect universally , end here, else show all skills slot to be assigned that trait to
            if (traitToApply.isUniversalApplied)
            {
                
                //ApplyTrait(traitToApply);               // apply effect + mark selected

                isGetNewTrait = false;
                EnchantUniversalTrait();
                HideLevelUpWindow();
                ReactivateUiUponFinish();
                EffectManager.Instance.CreatePlayerLevelUpEffect();
                return;
            }
            else
            {
                //move optionMenu[slot] to enchant position using dotween in 0.4f
                optionMenus[slot].GetComponent<RectTransform>().DOAnchorPos(optionMenuEnchantMovePos, 0.49f).SetEase(Ease.InOutSine).SetUpdate(true);
                ShowSkillTraitEnchantOption();
                return;
            }
            
            
            
            
        }

        if (isCoinOrHp && !isGetNewSkill)
        {
            if (giftToGet[slot])
            {
                EventManager.EmitEventData(GameEvent.ChangePlayerHp, 10f);
                //Debug.Log($"Player HP increased by 20. Current HP: {playerStatus.NowHp}"); // デバッグ用ログ
            }
            else
            {
                ResultMenuController.Instance.turnCoinGet += 50;
                //CurrencyManager.Instance.Add(30);
                //Debug.Log($"Player Coin increased by 30. Current Coin: {CurrencyManager.Instance.Coins}"); // デバッグ用ログ
            }
           
            //Debug.Log($"Player HP increased by 20. Current HP: {playerStatus.NowHp}");
            HideLevelUpWindow();
            ReactivateUiUponFinish(); // レベルアップ後にUIを再表示
            EffectManager.Instance.CreatePlayerLevelUpEffect(); // レベルアップエフェクト生成
            isCoinOrHp = false;

            return;
        }
 
        if (isGetNewSkill)
        {
            // 新規習得処理
            var pickedId   = skillIdToLevelUp[slot];
            var toActivate = activeSkillCasterCollections.FirstOrDefault(s => s.casterIdType == pickedId);

            if (toActivate != null)　toActivate.ActivateCaster(); // スキル有効
            else　Debug.LogError($"Couldn’t find pre-loaded skill {pickedId} to activate!");


            for(int i = 0; i < skillCoolDownUis.Length; i++)
            {
                if (!skillCoolDownUis[i].isUsed)
                {
                    skillCoolDownUis[i].isUsed = true; // 初回使用時に true にする
                    skillCoolDownUis[i].SkillSlotID = activeSkillCasterCollections.IndexOf(toActivate);                
                    activeSkillCastersHolder.Add(toActivate); //add this new skill to activeSkillCastersHolder
                    break;
                }
            }

            isGetNewSkill = false;
            HideLevelUpWindow();
            ReactivateUiUponFinish();                               // レベルアップ後にUIを再表示
            EffectManager.Instance.CreatePlayerLevelUpEffect();     // レベルアップエフェクト生成        
            return;
        }

        //==通常レベルアップ処理==//
        var caster = activeSkillCasterCollections[casterIdxToLevel[slot]];
        caster.IncreaseStatus(typeToLevelUp[slot], valueToLevelUp[slot],true);　// ステータス上昇
        
        //副作用
        if (sideTypeToApply[slot] != SideEffectType.None)
        {
            caster.IncreaseStatus(ConvertSideToStatus(sideTypeToApply[slot]),-sideValueToApply[slot],false);
        }
        
        
        HideLevelUpWindow();
        ReactivateUiUponFinish();                                   // レベルアップ後にUIを再表示
        EffectManager.Instance.CreatePlayerLevelUpEffect();         // レベルアップエフェクト生成
        
    }


    //Helper to convert SideEffectType to SkillStatusType
    SkillStatusType ConvertSideToStatus(SideEffectType t)
    => t switch
    {
        SideEffectType.DamagePercent    => SkillStatusType.Damage,
        SideEffectType.CooldownPercent  => SkillStatusType.Cooldown,
        SideEffectType.SpeedPercent     => SkillStatusType.Speed,
        SideEffectType.DurationPercent => SkillStatusType.Duration,
        _                               => SkillStatusType.None,
    };

    void GenerateOptions()
    {
        if (!waitingForPlayer) return;
        bool anyOptionGenerated = false;
        isSelectingTrait = false;

    //List to store the unique (CasterIndex, StatusType) pairs already generated for this level-up(Use Tuple)
    var generatedOptions = new List<System.Tuple<int, SkillStatusType>>();

    for (int i = 0; i < maxOptions; ++i)
    {
        sideTypeToApply[i] = SideEffectType.None;  // Clear side effect
        sideValueToApply[i] = 0f;
        isEvolutionOption[i] = false;

        int retryCount = 0; // NEW: A retry counter for the inner loop to prevent the game from freezing
        bool uniqueOptionFound = false;

        //Generate an option for slot 'i' until it finds one that hasn't been used in a previous slot (0 to i-1).
        do
        {
            //Check if there are any upgradeable skills.
            var viableIdx = activeSkillCasterCollections
                .Select((c, idx) => new { caster = c, index = idx })
                .Where(x => x.caster.isActivated && !x.caster.IsFullyMaxed())
                .Select(x => x.index)
                .ToList();

            // If no skills can be upgraded, trigger your fallback logic for all slots and exit.
            if (viableIdx.Count == 0)
            {
                Debug.Log("All skills are maxed out. Switching to HP/Coin options.");
                waitingForPlayer = false; // This flag signals the ShowLevelUpWindow to use the fallback UI.
                for (int p = 0; p < maxOptions; ++p)
                {
                    giftToGet[p] = UnityEngine.Random.Range(0, 2) == 0; // 50% get coin, 50% HP
                }
                return; 
            }

            // --- Start of generating a CANDIDATE option --- use temporary variables until we confirm the option is unique.
            int candidateCasterIndex = viableIdx[UnityEngine.Random.Range(0, viableIdx.Count)];
            var candidateCaster = activeSkillCasterCollections[candidateCasterIndex];
            var skillRule = skillSettings.Find(s => s.skillId == candidateCaster.casterIdType);

            if (skillRule == null)
            {
                Debug.LogError($"[SkillManager] No SkillSettings for casterId {candidateCaster.casterIdType}");
                retryCount++;
                continue; // Skip this and retry.
            }

            // Check for evolution first,a special case.
            bool canEvolve = candidateCaster.casterLevel >= (candidateCaster.casterLevelMax - 1) && !candidateCaster.isFinalSkill;
            if (canEvolve)
            {
                // An evolution is defined by its caster and a 'None' status type.
                var evolutionTuple = System.Tuple.Create(candidateCasterIndex, SkillStatusType.None);
                if (!generatedOptions.Contains(evolutionTuple))
                {
                    // This is a unique evolution option. Commit it.
                    rarityToLevelUp[i] = OptionRarity.Legendary;
                    casterIdxToLevel[i] = candidateCasterIndex;
                    typeToLevelUp[i] = SkillStatusType.None;
                    valueToLevelUp[i] = 0;
                    isEvolutionOption[i] = true;

                    generatedOptions.Add(evolutionTuple); // Remember this choice.
                    uniqueOptionFound = true; // Mark as found to exit the do-while loop.
                    anyOptionGenerated = true;
                 }
                // If it was a duplicate, uniqueOptionFound remains false, and the loop will retry.
            }
            else // This is a regular status upgrade.
            {
                var availableStatus = skillRule.statusList
                    .Where(st => candidateCaster.CurrentStatusLevel(st.statusType) < st.maxLevel)
                    .ToList();

                // If this specific, randomly chosen skill has no more upgrades, we must retry.
                if (availableStatus.Count == 0)
                {
                    retryCount++;
                    continue; // The do-while loop will try again with a new random skill.
                }

                var chosenStatus = availableStatus[UnityEngine.Random.Range(0, availableStatus.Count)];
                
                // This is the unique identifier for our option.
                var candidateTuple = System.Tuple.Create(candidateCasterIndex, chosenStatus.statusType);

                // NEW: The core check. Is this combination already in our list?
                if (!generatedOptions.Contains(candidateTuple))
                {
                    // It's unique! Now we can calculate values and commit them.
                    OptionRarity rar = RollRarity();
                    var rule = chosenStatus.rarityTable.Find(r => r.rarity == rar);
                    float add = UnityEngine.Random.Range(rule.randMin, rule.randMax + 1);
                    float val = rule.basePercent + add;

                        if (rar == OptionRarity.Legendary)
                        {
                            Vector3 spawnPos = new Vector3(0,1.3f,5);
                                if (i == 0) spawnPos.x = -4;
                                else if (i == 2) spawnPos.x = 4;

                            DG.Tweening.DOVirtual.DelayedCall(0.28f, PlayGetLegendSound).SetUpdate(true).OnComplete(() => {
                                renderEffectCamera.SetActive(true);

                                ControllerVibrationManager.Instance.Vibrate();
                                Instantiate(renderEffectObj, spawnPos, Quaternion.identity);
                            });


                            //PlayGetLegendSound();
                        }

                        // Commit the values to the final arrays for slot 'i'.
                        casterIdxToLevel[i] = candidateCasterIndex;
                    typeToLevelUp[i] = chosenStatus.statusType;
                    valueToLevelUp[i] = val;
                    rarityToLevelUp[i] = rar;

                    var se = RollSideEffect(chosenStatus.sideEffects, rar);
                    sideTypeToApply[i] = se.type;
                    sideValueToApply[i] = se.value;

                    generatedOptions.Add(candidateTuple); // Add to our list to prevent future duplicates.
                    uniqueOptionFound = true; // We found a valid, unique option, so we can exit the inner loop.
                    anyOptionGenerated = true; // <-- 2. Mark success when an option is found

                  }
                // If it was a duplicate, uniqueOptionFound remains false, and the loop will retry.
            }

            retryCount++;

        } while (!uniqueOptionFound && retryCount < 200); //Loop condition.

       
        if (!uniqueOptionFound)  //Safety check. If we exited the loop due to retries, we couldn't find a unique option.
        {
            Debug.LogWarning($"Could not find a unique option for slot {i} after {retryCount} tries. There might be no more unique upgrades available. This slot may be left empty or show a repeat from a previous level-up if not handled in ShowLevelUpWindow.");
            // To make this fully robust, you would add a check in ShowLevelUpWindow
            // to hide the UI for this slot if casterIdxToLevel[i] is invalid (e.g., -1).
        }

        // If the loop finished but we failed to generate even one option,
        // it means all remaining skills are "zombies" with no upgrades left.
        if (!anyOptionGenerated)
        {
            Debug.Log("No valid upgrade options could be generated. Switching to HP/Coin fallback.");
            waitingForPlayer = false; // Manually trigger the fallback state
            for (int p = 0; p < maxOptions; ++p)
            {
                giftToGet[p] = Random.Range(0, 2) == 0;
            }
        }

    }

    }

    void ReRollSideEffectForSlot(int slot)
{
    // Passive options & evolution cards never have a side effect
    if (isPassiveOption[slot] || isEvolutionOption[slot])
    {
        sideTypeToApply [slot] = SideEffectType.None;
        sideValueToApply[slot] = 0;
        return;
    }

    var caster   = activeSkillCasterCollections[casterIdxToLevel[slot]];
    var ruleRoot = skillSettings.Find(s => s.skillId == caster.casterIdType);
    var stRule   = ruleRoot?.statusList.Find(st => st.statusType == typeToLevelUp[slot]);
    if (stRule == null)  // safety guard
    {
        sideTypeToApply [slot] = SideEffectType.None;
        sideValueToApply[slot] = 0;
        return;
    }

    // Re-use the same helper you already wrote
    var se = RollSideEffect(stRule.sideEffects, rarityToLevelUp[slot]);
    sideTypeToApply [slot] = se.type;
    sideValueToApply[slot] = se.value;         
}


    (SideEffectType type, float value) RollSideEffect(List<SideEffectCandidate> candidates, OptionRarity mainRar)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return (SideEffectType.None, 0);
        }
    
        // This variable will hold the probability from all previously FAILED rolls.
        float accumulatedProbability = 0f;
    
        // We now iterate through the candidates sequentially. The order matters.
        foreach (var candidate in candidates)
        {
            // The chance for this candidate to be picked is its own probability
            // PLUS the probability of all the ones that came before it and failed.
            float effectiveProbability = candidate.probability + accumulatedProbability;
    
            // We roll the dice once for this candidate.
            if (Random.Range(0f, 100f) < effectiveProbability)
            {
                // SUCCESS! We've picked a side effect.
                // Now we calculate its value just like before and return immediately.
    
                // 3) 強化側と同じ手順で % を算出 (Calculate the value percentage, same as before)
                var rarRule = candidate.rarityTable
                              .Find(r => r.rarity == mainRar)     // “同じレア”優先 (Prioritize same rarity)
                           ?? candidate.rarityTable.First();      // なければ 1 行目 (If not found, use the first one)
                float add = Random.Range(rarRule.randMin, rarRule.randMax + 1);
                float val = rarRule.basePercent + add;
    
                return (candidate.sideType, val);
            }
            else
            {
                // FAILURE. This candidate was not picked.
                // Add its probability to the accumulator for the *next* candidate to use.
                accumulatedProbability += candidate.probability;
            }
        }
    
        // If the loop finishes without returning, it means every candidate failed their check.
        // No side effect occurs.
        return (SideEffectType.None, 0);
    }

    public void RerollOneOption(int slot)
    {
        if (reRollChance <= 0) return;
        rerollWaitCnt = 0.4f;
        reRollChance--;
        SoundEffect.Instance.Play(SoundList.LevelUp);

        GenerateSingleTraitOption(slot);
        ShowLevelUpWindow(false,slot); 
    }

    

    void GenerateSingleTraitOption(int targetSlot)
    {
        List<TraitData> sourcePool;
        if (isCharacterTrait)
        {
             if(getTraitTimeCount == 1) sourcePool = availableTraitPool2;
             else if (getTraitTimeCount == 2) sourcePool = availableTraitPool;
             else sourcePool = availableTraitPool3;
        }
        else
        {
            sourcePool = availableTraitPoolWeapon; //Mark the CURRENT trait (the one we are rerolling away) as "Seen"  so it doesn't come back immediately.
        }

        if (sourcePool == null) return;

        if (traitOptionToGet[targetSlot] != null)
        {
            _traitsSeenThisSession.Add(traitOptionToGet[targetSlot].traitType);
        }
        // 3. Identify traits currently shown in OTHER slots to exclude strictly
        // (We never want duplicates on the screen at the same time)
        HashSet<TraitData> currentVisibleTraits = new HashSet<TraitData>();
        for (int i = 0; i < maxOptions; i++)
        {
            if (i == targetSlot) continue; // Skip the slot we are changing
            if (traitOptionToGet[i] != null) currentVisibleTraits.Add(traitOptionToGet[i]);
        }

        // 2. Identify traits shown in OTHER slots to exclude
        HashSet<TraitData> excludedTraits = new HashSet<TraitData>();
        for (int i = 0; i < maxOptions; i++)
        {
            if (i == targetSlot) continue;
            if (traitOptionToGet[i] != null) excludedTraits.Add(traitOptionToGet[i]);
        }

        // 3. Filter candidates
        var candidates = sourcePool.Where(t => t != null && !t.isSelected && !excludedTraits.Contains(t) && 
        !currentVisibleTraits.Contains(t) && !_traitsSeenThisSession.Contains(t.traitType)&& AchievementManager.Instance.IsTraitUnlocked(t.traitType)).ToList();

         if(DebugKeyManager.Instance.isInSchoolDemo) candidates = sourcePool.Where(t => t != null &&t.isDemoRecommendedTrait && !t.isSelected && !excludedTraits.Contains(t) && 
        !currentVisibleTraits.Contains(t) && !_traitsSeenThisSession.Contains(t.traitType)&& AchievementManager.Instance.IsTraitUnlocked(t.traitType)).ToList();

        // If we have rerolled so many times that we ran out of "New" traits, 
        // we should relax the rule to avoid an empty slot or infinite loop.
        // We strictly keep the rule "Don't show what is currently on screen", but allow old history.
        if (candidates.Count == 0)
        {
            Debug.LogWarning("Ran out of unique fresh traits. Recycling previously seen traits.");
            candidates = sourcePool.Where(t => 
                t != null && 
                !t.isSelected && 
                !currentVisibleTraits.Contains(t) && 
                AchievementManager.Instance.IsTraitUnlocked(t.traitType)
            ).ToList();
        }

        // 4. Pick one
        if (candidates.Count > 0)
        {
            traitOptionToGet[targetSlot] = candidates[Random.Range(0, candidates.Count)];
        }
        else
        {
            // No candidates left. You might want to leave the old one or show nothing.
            // For now, let's keep the old one if no new unique one exists, 
            // or assign null if you want it blank.
            Debug.LogWarning("No unique trait candidates left for reroll.");
        }
    }

    public void RerollOption()
    {
        if (!waitingForPlayer) return;
        if (reRollChance <= 0) return;
        rerollWaitCnt = 0.4f;
        reRollChance--;
        SoundEffect.Instance.Play(SoundList.LevelUp);

        RerollButton.interactable = true;
        BoostButton.interactable = true;
        ExchangeButton.interactable = true;

        if (isGetNewSkill)   GenerateNewSkillOptions();
        else if (isGetNewTrait) GenerateNewTraitOption(isCharacterTrait);
        else                GenerateOptions();
        ShowLevelUpWindow();
    }

    OptionRarity RollRarity()                             
    {
        //float r = UnityEngine.Random.Range(0f, 100f);
        //float acc = 0;
        //foreach(var entry in rarityChances)
        //{
        //    acc += entry.probability;                // 累積確率
        //    if (r <= acc) return entry.rarity;       // 条件を満たしたレアリティを返す
        //}
        //return OptionRarity.Normal;

         var dynamicChances = new List<RarityChance>();
        foreach (var chance in rarityChances)
        {
            dynamicChances.Add(new RarityChance { rarity = chance.rarity, probability = chance.probability });
        }

        if (luck > 0)
        {
            //float finalLuck = (luck + BuffManager.Instance.gobalLuckAdd) / 2;
            float finalLuck = luck + BuffManager.Instance.gobalLuckAdd;

            var normalChance = dynamicChances.FirstOrDefault(c => c.rarity == OptionRarity.Normal);
            if (normalChance != null)
            {
                float amountToShift = Mathf.Min(finalLuck, normalChance.probability);
                normalChance.probability -= amountToShift;
                
                var higherRarities = dynamicChances.Where(c => c.rarity == OptionRarity.Rare || c.rarity == OptionRarity.Epic || c.rarity == OptionRarity.Legendary).ToList(); //Distribute the shifted amount proportionally to higher rarities ---
                float totalHigherRarityWeight = higherRarities.Sum(c => c.probability);

                if (totalHigherRarityWeight > 0)
                {
                    foreach (var highRarity in higherRarities)
                    {                        
                        float proportionalBonus = (highRarity.probability / totalHigherRarityWeight) * amountToShift; // The bonus for each rarity is its share of the total weight.
                        highRarity.probability += proportionalBonus;
                    }
                }
            }
        }
        
        float r = UnityEngine.Random.Range(0f, 100f);
        float acc = 0;
        foreach (var entry in dynamicChances)
        {
            acc += entry.probability;
            if (r <= acc) return entry.rarity;
        }

        return OptionRarity.Normal;

    }

    public void PlayGetLegendSound()
    {
        SoundEffect.Instance.PlayOneSound(getLegendSound, 0.49f);
    }

    void ResetOptionMenuPos()
    {
         //reset optionMenus position to start pos
        for (int i = 0; i < optionMenus.Length; i++)
        {
            optionMenus[i].GetComponent<RectTransform>().anchoredPosition = optionMenusStartPos[i];           
            optionMenus[i].gameObject.SetActive(true); //set all optionMenus active

        }
    }

    void SetFontSizeSmall()
    {
        for (int i = 0; i < maxOptions; ++i)
        {
            optionTypes[i].fontSize = 14.9f; // reset font size
        }
    }

    void SetFontSizeNormal()
    {
        for (int i = 0; i < maxOptions; ++i)
        {
            optionTypes[i].fontSize = 21f; // reset font size
        }
    }

    void ShowLevelUpWindow(bool isAnimateAll = true, int animatedCardId = 0)
    {
        bool isController = Gamepad.current != null;
        if(!isController) CursorManager.Instance.SetCursorNormal();

        EventManager.EmitEventData("ShowControllerIcons", isController);
      
        controllerPointingIcon.gameObject.SetActive(true);
        controllerPointingIconTrait.gameObject.SetActive(false);

        if (Gamepad.current == null)
        {
            controllerPointingIcon.gameObject.SetActive(false);
            controllerPointingIconTrait.gameObject.SetActive(false);
        }

        controllerPointingIcon.color = new Color(1, 1, 1, 1);
        controllerPointingIconTrait.color = new Color(1, 1, 1, 1);

        SetSelectedSlot(0,0);
        SetSelectTraitSlot(0);

        currentSelectedRow = 0;
        _currentSelectedSlot = 0;
        _canNavigate = false; // Set to false so the first call to SetSelectedSlot works
         RectTransform rect = controllerPointingIcon.GetComponent<RectTransform>();
        if(_currentSelectedSlot == 0)rect.anchoredPosition = new Vector2(-590f, rect.anchoredPosition.y);      
        else if(_currentSelectedSlot == 1) rect.anchoredPosition = new Vector2(0f, rect.anchoredPosition.y);
        else if (_currentSelectedSlot == 2) rect.anchoredPosition = new Vector2(590f, rect.anchoredPosition.y);

        SetFontSizeNormal();

        EventManager.EmitEvent("UpdateLevelUpFrame");

        ResetOptionMenuPos();

        //RerollButton.gameObject.SetActive(true);
        //ExchangeButton.gameObject.SetActive(true);
        //BoostButton.gameObject.SetActive(true);
        levelUpTitle.text = L.UI("ui.SkillLevelUp");
        levelUpTitleShaderEffect.text = "";


        EventManager.EmitEvent("OpenLevelUpWindow"); 

        GameManager.Instance.PauseGame();             // ゲームを一時停止
        if(isAnimateAll)LevelUpMenuAnimator.Instance.Open();          // レベルアップUIを開く
        else            LevelUpMenuAnimator.Instance.OpenOneElement(animatedCardId);
        
        HideUiWhenLevelUP();

        for (int i = 0; i < maxOptions; ++i){ 
            SetOptionStars(i, 0, false);
            optionStarFrameObjs[i].SetActive(false); // 星フレームを非表示

             RectTransform rt = optionTypes[i].GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, optionTypeTextPosY);


            isOptionEvolving[i] = false;// 進化初期化
        }

        

        for (int i = 0; i < skillEnchantButtons.Length; i++)
        {
            skillEnchantButtons[i].gameObject.SetActive(false);
        }

        characterEnchantButton.gameObject.SetActive(false);


        if (!waitingForPlayer)
        {
            isCoinOrHp = true;
            for (int i = 0; i < maxOptions; ++i)
            {
                optionMenus[i].gameObject.SetActive(true);

                if (giftToGet[i] == true)
                {
                    optionNames[i].text = L.UI("ui.getHp"); //local
                    optionImages[i].sprite = HpRecoverSprite; // アイコン画像を非表示
                }
                else
                {
                    optionNames[i].text = L.UI("ui.getCoin"); //local
                    optionImages[i].sprite = CoinGetSprite; // アイコン画像を非表示
                }

                RectTransform rt = optionTypes[i].GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, optionTypeTextPosY);
                optionTypes[i].fontSize = 21f;
                optionTypes[i].text = "";
                optionStatus[i].text = "";
                optionRarityTexts[i].text = "";
                optionSiderEffect[i].text = ""; // 副作用なし
 
             }
         return;
        }

        isCoinOrHp = false;

        //===GetNewTrait===//
        if (isGetNewTrait)
        {



            //ExchangeButton.gameObject.SetActive(false);
            //BoostButton.gameObject.SetActive(false);
            //levelUpTitle.text = "スキルエンチャント";
            if(isCharacterTrait)levelUpTitle.text = L.TraitTable("trait.playerCanEnchant");
            else levelUpTitle.text = L.TraitTable("trait.skillCanEnchant");

            for (int p = 0; p < 4; p++)
            {
                skillEnchantButtons[p].interactable = true;

                //make sure activeSkillCastersHolder has i element before access
                if(p < activeSkillCastersHolder.Count)
                {
                    if (activeSkillCastersHolder[p].traitDataHoldingList.Count >= 3)
                    {
                        skillEnchantButtons[p].interactable = false;
                    }
                }
            }

            for (int i = 0; i < maxOptions; ++i)
            {
                
                var trait = traitOptionToGet[i];
                bool has = trait != null;

                optionMenus[i].gameObject.SetActive(has); //Safety?
                if (!has) continue;

                optionNames[i].text       = "";
                RectTransform rt = optionTypes[i].GetComponent<RectTransform>();
                //rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -7f);
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -4.9f);      
                optionStatus[i].text      = "";                     
                optionSiderEffect[i].text = "";   
                optionTypes[i].fontSize = 14.9f; //17
                //optionTypes[i].text       = trait.traitDescription;
                //optionRarityTexts[i].text  = trait.traitName;   
                optionTypes[i].text = L.TraitDesc(trait.traitType);
                optionRarityTexts[i].text  = L.TraitName(trait.traitType);
                
                //optionRarityTexts[i].color = Color.red;
                optionRarityTexts[i].colorGradientPreset = enchantStyle;
                optionRarityTexts[i].enableVertexGradient = true;


                optionImages[i].sprite = trait.icon;

            }
            return; // exit here so the normal branches don't override

        }

        for (int i = 0; i < maxOptions; ++i)
        {
            //新しいスキルを取得する場合の処理
            if (isGetNewSkill)
            {
                if (newSkillIdxToAdd[i] < 0)         　
                {
                    optionMenus[i].gameObject.SetActive(false);　                // 新規習得時のUI更新
                    continue;
                }
                optionMenus[i].gameObject.SetActive(true);

                AllSkillCasterCollections[newSkillIdxToAdd[i]].RandTraitPairFromAvailableTraitList();
                var casterUI = AllSkillCasterCollections[newSkillIdxToAdd[i]];
                optionNames [i].text = "";                     // スキル名
                Color purpleColor = new Color(0.6f, 0.1f, 0.6f); // 紫色
                //optionNames[i].color = Color.yellow;
                //optionTypes [i].text = casterUI.skillDescription;
                optionTypes [i].text = L.SkillDesc(casterUI.casterIdType);        
                optionStatus[i].text = "";                                      //スキル紹介
                //optionTypes [i].text = casterUI.casterName;
                optionRarityTexts[i].text = L.SkillName(casterUI.casterIdType);                // レアリティ非表示 

                //optionRarityTexts[i].color = GetNewSkillColor; // 紫色に設定 gradient
                optionRarityTexts[i].colorGradientPreset = newSkillStyle;
                optionRarityTexts[i].enableVertexGradient = true;
                
                RectTransform rt = optionTypes[i].GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -5.69f);    

                optionSiderEffect[i].text = ""; // 副作用なし
                optionImages[i].sprite = casterUI.casterSpriteImage;    
                levelUpTitle.text = L.UI("ui.learnable");

                optionTypes[i].fontSize = 17f;

                traitSlots1[i].text = "";
                traitSlots2[i].text = "";
                //traitSlots1[i].text = casterUI.traitPairs[0].traitName;
                //traitSlots2[i].text = casterUI.traitPairs[1].traitName;

                continue;
            }

            // 進化オプションの処理
            if (isEvolutionOption[i])
            {
                //Index -1 out range safe guard
                if (!Valid(activeSkillCasterCollections, casterIdxToLevel[i])) {
                    optionMenus[i].gameObject.SetActive(false);
                    continue;
                }

                var casterEvo = activeSkillCasterCollections[casterIdxToLevel[i]];

                optionMenus [i].gameObject.SetActive(true);
                optionNames [i].text  = L.SkillName(casterEvo.casterIdType,true);
                optionTypes [i].text  = L.SkillDesc(casterEvo.casterIdType,true);
                optionStatus[i].text  = "";
                optionRarityTexts[i].text  = L.UI("ui.evolution"); 
                isOptionEvolving[i] = true;


                optionTypes[i].fontSize = 14.9f;

                //optionRarityTexts[i].color = rarityColorMap[OptionRarity.Legendary]; //gradient
                optionRarityTexts[i].colorGradientPreset = evolStyle;
                optionRarityTexts[i].enableVertexGradient = true;


                optionSiderEffect[i].text = ""; // 副作用なし
                optionImages[i].sprite     = casterEvo.casterSpriteImage;
                continue;                           // skip normal stat card
            }

            //Index -1 out range safe guard
            if (!Valid(activeSkillCasterCollections, casterIdxToLevel[i])) {
                optionMenus[i].gameObject.SetActive(false);
                continue;
            }
            optionMenus[i].gameObject.SetActive(true);
            var statusType = typeToLevelUp[i];
            float rawValue = valueToLevelUp[i];
            string sign    = rawValue > 0 ? "+" : rawValue < 0 ? "-" : "";
            if (statusType == SkillStatusType.Cooldown) sign = "-";          // クールダウンのみ符号反転

            //optionStatus[i].text = statusType == SkillStatusType.ProjectileNum? $"{sign}{Mathf.RoundToInt(Mathf.Abs(rawValue))}個": $"{sign}{Mathf.Abs(rawValue):0}%";　// ステータス増減量の文字列化
            optionStatus[i].text = statusType == SkillStatusType.ProjectileNum? $"{sign}{Mathf.RoundToInt(Mathf.Abs(rawValue))}": $"{sign}{Mathf.Abs(rawValue):0}%";

            var caster = activeSkillCasterCollections[casterIdxToLevel[i]];

            
            optionNames [i].text  = L.SkillName(caster.casterIdType); // スキル名
            optionTypes [i].text  = typeToLevelUp[i].ToLocalized();                     // 日本語タイプ名
            optionRarityTexts[i].text  = rarityToLevelUp[i].ToLocalized();              // 日本語レアリティ
            
            //optionRarityTexts[i].color = rarityColorMap[rarityToLevelUp[i]];     // レアリティカラー gradient
            //optionStatus[i].color = optionRarityTexts[i].color;
            
            ApplyRarityGradient(optionRarityTexts[i], rarityToLevelUp[i]);
            ApplyRarityGradient(optionStatus[i], rarityToLevelUp[i]);
            
            optionImages[i].sprite = caster.casterSpriteImage;

            

            SetOptionStars(i, caster.casterLevel-1, true); //cal and show lv star
            optionStarFrameObjs[i].SetActive(true);

            // 既存の optionStatus[i].text 作成の後ろに追記
            if (sideTypeToApply[i] != SideEffectType.None)
            {

                string debuffSign;
                Color debuffTextColor = Color.red;
                TMP_ColorGradient graPreset = normalStyle;

                if(sideTypeToApply[i] != SideEffectType.CooldownPercent)
                {
                    //debuffSign = sideValueToApply[i] > 0 ? "-" : "";
                    if (sideValueToApply[i] > 0)
                    {
                        debuffSign = "-"; // 正の値はデバフ
                        debuffTextColor = Color.red; // デバフの色は赤
                        graPreset = debuffNegativeStyle;

                    }
                    else if (sideValueToApply[i] < 0)
                    {
                        debuffSign = "+"; // 負の値はバフ
                        debuffTextColor = Color.green; // バフの色は緑
                        graPreset = rareStyle;

                    }
                    else
                    {
                        debuffSign = ""; // 0の場合は空文字
                        debuffTextColor = Color.white; // 0の場合は白色
                        graPreset = normalStyle;
                    }
                }
                else
                {
                    //debuffSign = sideValueToApply[i] > 0 ? "-" : "";
                    if (sideValueToApply[i] > 0)
                    {
                        debuffSign = "+"; // 正の値はデバフ
                        debuffTextColor = Color.red; // デバフの色は赤
                        graPreset = debuffNegativeStyle;

                    }
                    else if (sideValueToApply[i] < 0)
                    {
                        debuffSign = "-"; // 負の値はバフ
                        debuffTextColor = Color.green; // バフの色は緑
                        graPreset = rareStyle;

                    }
                    else
                    {
                        debuffSign = ""; // 0の場合は空文字
                        debuffTextColor = Color.white; // 0の場合は白色
                        graPreset = normalStyle;
                    }

                }
                
                string debuffLabel = sideTypeToApply[i].ToLocalized(); // 拡張メソッドで和訳
                optionSiderEffect[i].text = $"{debuffLabel} {debuffSign}{Mathf.Abs(sideValueToApply[i]):0}%";

                //optionSiderEffect[i].color = debuffTextColor;
                optionSiderEffect[i].color = Color.white;
                optionSiderEffect[i].colorGradientPreset = graPreset;
                optionSiderEffect[i].enableVertexGradient = true;

                if (sideValueToApply[i] == 0)
                {
                    optionSiderEffect[i].text = ""; // 副作用なし
                }

            }else
            {

                //text show ""
                optionSiderEffect[i].text = ""; // 副作用なし

            }

        }


        if (!isSkillWindowFirstTimeOpen)
        {
            isSkillWindowFirstTimeOpen = true;
            frameUiFx.SetOverlayEnable(false,redFrameColor); //turn off overlay at start
        }
        
        
    }

    public void HideUiWhenLevelUP()
    {
        foreach (var ui in uiMenuToHide)
        {
            if (ui != null) ui.SetActive(false);
        }

    }

    public void ReactivateUiUponFinish()
    {
        foreach (var ui in uiMenuToHide)
        {
            if (ui != null) ui.SetActive(true);
        }

        gameSceneButtonGroupRect.anchoredPosition = new Vector2(0, 150);
        //Dotween to move back to 0,0 in 0.35s
        gameSceneButtonGroupRect.DOAnchorPos(Vector2.zero, 0.35f).SetUpdate(true);

    }

    void HideLevelUpWindow()
    {
        //foreach (var cg in optionMenus)
        //{
        //    cg.alpha = 0;
        //    cg.interactable = cg.blocksRaycasts = false;
        //}

        

        controllerPointingIcon.color = new Color(1, 1, 1, 0); //hide icon
        controllerPointingIconTrait.color = new Color(1, 1, 1, 0); //hide icon
        controllerPointingIcon.gameObject.SetActive(false);
        controllerPointingIconTrait.gameObject.SetActive(false);
        controllerPointingIconSwap.gameObject.SetActive(false);

        if (CameraViewManager.Instance.currentMode == CameraViewManager.CameraMode.CloseView)
        {
            CameraViewManager.Instance.HideAndLockCursor();
        }

        GameManager.Instance.isBattling = true;

        EventManager.EmitEvent("UpdateLevelUpFrame");

        ExitExchangeMode(); 
        GameManager.Instance.UnPauseGame();
        LevelUpMenuAnimator.Instance.Close();
        levelupFrame.gameObject.SetActive(false);

        isCoinOrHp = false;

        isCharacterTrait = false;
        
        for (int i = 0; i < maxOptions; ++i)
        {
            optionRarityTexts[i].color = Color.white;


            //Safety Reset Other Data
            isEvolutionOption[i] = false; //Before Buggy part
            casterIdxToLevel[i] = -1; // -1 common value for invalid index
            typeToLevelUp[i] = SkillStatusType.None;
            skillIdToLevelUp[i] = SkillIdType.None;
            valueToLevelUp[i] = 0f;
            giftToGet[i] = false;
            newSkillIdxToAdd[i] = -1;

            rarityToLevelUp[i] = OptionRarity.Normal;
        }

        foreach (var slot in traitSlotToHide)
        {
            slot.SetActive(false);
        }

        levelUpTitle.text = "";

        SkillEffectManager.Instance.CloseRelatedTraitWindow();
        
        renderEffectCamera.SetActive(false);
        renderRawImage.SetActive(false);
        controllerPointingIconTrait.gameObject.SetActive(false);

        foreach (var obj in noBulletEffectObjList)
        {
            obj.SetActive(false);
        }

        //for controller , disable all mouse icon and pointing icon 
        boostMessage.SetActive(false);
        exchangeMessge.SetActive(false);
        CursorManager.Instance.SetCursorNormal();
        
        RerollButton.gameObject.SetActive(true);
        ExchangeButton.gameObject.SetActive(true);
        BoostButton.gameObject.SetActive(true);

        RerollAButton.gameObject.SetActive(false);
        RerollBButton.gameObject.SetActive(false);
        RerollCButton.gameObject.SetActive(false);

        isLevelUpWindowOpen = false;


        isGetNewTrait = false; // safe reset for next time open

    }

    float ReRollValueForSlot(int slot)
    {
        if (isPassiveOption[slot])
            return Random.Range(1f, 5f);                    
    
        var caster      = activeSkillCasterCollections[casterIdxToLevel[slot]];
        var skillRule   = skillSettings.Find(s => s.skillId == caster.casterIdType);
    
        if (skillRule == null) return valueToLevelUp[slot]; 
    
        var statRule    = skillRule.statusList.Find(st => st.statusType == typeToLevelUp[slot]);
        //if (statRule == null)  return valueToLevelUp[slot];      // keep old number, or choose your own default
        
        var rarRule     = statRule.rarityTable.Find(r => r.rarity == rarityToLevelUp[slot]);
    
        float add = Random.Range(rarRule.randMin, rarRule.randMax + 1);
        return rarRule.basePercent + add;
    }

    void ClearAllHighlights()
    {
        for (int i = 0; i < maxOptions; ++i)
        {
            optionNames      [i].color = Color.white;
            optionTypes      [i].color = Color.white;
            
            //optionStatus     [i].color = Color.white;
            
            //optionRarityTexts[i].color = rarityColorMap[rarityToLevelUp[i]];
            //ApplyRarityGradient(optionRarityTexts[i], OptionRarity.Normal); //gradient

        }

        frameUiFx.SetOverlayEnable(false,Color.blue);
        //frameUiFx.SetRoundWaveEnable(false);
        //SoundEffect.Instance.Play(SoundList.ButtonCancelSe);
    }

    void SuccessClearAllHighlights()
    {
        for (int i = 0; i < maxOptions; ++i)
        {
            optionNames      [i].color = Color.white;
            optionTypes      [i].color = Color.white;
        
        }

        frameUiFx.SetOverlayEnable(false,Color.blue);
        //SoundEffect.Instance.Play(SoundList.ButtonCancelSe);
    }

    // Enables the first {level} children of optionStarRoots[slot]. Others off.
    // If visible == false, the whole star row is hidden.
    private void SetOptionStars(int slot, int level, bool visible = true)
    {
        if (optionStarRoots == null || slot < 0 || slot >= optionStarRoots.Length) return;
        var root = optionStarRoots[slot];
        if (!root) return;
    
        root.gameObject.SetActive(visible);
        if (!visible) return;
    
        int childCount = root.childCount;                     // supports 5 or 6 stars without code changes
        int lv = Mathf.Clamp(level, 0, childCount);
        for (int i = 0; i < childCount; ++i)
        {
            root.GetChild(i).gameObject.SetActive(i < lv);
        }
    }

   

    public void GenerateTraitOptionToSelect(SkillCasterBase casterObj)
    {
        

        for(int i =0; i < activeSkillCastersHolder.Count; i++)
        {
            if (activeSkillCastersHolder[i].casterIdType == casterObj.casterIdType)
            {
                casterObjToReleaseEnchant = i; // find the index of the casterObj in activeSkillCastersHolder
                break;
            }
        }

        casterObj.RandTraitPairFromAvailableTraitList();

        traitOptionNameText[0].text = casterObj.traitPairs[0].traitName;
        traitOptionNameText[1].text = casterObj.traitPairs[1].traitName;

        traitOptionDescriptionText[0].text = casterObj.traitPairs[0].traitDescription;
        traitOptionDescriptionText[1].text = casterObj.traitPairs[1].traitDescription;

        //set all pos of traitOptionButtons to (0,-210) :
        for (int i = 0; i < traitOptionButtons.Length; i++)
        {
            traitOptionButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 350);
            traitOptionButtons[i].gameObject.SetActive(false); // hide before animation
        }

        //dotween move traitOptionButtons to ( (i % 2) * 200, 177 ) in 1 second:
        for (int i = 0; i < traitOptionButtons.Length; i++)
        {
            //traitOptionButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 177);
            traitOptionButtons[i].GetComponent<RectTransform>().DOAnchorPos(new Vector2(i * 770 - 420, 177), 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
            traitOptionButtons[i].gameObject.SetActive(true);
        }

    }

    public void SelectTraitOption(int slot)
    {

        traitToApply = activeSkillCastersHolder[casterObjToReleaseEnchant].traitPairs[slot]; 

        ShowSkillTraitEnchantOption();
    }

     public void ShowSkillTraitEnchantOption()
    {
        controllerPointingIconTrait.gameObject.SetActive(true);
        controllerPointingIcon.gameObject.SetActive(false);

        if (Gamepad.current == null)
        {
            controllerPointingIcon.gameObject.SetActive(false);
            controllerPointingIconTrait.gameObject.SetActive(false);
        }

        SetSelectTraitSlot(0);
        GameManager.Instance.PauseGame();             // ゲームを一時停止

        //levelUpTitle.text = "適用するスキルを選択";
        levelUpTitle.text = L.UI("ui.selectSkillToEnchant");

        RerollButton.gameObject.SetActive(false);
        ExchangeButton.gameObject.SetActive(false);
        BoostButton.gameObject.SetActive(false);

        if(traitToApply.traitType == TraitType.MoreBullet)
        {
            for(int i = 0; i < activeSkillCastersHolder.Count; i++)
            {
                if (!activeSkillCastersHolder[i].isMultiBulletEnabled)
                {
                    noBulletEffectObjList[i].SetActive(true);
                }
            }
        }

        //deactive level up button
        //for (int i = 0; i < maxOptions; ++i)
        //{
        //    optionButtons[i].interactable = false;
        //}

        for (int i = 0; i < activeSkillCastersHolder.Count; i++)
        {
            skillEnchantButtons[i].image.sprite = activeSkillCastersHolder[i].casterSpriteImage;
            skillEnchantButtons[i].gameObject.SetActive(true);

            //set all button's recttransform anchor position to 0, -420
            skillEnchantButtons[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -700);

            //dotween move button's recttransform to ( (i % 3) * 200, 0 ) in 1 second:
            //skillEnchantButtons[i].GetComponent<RectTransform>().DOAnchorPos(new Vector2(i * 210-420, -350), 1f).SetEase(Ease.OutBack).SetUpdate(true);
            skillEnchantButtons[i].GetComponent<RectTransform>().DOAnchorPos(new Vector2(219, i * -210 + 280), 1f).SetEase(Ease.OutBack).SetUpdate(true);

        }

        //delaycall
        DOVirtual.DelayedCall(0.35f, PopCharacterEnchantUi).SetUpdate(true);

       
    }

    void PopCharacterEnchantUi()
    {
         characterEnchantButton.gameObject.SetActive(true);
        characterEnchantButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -700);
        characterEnchantButton.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-420, -359), 0.7f).SetEase(Ease.OutBack).SetUpdate(true);
    }


    public void EnchantUniversalTrait()
    {
        if(enchantWaitCdCnt > 0f) return;
        enchantWaitCdCnt = 0.77f;

        //if(!traitToApply.isTraitRepeateable)traitToApply.isSelected = true;
        for(int i = 0; i < maxOptions; i++)
        {
            if (!traitOptionToGet[i].isTraitRepeateable) traitOptionToGet[i].isSelected = true ;
        }

        SkillEffectManager.Instance.SetUniversalTrait(traitToApply);
        EventManager.EmitEvent("UpdateNewTriat");
        //ResetOptionMenuPos();

        traitAlreadySelected.Add(traitToApply);
        
        //AchievementManager.Instance.UnlockTrait(traitToApply.traitType);

        ResetOptionMenuPos();
        isSelectingTrait = false;
    }

    public void EnchantTraitToCaster(int slotId)
    {
        if(enchantWaitCdCnt > 0f) return;
        enchantWaitCdCnt = 1.77f;

        traitAlreadySelected.Add(traitToApply);
        
        //AchievementManager.Instance.UnlockTrait(traitToApply.traitType);
        

        //if(!traitToApply.isTraitRepeateable)traitToApply.isSelected = true;

        for(int i = 0; i < maxOptions; i++)
        {
            if (!traitOptionToGet[i].isTraitRepeateable) traitOptionToGet[i].isSelected = true ;
        }

        activeSkillCastersHolder[slotId].SetSkillTrait(traitToApply);
        Debug.Log($"Trait {traitToApply.traitName} applied to skill {activeSkillCastersHolder[slotId].casterIdType}.");

        if (traitToApply.isDoubleCast)
        {
            playerStatus.EnableDoubleCastModel();
        }

        ControllerVibrationManager.Instance.Vibrate();

        DOVirtual.DelayedCall(0.7f, ResetEnchantSlotPos).SetUpdate(true).OnComplete(() => {

        });

        

        
        SoundEffect.Instance.PlayOneSound(pickTraitSe, 0.77f);
        EventManager.EmitEvent("UpdateNewTriat");

        //for (int i = 0; i < maxOptions; ++i)
        //{
        //    optionButtons[i].interactable = true;
        //}

        

        return;

    }

    void ResetEnchantSlotPos()
    {

        //dotween reset all skillEnchantButtons' recttransform anchor position to 0, -420
        for (int i = 0; i < activeSkillCastersHolder.Count; i++)
        {
            skillEnchantButtons[i].GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -700), 0.42f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
                skillEnchantButtons[i].gameObject.SetActive(false); // hide after animation
            });
        }

        characterEnchantButton.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -700), 0.42f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
            characterEnchantButton.gameObject.SetActive(false);

            isGetNewTrait = false;
            HideLevelUpWindow();
            ReactivateUiUponFinish();
            EffectManager.Instance.CreatePlayerLevelUpEffect();

            ResetOptionMenuPos();
            isSelectingTrait = false;
            
        });

    }

    public void Start()
    {

        BoostButton.onClick.AddListener(ToggleBoostMode);          
        ExchangeButton.onClick.AddListener(ToggleExchangeMode);
        currentPick = new FieldPick { slot = -1 };
        frameUiFx = levelupFrame.GetComponent<UIFXController>();
        playerStatus = GameObject.FindWithTag("Player").GetComponent<PlayerState>();

        int baseSkillId = 0;

        //for loop the activeSkillCasterCollections, find the one with isBaseSkill == true, assign to baseSkillId
        for (int i = 0; i < activeSkillCasterCollections.Count; i++)
        {
            if (activeSkillCasterCollections[i].isPlayerBaseSkill && activeSkillCasterCollections[i].characterJobId == (int)GameManager.Instance.playerData.jobId)
            {
                baseSkillId = i;
                break;
            }
        }

        for (int i = 0; i < skillCoolDownUis.Length; i++)
            {
                if (!skillCoolDownUis[i].isUsed)
                {
                    skillCoolDownUis[i].isUsed = true; // 初回使用時に true にする
                    skillCoolDownUis[i].SkillSlotID = activeSkillCasterCollections.IndexOf(activeSkillCasterCollections[baseSkillId]);
                    activeSkillCastersHolder.Add(activeSkillCasterCollections[baseSkillId]); //add this new skill to activeSkillCastersHolder
                break;
                }
            }

    }

    public void GetTraitLevelUp()
    {
        

        if (TimeManager.Instance.gameTimePassed >= 60 * 10)
        {
            DebugGetOneSkillTrait();

        }
        else
        {
            isNotLvUp = true;
            EventManager.EmitEvent("PlayerLevelUp");
        }
            
    }

    [ContextMenu("Debug Get One Skill Trait")]
    public void DebugGetOneSkillTrait()
    {
        isNotLvUpGetSkillTrait = true;
        EventManager.EmitEvent("PlayerLevelUp");
        
    }

    void Update() {

        if (!isLevelUpWindowOpen) return;

        UpdateRightButtonToggle();
        UpdateControllerInput();

        // if press r 
        //if (Input.GetKeyDown(KeyCode.R)){
        //    RerollOneOption(0);
        //}

        


        if (Input.GetKeyDown(KeyCode.T) && GameManager.Instance.isDebugMode)
        {
            isNotLvUp = true;
            EventManager.EmitEvent("PlayerLevelUp");
        }

        if (Input.GetKeyDown(KeyCode.Y) && GameManager.Instance.isDebugMode)
        {
            RerollOption();
        }

        rerollWaitCnt -= Time.unscaledDeltaTime;
        enchantWaitCdCnt -= Time.unscaledDeltaTime;

        if (isGetNewTrait)
        {
            rerollAButtonText.text = L.UI("ui.reroll") + "A" + $"({reRollChance})";
            rerollBButtonText.text = L.UI("ui.reroll") + "B" + $"({reRollChance})";
            rerollCButtonText.text = L.UI("ui.reroll") + "C" + $"({reRollChance})";

            RerollAButton.interactable = reRollChance > 0;
            RerollBButton.interactable = reRollChance > 0;
            RerollCButton.interactable = reRollChance > 0;
        }


        if (RerollButton != null)
        {
            //RerollButton.GetComponentInChildren<TextMeshProUGUI>().text = $"リロール({reRollChance})";
            //RerollButton.GetComponentInChildren<TextMeshProUGUI>().text = L.UI("ui.reroll") + $"({reRollChance})"; //local
            rerollButtonText.text = L.UI("ui.reroll") + $"({reRollChance})";

            RerollButton.interactable = reRollChance > 0 && !isExchangeMode && !isBoostMode && !isGetNewTrait;

            if (reRollChance > 0 && !isExchangeMode && !isBoostMode && !isGetNewTrait)
            {
                if (rerollButtonFrame.activeSelf == false) rerollButtonFrame.SetActive(true);
                rerollButtonText.DOFade(1f, 0.14f);

            }
            else
            {
                if (rerollButtonFrame.activeSelf == true) rerollButtonFrame.SetActive(false);
                //rerollButtonText.DOFade(0.14f, 0.14f);

            }

        }

        if (BoostButton != null)
        {
             //BoostButton.GetComponentInChildren<TextMeshProUGUI>().text = $"ブースト({boostChance})";
            //BoostButton.GetComponentInChildren<TextMeshProUGUI>().text = L.UI("ui.boost") + $"({boostChance})"; //local
            boostButtonText.text = L.UI("ui.boost") + $"({boostChance})";

            BoostButton.interactable = boostChance > 0  && !isGetNewTrait && !isExchangeMode && !isGetNewSkill;
            BoostButton.image.color = isBoostMode ? new Color(0f,1f,0.7f) : Color.white;

            if (boostChance > 0 && !isGetNewTrait && !isExchangeMode && !isGetNewSkill)
            {
                if (boostButtonFrame.activeSelf == false) boostButtonFrame.SetActive(true);
                boostButtonText.DOFade(1f, 0.14f);

            }
            else
            {
                if (boostButtonFrame.activeSelf == true) boostButtonFrame.SetActive(false);
                //boostButtonText.DOFade(0.14f, 0.14f);
            }

        }


        if (ExchangeButton != null)
        {
            //var txt  = ExchangeButton.GetComponentInChildren<TextMeshProUGUI>();
            //txt.text = $"スワップ({exchangeChance})";  
            //txt.text = L.UI("ui.swap") + $"({exchangeChance})";        //local
            
            exchangeButtonText.text = L.UI("ui.swap") + $"({exchangeChance})";

            ExchangeButton.interactable = waitingForPlayer && !isGetNewSkill && exchangeChance > 0 && !isGetNewTrait && !isBoostMode;
            ExchangeButton.image.color = isExchangeMode ? new Color(0f,1f,0.7f) : Color.white;

            if (waitingForPlayer && exchangeChance > 0 && !isGetNewSkill && !isGetNewTrait && !isBoostMode)
            {
                if (exchangeButtonFrame.activeSelf == false)
                {
                    exchangeButtonFrame.SetActive(true);
                    //exchangeButtonText.DOFade(1f, 0.14f);
                }

            }
            else
            {
                if (exchangeButtonFrame.activeSelf == true)
                {
                    exchangeButtonFrame.SetActive(false);
                    //exchangeButtonText.DOFade(0.21f, 0.14f);
                }
            }

        }


        //if press R
        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    //ShowSkillEnchantOption();

        //    GenerateTraitOptionToSelect(activeSkillCastersHolder[0]);

        //}


    }

    void ApplyRarityGradient(TextMeshProUGUI tmp, OptionRarity r)
    {
        if (!tmp) return;
    
        // IMPORTANT: keep the face/tint white so it doesn't multiply your gradient
        tmp.color = Color.white;
    #if TMP_ESSENTIALS_2_1_OR_NEWER
        tmp.colorGradientPreset = _rarityGradientMap.TryGetValue(r, out var g) ? g : null;
        tmp.colorGradient       = (tmp.colorGradientPreset != null);
    #else
        // Older TMP versions use enableVertexGradient
        tmp.colorGradientPreset = _rarityGradientMap.TryGetValue(r, out var g) ? g : null;
        tmp.enableVertexGradient = (tmp.colorGradientPreset != null);
    #endif
    }

    public void UpdateRightButtonToggle()
    {
        //if press right mouse button
        if (Input.GetMouseButtonDown(1))
        {
            if (isExchangeMode)
            {
                
                ToggleExchangeMode();
            }
            else if (isBoostMode)
            {
               
                ToggleBoostMode();
            }
           

        }
    }


    public int currenTraitSlot = 0;

    public int exSlot = 0;
    public int exKind = 0;
    public Image controllerPointingIconSwap;

    public void UpdateControllerInput()
    {
        if(UIManager.Instance.isStatusMenuOpen) return;
        //if not gamepad connected, return
        if (Gamepad.current == null) return;

        controllerMoveCdCnt -= Time.unscaledDeltaTime;


        if (isGetNewTrait && waitingForPlayer && isSelectingTrait)
        {
            if (Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                RerollOneOption(0);
            }
            if (Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                RerollOneOption(1);
            }
            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                RerollOneOption(2);
            }
        }

        if (Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            if(isBoostMode)
            {
                ToggleBoostMode();
                CursorManager.Instance.SetCursorOff();
            }
            else if (isExchangeMode)
            {
                ToggleExchangeMode();
                CursorManager.Instance.SetCursorOff();
            }
        }
        

        //if press controller Y button, Boost Mode :
        if (Gamepad.current.leftShoulder.wasPressedThisFrame && RerollButton.interactable)
        {
            //ToggleBoostMode();
            RerollOption();
        }

        if(Gamepad.current.buttonNorth.wasPressedThisFrame && BoostButton.interactable)
        {
            ToggleBoostMode();

            if (isBoostMode)
            {
                controllerPointingIcon.gameObject.SetActive(true);
                controllerPointingIconTrait.gameObject.SetActive(false);
                controllerPointingIconSwap.gameObject.SetActive(false);
                //controllerPointingIcon.color = new Color(238, 48, 80, 1);
            }

            CursorManager.Instance.SetCursorOff();
        }

        if(Gamepad.current.buttonWest.wasPressedThisFrame && ExchangeButton.interactable)
        {
            ToggleExchangeMode();

            if (isExchangeMode)
            {
                exSlot = 0;
                exKind = 0;
                SetSwapExchangeSlot(exSlot, exKind);

                controllerPointingIcon.gameObject.SetActive(false);
                controllerPointingIconTrait.gameObject.SetActive(false);
                controllerPointingIconSwap.gameObject.SetActive(true);

                EventManager.EmitEvent("ButtonResetScale");
            }

            

            CursorManager.Instance.SetCursorOff();
        }

        if (isExchangeMode)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
    
            // ─── RIGHT MOVEMENT ───
            if (horizontalInput > 0.5f && _canNavigate)
            {
                int nextSlot = exSlot + 1;

                // 1. First Wrap Check
                if (nextSlot >= maxOptions) nextSlot = 0;

                // 2. Skip Check (If we landed on the picked slot)
                if (isCurrentlyPickingFieldOption && nextSlot == currentExchangeSlotIndex)
                {
                    nextSlot += 1; // Jump over it
                    // 3. Second Wrap Check (In case jumping over it pushed us out of bounds)
                    if (nextSlot >= maxOptions) nextSlot = 0;
                }

                SetSwapExchangeSlot(nextSlot, exKind);
                _canNavigate = false; // Prevent rapid scrolling
            }
            // ─── LEFT MOVEMENT ───
            else if (horizontalInput < -0.5f && _canNavigate)
            {
                int nextSlot = exSlot - 1;

                // 1. First Wrap Check
                if (nextSlot < 0) nextSlot = maxOptions - 1;

                // 2. Skip Check
                if (isCurrentlyPickingFieldOption && nextSlot == currentExchangeSlotIndex)
                {
                    nextSlot -= 1; // Jump over it
                    // 3. Second Wrap Check
                    if (nextSlot < 0) nextSlot = maxOptions - 1;
                }

                SetSwapExchangeSlot(nextSlot, exKind);
                _canNavigate = false;
            }

            else if (verticalInput > 0.5f && _canNavigate && !isCurrentlyPickingFieldOption)
            {
                int nextKind = exKind - 1;
                if (nextKind < 0)
                {
                    nextKind = 2; // Wrap around
                }
                SetSwapExchangeSlot(exSlot, nextKind);
            }

            else if (verticalInput < -0.5f && _canNavigate && !isCurrentlyPickingFieldOption)
            {
                int nextKind = exKind + 1;
                if (nextKind >= 3)
                {
                    nextKind = 0; // Wrap around
                }
                SetSwapExchangeSlot(exSlot, nextKind);

            }

            else if (Mathf.Abs(horizontalInput) < 0.5f && Mathf.Abs(verticalInput) < 0.5f)
            {
                _canNavigate = true;
            }

            if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                int adjustedExKind = exKind;
                if(exKind == 1) adjustedExKind = 2;
                else if (exKind == 2) adjustedExKind = 1;

                OnOptionFieldClicked(exSlot, adjustedExKind);
                Debug.Log($"Exchange option slot {exSlot}, kind {adjustedExKind} selected via controller.");
                return;
            }

        }


        // --- NEW: Controller handling for Coin/HP fallback ---
if (isCoinOrHp)
{
    controllerPointingIcon.gameObject.SetActive(true);
    controllerPointingIconTrait.gameObject.SetActive(false);
    controllerPointingIconSwap.gameObject.SetActive(false);

    float horizontalInput = Input.GetAxisRaw("Horizontal");

    if (horizontalInput > 0.5f && _canNavigate)
    {
        int nextSlot = _currentSelectedSlot + 1;
        if (nextSlot >= maxOptions) nextSlot = 0;
        SetSelectedSlot(nextSlot,currentSelectedRow);
    }
    else if (horizontalInput < -0.5f && _canNavigate)
    {
        int nextSlot = _currentSelectedSlot - 1;
        if (nextSlot < 0) nextSlot = maxOptions - 1;
        SetSelectedSlot(nextSlot,currentSelectedRow);
    }
    else if (Mathf.Abs(horizontalInput) < 0.5f)
    {
        _canNavigate = true;
    }

   

                // A / East confirm
    if (Gamepad.current.buttonEast.wasPressedThisFrame)
    {
        // This will execute the isCoinOrHp branch already present in OnOptionButtonPressed
        OnOptionButtonPressed(_currentSelectedSlot);
        Debug.Log($"[Coin/HP] slot {_currentSelectedSlot} selected via controller.");
        return;
    }
}

        if (waitingForPlayer && !isExchangeMode)
        {

            

            controllerPointingIcon.gameObject.SetActive(true);
            controllerPointingIconTrait.gameObject.SetActive(false);
            controllerPointingIconSwap.gameObject.SetActive(false);

            float controllerMoveCdCntMax = 0.35f;

            float horizontalInput = Input.GetAxisRaw("Horizontal");

            //right
            if (horizontalInput > 0.5f && _canNavigate && controllerMoveCdCnt <= 0)
            {
                controllerMoveCdCnt = controllerMoveCdCntMax;
                int nextSlot = _currentSelectedSlot + 1;            

                if (nextSlot >= maxOptions)
                {
                    nextSlot = 0; // Wrap around
                }

                 if (!levelUpCards[nextSlot].gameObject.activeSelf) return; // prevent selecting inactive slot
                SetSelectedSlot(nextSlot, currentSelectedRow);
            }
            // Check for left movement
            else if (horizontalInput < -0.5f && _canNavigate && controllerMoveCdCnt <= 0)
            {
                controllerMoveCdCnt = controllerMoveCdCntMax;
                int nextSlot = _currentSelectedSlot - 1;
                if (nextSlot < 0)
                {
                    nextSlot = maxOptions - 1; // Wrap around
                }

                 if (!levelUpCards[nextSlot].gameObject.activeSelf) return; // prevent selecting inactive slot

                SetSelectedSlot(nextSlot, currentSelectedRow);
            }
            // Reset navigation flag when stick is centered
            else if (Mathf.Abs(horizontalInput) < 0.5f)
            {
                _canNavigate = true;
            }

             float verticalInput = Input.GetAxisRaw("Vertical");
            if (verticalInput < -0.5f && _canNavigate && controllerMoveCdCnt <= 0)
            {
                if (currentSelectedRow >= 1) return;
                controllerMoveCdCnt = controllerMoveCdCntMax;
                int nextRow = currentSelectedRow + 1;
                //if (nextRow >= 2) nextRow = 0;
                SetSelectedSlot(_currentSelectedSlot, nextRow);

            }
            else if (verticalInput > 0.5f && _canNavigate && controllerMoveCdCnt <= 0)
            {
                if (currentSelectedRow == 0) return;
                controllerMoveCdCnt = controllerMoveCdCntMax;
                int nextRow = currentSelectedRow - 1;
                //if (nextRow < 0) nextRow = 1;
                SetSelectedSlot(_currentSelectedSlot, nextRow);

            }
            else if (Mathf.Abs(verticalInput) < 0.5f)
            {
                _canNavigate = true;
            }

            //if (Input.GetKeyDown(KeyCode.Return))
            if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                // Call your existing function with the selected slot index
                if (waitingForPlayer)
                {
                    if(currentSelectedRow == 0)OnOptionButtonPressed(_currentSelectedSlot);
                    else
                    {

                        if (isGetNewTrait && waitingForPlayer && isSelectingTrait)
                        {
                            if (_currentSelectedSlot == 0) RerollOneOption(0);
                            else if (_currentSelectedSlot == 1) RerollOneOption(1);
                            else if (_currentSelectedSlot == 2) RerollOneOption(2);


                        }
                        else 
                        {
                            if (_currentSelectedSlot == 0) RerollOption();
                            else if (_currentSelectedSlot == 1)
                            {
                                ToggleBoostMode();

                                 if (isBoostMode)
                                 {
                                     controllerPointingIcon.gameObject.SetActive(true);
                                     controllerPointingIconTrait.gameObject.SetActive(false);
                                     controllerPointingIconSwap.gameObject.SetActive(false);
                                     //controllerPointingIcon.color = new Color(238, 48, 80, 1);
                                 }

                                 CursorManager.Instance.SetCursorOff();

                            }
                            else if (_currentSelectedSlot == 2)
                            {
                                ToggleExchangeMode();

                                if (isExchangeMode)
                                {
                                    exSlot = 0;
                                    exKind = 0;
                                    SetSwapExchangeSlot(exSlot, exKind);

                                    controllerPointingIcon.gameObject.SetActive(false);
                                    controllerPointingIconTrait.gameObject.SetActive(false);
                                    controllerPointingIconSwap.gameObject.SetActive(true);

                                    EventManager.EmitEvent("ButtonResetScale");
                                }

                                

                                CursorManager.Instance.SetCursorOff();

                            }


                        }

                        
                    }
                }
               
                //debug log slot
                Debug.Log($"Option slot {_currentSelectedSlot} selected via controller.");
                return;
            }
        }



        
         if(isGetNewTrait && !waitingForPlayer)
        {
            float verticalInput = Input.GetAxisRaw("Vertical");

            controllerPointingIcon.gameObject.SetActive(false);
            controllerPointingIconTrait.gameObject.SetActive(true);
            controllerPointingIconSwap.gameObject.SetActive(false);

            if (verticalInput > 0.5f && _canNavigate && controllerMoveCdCnt <= 0)
            {
                controllerMoveCdCnt = 0.28f;
                
                // DECREASE index to go UP visually
                int nextSlot = currenTraitSlot - 1; 

                if (nextSlot < 0)
                {
                    nextSlot = activeSkillCastersHolder.Count - 1; // Wrap to bottom
                    controllerMoveCdCnt = 0.42f;
                }
                SetSelectTraitSlot(nextSlot);
            }
            // 2. STICK DOWN ( < -0.5 ) -> Go from 0 to 1 to 2 (Increase Index)
            else if (verticalInput < -0.5f && _canNavigate && controllerMoveCdCnt <= 0)
            {
                controllerMoveCdCnt = 0.28f;

                // INCREASE index to go DOWN visually
                int nextSlot = currenTraitSlot + 1; 

                if (nextSlot >= activeSkillCastersHolder.Count)
                {
                    nextSlot = 0; // Wrap to top
                    controllerMoveCdCnt = 0.42f;
                }
                SetSelectTraitSlot(nextSlot);
            }
            // Reset navigation flag when stick is centered
            else if (Mathf.Abs(verticalInput) < 0.5f)
            {
                _canNavigate = true;
            }

            //if (Input.GetKeyDown(KeyCode.Return))
            //if (Input.GetButtonDown("Submit"))
            if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                EnchantTraitToCaster(currenTraitSlot);
                Debug.Log($"Trait option slot {currenTraitSlot} selected via controller.");
                return;
            }

        }


    }

    public void SetSelectTraitSlot(int newSlotIndex)
    {
        if (newSlotIndex < 0 || newSlotIndex >= 4) return;

        if(currenTraitSlot == newSlotIndex && !_canNavigate) return;

        currenTraitSlot = newSlotIndex;

        RectTransform rect = controllerPointingIconTrait.GetComponent<RectTransform>();
        if (newSlotIndex == 0)
        {
            rect.anchoredPosition = new Vector2(50f, 210);
            ObjectRepeatMoveController repeat = controllerPointingIconTrait.GetComponent<ObjectRepeatMoveController>();
            repeat.SetNewOrigin(rect.anchoredPosition);
        }
        else if (newSlotIndex == 1)
        {
            rect.anchoredPosition = new Vector2(50f, 0);
            ObjectRepeatMoveController repeat = controllerPointingIconTrait.GetComponent<ObjectRepeatMoveController>();
            repeat.SetNewOrigin(rect.anchoredPosition);
        }
        else if (newSlotIndex == 2)
        {
            rect.anchoredPosition = new Vector2(50f, -210);
            ObjectRepeatMoveController repeat = controllerPointingIconTrait.GetComponent<ObjectRepeatMoveController>();
            repeat.SetNewOrigin(rect.anchoredPosition);
        }
        else if (newSlotIndex == 3)
        {
            rect.anchoredPosition = new Vector2(50f, -420f);
            ObjectRepeatMoveController repeat = controllerPointingIconTrait.GetComponent<ObjectRepeatMoveController>();
            repeat.SetNewOrigin(rect.anchoredPosition);
        }

    }

    public void SetSwapExchangeSlot(int _slotId, int _kindId)
    {

        if(_slotId < 0 || _slotId >= maxOptions) return;
        if (_kindId < 0 || _kindId >= 3) return;
        if(!_canNavigate) return;
        exSlot = _slotId;
        exKind = _kindId;


        SoundEffect.Instance.Play(SoundList.UiHover);
        _canNavigate = false;

        float pointerX;
        float pointerY;

        float xOffset = -70f;

        if(_slotId == 0)
        {
            pointerX = -749f + xOffset;
        }
        else if (_slotId == 1)
        {
            pointerX = -128f + xOffset;
        }
        else //slot 2
        {
            pointerX = 470f + xOffset;
        }

        if(_kindId == 0)
        {
            pointerY = 177f;
        }
        else if (_kindId == 1) //kind 1
        {
            pointerY = -77f;
        }
        else
        {
            pointerY = -140f;
        }

        RectTransform rect = controllerPointingIconSwap.GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(pointerX, pointerY);
        ObjectRepeatMoveController repeat = controllerPointingIconSwap.GetComponent<ObjectRepeatMoveController>();
        repeat.SetNewOrigin(rect.anchoredPosition);




    }

    public void SetSelectedSlot(int newSlotIndex, int newRowIndex)
{
    // Clamp the index to be safe
    if (newSlotIndex < 0 || newSlotIndex >= maxOptions) return;

    // If the selection hasn't changed, do nothing
    if (_currentSelectedSlot == newSlotIndex && !_canNavigate) return; // _canNavigate check handles initial call

    // Update the index
    _currentSelectedSlot = newSlotIndex;

    currentSelectedRow = newRowIndex;

        if(currentSelectedRow == 0)
        {
             for (int i = 0; i < levelUpCards.Length; i++)  // Tell each card whether it is selected or not
             {
                 if (i == _currentSelectedSlot)
                 {
                     levelUpCards[i].Select();

                         if (isGetNewTrait)
                         {
                             SkillEffectManager.Instance.ShowRelatedTraitWindow(SkillManager.Instance.traitOptionToGet[i].relatedTraitList, i);
                         }

                 }
                 else
                 {
                     levelUpCards[i].Deselect();

                       
                 }
               }
        }
        else
        {
            for (int i = 0; i < levelUpCards.Length; i++)  // Tell each card whether it is selected or not
            {
                 levelUpCards[i].Deselect();
            }

            if (isGetNewTrait)
            {
                SkillEffectManager.Instance.HideRelatedTraitWindow();
            }

        }
       
   
    
    
    SoundEffect.Instance.Play(SoundList.UiHover);
    _canNavigate = false; // Prevents input from being read again on the same press

        float featherPosY = 382f;

        if (newRowIndex == 1) featherPosY = -280f;
        

        RectTransform rect = controllerPointingIcon.GetComponent<RectTransform>();

      
        if (newSlotIndex == 0)
        {
            rect.anchoredPosition = new Vector2(-590f, featherPosY);
            ObjectRepeatMoveController repeat = controllerPointingIcon.GetComponent<ObjectRepeatMoveController>();
            repeat.SetNewOrigin(rect.anchoredPosition);
        }
        else if (newSlotIndex == 1)
        {
            rect.anchoredPosition = new Vector2(21f, featherPosY);
            ObjectRepeatMoveController repeat = controllerPointingIcon.GetComponent<ObjectRepeatMoveController>();
            repeat.SetNewOrigin(rect.anchoredPosition);
        }
        else if (newSlotIndex == 2)
        {
            rect.anchoredPosition = new Vector2(630f, featherPosY);
            ObjectRepeatMoveController repeat = controllerPointingIcon.GetComponent<ObjectRepeatMoveController>();
            repeat.SetNewOrigin(rect.anchoredPosition);
        }

        if (currentSelectedRow == 0) featherSpawnPosY = 3.5f;
        else featherSpawnPosY = -0.7f;

        Vector3 spawnPos = new Vector3(0,featherSpawnPosY,5);
        if (newSlotIndex == 0) spawnPos.x = -4;
        else if (newSlotIndex == 2) spawnPos.x = 4;
        renderEffectCamera.SetActive(true);
        Instantiate(renderFeatherEffectObj, spawnPos, Quaternion.identity);

    }

 


}