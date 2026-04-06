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

public class MapSandStormSpawner : MonoBehaviour
{

    public GameObject sandStormObj;
    public float spawnTimer = 3f;
    public float spawnInterval = 3f;
    private float spawnRadius = 21f;
    public int spawnNum = 3;

    public Vector3 spawnCenterPos;
    public Transform playerTrans;

    public AudioClip windSound;

    public Transform questTrans;


    void HandleSpawnSandStorm()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = spawnInterval;

            SoundEffect.Instance.PlayOneSound(windSound, 1f);

            int numToSpawn = Random.Range(2, 3);

            for (int i = 0; i < numToSpawn; i++)
            {
                Vector2 randomPos = Random.insideUnitCircle * spawnRadius;

                //ensure randomPos is not too close to the player  ,at least 14f away
                while (randomPos.magnitude < 14f)
                {
                    randomPos = Random.insideUnitCircle * spawnRadius;
                }

                Vector3 spawnPos = new Vector3(spawnCenterPos.x + randomPos.x, 0f, spawnCenterPos.z + randomPos.y);
                GameObject obj = Instantiate(sandStormObj, spawnPos, Quaternion.identity);
                SkillSMove tornadoSMove = obj.GetComponent<SkillSMove>();
                float amp = Random.Range(0.79f, 1.49f);
                float wave = Random.Range(79f, 149f);
                float spd = Random.Range(4.29f, 5.69f);

                Vector3 spawnPosToPlayer = (playerTrans.position - spawnPos).normalized;

                tornadoSMove.Configure(spawnPosToPlayer, spd, amp, wave, true); 

            }
        }

    }



    void Start()
    {
        playerTrans = GameObject.FindWithTag("Player").transform;

    }

    void Update()
    {
        spawnCenterPos = playerTrans.position;

        float distFromPlayerToQuest = Vector3.Distance(playerTrans.position, questTrans.position);
        
        if (distFromPlayerToQuest < 17f) {
        HandleSpawnSandStorm();
        }

        
    }
}

