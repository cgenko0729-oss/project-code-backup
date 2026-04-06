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

public class TraitStrawManAction : MonoBehaviour
{

    public float lifeTimer = 0f;
    private float lifeTimerMax = 28f;

    public float attackRange = 7f;

    private float attackDamage = 99f;

    public float attackInterval = 0.77f;
    public float attackTimer = 4.2f;

    public SphereCollider attackCol;
    public ParticleSystem attackEffect;
    public ParticleSystem endeffect; 
    public bool isEndEffectPlayed = false;

    private void OnEnable()
    {
        EventManager.StartListening(GameEvent.PlayerGetDamage, DoAttack);   

    }

    private void OnDisable()
    {
        EventManager.StopListening(GameEvent.PlayerGetDamage, DoAttack);
    }



    void Start()
    {
        attackCol = GetComponent<SphereCollider>();
        lifeTimer = lifeTimerMax;

        attackTimer = 0.35f;
    }

    void Update()
    {

        UpdateAttack();
        UpdateLife();

    }

    void UpdateAttack()
    {

        attackTimer -= Time.deltaTime;
        if(attackTimer <= 0f)
        {
            DoAttack();
             attackTimer = Random.Range(1.49f, 2.19f);
        }

        if (attackInterval > 0f)
        {
            attackInterval -= Time.deltaTime;
            if(attackInterval < 0f)
            {
                attackInterval = 0f;
                attackCol.enabled = false;

              
                //Debug.Log("StrawMan Attack Ended");
            }
        }
    }

    void UpdateLife()
    {
        lifeTimer -= Time.deltaTime;
        if(lifeTimer <= 0f)
        {
            Destroy(this.gameObject);
            SkillEffectManager.Instance.strawManNum--;
            ActiveBuffManager.Instance.ReduceStack(TraitType.GetDamageSummonPuppet);
        }

        if(lifeTimer <= 1.4f && !isEndEffectPlayed)
        {
            endeffect.Play();
            isEndEffectPlayed = true;
        }

    }

    void DoAttack()
    {
        attackCol.enabled = true;
        attackInterval = 0.77f;
        attackEffect.Play();
        //Debug.Log("StrawMan Attack Triggered");

        transform.DORotate(new Vector3(0f, 360f, 0f), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear);

    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.CompareTag("Enemy"))
        {
            EnemyStatusBase enemyStat = col.GetComponent<EnemyStatusBase>();

            enemyStat.TakeDamage(attackDamage);

        }



    }

}

