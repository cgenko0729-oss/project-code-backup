using DG.Tweening;
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using System.Linq;
using TigerForge;               //EventManager
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class PetSelectController : MonoBehaviour
{
    [Header("ペットリストへの追加に必要なデータ")]
    [SerializeField] private GameObject petIconPrefab;
    [SerializeField] private GridLayoutGroup content;
    [SerializeField] private PetDataBase collectedPetDataBase;
    [SerializeField] private PetDataBase allPetDataBase;
    [SerializeField] private Toggle changeAllPetListToggle;
    private bool reloadPetListFlg = false;
    private PetDataBase nowUsePetDataBase;

    [Header("選択しているペットの情報表示")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image skillIconImage;
    public Sprite dontHasSkillIconImage;
    public Color activeSkillTypeColor;
    public Color passiveSkillTypeColor;
    [SerializeField] private TextMeshProUGUI skillTypeText;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescText;
    [SerializeField] private TextMeshProUGUI skillCooldownText;
    [SerializeField] private TextMeshProUGUI attackPowerText;
    [SerializeField] private List<Image> roleIconImageList;
    [SerializeField] private List<GameObject> petModelList;

    [Header("選択されたペットのアイコン表示")]
    public TextMeshProUGUI maxSelectedNum;
    public TextMeshProUGUI nowSelectedNum;
    public Image[] selectedPetIconImages;
    
    [Header("ペットのモデルをまとめて子に持つ親のゲームオブジェクト")]
    [SerializeField] private GameObject petModelParentObj;
    public int prevHoverdNum = 0;
    public int nowHoverdNum = 0;

    [Header("選択に必要な情報")]
    [SerializeField] private List<Sprite> roleIconSpritesList;
    public EventSystem eventSystem;
    public GraphicRaycaster raycaster;

    public TextMeshProUGUI nowOwnedPetNum;
    public TextMeshProUGUI maxOwnedPetNum;

    private void Start()
    {
        // ペットモデルリストにシーンに配置しているモデルを追加
        if (petModelParentObj != null)
        {
            List<TitlePetData> petModelObjects = new List<TitlePetData>();
            petModelParentObj.GetComponentsInChildren<TitlePetData>(petModelObjects);
            foreach (var newPet in petModelObjects)
            {
                // すでにリストにあるペットは追加を行わない
                var uniquePetObj = petModelList.Find(
                    obj => obj.GetComponent<TitlePetData>()?.petType == newPet.petType);
                if(uniquePetObj != null) { continue; }

                petModelList.Add(newPet.gameObject);
            }
        }

        nowOwnedPetNum.text = collectedPetDataBase.allPetData.Count().ToString();
        maxOwnedPetNum.text = allPetDataBase.allPetData.Count().ToString();

    }

    private void OnEnable()
    {
        // ペットリストのリロードを行う
        nowUsePetDataBase = collectedPetDataBase;
        ReloadPetList();
        changeAllPetListToggle.isOn = false;

        // 初期には何も表示しない
        foreach (var roleIcon in roleIconImageList)
        {
            roleIcon.enabled = false;
        }

        nameText.text = "---";
        attackPowerText.text = "-";
        skillIconImage.sprite = dontHasSkillIconImage;
        skillTypeText.text = "---";
        skillTypeText.color = Color.white;
        skillNameText.text = "---";
        skillDescText.text = "---";
        skillCooldownText.gameObject.SetActive(false);

        // ペットを連れていける数だけ選択枠を表示する
        for (int i = 0; i < selectedPetIconImages.Length; i++) 
        {
            selectedPetIconImages[i].enabled =
                i < PetSelectDataManager.Instance.MaxPets;
        }
    }

    public void OnEnableReloadListFlg()
    {
        reloadPetListFlg = true;
    }

    private void ReloadPetList()
    {
        // 一度ContentのIconGroupを全て破棄する
        foreach (Transform child in content.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        // 現在使用できるペットデータを全てContentに追加する
        List<PetData> lockedPetDatas= new List<PetData>();
        foreach (PetData petData in nowUsePetDataBase.allPetData)
        {
            if (petData.petType == PetType.None) { continue; }

            // 未開放のペットは未開放リストへ追加
            if (petData.isUnlocked == false) 
            {
                lockedPetDatas.Add(petData);
                continue;
            }

            // リストへ追加
            AddContentPet(petData);
        }

        // 未開放のペットをリストへ追加する
        foreach(var petData in lockedPetDatas)
        {
            // リストへ追加
            AddContentPet(petData);
        }
    }

    private void AddContentPet(PetData petData)
    {
        GameObject newIconGroup = Instantiate(petIconPrefab, content.transform);
        var iconDataComp = newIconGroup.GetComponent<PetIconData>();
        iconDataComp.SetPetData(petData);
    }

    void Update()
    {
        // ペットリストのリロードが行われるかを調べる
        if(reloadPetListFlg == true)
        {
            reloadPetListFlg = false;

            // 両方のペットリストがセットされていなければ処理を行わない
            if(allPetDataBase == null || collectedPetDataBase == null) { return; }

            if(changeAllPetListToggle.isOn == true)
            {
                nowUsePetDataBase = allPetDataBase;
            }
            else
            {
                nowUsePetDataBase = collectedPetDataBase;
            }

            ReloadPetList();
        }

        // カーソルがアイコンに乗っているかどうかを調べる
        OnCursorIcon();

        // 選択中のペット情報を変更する
        ChangeSelectedPetData();

        // 選択中のペットの情報の更新
        UpdateSelectedPetUI();
    }

    // どのペットアイコンにカーソルが乗っているかを調べる
    private void OnCursorIcon()
    {
        if(InputDeviceManager.Instance.GetLastUsedDevice() is Gamepad)
        {
            var uiObj = EventSystem.current.currentSelectedGameObject;
            var iconDataComp = uiObj?.GetComponent<PetIconData>();
            if (iconDataComp != null)
            {
                nowHoverdNum = (int)iconDataComp.petData.petType;
            }
        }
        else if (eventSystem != null && raycaster != null)
        {
            PointerEventData pointerData = new PointerEventData(eventSystem)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(pointerData, results);

            foreach (RaycastResult result in results)
            {
                var nowHoverdUi = result.gameObject.transform.parent.gameObject;
                var iconDataComp = nowHoverdUi.GetComponent<PetIconData>();
                if (iconDataComp != null)
                {
                    nowHoverdNum = (int)iconDataComp.petData.petType;
                    break;
                }
            }
        }
        else
        {
            Debug.Log("Gamepadが接続されていないか、\n'EventSystem'または'GraphicRaycaster'がアタッチされていません");
        }
    }

    // 乗っているペットのナンバーが違う場合は、変更を行う
    private void ChangeSelectedPetData()
    {
        if (nowHoverdNum == prevHoverdNum) { return; }

        // 乗っているペットアイコンのペットタイプからリストの何番目のペットなのかを取得する
        PetData selectedPetData =
            nowUsePetDataBase.allPetData.Find(data => (int)data.petType == nowHoverdNum);

        // 表示されているペットモデルを切り替える
        foreach (var petModel in petModelList)
        {
            var petNum = (int)petModel.GetComponent<TitlePetData>().petType;

            // 乗っているペットのタイプが同じであれば表示し、異なれば非表示にする
            petModel.gameObject.SetActive(petNum == nowHoverdNum);
        }

        var model = petModelList.Find(data => data.activeSelf == true);
        if(model != null)
        {
            model.GetComponent<TitlePetData>()?.ChangeSilhouette(!selectedPetData.isUnlocked);
        }

        // 乗っているペットが未開放であれば早期リターンする
        if(selectedPetData.isUnlocked == false) 
        {
            nameText.text =  LocalizationSettings.StringDatabase.GetLocalizedString("Enums", "petisUnlock");
            attackPowerText.text = "-";
            skillIconImage.sprite = dontHasSkillIconImage;
            skillTypeText.text = "---";
            skillTypeText.color = Color.white;
            skillNameText.text = "---";
            skillDescText.text = "---";
            skillCooldownText.gameObject.SetActive(false);

            skillDescText.text = L.MapName(selectedPetData.petMap);

            if(selectedPetData.petMap != MapType.AchiUnlockType1 || selectedPetData.petMap != MapType.AchiUnlockType2)
            {
                skillDescText.text += "\n" + LocalizationSettings.StringDatabase.GetLocalizedString(
            "Enums", $"getPetMsg");
            }

            foreach (var icon in roleIconImageList)
            {
                icon.enabled = false;
            }

            prevHoverdNum = nowHoverdNum;
            return; 
        }


        // 選択中のペット情報をセットして選択中ナンバーを入れ替える
        {
            nameText.text = L.PetName(selectedPetData.petType);
            attackPowerText.text = selectedPetData.attackPower.ToString();

            // スキルの情報を取得してテキストに変換
            bool hasSkills = selectedPetData.hasActiveSkill | selectedPetData.hasPassiveSkill;
            Sprite iconSprite = dontHasSkillIconImage;
            string typeStr = "";
            Color typeCol = Color.white;
            string nameStr = "";
            string descriptionStr = "スキル未所持";
            bool drawCooldown = false;
            string cooldownStr = L.UI("Pet_SkillCooldown");
            if (hasSkills)
            {
                var skill = selectedPetData.skills[0];
                iconSprite = skill.skillicon;
                switch (skill.skilltype)
                {
                    // アクティブスキルの場合
                    case SkillType.Active:
                        typeStr = L.UI("Pet.ActiveSkill");
                        typeCol = activeSkillTypeColor;
                        drawCooldown = true;
                        cooldownStr += selectedPetData.activeSkillTotalCooldown;
                        break;
                    // パッシブスキルの場合
                    case SkillType.Passive:
                        typeStr = L.UI("Pet.PassiveSkill");
                        typeCol = passiveSkillTypeColor;
                        break;
                }
                nameStr = L.PetSkillName(selectedPetData.petType);
                descriptionStr = L.PetSkillDesc(selectedPetData.petType);
            }

            // 取得したスキル情報をセットする
            skillIconImage.sprite = iconSprite;
            skillTypeText.text = typeStr;
            skillTypeText.color = typeCol;
            skillNameText.text = nameStr;
            skillDescText.text = descriptionStr;
            skillCooldownText.gameObject.SetActive(drawCooldown);
            skillCooldownText.text = cooldownStr;

            prevHoverdNum = nowHoverdNum;
        }

        var roles = selectedPetData.petRoles;
        int petRoleNum = roles.Count;
        int n = 0;
        foreach (var icon in roleIconImageList)
        {
            if (n < petRoleNum)
            {
                icon.enabled = true;
                icon.sprite = roleIconSpritesList[(int)roles[n]];
            }
            else
            {
                icon.enabled = false;
                icon.sprite = null;
            }

            n++;
        }


    }

    private void UpdateSelectedPetUI()
    {
        // 選択されたペットの情報を取得する
        var selectedPets = PetSelectDataManager.Instance.SelectedPets;

        // 選択中のペットの数を更新
        if (maxSelectedNum != null && nowSelectedNum != null)
        {
            int selectedCount = selectedPets.Count;
            nowSelectedNum.text =
                selectedCount.ToString();
            int maxCount = PetSelectDataManager.Instance.MaxPets;
            maxSelectedNum.text = maxCount.ToString();

            Color textColor = Color.white;
            if (selectedCount >= maxCount)
            {
                textColor = Color.red;
            }
            nowSelectedNum.color = textColor;
        }

        // 選択中のペットアイコンを更新
        for (int i = 0; i < PetSelectDataManager.Instance.MaxPets; i++)
        {
            if (selectedPetIconImages[i] == null || 
                i >= PetSelectDataManager.Instance.MaxPets) { continue; }

            Sprite setSprite = null;
            bool enableFlg = false;
            if (i < selectedPets.Count)
            {
                setSprite = selectedPets[i].petIcon;
                enableFlg = true;
            }
            selectedPetIconImages[i].sprite = setSprite;
            selectedPetIconImages[i].enabled = enableFlg;
        }
    }
}