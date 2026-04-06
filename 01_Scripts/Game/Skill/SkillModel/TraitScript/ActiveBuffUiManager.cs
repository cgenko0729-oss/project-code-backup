using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class ActiveBuffUiManager : Singleton<ActiveBuffUiManager>
{

    [SerializeField] private GameObject buffDisplayPrefab;
    [SerializeField] private Transform buffPanel; 

    private Dictionary<TraitType, GameObject> activeBuffDisplays = new Dictionary<TraitType, GameObject>(); // Dictionary to hold references to the instantiated UI objects for each buff type

    public void UpdateBuffDisplay(TraitType buffType, int stackCount, Sprite icon, bool isStackable,bool isTriggerFrameEffect = false)
    {
        GameObject buffDisplayInstance;
        


        if (activeBuffDisplays.ContainsKey(buffType))
        {
            buffDisplayInstance = activeBuffDisplays[buffType];
        }
        else // If not, create a new one
        {
            buffDisplayInstance = Instantiate(buffDisplayPrefab, buffPanel);
            buffDisplayInstance.GetComponent<BuffUiObject>().Icon.sprite = icon;
            //buffObj.Icon.sprite = icon;
            activeBuffDisplays.Add(buffType, buffDisplayInstance);
        }

        

        BuffUiObject buffObj = buffDisplayInstance.GetComponent<BuffUiObject>();

        Image buffFrameImg = buffObj.Frame;
        if (isTriggerFrameEffect)
        {
            //set color of buffFrameImg to (175,175,175,255)
            buffFrameImg.color = new Color32(175, 175, 175, 255);

            //fade the color of buffFrameImg from (175,175,,175,255) to (255,255,255,255) in 0.2 seconds and back to (175,175,175,255) in 0.2 seconds
            buffFrameImg.DOColor(new Color32(225, 219, 0, 255), 0.2f).OnComplete(() => {
                buffFrameImg.DOColor(new Color32(175, 175, 175, 255), 0.2f);
            });

        }

        if (stackCount > -1) //0
        {
            buffDisplayInstance.SetActive(true);
            TextMeshProUGUI stackText = buffObj.StackText;
            if (stackText != null)
            {
                stackText.text = stackCount.ToString();
                if(!isStackable) stackText.text = "";// Hide stack text if not stackable
            }

        }

        if(stackCount == 0)
        {
            //image alpha to 0.5f
            Image buffImage = buffObj.Icon;
            Color tempColor = buffImage.color;
            tempColor.a = 0.28f;
            buffImage.color = tempColor;
            buffObj.Frame.color = tempColor;

        }
        else
        {
            //image alpha to 1f
            Image buffImage = buffObj.Icon;
            Color tempColor = buffImage.color;
            tempColor.a = 1f;
            buffImage.color = tempColor;
            //buffObj.Frame.color = tempColor;

        }

        //else buffDisplayInstance.SetActive(false);

    }


    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

