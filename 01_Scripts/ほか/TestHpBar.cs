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

public class TestHpBar : MonoBehaviour
{

    public Image Dmg;

    PlayerState playerStatus;

    public float oldHp; // プレイヤーがダメージを受ける前のHP
    public float afterHp; // プレイヤーがダメージを受けた後のHP

    public float MaxHp;

    void OnEnable()
    {
        EventManager.StartListening(GameEvent.ChangePlayerHp,DmgAnimation );

    }

    public void DmgAnimation()
    {
        oldHp = playerStatus.NowHp; // 現在のHPを取得
        
        Debug.Log("oldHp" + oldHp);

        float damage = EventManager.GetFloat(GameEvent.ChangePlayerHp);

        afterHp = oldHp + damage;

        Debug.Log("afterHp" + afterHp);

    
        //Dmg.fillAmount = afterHp / MaxHp;
        
        Dmg.fillAmount = oldHp/MaxHp;

        Invoke("StartHpAnimation", 0.5f);

         Debug.Log("fillAmount = " + Dmg.fillAmount);
    }

    void StartHpAnimation()
    {
        Dmg.DOFillAmount((afterHp/MaxHp), 0.5f).SetEase(Ease.Linear);
    }

    void Start()
    {
       //find player tag
       playerStatus = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>();


        //Dotween Dmg.fillAmount from 1 to 0 in 0.5 seconds

        //Dmg.DOFillAmount(0.3f, 0.5f).SetEase(Ease.Linear).OnComplete(() => {
        //    //Debug.Log("Dmg.fillAmount = 1f");
        //    //Dmg.fillAmount = 0f;
        //});

        Dmg = GetComponent<Image>();

    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.U))
        {
            Dmg.fillAmount = 1f;
            Dmg.DOFillAmount(0.3f, 0.5f).SetEase(Ease.Linear);
        }

    }
}

