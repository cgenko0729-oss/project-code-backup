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

public class PetIconData : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("ペットの情報")]
    public PetData petData;
    public bool isSelected = false;

    [Header("アイコン表示")]
    public Image petIconImage;
    public Image lockedIconImage;
    public GameObject selectedEffect;
    public TextMeshProUGUI selectedNum;

    [Header("マウスが乗っている時の拡縮情報")]
    [SerializeField] public float scaleChangeTime = 0;
    [SerializeField] public float defaultScale = 1.0f;
    [SerializeField] public float addScale = 0.5f;

    void Update()
    {
        if(isSelected == true && selectedNum != null)
        {
            int num = PetSelectDataManager.Instance.SelectedPets.IndexOf(petData);
            selectedNum.text = (num + 1).ToString();
        }
    }

    public void SetPetData(PetData petData)
    {
        // ペットアイコンの設定
        petIconImage.sprite = petData.petIcon;
        Color iconColor = Color.white;
        if (petData.isUnlocked != true)
        {
            iconColor = Color.black;
        }
        petIconImage.color = iconColor;
        // 未開放ならロックアイコンを表示
        lockedIconImage.gameObject.SetActive(!petData.isUnlocked);

        selectedEffect.SetActive(false);
        var pet = PetSelectDataManager.Instance.SelectedPets.Find(
                    data => data.petType == petData.petType);
        if(pet != null)
        {
            selectedEffect.SetActive(true);
            isSelected = true;
        }

        this.petData = petData;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(defaultScale + addScale, scaleChangeTime);

        EventManager.EmitEvent("ResetPetCameraAngle");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(defaultScale, scaleChangeTime);
    }

    public void OnClicked()
    {
        if(selectedEffect == null) { return; }
        if(petData.isUnlocked != true) { return; }

        

        // 選択中かどうかを切り替える
        isSelected = !isSelected;
        if(isSelected == true)
        {
            if (PetSelectDataManager.Instance.AddPet(petData))
            {
                selectedEffect.SetActive(true);
            }
        }
        else
        {
            PetSelectDataManager.Instance.RemovePet(petData);
            selectedEffect.SetActive(false);
        }
    }
}
