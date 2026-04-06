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

public class TraitGiftAngelAction : MonoBehaviour
{
    public Transform playerTrans;

    public float homingSpeed = 7f;

    public float distWithPlayer = 4f;

    public float fixHeight = 2.1f;

    public bool isGaveGift = false;

    public float lifeTimer = 3f;

    public GameObject giftPrefab;
    public ParticleSystem summonGiftEffect;

    public bool isAngel = false;

    public Collider itemBoxCol;
    public ParticleSystem fallGroundEffect;
    public bool isColEnabled = false;


    void Start()
    {
        if (isAngel)
        {
            playerTrans = GameObject.FindWithTag("Player").transform;
        }
        else
        {
            itemBoxCol = GetComponent<Collider>();
            itemBoxCol.enabled = false;
        }
            

    }

    void Update()
    {

        AngelAction();
        ItemBoxAction();
        
      

        

    }


    void GiveGift()
    {
        Instantiate(giftPrefab, transform.position, Quaternion.identity);
        if (summonGiftEffect) summonGiftEffect.Play();

    }

    void ItemBoxAction()
    {
        if(isAngel) return;
      

        if (!isColEnabled)
        {
            isColEnabled = true;

            //dowtween move posy to 0.0f in 0.5s
            transform.DOMoveY(0.0f, 1f).SetEase(Ease.OutBounce).OnComplete(() =>
            {
                itemBoxCol.enabled = true;
                
            });

            Invoke("BoxEffect", 0.7f);

        }


    }

    void BoxEffect()
    {
        if (fallGroundEffect) fallGroundEffect.Play();

                //PLay a SOund
        SoundEffect.Instance.Play(SoundList.SpiderBossFallGroundSe);
    }

    void AngelAction()
    {
        if (!isAngel) return;
        if (distWithPlayer > 2.5f)
        {
            distWithPlayer = Vector3.Distance(playerTrans.position, transform.position);
            Vector3 dir = (playerTrans.position - transform.position).normalized;
            transform.position += dir * homingSpeed * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, fixHeight, transform.position.z);

            Quaternion lookRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = -90f;
            transform.rotation = Quaternion.Euler(euler);



        }
        else if (!isGaveGift)
        {
            isGaveGift = true;
            GiveGift();
        }
        else if (isGaveGift)
        {

            Vector3 dir = (playerTrans.position - transform.position).normalized;
            transform.position += -dir * (homingSpeed*2) * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, fixHeight, transform.position.z);

            Quaternion lookRotation = Quaternion.LookRotation(-dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = -90f;
            transform.rotation = Quaternion.Euler(euler);

            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f)
            {
                Destroy(gameObject);
            }

        }
    }

}

