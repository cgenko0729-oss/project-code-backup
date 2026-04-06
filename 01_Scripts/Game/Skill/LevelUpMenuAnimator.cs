using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class LevelUpMenuAnimator : Singleton<LevelUpMenuAnimator>
{
    public CanvasGroup blocker;         
    public LevelUpCard[] cards;         

    public void Open()
    {
        gameObject.SetActive(true);
        blocker.alpha = 0;
        blocker.DOFade(1f, 0.21f).SetUpdate(true);
        blocker.blocksRaycasts = true;
        for (int i = 0; i < cards.Length; ++i)
        {
            cards[i].Show(i);

        }
    }

    public void OpenOneElement(int id)
    {
        gameObject.SetActive(true);
        blocker.alpha = 0;
        blocker.DOFade(1f, 0.21f).SetUpdate(true);
        blocker.blocksRaycasts = true;
        cards[id].Show(id);


    }

    public void Close()
    {
        //foreach (var c in cards)  c.Hide();
        //blocker.DOFade(0, 0.2f).SetUpdate(true).OnComplete(() => 
        gameObject.SetActive(false);
        blocker.blocksRaycasts = false;
    }
}

