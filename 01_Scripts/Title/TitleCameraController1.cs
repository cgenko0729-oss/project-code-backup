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

public class TitleCameraController : MonoBehaviour
{
    public GameObject[] cameraTargetList;
    public PlayerData playerData;

    private int nowJobId;
    private int prevJobId;
    private Transform defalutTrans;

    void Start()
    {
        defalutTrans = transform;
        nowJobId = (int)playerData.jobId;
        prevJobId = nowJobId;
    }

    void Update()
    {
        
    }
}

