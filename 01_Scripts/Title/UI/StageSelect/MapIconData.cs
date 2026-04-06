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
using UnityEngine.PlayerLoop;
using UnityEngine.EventSystems;

public class MapIconData : MonoBehaviour
{
    [Header("アイコンの表示に必要なデータ")]
    public MapType iconMapType = MapType.None;
    [SerializeField] public Image mapIconImage;
    [SerializeField] public TextMeshProUGUI mapName;
    [SerializeField] public GameObject[] difficultyStars;
    [SerializeField] public string[] difficultyName;
    [SerializeField] public string[] stageName;

    [Header("マップを選択するために必要なデータ")]
    [SerializeField] public MapData mapData;
    public GameObject selectedEffect;

    public GameObject playSettingGroup;
    public GameObject normalModeBtn;
    public GameObject startBtn;

    public Button interactButton;
    public bool isInteractable = false;

    private void Update()
    {
        mapName.text = L.MapName(iconMapType);

        if (interactButton == null) return;

        if (interactButton)
        {
            isInteractable = interactButton.interactable;
            //interactButton.interactable = false;
        }

        switch (iconMapType)
        {
            case MapType.None:
                break;
            case MapType.SpiderForest:
                interactButton.interactable = true;
                break;
            case MapType.AncientForest:
                if (AchievementManager.Instance.progressData.isStageAncientForestUnlocked)
                {
                    //Debug.Log("古代の森の解放条件を満たしていました");
                    interactButton.interactable = true;
                }
                else
                {
                    interactButton.interactable = false;
                }
                break;
            case MapType.Castle:
                break;
            case MapType.Desert:
                if (AchievementManager.Instance.progressData.isStageDesertUnlocked)
                {
                    interactButton.interactable = true;
                }
                else
                {
                    interactButton.interactable = false;
                }
                break;
            case MapType.Temple:
                if (AchievementManager.Instance.progressData.isStageTempleUnlocked)
                {
                    interactButton.interactable = true;
                }
                else
                {
                    interactButton.interactable = false;
                }
                break;
            case MapType.Max:
                break;
            case MapType.AchiUnlockType1:
                break;
            case MapType.AchiUnlockType2:
                break;
            case MapType.AchiUnlockType3:
                break;
            case MapType.AchiUnlockType4:
                break;
            default:
                break;
        }

        //string key = "Stage_" + (iconMapType - 1).ToString() + "_AnyDifficulty_Cleared";
        //if (iconMapType != MapType.SpiderForest &&
        //    AchievementManager.Instance.progressData.unlockedAchievementIds.Contains(key) == false)
        //{
        //    //Debug.Log(key + "のアチーブを調べて未クリア状態でした");
        //    interactButton = GetComponentInChildren<Button>();
        //    GetComponentInChildren<Button>().interactable = false;
        //}
        //else
        //{
        //    interactButton = GetComponentInChildren<Button>();
        //    GetComponentInChildren<Button>().interactable = true;
        //    //Debug.Log(key + "MapOpen");
        //}



    }

    public void SetData(MapType _maptype,
        Sprite _mapIconSprite,GameObject _selectedEffect)
    {
        if(_maptype == MapType.None)
        {
            Debug.Log("渡されたMapTypeが'None'でした");
            return;
        }

        // 渡されたデータから情報を取得する
        iconMapType = _maptype;
        mapIconImage.sprite = _mapIconSprite;
        mapName.text = L.MapName(iconMapType);
        selectedEffect = _selectedEffect;

        if (_maptype == MapType.Castle)
        {
            GetComponentInChildren<Button>().interactable = false;
            GetComponentInChildren<TitleButtonController>().enabled = false;
        }

        GetComponentInChildren<Button>().interactable = false;
        Debug.Log("マップアイコンの解放条件を調べます, MapType:" + _maptype.ToString());

        DOVirtual.DelayedCall(3.1f, () => {
             // 解放条件(前のステージを難易度関係なくクリアしている)を満たしていなければ選択不可にする
            string key = "Stage_" + (_maptype - 1).ToString() + "_AnyDifficulty_Cleared";
        if (_maptype != MapType.SpiderForest &&
            AchievementManager.Instance.progressData.unlockedAchievementIds.Contains(key) == false)
        {
            //Debug.Log(key + "のアチーブを調べて未クリア状態でした");
            interactButton = GetComponentInChildren<Button>();
            GetComponentInChildren<Button>().interactable = false;
        }
        else
        {
            interactButton = GetComponentInChildren<Button>();
            GetComponentInChildren<Button>().interactable = true;
            //Debug.Log(key + "MapOpen");
        }

        });

           

        //switch (_maptype)
        //{   
        //    case MapType.None:
        //        break;
        //    case MapType.SpiderForest:
        //        GetComponentInChildren<Button>().interactable = true;
        //        break;
        //    case MapType.AncientForest:
        //        if (AchievementManager.Instance.progressData.isStageAncientForestUnlocked)
        //        {
        //            Debug.Log("古代の森の解放条件を満たしていました");
        //            GetComponentInChildren<Button>().interactable = true;
        //        }
        //        break;
        //    case MapType.Castle:
        //        break;
        //    case MapType.Desert:
        //        if (AchievementManager.Instance.progressData.isStageDesertUnlocked)
        //        {
        //            GetComponentInChildren<Button>().interactable = true;
        //        }
        //        break;
        //    case MapType.Temple:
        //        if (AchievementManager.Instance.progressData.isStageTempleUnlocked)
        //        {
        //            GetComponentInChildren<Button>().interactable = true;
        //        }
        //        break;
        //    case MapType.Max:
        //        break;
        //    case MapType.AchiUnlockType1:
        //        break;
        //    case MapType.AchiUnlockType2:
        //        break;
        //    case MapType.AchiUnlockType3:
        //        break;
        //    case MapType.AchiUnlockType4:
        //        break;
        //    default:
        //        break;
        //}




        if (_maptype == MapType.Castle) { return; }
        int difficultyType = 0;
        foreach(var star in difficultyStars)
        {
            string achivementName = 
                "Stage_" + _maptype.ToString() + "_" 
                + ((DifficultyType)difficultyType).ToString() + "_Cleared";
            bool clearFlg =
                //!AchievementManager.Instance.achievementDictionary.ContainsKey(achivementName) ||
                AchievementManager.Instance.progressData.unlockedAchievementIds.Contains(achivementName);


            if (clearFlg == true)
            {
            //Debug.Log(achivementName + "のアチーブフラグを調べてTrueでした");
                
            }

            difficultyStars[difficultyType].SetActive(clearFlg);

            difficultyType++;
        }
    }

    public void OnClick()
    {
        mapData.mapType = iconMapType;
        var btnTrans = GetComponentInChildren<Button>()?.transform;

        if (selectedEffect != null && btnTrans != null)
        {
            selectedEffect.transform.SetParent(btnTrans);
            selectedEffect.transform.localPosition = Vector3.zero;
            selectedEffect.transform.localScale = Vector3.one;
            selectedEffect.SetActive(true);
        }

        startBtn.GetComponent<Button>().interactable = true;
        EventSystem.current.SetSelectedGameObject(normalModeBtn);
        if (playSettingGroup != null)
        {
            playSettingGroup.GetComponent<CanvasGroup>().interactable = true;
        }
    }
}

