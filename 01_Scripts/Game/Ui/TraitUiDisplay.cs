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
using UnityEngine.InputSystem;

public class TraitUiDisplay : MonoBehaviour, IPointerClickHandler,ISubmitHandler
{
    public TraitData traitData;
    public TraitType traitType;
    public TextMeshProUGUI traitName;
    public TextMeshProUGUI traitDescription;
    public Image traitIcon;
    public Image isUnlockMessage;

    public bool isClickable = true;
    public bool isCloseOnInput = false;

    public bool isInActive = false;

    public List<TraitUiRelatedTraitDisplay> relatedTraitList;

    private void OnEnable()
    {
        if (isInActive) return;
        EventManager.StartListening(GameEvent.LanguageChanged, ResetCard);
    }

    private void OnDisable()
    {
         if (isInActive) return;
        EventManager.StopListening(GameEvent.LanguageChanged, ResetCard);
    }

     public void DeactivateOnAnyPlayerInput()
    {
        if (!isInActive)
        {
            if(!isCloseOnInput) return;
            if (TraitCardViewManager.Instance.viewCooldown > 0) return;

            //if ((Input.anyKeyDown || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))&& !Input.GetMouseButtonDown(0))      
            if ((Input.anyKeyDown || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))      
            {
                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false); // deactivate this slot display
                    TraitCardViewManager.Instance.viewCooldown = 0.42f;
                }
                
            }
        }
        else
        {
            if ((Input.anyKeyDown || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))      
            {
                
               
                

                //Debug.Log("DeactivateOnAnyPlayerInput called");
                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false); // deactivate this slot display
                    //TraitCardViewManager.Instance.viewCooldown = 0.42f;
                }
                
            }

            if(Gamepad.current != null)
            {
                //if any controller input detected then return:
                //if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                //    Gamepad.current.buttonNorth.wasPressedThisFrame ||
                //    Gamepad.current.buttonEast.wasPressedThisFrame ||
                //    Gamepad.current.buttonWest.wasPressedThisFrame ||
                //    Gamepad.current.leftShoulder.wasPressedThisFrame ||
                //    Gamepad.current.rightShoulder.wasPressedThisFrame ||
                //    Gamepad.current.leftTrigger.wasPressedThisFrame ||
                //    Gamepad.current.rightTrigger.wasPressedThisFrame ||
                //    Gamepad.current.dpad.up.wasPressedThisFrame ||
                //    Gamepad.current.dpad.down.wasPressedThisFrame ||
                //    Gamepad.current.dpad.left.wasPressedThisFrame ||
                //    Gamepad.current.dpad.right.wasPressedThisFrame ||
                //    Gamepad.current.leftStickButton.wasPressedThisFrame ||
                //    Gamepad.current.rightStickButton.wasPressedThisFrame)

                     if (Gamepad.current.buttonSouth.wasPressedThisFrame ||Gamepad.current.buttonEast.wasPressedThisFrame)
                {
                    if (gameObject.activeSelf)
                    {
                        gameObject.SetActive(false); // deactivate this slot display
                        //TraitCardViewManager.Instance.viewCooldown = 0.42f;
                    }
                }
            }

        }
        

    }


    private void Start()
    {
         if (isInActive) return;
        if (traitData != null)
        {
            traitName.text = L.TraitName(traitData.traitType);
            traitDescription.text = L.TraitDesc(traitData.traitType);
            traitIcon.sprite = traitData.icon;
            traitType = traitData.traitType;

        }
    }

    public void Update()
    {
         //if (isInActive) return;
        //if press R    
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    if (traitData != null)
        //    {
        //        traitName.text = L.TraitName(traitData.traitType);
        //        traitDescription.text = L.TraitDesc(traitData.traitType);
        //        traitIcon.sprite = traitData.icon;
        //        traitType = traitData.traitType;

        //    }
        //}

        DeactivateOnAnyPlayerInput();
        

    }

    public void ResetCard()
    {
        if (traitData != null)
        {
            traitName.text = L.TraitName(traitData.traitType);
            traitDescription.text = L.TraitDesc(traitData.traitType);
           
            foreach (TraitUiRelatedTraitDisplay relatedTrait in relatedTraitList)
            {
                relatedTrait.SetUpRelatedTraitInfo(traitData);
            }
        }

    }

    public void SetUpCard(TraitData data)
    {
         if (isInActive) return;
        traitData = data;
        if (traitData != null)
        {
            traitName.text = L.TraitName(traitData.traitType);
            traitDescription.text = L.TraitDesc(traitData.traitType);
            traitIcon.sprite = traitData.icon;
            traitType = traitData.traitType;
            if (traitData.isTraitUnlocked)
            {


                isUnlockMessage.gameObject.SetActive(false);
                Button btn = GetComponent<Button>();
                btn.interactable = true;
            }
            else
            {

                isUnlockMessage.gameObject.SetActive(true);
                Button btn = GetComponent<Button>();
                btn.interactable = false;
                traitDescription.DOFade(0.0f, 0.1f);
               
                //isUnlockMessage.gameObject.SetActive(false);
                //Button btn = GetComponent<Button>();
                //btn.interactable = true;
            }

            foreach (TraitUiRelatedTraitDisplay relatedTrait in relatedTraitList)
            {
                relatedTrait.SetUpRelatedTraitInfo(traitData);
            }

        }
    }

        [ContextMenu("Update UI Display")]
    private void UpdateUIDisplayInEditor()
    {
        traitName.text = L.TraitName(traitData.traitType);
            traitDescription.text = L.TraitDesc(traitData.traitType);
            traitIcon.sprite = traitData.icon;
            traitType = traitData.traitType;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!traitData.isTraitUnlocked) return;
         if (isInActive) return;
        if(!isClickable) return;
        if (TraitCardViewManager.Instance.viewCooldown > 0) return;

        TraitCardViewManager.Instance.ShowDetailCard(traitData);

    }

    public void OnSubmit(BaseEventData eventData)
    {
        if (!traitData.isTraitUnlocked) return;
        if (isInActive) return;
        if(!isClickable) return;
        if (TraitCardViewManager.Instance.viewCooldown > 0) return;

        TraitCardViewManager.Instance.ShowDetailCard(traitData);
    }




}

