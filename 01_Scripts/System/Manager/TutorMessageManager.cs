using Cysharp.Threading.Tasks;
using DG.Tweening;
using MonsterLove.StateMachine; //StateMachine
using QFSW.MOP2;                //Object Pool
using System.Collections.Generic;
using TigerForge;               //EventManager
using TMPro;    
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;     

public class TutorMessageManager : Singleton<TutorMessageManager>
{

    public MenuOpenAnimator tutorMenu;

    public bool isShowingTutorImage = false;

    public List<GameObject> tutorPage1;
    public List<GameObject> tutorPage2;
    public List<GameObject> tutorPage3;
    public int currentTutorPage = 1;

    public AudioClip openMenuSound;

    void Start()
    {
        DOVirtual.DelayedCall(3.5f, () =>
        {
            DelayLoadTutorMessage();
        });

    }

    public void DelayLoadTutorMessage()
    {
        if (!AchievementManager.Instance.progressData.isTutorialSkillFinished)
        {
            AchievementManager.Instance.progressData.isTutorialSkillFinished = true;
            AchievementManager.Instance.SaveProgress();
            tutorMenu.gameObject.SetActive(true);
            tutorMenu.PlayeMenuAni(show: true); 
            isShowingTutorImage = true;
            OpenPage(1);
            SoundEffect.Instance.PlayOneSound(openMenuSound,0.7f);
            Time.timeScale = 0f;

        }
    }

    void Update()
    {
        UpdateTutorProgress();


    }

    public void CloseTutorMenu()
    {
        if (tutorMenu.gameObject.activeSelf == false) return;
        if (!OptionMenuManager.Instance.isOptionMenuOpen)
        {
            Time.timeScale = 1f;
        }
        tutorMenu.PlayeMenuAni(show: false);
        isShowingTutorImage = false;
        SoundEffect.Instance.Play(SoundList.ShopCloseSe);
    }

    public void OpenTutorMenu()
    {
        tutorMenu.gameObject.SetActive(true);
        tutorMenu.PlayeMenuAni(show: true);
        isShowingTutorImage = true;
        OpenPage(1);
        SoundEffect.Instance.Play(SoundList.ShopOpenSe);
        //Debug.Log("Open Tutor Menu");

    }

    public void UpdateTutorProgress()
    {
        //if not controller :
        if ( Gamepad.current != null && isShowingTutorImage)
        {
            // if controller press right shoulder button
            if (Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                ToTurtorNextPage();
            }
            else if (Gamepad.current.leftShoulder.wasPressedThisFrame)
            {
                ToTutorPrePage();
            }

            //if press A button, close the tutor menu
            if (Gamepad.current.aButton.wasPressedThisFrame)
            {
                CloseTutorMenu();
            }




        }

    }

    public void ToTurtorNextPage()
    {
        //page 1 to page 2 , 2 to 3 :
        if (currentTutorPage < 3)
        {
            OpenPage(currentTutorPage + 1);
        }

        SoundEffect.Instance.Play(SoundList.UiClick);

    }

    public void ToTutorPrePage()
    {
        //page 3 to page 2 , 2 to 1 :
        if (currentTutorPage > 1)
        {
            OpenPage(currentTutorPage - 1);
        }

        SoundEffect.Instance.Play(SoundList.UiClick);
    }

    public void OpenPage(int pageNum)
    {
        switch (pageNum)
        {
            case 1:
                foreach (var item in tutorPage1)
                {
                    item.SetActive(true);
                }
                foreach (var item in tutorPage2)
                {
                    item.SetActive(false);
                }
                foreach (var item in tutorPage3)
                {
                    item.SetActive(false);
                }
                currentTutorPage = 1;
                break;
                case 2:
                    foreach (var item in tutorPage1)
                {
                    item.SetActive(false);
                }
                    foreach (var item in tutorPage2)
                {
                    item.SetActive(true);
                }
                    foreach (var item in tutorPage3)
                {
                    item.SetActive(false);
                }
                    currentTutorPage = 2;
                    break;
                case 3:
                    foreach (var item in tutorPage1)
                {
                    item.SetActive(false);
                }
                    foreach (var item in tutorPage2)
                {
                    item.SetActive(false);
                }
                    foreach (var item in tutorPage3)
                {
                    item.SetActive(true);
                }
                    currentTutorPage = 3;
                    break;
            default:
                break;
        }



    }

}

