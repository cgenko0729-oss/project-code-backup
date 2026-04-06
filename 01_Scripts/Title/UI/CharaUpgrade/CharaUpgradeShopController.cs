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
using UnityEngine.Windows.Speech;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

[System.Serializable]
public struct OneCharaShopItems
{
    public GameObject parentObj;
    public JobId jobId;
    public CharaUpgradeShopItemData[] shopItems;
}

public class CharaUpgradeShopController : MonoBehaviour
{
    [Header("キャラごとの全ての強化内容を格納するリスト")]
    public OneCharaShopItems[] allCharaShopItems;
    [Header("現在表示されているキャラのJobId")]
    public JobId nowJobId;
    public PlayerData playerData;
    [Header("選択中の強化項目の内容表示")]
    public TextMeshProUGUI upgradeNameText;
    public TextMeshProUGUI upgradeDescText;
    public TextMeshProUGUI upgradeUnlockCoinText;
    public TextMeshProUGUI currentCoinText;
    [Header("最初に選択されるUIオブジェクト")]
    public List<Button> firstSelectObjects = new();

    public Toggle dogToggle;
    public Toggle rabbitToggle;
    public Toggle birdToggle;
    public Toggle lionToggle;

    private void OnEnable()
    {
        nowJobId = playerData.jobId;
        allCharaShopItems[(int)nowJobId].parentObj.SetActive(true);
        
        EventManager.StartListening("UpdateAllShopItems", UpdateAllShopItems);
    }

    private void OnDisable()
    {
        EventManager.StopListening("UpdateAllShopItems", UpdateAllShopItems);
    }

    void Start()
    {
        for (int i = 0; i < (int)JobId.MAX; i++)
        {
            allCharaShopItems[i].parentObj.SetActive(true);
            OneCharaShopItems oneCharaShopItems = allCharaShopItems[i];
            oneCharaShopItems.jobId = (JobId)i;
            if (oneCharaShopItems.parentObj != null &&
                oneCharaShopItems.parentObj.activeSelf == true)
            {
                oneCharaShopItems.shopItems =
                    oneCharaShopItems.parentObj.GetComponentsInChildren<CharaUpgradeShopItemData>();
            }
            allCharaShopItems[i].parentObj.SetActive(false);
            allCharaShopItems[i] = oneCharaShopItems;

            var firstButton = allCharaShopItems[i].parentObj.GetComponentInChildren<Button>();
            if (firstButton != null)
            {
                firstSelectObjects.Add(firstButton);
            }
        }

        EventManager.EmitEvent("UpdateAllShopItems");
        gameObject.SetActive(false);
    }

    void Update()
    {
        int currentCoinNum = CurrencyManager.Instance.Coins;
        int unlockCoinNum = CharaUpgradeShopManager.Instance.nowSelectedUnlockCoin;
        upgradeNameText.text = CharaUpgradeShopManager.Instance.nowSelectedName;
        upgradeDescText.text = CharaUpgradeShopManager.Instance.nowSelectedDesc;
        upgradeUnlockCoinText.text = unlockCoinNum.ToString();
        Color currnetCoinNumColor = Color.white;
        if(currentCoinNum < unlockCoinNum)
        {
            currnetCoinNumColor = Color.red;
        }
        currentCoinText.text = currentCoinNum.ToString();
        currentCoinText.color = currnetCoinNumColor;

        if (InputDeviceManager.Instance.GetLastUsedDevice() is not Gamepad) { return; }
        // Rボタンで強化キャラを1つ右に変更
        JobId nextJobid = nowJobId;
        if (Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            nextJobid--;
            if (nextJobid < JobId.DogKnight)
            {
                nextJobid = JobId.MAX - 1;
            }

            

            ChangeUpgradeChara((int)nextJobid);

            switch (nextJobid)
            {
                case JobId.DogKnight:
                    dogToggle.isOn = true;
                    break;
                case JobId.Wizard:
                    birdToggle.isOn = true;
                    break;
                case JobId.Archer:
                    rabbitToggle.isOn = true;
                    break;
                case JobId.Warrior:
                    lionToggle.isOn = true;
                    break;
                case JobId.MAX:
                    break;
                default:
                    break;
            }

            Debug.Log("nextJobid: " + nextJobid);

            

        }
        // Lボタンで強化キャラを1つ右に変更
        else if (Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            nextJobid++;
            if (nextJobid == JobId.MAX)
            {
                nextJobid = JobId.DogKnight;
            }

            

            ChangeUpgradeChara((int)nextJobid);

            
               switch (nextJobid)
            {
                case JobId.DogKnight:
                    dogToggle.isOn = true;
                    break;
                case JobId.Wizard:
                    birdToggle.isOn = true;
                    break;
                case JobId.Archer:
                    rabbitToggle.isOn = true;
                    break;
                case JobId.Warrior:
                    lionToggle.isOn = true;
                    break;
                case JobId.MAX:
                    break;
                default:
                    break;
            }

            Debug.Log("nextJobid: " + nextJobid);
            

        }
    }

    public void UpdateAllShopItems()
    {
        int jobId = (int)nowJobId;

        // 表示中キャラの全アイテムの解放可能かどうかの判定を行う
        foreach (var shopItem in allCharaShopItems[jobId].shopItems)
        {
            shopItem.UpdateEnableUnlock();
        }

        // 表示中キャラの全アイテムが行う他アイテムのロック処理を行う
        foreach (var shopItem in allCharaShopItems[jobId].shopItems)
        {
            shopItem.UpdateExclusion();
        }
    }

    // 強化するキャラを切り替える
    public void ChangeUpgradeChara(int jobid)
    {
        // キャラごとにアイテムをまとめたものが存在するかを確かめる
        GameObject nowUpgradeGroup = 
            allCharaShopItems[(int)nowJobId].parentObj;
        GameObject nextUpgradeGroup =
            allCharaShopItems[(int)jobid].parentObj;
        if(nowUpgradeGroup == null || nextUpgradeGroup == null) { return; }

        // 表示を切り替える
        nowUpgradeGroup.SetActive(false);
        nextUpgradeGroup.SetActive(true);

        // JobIdを変更する
        nowJobId = (JobId)jobid;
        EventManager.EmitEvent("UpdateAllShopItems");

        // 1つ目の強化要素を選択中にする
        if (firstSelectObjects[jobid] != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectObjects[jobid].gameObject);
        }
    }

    [ContextMenu("UpdateAllCharaShopItems")]
    public void UpdateAllCharaShopItems()
    {
        for (int i = 0; i < (int)JobId.MAX; i++)
        {
            allCharaShopItems[i].parentObj.SetActive(true);
            OneCharaShopItems oneCharaShopItems = allCharaShopItems[i];
            oneCharaShopItems.jobId = (JobId)i; 
            if (oneCharaShopItems.parentObj != null &&
                oneCharaShopItems.parentObj.activeSelf == true)
            {
                oneCharaShopItems.shopItems =
                    oneCharaShopItems.parentObj.GetComponentsInChildren<CharaUpgradeShopItemData>();
            }
            allCharaShopItems[i].parentObj.SetActive(false);
            allCharaShopItems[i] = oneCharaShopItems;
        }
    }


}

