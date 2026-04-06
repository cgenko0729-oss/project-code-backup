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

public class SkillBlackHoleAbsorb : MonoBehaviour
{

    public float absorbCnt = 3f;
    public float absorCntMax = 2.8f;

    public float absorbPower = 3.5f; //The power(distance and amount) to pull enemies toward the center

    public SphereCollider col;

    public void Start()
    {
        col = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        absorbCnt -= Time.deltaTime;
        if (absorbCnt <= 0)
        {
            absorbCnt = absorCntMax;

            //SPhereCast to detect enemies layer :

            Vector3 centerPos = transform.position;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, col.radius * 1.35f, LayerMask.GetMask("EnemySpider"));
            foreach (var hitCollider in hitColliders)
            {
                //Move the enemy toward the center of the black hole with absorbPower amount with dotween  : 
                //Vector3 moveTarget = (centerPos - hitCollider.transform.position).normalized * absorbPower;
                //hitCollider.transform.DOMove(hitCollider.transform.position + moveTarget, 0.5f).SetEase(Ease.InOutSine);

                Vector3 enemyPosition = hitCollider.transform.position;
                Vector3 finalDestination = Vector3.MoveTowards(enemyPosition, centerPos, absorbPower);
                hitCollider.transform.DOMove(finalDestination, 0.5f).SetEase(Ease.InOutSine);

            }

        }


    }

    public void AbsorbEnemyTowardCeter()
    {
        
    }

        //OnTriggerEnter with EnemyTag
        private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            
        }
    }

}

