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

public class MidBossHpBar : MonoBehaviour
{
    public GameObject spawnedHpBar;
    public GameObject hpbarObject;
    public EnemyStatusBase enemyStatus;

    //public Canvas mainCanvas;
    public GameObject canvasBottomTrans;

    void Start()
    {
        enemyStatus = GetComponentInParent<EnemyStatusBase>();
        
        canvasBottomTrans = GameObject.FindGameObjectWithTag("UiCanvasBottom");

        spawnedHpBar = Instantiate(hpbarObject, canvasBottomTrans.transform);
        HpBar3D hpBar3D = spawnedHpBar.GetComponent<HpBar3D>();
        hpBar3D.enemyStatus = enemyStatus;
        hpBar3D.headTran = transform;
        


    }

    void Update()
    {
       

    }

    //OnDestroy
    private void OnDestroy()
    {
        if(spawnedHpBar != null)spawnedHpBar.SetActive(false);
    }

}

