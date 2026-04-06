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

public class BossMagicBallCircleMove : MonoBehaviour
{
    
    public Transform target;  // 円運動の中心,通常は Player        
    public float phase;       // 現在角度 [rad]        
    public float omega;       // 角速度 [rad/s]           
    public float radius;      // 半径
    public float height;      // Y 偏差（例: 空中に浮かせたい時）

    public float circleTime = 10f;

    public bool isActivated = true; //有効か　
    public bool isFacingOutward = false; // 外向きかどうか
    
    [SerializeField]private float yawOffset = 0f; // 90, -90, 180, etc. set per prefab
    [SerializeField] private float pitchOffset = 0f; // 0, 30, 45, etc. set per prefab
    public Vector3 radialDir;

    Transform playerTrans;
    public float homingPlayerCnt = 3.5f;
    private float homingPlayerSpeed = 4.9f;

    public Vector3 finalFacingDir;
    public float finalHomingCnt = 3.5f;


    public void Start()
    {
       playerTrans = GameObject.FindGameObjectWithTag("Player").transform;
    }
    public void Init(Transform target, float phase, float revolutionsPerSecond, float radius, float circleTime)
    {
        this.target  = target;
        this.phase   = phase;
        this.omega   = revolutionsPerSecond * 2f * Mathf.PI; 
        this.radius  = radius;
        this.circleTime = circleTime;

    }

    void Update()
    {
        circleTime -= Time.deltaTime; 

        if (circleTime <= 0)
        {
            if(homingPlayerCnt > 0f)
            {
                homingPlayerCnt -= Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, playerTrans.position, homingPlayerSpeed * Time.deltaTime);

                Vector3 pos = transform.position;
                pos.y = height;
                transform.position = pos;

                finalFacingDir = (playerTrans.position - transform.position).normalized;
            }
            else
            {
                finalHomingCnt -= Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, finalFacingDir, homingPlayerSpeed * Time.deltaTime);
                Vector3 pos = transform.position;
                pos.y = height;
                transform.position = pos;

                if (finalHomingCnt <= 0f) Destroy(gameObject); 
            }


        }
        else
        {

            if(!isActivated && isFacingOutward)
            {
                radialDir = new(Mathf.Cos(phase), 0f, Mathf.Sin(phase));
                transform.rotation = Quaternion.LookRotation(radialDir, Vector3.up) * Quaternion.Euler(0f, yawOffset, pitchOffset + 90f);
            }


            if(!isActivated) return; //有効でない場合は何もしない

            phase += omega * Time.deltaTime;

            Vector3 offset = new Vector3(Mathf.Cos(phase), 0f, Mathf.Sin(phase)) * radius;
            offset.y = height;

            transform.position = target.position + offset;

            if (isFacingOutward)
            {
                radialDir = new(Mathf.Cos(phase), 0f, Mathf.Sin(phase));
                transform.rotation = Quaternion.LookRotation(radialDir, Vector3.up) * Quaternion.Euler(0f, yawOffset, pitchOffset);
            }

        }

        
        

    }

}

