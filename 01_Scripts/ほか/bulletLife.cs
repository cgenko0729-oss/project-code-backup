using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool






public class bulletLife : MonoBehaviour
{
    public float lifeTime = 5;
    void Start()
    {
        
    }

    void Update()
    {
        lifeTime-= Time.deltaTime;

       if(lifeTime <= 0) {
            lifeTime = 5;
            BulletSpawner.Instance.bulletPool.Release(this.gameObject);
        }

    }
}

