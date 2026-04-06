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


[RequireComponent(typeof(MeshRenderer))]
public class RingAoeIndicator : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] float radius      = 4f;     // metres
    [SerializeField] float windUpTime  = 3f;     // seconds until hit
    [SerializeField] float lingerTime  = 0.25f;  // keep ring after hit
    [SerializeField] int   damage      = 25;
    [SerializeField] LayerMask targetLayers;

    static readonly Collider[] buf = new Collider[16];
    Material mat;

    void Awake()
    {
        // clone material so each telegraph has its own _Progress
        mat = GetComponent<MeshRenderer>().material;
    }

    void OnEnable()
    {
        // 1) set size once
        transform.localScale = new Vector3(radius * 2, 1, radius * 2);

        // 2) start tween
        mat.SetFloat("_Progress", 0);
        DOTween.To(() => 0f, p => mat.SetFloat("_Progress", p), 1f, windUpTime)
               .OnComplete(OnHit);
    }

    void OnHit()
    {
        // damage all inside radius
        int n = Physics.OverlapSphereNonAlloc(
                    transform.position, radius, buf, targetLayers);
        for (int i = 0; i < n; i++)
            buf[i].GetComponent<IDamageable>()?.TakeDamage(damage);

        // optional VFX here c

        // auto-hide after linger
        DOVirtual.DelayedCall(lingerTime, () => gameObject.SetActive(false));
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif


}

