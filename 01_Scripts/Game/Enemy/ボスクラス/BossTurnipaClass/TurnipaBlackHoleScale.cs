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

public class TurnipaBlackHoleScale : MonoBehaviour
{
    public bool startScaling = true;
    public bool startFalling = false;
    public bool finishedFalling = false;

    public float scaleTimer = 3f;
    public float scaleSpeed = 0.5f;



    public Vector3 fallTarget = new Vector3(0, 0, 0);

    public Vector3 scaleTarget = new Vector3(2.8f, 2.8f, 2.8f); // Target scale for the black hole

    public GameObject blackHoleExplodeObject;

    void Start()
    {

    }

    void Update()
    {
        
        ScaleBlackHole();
        FallBlackHole();

    }


    void ScaleBlackHole()
    {
        scaleTimer -= Time.deltaTime;
        if (scaleTimer <= 0 && !startFalling)
        {

            //DOMove to posy = 5.6
            transform.DOMoveY(5.6f, 4.9f).SetEase(Ease.Linear);

            //Dotween scale to targetScale within 3 seconds
            transform.DOScale(scaleTarget, 4.9f).OnComplete(() => {
                startFalling = true; // Start falling after scaling is complete
            });

        }

    }

    void FallBlackHole()
    {
        if (startFalling && !finishedFalling)
        {
            finishedFalling = true;

            //Dotween move to fallTarget position within 3 seconds
            transform.DOMove(fallTarget, 0.5f).OnComplete(() => {
                Destroy(gameObject);
   
                GameObject explodeObj = Instantiate(blackHoleExplodeObject,new Vector3(0,0.56f,0), Quaternion.identity);
                SoundEffect.Instance.Play(SoundList.TurnipaBossBigBombSe);
                CameraShake.Instance.StartTurnipaJumpShake();

            });


        }

    }

}

