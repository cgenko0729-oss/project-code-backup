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

public class TraitDoubleCastAnim : MonoBehaviour
{

    public float animCdCnt = 0.35f;

    private void OnEnable()
    {
        EventManager.StartListening("OnPlayDoubleCastAnim", DoDoubleCastAnim);
    }

    private void OnDisable()
    {
        EventManager.StopListening("OnPlayDoubleCastAnim", DoDoubleCastAnim);
    }

    void Start()
    {
        
    }

    public Vector3 punchRotationAmount = new Vector3(30f, 0f, 0f); //30 about X
    public float punchDuration        = 0.2f;
    public int   punchVibrato         = 10;   
    public float punchElasticity      = 0.5f; 

    void Update()
    {
        ////if press T key
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //   transform.DOPunchRotation(punchRotationAmount,punchDuration,vibrato: punchVibrato,elasticity: punchElasticity).SetEase(Ease.OutQuad);
        //}

        animCdCnt -= Time.deltaTime;

    }

    void DoDoubleCastAnim()
    {

        if(animCdCnt <= 0f)
        {
             animCdCnt = 0.35f;
            transform.DOPunchRotation(punchRotationAmount,punchDuration,vibrato: punchVibrato,elasticity: punchElasticity).SetEase(Ease.OutQuad);

        }
       
    }

}

