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

public class PetGetMessageUiView : MonoBehaviour
{
    public Image petIconImage;
    public TextMeshProUGUI petMessageText;
    public CanvasGroup cg;

    private Vector2 startPos  = new Vector2(-700, 70);
    private Vector2 endPos    = new Vector2(-700, 210);

    void Start()
    {
        if (!petMessageText) petMessageText = GetComponent<TextMeshProUGUI>();
        if (!cg) cg = GetComponent<CanvasGroup>();
        transform.localPosition = startPos;

        //dotween move and fade then destroy
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOLocalMove(endPos, 2.8f).SetEase(Ease.Linear));

        seq.OnComplete(() => {
            cg.DOFade(0, 2.1f).OnComplete(() => {
                Destroy(gameObject);
            });



        });



    }

    void Update()
    {
        
    }

    public void SetPetMessageText(string message, Sprite iconImg)
    {
        petMessageText.text = message;
        petIconImage.sprite = iconImg;
    }

}

