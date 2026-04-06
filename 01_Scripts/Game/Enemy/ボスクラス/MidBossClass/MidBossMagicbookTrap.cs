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
using System.ComponentModel;
using UnityEngine.VFX;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;

public class MidBossMagicbookTrap : MonoBehaviour
{
    [System.Serializable]
    public struct EffectData
    {
        [Tooltip("エフェクトのGameObject")]
        public GameObject effectObj;
        [Tooltip("エフェクト再生までの間隔")]
        public float playEffectDuration;
    }

    [Header("魔法陣の情報")]
    public EffectData magicCircle;
    [Header("魔法の情報")]
    public EffectData magicEnergy;
    [Header("AoEの情報")]
    public float aoeSpawnDuration;
    public float aoeSpawnRadiusSize;
    public float aoeDamage;

    public float count = 0;
    public bool isPlayEnergyEffect = false;
    private ParticleSystem energyEffectParticleSystem;

    void Start()
    {
        if (magicCircle.effectObj != null || magicEnergy.effectObj != null)
        {
            magicCircle.effectObj.SetActive(false);
            magicEnergy.effectObj.SetActive(false);
        }
        else
        {
            Debug.Log("EffectObjがセットされていません");
        }

        energyEffectParticleSystem =  magicEnergy.effectObj.GetComponent<ParticleSystem>();
        if (energyEffectParticleSystem == null)
        {
            Debug.Log("magicEnergyのEffectObjectについているParticleSystemを取得できませんでした");
        }

    }

    void Update()
    {
        count += Time.deltaTime;

        if (magicCircle.playEffectDuration <= count &&
            magicCircle.effectObj.activeSelf == false)
        {
            magicCircle.effectObj.SetActive(true);

            EnemyEffectManager.Instance.SpawnAoeCircle(transform.position,
                aoeSpawnRadiusSize, aoeSpawnDuration, aoeDamage);
        }

        if (magicEnergy.playEffectDuration <= count &&
            magicEnergy.effectObj.activeSelf == false)
        {
            magicEnergy.effectObj.SetActive(true);
            isPlayEnergyEffect = true;
        }

        if(isPlayEnergyEffect == true &&
            energyEffectParticleSystem.IsAlive(true) == false)
        {
            Destroy(this.gameObject);
        }
    }
}




