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

public class PetSkillUIController : MonoBehaviour
{
    [Header("追加するアイコングループのPrefab")]
    public GameObject petSkillIconPrefab;

    public Sprite[] numberKeySprites;
    public Sprite[] dirButtonSprites;

    private void OnEnable()
    {
        EventManager.StartListening("isGameOver", EnableUI);
    }

    private void OnDisable()
    {
        EventManager.StopListening("isGameOver", EnableUI);
    }

    void Start()
    {
        var selectedPets = PetSelectDataManager.Instance.SelectedPets;
        int selectedNum = 0;
        foreach(var pet in selectedPets)
        {
            if(pet == null)
            {
                Debug.Log(pet.petName + "のデータがありません") ;
                continue;
            }

            GameObject iconGroup = Instantiate(petSkillIconPrefab, transform);
            PetSkillIconData iconData = iconGroup.GetComponent<PetSkillIconData>();
            iconData.SetPetSkillData(pet, numberKeySprites[selectedNum], dirButtonSprites[selectedNum]);

            selectedNum++;
        }
    }

    private void EnableUI()
    {
        this.gameObject.SetActive(false);
    }
}

