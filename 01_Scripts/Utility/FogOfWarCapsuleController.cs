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
using VolumetricFogAndMist2;

public class FogOfWarCapsuleController : MonoBehaviour
{

    public VolumetricFog fogVolume;
    public float fogHoleRadius = 8f;
    public float distanceCheck = 1f;
    public float clearDuration = 0.2f;

    public Transform playerTrans;

    public float fogUpdateCnt = 0.3f;

    void Start()
    {
        playerTrans = GameObject.FindGameObjectWithTag("Player").transform;

        fogVolume = GameObject.FindGameObjectWithTag("FogOfWar").GetComponent<VolumetricFog>();

    }

    void Update()
    {

        fogUpdateCnt -= Time.deltaTime;

        transform.position = playerTrans.position;

        if (fogUpdateCnt <= 0)
        {
            fogUpdateCnt = 0.3f;
            fogVolume.SetFogOfWarAlpha(transform.position, radius: fogHoleRadius, fogNewAlpha: 0, duration: clearDuration);
            //Debug.Log("FogReset");
        }


    }




    //public VolumetricFog fogVolume;

    //[Tooltip("The radius of the clearing circle, as a percentage of the fog volume's width.")]
    //[Range(0.1f, 1.0f)]
    //public float fogHoleRadiusPercentage = 0.4f; // We'll clear 40% of the width at a time

    //public float distanceCheck = 1f;
    //public float clearDuration = 0.2f;

    //public Transform playerTrans;
    //public float fogUpdateCnt = 0.3f;

    //void Start()
    //{
    //    playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
    //}

    //void Update()
    //{
    //    fogUpdateCnt -= Time.deltaTime;
    //    transform.position = playerTrans.position;

    //    if (fogUpdateCnt <= 0)
    //    {
    //        fogUpdateCnt = 0.3f;

    //        // Calculate the actual radius in world units based on the fog volume's current size
    //        float actualRadius = fogVolume.fogOfWarSize.x * fogHoleRadiusPercentage;

    //        fogVolume.SetFogOfWarAlpha(transform.position, radius: actualRadius, fogNewAlpha: 0, duration: clearDuration);
    //        Debug.Log("FogReset");
    //    }
    //}






}

