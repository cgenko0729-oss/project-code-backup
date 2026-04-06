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

public class UiStatusMenuTraitDescDisplay : MonoBehaviour
{
    public GameObject scrollViewObj;
    public  Transform contentContainer;

    public GameObject traitUiObj;

    public List<int> allCasterIndex;

    public float fixY = -140f;

    

    private void OnEnable()
    {
        EventManager.StartListening(GameEvent.OnActiveSkillDataWindow,OnPopulateTraitDescMenu);
        EventManager.StartListening(GameEvent.OnInactiveSkillDataWindow,OnCloseTraitDescMenu);
    }

    private void OnDisable()
    {
        EventManager.StopListening(GameEvent.OnActiveSkillDataWindow,OnPopulateTraitDescMenu);
        EventManager.StopListening(GameEvent.OnInactiveSkillDataWindow,OnCloseTraitDescMenu);

    }

    void OnPopulateTraitDescMenu()
    {

        //allCasterIndex = activeSkillCasterCollections's Active item 's index to a list 
        allCasterIndex.Clear();
        for (int i = 0; i < SkillManager.Instance.activeSkillCasterCollections.Count; i++)
        {
            if (SkillManager.Instance.activeSkillCasterCollections[i].isActivated)
            {
                allCasterIndex.Add(i);
            }
        }

        //debug log the whole int list 


        scrollViewObj.gameObject.SetActive(true);

        int skillIndex = EventManager.GetInt(GameEvent.OnActiveSkillDataWindow);

        int indexInAllCasterList = allCasterIndex.IndexOf(skillIndex);

        //Debug.Log("All Caster Index List: " + string.Join(", ", allCasterIndex));
        //Debug.Log("Populate Trait Desc Menu for Skill Index: " + skillIndex);
        //Debug.Log("Index in All Caster List: " + indexInAllCasterList);

        if(indexInAllCasterList <= 1)
        {
           //scrollViewObj recttransform y to 276:
              scrollViewObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(470, fixY);
        }
        else
        {
            //scrollViewObj recttransform y to 0:
            scrollViewObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(470, fixY);
        }

        var skillCaster = SkillManager.Instance.activeSkillCasterCollections[skillIndex];

        int traitCount = skillCaster.traitDataHoldingList.Count;
        for (int i = 0; i < traitCount; i++)
        {
            var traitData = skillCaster.traitDataHoldingList[i];
            GameObject traitUiInstance = Instantiate(traitUiObj, contentContainer);
            UiStatusMenuTraitUiObj traitUi = traitUiInstance.GetComponent<UiStatusMenuTraitUiObj>();
            traitUi.SetUp(traitData.traitType);
        }

    }

    void OnCloseTraitDescMenu()
    {
        scrollViewObj.gameObject.SetActive(false);

        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

    }


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

