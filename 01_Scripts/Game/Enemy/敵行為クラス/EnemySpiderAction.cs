using UnityEngine;

public class EnemySpiderAction : EnemyActionBase
{

    public bool isEnableHoming = true;
   
    protected override void Start()
    {
        InitSeperationInfo();
        GetPlayerInfo();

        ResetHeight();

    }

    public void OnEnable()
    {
        ResetHeight();

        //if(isIceDebuff)currentAnimState.speed = 1f;
        //isIceDebuff = false;
        //isPoisonDebuff = false;
        //poisonSpeedDownFactor = 1f;
        //iceDebuffFactor = 1f;

        isSeperateEnabled = true;
        
    }

    protected override void Update()
    {
        if(!isEnableHoming) return; 

        EnemyFollow();
        EnemySeparation();
        EnemyRotation();
        UpdateDistToPlayerInXZ();
        //UpdateSpeedDebuff();
        //UpdateIceDebuff();
    }

    public void ResetHeight()
    {
        Vector3 pos = transform.position;
        pos.y = -0.149f; 
        transform.position = pos;
    }

}
