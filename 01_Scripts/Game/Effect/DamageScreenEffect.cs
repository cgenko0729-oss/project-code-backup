using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class DamageScreenEffect : MonoBehaviour
{

    public Image damageScreenImg;

    public float damageAmount;

    public float imgAlpha;

    public PlayerState playerStatus;

    void Start()
    {
        playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();
    }

    public void OnEnable()
    {
        EventManager.StartListening(GameEvent.ChangePlayerHp, PlayScreenDamageEffect);
    }

    public void PlayScreenDamageEffect()
    {
        //–³“GƒAƒCƒeƒ€‚ðŽæ“¾‚µ‚Ä‚¢‚éê‡‚Íreturn‚ð•Ô‚·
        if (ItemManager.Instance.pickUpInvincble)
        {
            return;
        }

        damageAmount = EventManager.GetFloat(GameEvent.ChangePlayerHp);
        if (damageAmount >= 0) return;
        if(playerStatus.NowHp <= 0)
        {
            damageScreenImg.DOFade(0f, 0.1f).SetUpdate(true);
            return;
        }

        //if (playerStatus.NowHp >= playerStatus.MaxHp * 0.59f) return;

        damageScreenImg.gameObject.SetActive(true);
        
        damageScreenImg.DOFade(0.77f, 0.28f).SetLoops(4, LoopType.Yoyo).SetUpdate(true).OnComplete(() => {
            damageScreenImg.DOFade(0.0f, 0.42f).SetUpdate(true);
        });



    }

    void Update()
    {
        imgAlpha = damageScreenImg.color.a;


    }
}

