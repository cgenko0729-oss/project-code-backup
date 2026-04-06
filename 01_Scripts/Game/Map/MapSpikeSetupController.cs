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

public class MapSpikeSetupController : MonoBehaviour
{

    public float dmgToPlayer = -10f;

    public float startPosY;
    public float movePosYTo = -0.42f;

    public GameObject spikeObj;

    void Start()
    {
        startPosY = spikeObj.transform.localPosition.y;

    }

    void Update()
    {
        
    }


    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            EventManager.EmitEventData(GameEvent.ChangePlayerHp, dmgToPlayer);

            MoveSpikeTrapUpAndDownDotween();
            
        }
    }

    public void MoveSpikeTrapUpAndDownDotween()
    {
        spikeObj.transform.DOLocalMoveY(movePosYTo, 0.2f).OnComplete(() => {
            spikeObj.transform.DOLocalMoveY(startPosY, 0.2f).SetDelay(0.5f);
        });
    }

}

