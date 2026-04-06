using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class TitleTutorManager : Singleton<TitleTutorManager>
{

    public GameObject tutorEndlessModeObj;
    public GameObject tutorSkillTreeObj;

    public AudioClip triggerTutorSe;

    public bool isEndlessTutorOpened = false;
    public bool isSkillTreeTutorOpened = false;

    public GameObject endlessTutorCloseBUtton;
    public GameObject skillTreeTutorCloseButton;



    void Start()
    {
        
    }

    void Update()
    {
        CheckInStageSelect();
        CheckInCharaSelect();
    }

    [ContextMenu("DebugResetTutor")]
    public void DebugResetTutor()
    {
        AchievementManager.Instance.progressData.isTutorEndlessModeFinished = false;
        AchievementManager.Instance.progressData.isTutorSkillTreeFinished = false;
        
        AchievementManager.Instance.SaveProgress();

    }

    public void CloseEndlessTutor()
    {
        if(isEndlessTutorOpened)
        {
            MenuOpenAnimator ani = tutorEndlessModeObj.GetComponent<MenuOpenAnimator>();
            ani.PlayeMenuAni(false);
            isEndlessTutorOpened = false;
            endlessTutorCloseBUtton.GetComponent<Button>().interactable = false;
        }

    }

    public void CloseSkillTreeTutor()
    {
        if(isSkillTreeTutorOpened)
        {
            MenuOpenAnimator ani = tutorSkillTreeObj.GetComponent<MenuOpenAnimator>();
            ani.PlayeMenuAni(false);
            isSkillTreeTutorOpened = false;
            skillTreeTutorCloseButton.GetComponent<Button>().interactable = false;
        }
    }

    public void CheckInStageSelect()
    {

        if (isEndlessTutorOpened)
        {
            if(Gamepad.current != null)
            {
                if(Gamepad.current.aButton.wasPressedThisFrame)
                {
                    CloseEndlessTutor();
                }
            }

            //if press e key, close the tutor
            if(Input.GetKeyDown(KeyCode.E))
            {
                CloseEndlessTutor();
            }


        }


        if (GamePadManager.Instance.currentTitleScenePage == TitleScenePage.MapSelectPage)
        {
            if (AchievementManager.Instance.progressData.isTutorialSkillFinished && !AchievementManager.Instance.progressData.isTutorEndlessModeFinished)
            {



                AchievementManager.Instance.progressData.isTutorEndlessModeFinished = true;
                tutorEndlessModeObj.SetActive(true);
                MenuOpenAnimator ani = tutorEndlessModeObj.GetComponent<MenuOpenAnimator>();
                ani.PlayeMenuAni(true);
                AchievementManager.Instance.SaveProgress();
                SoundEffect.Instance.PlayOneSound(triggerTutorSe,0.7f);
                isEndlessTutorOpened = true;
                Debug.Log("Finish Endless Mode Tutor");

                DOVirtual.DelayedCall(1f, () => {
                    EventSystem.current.SetSelectedGameObject(endlessTutorCloseBUtton);
                });

            }

        }
        
    }

    public void CheckInCharaSelect()
    {

        if (isSkillTreeTutorOpened && Gamepad.current != null)
        {
            if(Gamepad.current != null)
            {
                if(Gamepad.current.aButton.wasPressedThisFrame)
                {
                    CloseSkillTreeTutor();
                }
            }

            //if press e key, close the tutor
            if(Input.GetKeyDown(KeyCode.E))
            {
                CloseSkillTreeTutor();
            }

        }


        if (GamePadManager.Instance.currentTitleScenePage == TitleScenePage.CharacterSelectPage)
        {
            if (AchievementManager.Instance.progressData.isTutorialSkillFinished && !AchievementManager.Instance.progressData.isTutorSkillTreeFinished)
            {
                AchievementManager.Instance.progressData.isTutorSkillTreeFinished = true;
                tutorSkillTreeObj.SetActive(true);
                MenuOpenAnimator ani = tutorSkillTreeObj.GetComponent<MenuOpenAnimator>();
                ani.PlayeMenuAni(true);
                AchievementManager.Instance.SaveProgress();
                SoundEffect.Instance.PlayOneSound(triggerTutorSe,0.7f);
                isSkillTreeTutorOpened = true;
                Debug.Log("Finish Skill Tree Tutor");

                //Set EventSYstem current selected object to the skillTreeTutorCloseButton
                DOVirtual.DelayedCall(1f, () => {
                    EventSystem.current.SetSelectedGameObject(skillTreeTutorCloseButton);
                });
                    
            }

        }

    }

}

