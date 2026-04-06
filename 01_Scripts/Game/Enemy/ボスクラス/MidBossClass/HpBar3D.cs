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

public class HpBar3D : MonoBehaviour
{

    public EnemyStatusBase enemyStatus;
    public float enemyHp;

    public Camera mainCam;

    public Transform headTran;
    public Vector3 headOffset;

    public Image hpbarImage;
    public Image hpbarFrameImage;

    void Start()
    {

        mainCam = Camera.main;

    }

    void LateUpdate()
    {
        if (!enemyStatus) return;
        enemyHp = enemyStatus.enemyHp;

        Vector3 worldPos = headTran.position;
        worldPos += headOffset;
        Vector3 screenPos = mainCam.WorldToScreenPoint(worldPos);

        transform.position = screenPos;

        float healthRatio = enemyHp / enemyStatus.enemyMaxHp;
        hpbarImage.fillAmount = healthRatio;

    }
}

