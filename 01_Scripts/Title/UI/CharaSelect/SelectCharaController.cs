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

public class SelectCharaController : MonoBehaviour
{
    [Header("選んでいるキャラの情報表示")]
    public TextMeshProUGUI charaName;
    public TextMeshProUGUI charaJobType;
    public PlayerData playerData;

    public Image skillImage;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI skillDescription;
    public SkillCasterBase[] defaultSkillCasterList;

    public List<GameObject> selectIconsList;
    public GameObject isSelectedIocn;

    void Start()
    {
        playerData = PlayerDataManager.Instance.playerData;

        int typeMaxNum = (int)JobId.MAX;
        for (int type = 0; type < typeMaxNum; type++)
        {
            // 保存されたアチーブメントからプレイヤーが解放済みかを調べる
            playerData.StatusDataList[type].isUnlocked =
                AchievementManager.Instance.IsCharacterUnlocked((JobId)type);


            switch (type)
            {
                case (int)JobId.DogKnight:
                    if (AchievementManager.Instance.progressData.isCharacter1Unlocked) playerData.StatusDataList[type].isUnlocked = true;
                    break;
                case (int)JobId.Archer:
                    if (AchievementManager.Instance.progressData.isCharacter2Unlocked) playerData.StatusDataList[type].isUnlocked = true;
                    break;
                case (int)JobId.Warrior:
                    if (AchievementManager.Instance.progressData.isCharacter3Unlocked) playerData.StatusDataList[type].isUnlocked = true;
                    break;
                case (int)JobId.Wizard:
                    if (AchievementManager.Instance.progressData.isCharacter4Unlocked) playerData.StatusDataList[type].isUnlocked = true;
                    break;
                default:
                    break;
            }

            //playerData.StatusDataList[type].isUnlocked = true;


            if (playerData.StatusDataList[type].isUnlocked == true)
            {
                selectIconsList[type].SetActive(true);
            }
        }

        //Demo Only Unlock DogKnight and Rabbit Archer
        //for (int i = 0; i < playerData.StatusDataList.Length; i++)
        //{
        //    playerData.StatusDataList[i].isUnlocked = false;

        //    if(i == (int)JobId.DogKnight || i == (int)JobId.Archer)
        //    {
        //        playerData.StatusDataList[i].isUnlocked = true;

        //    }

        //}
       


    }

    private void OnEnable()
    {
        ChangeChara((int)playerData.jobId);
    }

    void Update()
    {
        charaJobType.text = L.CharacterJobType(playerData.jobId);
        charaName.text = L.CharacterName(playerData.jobId);
    }

    public void OnClickRightButton()
    {
        JobId nextJobId = playerData.jobId;
        bool loopback = false;
        while (true)
        {
            nextJobId++;
            if (nextJobId >= JobId.MAX)
            {
                nextJobId = JobId.DogKnight;

                // 永久ループ阻止
                if(loopback == false)
                {
                    loopback = true;
                }
                else
                {
                    Debug.Log("アンロックされたキャラを見つけられなかった");
                    // 強制的にWhileを抜ける
                    break;
                }
            }
            // nextJobIdのプレイヤーがアンロックされていればWhileを抜ける
            if (playerData.StatusDataList[(int)nextJobId].isUnlocked == true) { break; }
        }

        if (ChangeChara((int)nextJobId))
        {
            playerData.jobId = nextJobId;
        }
    }

    public void OnClickLeftButton()
    {
        JobId nextJobId = playerData.jobId;

        while(true)
        {
            nextJobId--;
            if (nextJobId < JobId.DogKnight)
            {
                nextJobId = JobId.MAX - 1;
            }

            if (playerData.StatusDataList[(int)nextJobId].isUnlocked == true) { break; }
        }

        if(ChangeChara((int)nextJobId))
        {
            playerData.jobId = nextJobId;
        }
    }

    bool ChangeChara(int _nextJobId)
    {
        // 表示している選択中のキャラ情報を切り替える
        var skillCaster = defaultSkillCasterList[_nextJobId];
        if (skillCaster == null) { return false; }
        skillImage.sprite = skillCaster.casterSpriteImage;
        skillName.text = L.SkillName(skillCaster.casterIdType);
        skillDescription.text = L.SkillDesc(skillCaster.casterIdType);

        // どのキャラを選択中かのアイコンを切り替える
        var iconBackTrans = selectIconsList[_nextJobId].transform;
        isSelectedIocn.transform.SetParent(iconBackTrans);
        isSelectedIocn.transform.localPosition = Vector3.zero;

        return true;
    }
}

