using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;

public class CoinSpawner : Singleton<CoinSpawner>
{

    public ObjectPool coinPool;

    public GameObject coinObj;

    public GameObject magnetObj;
    public GameObject hpPotionObj;

    public ObjectPool starExpObjPool;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SpawnHpPotionObj(Vector3 spawnCenterPos,bool hasRand = false)
    {
        Vector3 spawnPos;

        if (hasRand)
        {
             spawnPos = new Vector3(
             Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f),
             spawnCenterPos.y,
             Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f)
         );

          while (Vector3.Distance(spawnPos, spawnCenterPos) < 2.8f)
         {
             spawnPos = new Vector3(
                 Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f),
                 spawnCenterPos.y,
                 Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f)
             );
         }
        }
        else
        {
            spawnPos = spawnCenterPos;
        }
        
        

         Instantiate(hpPotionObj, spawnPos, Quaternion.identity);

    }

 
    public void SpawnChestRewardItem(Vector3 spawnCenterPos, bool hasExpStar = true)
    {

        //spawn one magnet obj , spawnPos should keep at least 2.8f away from spawnCenterPos, and not too far away (max 4.2f)
        Vector3 spawnPos = new Vector3(
            Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f),
            spawnCenterPos.y,
            Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f)
        );

        while (Vector3.Distance(spawnPos, spawnCenterPos) < 2.8f)
        {
            spawnPos = new Vector3(
                Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f),
                spawnCenterPos.y,
                Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f)
            );
        }

        Instantiate(magnetObj, spawnPos, Quaternion.identity);


        //also spawnHpPotion
        spawnPos = new Vector3(
            Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f),
            spawnCenterPos.y,
            Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f)
        );

        while (Vector3.Distance(spawnPos, spawnCenterPos) < 2.8f)
        {
            spawnPos = new Vector3(
                Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f),
                spawnCenterPos.y,
                Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f)
            );
        }
        
        Instantiate(hpPotionObj, spawnPos, Quaternion.identity); //quick mode disale


        //also spawn starExpObjPool
        spawnPos = new Vector3(
            Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f),
            spawnCenterPos.y,
            Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f)
        );
        while (Vector3.Distance(spawnPos, spawnCenterPos) < 2.8f)
        {
            spawnPos = new Vector3(
                Random.Range(spawnCenterPos.x - 4.2f, spawnCenterPos.x + 4.2f),
                spawnCenterPos.y,
                Random.Range(spawnCenterPos.z - 4.2f, spawnCenterPos.z + 4.2f)
            );
        }

        if (hasExpStar)
        {
            GameObject expObj;     
            expObj= starExpObjPool.GetObject(spawnPos, Quaternion.identity);
        }
        


    }

    public async void SpawnWithDelay(Vector3 spawnCenterPos, int _spawnNum = 14)
    {

        int spawnNum = _spawnNum;
        Vector3 spawnPos;

        //spawn each coin with delay of UniTask.Delay(140)
        for (int i = 0; i < spawnNum; i++)
        {
            spawnPos = new Vector3(
                Random.Range(spawnCenterPos.x - 3.5f, spawnCenterPos.x + 3.5f),
                spawnCenterPos.y,
                Random.Range(spawnCenterPos.z - 3.5f, spawnCenterPos.z + 3.5f)
            );


            CoinMove coinMove = coinPool.GetObjectComponent<CoinMove>(spawnCenterPos);
            //GameObject coin = Instantiate(coinObj, spawnCenterPos, Quaternion.identity);
            //CoinMove coinMove = coin.GetComponent<CoinMove>();
            coinMove.initPos = spawnCenterPos;
            coinMove.targetPos = spawnPos;
            coinMove.isCoinActivated = false;
            coinMove.TransitionFromInitPosToSpawnPosWithCurveMovement();

            SoundEffect.Instance.Play(SoundList.DropCoin);

            await UniTask.Delay(140);
        }






    }

    public void SpawnCoin(Vector3 spawnCenterPos, int coinNum = 30, float spawnRange = 2f)
    {
        int spawnNum = coinNum;
        Vector3 spawnPos;



        for (int i = 0; i < spawnNum; i++)
        {
            spawnPos = new Vector3(
                Random.Range(spawnCenterPos.x - spawnRange, spawnCenterPos.x + spawnRange),
                spawnCenterPos.y,
                Random.Range(spawnCenterPos.z - spawnRange, spawnCenterPos.z + spawnRange)
            );

            CoinMove coinMove = coinPool.GetObjectComponent<CoinMove>(spawnCenterPos);
            //GameObject coin = Instantiate(coinObj, spawnCenterPos, Quaternion.identity);
            //CoinMove coinMove = coin.GetComponent<CoinMove>();
            coinMove.initPos = spawnCenterPos;
            coinMove.targetPos = spawnPos;
            coinMove.isCoinActivated = false;
            coinMove.TransitionFromInitPosToSpawnPosWithCurveMovement();

            //coin.transform.localScale = Vector3.one * Random.Range(0.5f, 1.5f);
        }


    }

}

