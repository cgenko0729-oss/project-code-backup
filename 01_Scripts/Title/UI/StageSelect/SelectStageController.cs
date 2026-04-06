using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using Cysharp.Threading.Tasks;
using EasyTransition;
using UnityEngine.EventSystems;

public class SelectStageController : MonoBehaviour
{
    [Header("アイコンリストへの追加")]
    [SerializeField]public GridLayoutGroup content;
    [SerializeField]public GameObject stageIconGroupPrefab;
    [SerializeField]public List<Sprite> mapIconSpriteList = new List<Sprite>();
    [SerializeField] public GameObject selectedEffect;

    [Header("ゲームモードの説明文の変更")]
    [SerializeField] private TextMeshProUGUI modeDescText;

    [Header("難易度の説明文の変更")]
    [SerializeField] private TextMeshProUGUI enemyHpDescText;
    [SerializeField] private TextMeshProUGUI petGetChanceDescText;
    [SerializeField] private TextMeshProUGUI getCoinMultiplierDescText;

    [Header("シーン遷移のデータ")]
    [Tooltip("トリガーとなるボタンオブジェクト")]
    public GameObject startButton;
    public MapData mapData;
    [Tooltip("フェードアウトの演出")]
    public TransitionSettings transSettings;
    [Tooltip("開始までの時間(秒)")]
    public float fadeStartTime = 0;
    private bool onClickFlg = false;

    public Slider progressBar;
    public TextMeshProUGUI progressText;

    public Image fadeOutImage;

    public GameObject firstListObj;
    public GameObject nowSelectedObj;

    public GameObject playSettingGroup;
    public GameObject normalModeBtn;

    private void OnEnable()
    {
        if(firstListObj != null)
        {
            EventSystem.current.SetSelectedGameObject(firstListObj);
        }
    }

    private void Start()
    {
        if (playSettingGroup != null)
        {
            playSettingGroup.GetComponent<CanvasGroup>().interactable = false;
        }

        startButton.GetComponent<Button>().interactable = false;
        mapData.mapType = MapType.SpiderForest;
        
        if(selectedEffect != null)
        {
            selectedEffect.SetActive(false);
        }

        //DemoStageSelectLayout();
        //return; //demo early return;

        int mapMaxType = (int)MapType.Max;
        for (int type = 1; type < mapMaxType; type++)
        {
            if(type == (int)MapType.Castle ) { continue; }

            var newIconGroup = Instantiate(stageIconGroupPrefab, content.transform);
            var iconDataComp = newIconGroup.GetComponent<MapIconData>();

            Sprite mapIconSprite = mapIconSpriteList[type - 1];
            if (selectedEffect != null && mapIconSprite != null)
            {
                iconDataComp.SetData((MapType)type, mapIconSprite, selectedEffect);
            }

            iconDataComp.playSettingGroup = playSettingGroup;
            iconDataComp.normalModeBtn = normalModeBtn;
            iconDataComp.startBtn = startButton; 

            // リストの一番最初の要素を取得しておく
            if (firstListObj == null)
            {
                Button btnComp = newIconGroup.GetComponentInChildren<Button>();
                firstListObj = btnComp.gameObject;
            }
        }

        if(startButton != null)
        {
            startButton.GetComponent<Button>().interactable = false;
        }
    }

    private void Update()
    {
        // ゲームモードによってモードの説明文を変化させる
        if (modeDescText != null)
        {
            string key = "Title_GameMode";
            if (StageManager.Instance.isEndlessMode == true)
            {
                key += "Endless.Desc";
            }
            else
            {
                key += "Normal.Desc";
            }
            modeDescText.text = L.UI(key);
        }

        // 難易度によって難易度の説明文を変化させる
        if(enemyHpDescText != null && petGetChanceDescText != null && getCoinMultiplierDescText != null)
        {
            string key = "Title_DifficultyDesc.";
            DifficultyType difficultyType = mapData.stageDifficulty;
            
            // 敵のHPの増加量
            {
                string descStr = L.UI(key + "EnemyHpDesc." + difficultyType.ToString());
                enemyHpDescText.text = descStr;
            }

            // ペットの入手確率
            {
                string descStr = L.UI(key + "PetGetChanceDesc");
                float difficultyPetGetChance =
                    (PetGetChanceManager.Instance.ChangeGetChanceDifficulty(difficultyType) - 1.0f) * 100;
                descStr += difficultyPetGetChance.ToString() + "%UP";
                petGetChanceDescText.text = descStr;
            }

            // コインの獲得倍率
            {
                string descStr = L.UI(key + "GetCoinMultplierDesc");
                float getCoinMultiplier = ((int)mapData.stageDifficulty - 1) * 25.0f;
                descStr += getCoinMultiplier.ToString() + "%UP";
                getCoinMultiplierDescText.text = descStr;
            }
        }

        var obj = EventSystem.current.currentSelectedGameObject;
        nowSelectedObj = obj;
    }

    private void DemoStageSelectLayout()
    {
        List<GameObject> list = new List<GameObject>();
        int mapMaxType = (int)MapType.Max;
        for (int type = 1; type < mapMaxType; type++)
        {
           
            if(type == (int)MapType.Castle) continue;  //skip 3 (Castle)
            

            var newIconGroup = Instantiate(stageIconGroupPrefab);
            
            var iconDataComp = newIconGroup.GetComponent<MapIconData>();

            Sprite mapIconSprite = mapIconSpriteList[type - 1];
            if (selectedEffect != null && mapIconSprite != null)
            {
                iconDataComp.SetData((MapType)type, mapIconSprite, selectedEffect);
            }

            iconDataComp.playSettingGroup = playSettingGroup;
            iconDataComp.normalModeBtn = normalModeBtn;
            iconDataComp.startBtn = startButton;

            if ((type != (int)MapType.Castle))
            {

                newIconGroup.transform.SetParent(content.transform);
                newIconGroup.transform.localScale = Vector3.one;

                // リストの一番最初の要素を取得しておく
                if (firstListObj == null)
                {
                    Button btnComp = newIconGroup.GetComponentInChildren<Button>();
                    if (btnComp?.interactable == true)
                    {
                        firstListObj = btnComp.gameObject;
                    }

                    

                }

                //Demo Lock SpiderForest and Temple Map Button
                Button btnCompEach = newIconGroup.GetComponentInChildren<Button>();
                btnCompEach.interactable = true;
                //if(type == (int)MapType.SpiderForest || type == (int)MapType.Temple)
                //{
                //    btnCompEach.interactable = false;
                //}
                

            }
            else
            {
                list.Add(newIconGroup);
            }
        }

        foreach(var obj in list)
        {
            obj.transform.SetParent(content.transform);
            obj.transform.localScale = Vector3.one;
        }

        if (startButton != null)
        {
            startButton.GetComponent<Button>().interactable = false;
        }
    }

    public void OnClickStartButton()
    {
        if (onClickFlg == true) { return; }
        onClickFlg = true;

        EventManager.EmitEvent(GameEvent.pushTitlebtn);

        // ゲームシーンに遷移する
        //TransitionManager.Instance().Transition("GameScene", transSettings, fadeStartTime);

        //TransitionManager.Instance().Transition(transSettings,0.14f);
        
        Invoke("TransitionToTestScene", 1.28f);
    }

    void TransitionToTestScene()
    {
        //use dotween to fade out fadeout image 's alpha from 0 to 1 in 0.4 seconds

        fadeOutImage.DOFade(1f, 0.4f).OnComplete(() => {
            DoLoadGameScene();
        });
    }

    void DoLoadGameScene()
    {
        SceneLoader.LoadScene("GameScene");
    }
}

