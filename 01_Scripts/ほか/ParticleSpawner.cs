using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;

using TigerForge;           //EventManager
using QFSW.MOP2;            //Object Pool



public class ParticleSpawner : Singleton<ParticleSpawner>
{

     public ParticleSystem fireParticlePrefab;
    void Start()
    {
        
    }

    void Update()
    {

        if(Input.GetKeyDown(KeyCode.P)) {
            Vector3 spawnPos = new Vector3(Random.Range(-15f, 15f), 5, Random.Range(-15f, 15f));
             var main = fireParticlePrefab.main;
            main.startColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            main.loop = true;

            //fireParticlePrefab.Play();
            ParticleSystem particleObj = Instantiate(fireParticlePrefab, spawnPos, Quaternion.identity);
            
            ParticleSystem[] allParticles = particleObj.GetComponentsInChildren<ParticleSystem>(true);

            foreach (var pss in allParticles)
        {
            var main2 = pss.main;
            main2.loop = true;

            // Optional: random color
            main2.startColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }
            
            //var ps = particleObj.GetComponent<ParticleSystem>();
            particleObj.Play();
        }


    }
}

