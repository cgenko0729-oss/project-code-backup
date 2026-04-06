using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class HIdeManager : Singleton<HIdeManager>
{
   
    public GameObject[] stageClearOverHideObjects;

    public GameObject[] cutSceneUiObjects;

    public GameObject AllSpawners;

    public GameObject[] controllerIconList;

    private void OnEnable()
    {
        EventManager.StartListening("ShowControllerIcons", HideControllerIcons);
    }

    private void OnDisable()
    {
        EventManager.StopListening("ShowControllerIcons", HideControllerIcons);
    }


    public void HideControllerIcons()
    {
        bool isController = EventManager.GetBool("ShowControllerIcons");

       Debug.Log("Controller Icon Hide State: " + isController.ToString());

        if (isController)
        {
            foreach (GameObject obj in controllerIconList)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

        }
        else
        {
            foreach (GameObject obj in controllerIconList)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

        }
    }

    public void HideStageClearObject()
    {
        foreach (GameObject obj in stageClearOverHideObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    public void HideCutsceneUiObjects()
    {
        foreach (GameObject obj in cutSceneUiObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }

    }

    public void RestoreCutsceneUiObjects()
    {
        foreach (GameObject obj in cutSceneUiObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

    }

    public void HideSpawners()
    {

        if (AllSpawners != null)
        {
            AllSpawners.SetActive(false);
        }
    }


 }

