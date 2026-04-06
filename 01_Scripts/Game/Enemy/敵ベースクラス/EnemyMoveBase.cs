using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool

public class EnemyMoveBase : MonoBehaviour
{

    [Header("Separation (Avoid Stacking)")]
    public float separateRadius  = 0.5f;   
    public float seperatePower = 14f;
    public LayerMask sperateLayer;   

    public bool isSeperateEnabled = true;
    
    public const int maxNeighbors = 14;
    Collider[] neighborsCols = new Collider[maxNeighbors];

    int frameCounter;
    [Range(1,5)] public int seperatePerN = 1;

    float fixedY;
    float r2, invR2;

    [Header("Homing")]
    public float moveSpd = 3f;
    Transform playerTrans;
    public bool isSimpleHoming = true;

    Vector2 move2D;
    Vector2 self;

    public float distToPlayer;
    public float distNeedToHoming = 28f;


    private void Awake()
    {      
        

       
        
    }

    protected virtual void Start()
    {
        InitSeperationInfo();
        InitHomingInfo();

    }

    protected virtual void Update()
    {
        EnemyFollow();
        EnemySeparation();
        EnemyRotation();
    }

    protected void InitSeperationInfo()
    {
        fixedY = transform.position.y;    
        r2     = separateRadius * separateRadius;   
        invR2  = 1f / r2;
    }

    protected void InitHomingInfo()
    {
         var p = GameObject.FindWithTag("Player");
        if (p) playerTrans = p.transform;
    }

    private void EnemyFollow()
    {
        if (!playerTrans) return;
        if(!isSimpleHoming) return;

        self = new Vector2(transform.position.x, transform.position.z);
        Vector2 player = new Vector2(playerTrans.position.x, playerTrans.position.z);
        Vector2 toPlayer = (player - self).normalized;
        move2D   = toPlayer * moveSpd * Time.deltaTime;
    }

    private void EnemyRotation()
    {
        Vector3 dir = (new Vector2(playerTrans.position.x, playerTrans.position.z) - new Vector2(transform.position.x, transform.position.z)).normalized;
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(dir.x, 0, dir.y));
        if (dir != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, 5f * Time.deltaTime);

    }

    private void EnemySeparation()
    {
        if (!isSeperateEnabled) return;

        frameCounter = (frameCounter + 1) % seperatePerN;
        if (frameCounter == 0) move2D += ComputeSeparationXZ(self) * Time.deltaTime;
        Vector3 next = new Vector3(self.x + move2D.x,fixedY,self.y + move2D.y);
        transform.position = next;
    }

    Vector2 ComputeSeparationXZ(Vector2 me) 
    {
        Vector2 sep = Vector2.zero;
        int count = Physics.OverlapSphereNonAlloc(new Vector3(me.x, fixedY, me.y), separateRadius, neighborsCols, sperateLayer, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++) {
          var c = neighborsCols[i];
          if (c.gameObject == gameObject) continue;
          Vector3 cp = c.transform.position;
          Vector2 other = new Vector2(cp.x, cp.z);
          Vector2 off   = me - other;
          float d2      = off.sqrMagnitude;
          if (d2 > 0f && d2 < r2) {
            // approximate (r² - d²)/r² falloff
            float factor = (r2 - d2) * invR2;
            sep += off * (factor * seperatePower * 0.5f);
          }
        }

        return sep;
    }

}

