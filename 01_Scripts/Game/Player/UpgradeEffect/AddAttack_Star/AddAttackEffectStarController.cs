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

public class AddAttackEffectStarController : MonoBehaviour
{
    [Header("当たり判定の情報")]
    [SerializeField, Tooltip("最初のダメージ発生までの間隔")]
    private float startInterval = 0;
    [SerializeField,Tooltip("デフォルトのダメージ発生間隔")]
    private float defaultDamageInterval = 0;
    [SerializeField,Tooltip("ダメージ継続時間")]
    private float activeDuration = 0;

    [SerializeField, Header("ダメージ量")]
    public float damageAmount = 0;

    public ParticleSystem[] particleSystems;
    public Collider particleCollider;

    private float finalDamageInterval = 0;
    private float timeCounter = 0;

    public void SpawnEffect(float damage = 0, float addInterval = 0)
    {
        timeCounter = 0;
        particleCollider.enabled = false;
        damageAmount = damage;
        finalDamageInterval = defaultDamageInterval + addInterval;
    }

    void Update()
    {
        if(particleCollider != null)
        {
            particleCollider.enabled = true;
        }

        timeCounter += Time.deltaTime;
        if(timeCounter >= startInterval)
        {
            if(timeCounter - startInterval >= activeDuration)
            {
                if((int)timeCounter / finalDamageInterval == 0)
                {
                    particleCollider.enabled = false;
                }

                bool isActiveEffectFlg = false;
                foreach(var particle in particleSystems)
                {
                    if(particle == null) { continue; }
                    isActiveEffectFlg = true;
                    var main = particle.main;
                    main.loop = false;
                }

                if(isActiveEffectFlg == false)
                {
                    GameObject.Destroy(this.gameObject, 0.1f);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.CompareTag("Enemy"))
        {
            var hitObject = col.gameObject;
            var enemyState = hitObject.GetComponent<EnemyStatusBase>();
            enemyState.TakeDamage(damageAmount, true, SkillIdType.starMagic);
        }
    }
}

