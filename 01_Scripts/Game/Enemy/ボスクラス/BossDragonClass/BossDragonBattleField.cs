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

public class BossDragonBattleField : MonoBehaviour
{

    GameObject fogVoid;
    private Vector3 fogvoidScale = new Vector3(35.9f, 28, 42f);
    public Vector3  fogPos = new Vector3(4.3f, 5.5f, 0.119f);

    void Start()
    {
        //find by tag fogVoid;
        fogVoid = GameObject.FindGameObjectWithTag("FogVoid");

        fogVoid.transform.localScale = fogvoidScale;
        fogVoid.transform.position = fogPos;

    }

    void Update()
    {
        
    }
}

