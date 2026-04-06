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
using UnityEngine.EventSystems;
using System.Net.Sockets;

// 強化オプションの種類と変化率をまとめた構造体
[System.Serializable]
public class OptionLvObjects
{
    public SkillStatusType skillStatusType;
    public TextMeshProUGUI scalerNum;
}

public class SkillDataWindowController : MonoBehaviour
{
    [Header("スキルデータのセットに必要な情報")]
    public List<GameObject> lvStars = new List<GameObject>();
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI finalDescriptionText;
    public int nowLv = 0;

    [Header("各オプションのレベル")]
    [SerializeField]public List<OptionLvObjects> optionLvObjList;

    [Header("エンチャントアイコン")]
    [SerializeField] public List<Image> anchantImageList;

    [Header("表示に使う情報")]
    [SerializeField] public CanvasGroup canvasGroup;

    public SkillCasterBase caster;

    void Start()
    {
        foreach(var star in lvStars)
        {
            star.SetActive(false);
        }

        if(canvasGroup == null )
        {
            Debug.Log("CanvasGroupが見つかりませんでした");
        }
        else
        {
            canvasGroup.alpha = 0;
        }
    }

    private void OnEnable()
    {
        EventManager.StartListening(GameEvent.OnActiveSkillDataWindow,OnActiveWindow);
        EventManager.StartListening(GameEvent.OnInactiveSkillDataWindow,OnInactiveWindow);
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameEvent.OnActiveSkillDataWindow, OnActiveWindow);
        EventManager.StopListening(GameEvent.OnInactiveSkillDataWindow, OnInactiveWindow);
    }

    public void OnActiveWindow()
    {
        int skillIndex = 
            EventManager.GetInt(GameEvent.OnActiveSkillDataWindow);

        // スキル情報をセットする
        SetSkillData(skillIndex);
        // ウィンドウを表示させる
        canvasGroup.alpha = 1;
    }

    public void OnInactiveWindow()
    {
        canvasGroup.alpha = 0;
    }

    void SetSkillData(int _skillNum)
    {
        var skillCaster = SkillManager.Instance.activeSkillCasterCollections[_skillNum];
        nameText.text = L.SkillName(skillCaster.casterIdType);
        descriptionText.text = L.SkillDesc(skillCaster.casterIdType);
        finalDescriptionText.text = L.SkillDesc(skillCaster.casterIdType,true);
        nowLv = skillCaster.casterLevel;

        // レベルの星アイコンの表示・非表示切り替え
        for(int n = 4;n >= 0;n--)
        {
            if(n >= nowLv - 1)
            {
                lvStars[n].SetActive(false);
            }
            else
            {
                lvStars[n].SetActive(true);
            }
        }

        // オプションの変化率をセットする
        SetOptionScaler(skillCaster);

        // 現在スキルについているエンチャントの画像をセットする
        SetAnchantImage(skillCaster);
    }

    // オプションでの変化率をセット
    private void SetOptionScaler(SkillCasterBase _skillCaster)
    {
        List<float> optionScalerList = new List<float>();
        optionScalerList.Add(_skillCaster.coolDownFactor);
        optionScalerList.Add(_skillCaster.speedScaler);
        optionScalerList.Add(_skillCaster.durationScaler);
        optionScalerList.Add(_skillCaster.damageScaler);
        optionScalerList.Add(_skillCaster.sizeScaler);
        optionScalerList.Add(_skillCaster.projectileNumScaler);

        // それぞれのオプションのレベルをセットしていく
        for (int type = 0; type < optionLvObjList.Count; type++)
        {
            // 変化率を求める
            float scalerNum = optionScalerList[type];
            if (type == (int)SkillStatusType.ProjectileNum && scalerNum < 0)
            {
                scalerNum = 0;
            }
            // クールダウンの変化率を%の値に直す
            if (type == (int)SkillStatusType.Cooldown)
            {
                //scalerNum *= 100;
                scalerNum = (1 - scalerNum) * 100 * -1;
            }
            // 整数に直す
            scalerNum = Mathf.Round(scalerNum);

            string setText = string.Empty;
            Color textCol = Color.white;

            // テキストの色や符号の追加
            if ((type == (int)SkillStatusType.Cooldown && scalerNum < 0) ||
                (type != (int)SkillStatusType.Cooldown && scalerNum > 0))
            {
                if (type != (int)SkillStatusType.Cooldown)
                {
                    setText += "+";
                }
                textCol = Color.green;
            }
            else if ((type == (int)SkillStatusType.Cooldown && scalerNum > 0) ||
                (type != (int)SkillStatusType.Cooldown && scalerNum < 0))
            {
                if (type == (int)SkillStatusType.Cooldown)
                {
                    setText = "+";
                }
                textCol = Color.red;
            }

            // (弾数以外は)「%」を追加する
            setText += (int)scalerNum;
            if (type != (int)SkillStatusType.ProjectileNum)
            {
                setText += "%";
            }

            optionLvObjList[type].scalerNum.text = setText;
            optionLvObjList[type].scalerNum.color = textCol;
        }
    }

    private void SetAnchantImage(SkillCasterBase _skillCaster)
    {
        // 一度全てのアイコンを非表示にする
        foreach(var icon in anchantImageList)
        {
            icon.enabled = false;
        }

        var holdingAnchants = _skillCaster.traitDataHoldingList;
        for (int n = 0; n < holdingAnchants.Count; n++)
        {
            var anchantImage = holdingAnchants[n].icon;
            if(anchantImage != null)
            {
                anchantImageList[n].sprite = anchantImage;
                anchantImageList[n].enabled = true;
            }
        }
    }
}
