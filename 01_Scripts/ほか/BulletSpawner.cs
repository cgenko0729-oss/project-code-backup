using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;

using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool



public class BulletSpawner : Singleton<BulletSpawner>
{
    public ObjectPool bulletPool;

    float space = 70;
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) {
            bulletPool.GetObject(new Vector3(Random.Range(-space,space),Random.Range(-space,space),Random.Range(-space,space)));
        }

    }
}

