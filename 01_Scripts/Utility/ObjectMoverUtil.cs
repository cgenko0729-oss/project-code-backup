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

public class ObjectMoverUtil : MonoBehaviour
{
    public Vector3 movePosStart;
    public Vector3 movePosEnd;
    public float moveTime = 0.7f;

    public bool isMoveRot = false;

    void Start()
    {
        //movePosStart = transform.position;

        transform.position = new Vector3(transform.position.x, movePosStart.y, transform.position.z);


        //rotation.x = -90
        if (isMoveRot)
        {
            transform.rotation = Quaternion.Euler(-90, 0, 0);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }



        //dotween move pos from movePosStart to movePosEnd in moveTime seconds
        transform.DOMove(movePosEnd, moveTime)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                //transform.position = movePosStart;
            });

    }

    private void OnEnable()
    {
        transform.position = new Vector3(transform.position.x, movePosStart.y, transform.position.z);




        //dotween move pos from movePosStart to movePosEnd in moveTime seconds
        transform.DOMove(movePosEnd, moveTime)
            .SetEase(Ease.Linear)
            .OnComplete(() => {
                //transform.position = movePosStart;
            });
        
    }

    void Update()
    {
        
    }
}

