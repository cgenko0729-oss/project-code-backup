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
using VolumetricFogAndMist2;

public class FogOfWardTracker : MonoBehaviour
{
    public VolumetricFog fogVolume;
    public GameQuestHandler questHandler; 

    public float percentage = 100f;

    public float percentageCalCnt = 0.3f;
    public float percentageCalCntMax = 0.1f;

    private void Start()
    {
      fogVolume = GameObject.FindGameObjectWithTag("FogOfWar").GetComponent<VolumetricFog>();
      fogVolume.gameObject.transform.position = new Vector3(39, 0.1f, -27f);
    }

    void Update() {

        percentageCalCnt -= Time.deltaTime;

        if(percentageCalCnt <= 0)
        {
            percentageCalCnt = percentageCalCntMax;
            fogVolume.UpdateClearedFogPercentage();
            percentage = fogVolume.fogOfWarClearedPercentage;

             //Debug.Log("Map explored: " + percentage.ToString("F2") + "%");

            // You can now use this value for any game logic!
            if (percentage >= 100f)
            {
                //Debug.Log("Congratulations! You have explored the entire map!");
                // Give player an achievement, open a new area, etc.
            }
        }

        questHandler.currentProgress = percentage * 1.77f; //1.49


        // For example, press the 'M' key to check the map completion
        //if (Input.GetKeyDown(KeyCode.M))
        //{

        //    // 1. Tell the fog volume to run the calculation
        //    fogVolume.UpdateClearedFogPercentage();

        //    // 2. Now you can get the result from the public variable
        //    percentage = fogVolume.fogOfWarClearedPercentage;

        //    Debug.Log("Map explored: " + percentage.ToString("F2") + "%");

        //    // You can now use this value for any game logic!
        //    if (percentage >= 100f)
        //    {
        //        Debug.Log("Congratulations! You have explored the entire map!");
        //        // Give player an achievement, open a new area, etc.
        //    }
        //}



    }


    //when it get destroyed
    private void OnDestroy()
    {
        fogVolume.gameObject.SetActive(false);
    }





}

