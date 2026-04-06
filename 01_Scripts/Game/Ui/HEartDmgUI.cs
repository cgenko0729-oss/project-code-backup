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

public class HEartDmgUI : MonoBehaviour
{
    private GameObject Player;
    
    public Image DmgWhite;
    
  
    float PlayerHp;

    float DefaultHpDmg = 0.06f;

    public float MaxHp;
    public float OldHp;
    public float AfterHp;
    public float damage;
    void OnEnable()
    {
        EventManager.StartListening(GameEvent.ChangePlayerHp,DmgAnimation);
    }
    
    public void DmgAnimation()
    {
        OldHp = PlayerHp;
        //Debug.Log("oldHp" + OldHp);

        damage = EventManager.GetFloat(GameEvent.ChangePlayerHp);
        damage *= (1 - BuffManager.Instance.gobalPlayerDefenceAdd/100);
        AfterHp = OldHp + damage;

        //Debug.Log("AfterHp" + AfterHp);

        DmgWhite.fillAmount = OldHp / MaxHp;

        //Debug.Log("fillAmount" + DmgWhite.fillAmount);
       
        //DmgWhite.DOFillAmount(AfterHp / MaxHp, 0.7f);

        Invoke("StartHpAnimation", 0.01f);
    }

    public void StartHpAnimation()
    {
        DmgWhite.color = new Color(1, 1, 1, 1);

        DmgWhite.DOFillAmount(AfterHp / MaxHp, -damage * DefaultHpDmg).OnComplete( 
            ()=>{
                DmgWhite.color = new Color(1, 1, 1, 0);
            });
    }
    void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        
        DmgWhite = GetComponent<Image>();
    }

    void Update()
    {
        MaxHp = Player.GetComponent<PlayerState>().MaxHp;

        PlayerHp = Player.GetComponent<PlayerState>().NowHp;
    }
}

