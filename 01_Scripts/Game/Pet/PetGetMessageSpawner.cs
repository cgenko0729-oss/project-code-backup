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

public class PetGetMessageSpawner : Singleton<PetGetMessageSpawner>
{
    public GameObject petMessagePrefab;

    public RectTransform uiCanvasTrans;



    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SpawnPetMessagObj(PetType type)
    {
        //debug log 
        Debug.Log("SpawnPetMessagObj: " + L.PetName(type));

        GameObject petMsg = Instantiate(petMessagePrefab, transform.position, Quaternion.identity, uiCanvasTrans);
        PetGetMessageUiView petMsgView = petMsg.GetComponent<PetGetMessageUiView>();
        PetData targetData = PetGetChanceManager.Instance.masterPetDataBase.allPetData.Find(x => x.petType == type);
        petMsgView.SetPetMessageText(L.PetName(type),targetData.petIcon);

    }

}

