using UnityEngine;
using UnityEngine.UI;
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public enum TitleScenePage
{
    StartPage           = 0,
    CharacterSelectPage = 1,
    PetSelectPage       = 2,
    MapSelectPage       = 3,
    ShopPage            = 4,
    SettingPage         = 5,
    AchievementPage     = 6,
    EndGamePage         = 7,
    CharaUpgradePage    = 8,
}

public class GamePadManager : Singleton<GamePadManager>
{
    public ScrollRect welcomeMessageRect;
    public ScrollRect trophyScrollRect;
    public ScrollRect traitCardListRect;

    public SettingPageController settingPageController;

    public Button achExitButton;

    public Button settingExitButton;
    public Button settingSaveButton;

    public Button ShopExitButton;
    public Button ShopBuyButton;
    public Button ShopRefundButton;

    // キャラセレクトの入力
    public Button charaChangeL_Button;
    public Button charaChangeR_Button;

    public float scrollSpeedGobal = 0.5f;
    public float scrollSpeedAddWelcomPage = 2.8f;
    public float scrollSpeedAddTrophyPage = 1.0f;
    public float scrollSpeedAddTraitCardPage = 1.49f;

    public TitleScenePage currentTitleScenePage = TitleScenePage.StartPage;

    public GameObject optionMenuFirstButton;

    public void SetFocusOptionMenuFirstButton()
    {

        EventSystem.current.SetSelectedGameObject(optionMenuFirstButton);
    }

    void Start()
    {

    }

    void Update()
    {
        if (Gamepad.current == null) return;

        float rightStickVertical = Input.GetAxis("RightStickVertical");

        if (currentTitleScenePage == TitleScenePage.StartPage && welcomeMessageRect && welcomeMessageRect.gameObject.activeSelf)
        {
            if (Mathf.Abs(rightStickVertical) > 0.1f)
            {
                float scrollAmount = -rightStickVertical * scrollSpeedGobal * scrollSpeedAddWelcomPage * Time.deltaTime;
                float newPosition = welcomeMessageRect.verticalNormalizedPosition + scrollAmount;

                welcomeMessageRect.verticalNormalizedPosition = Mathf.Clamp01(newPosition);

            }
        }

        if (trophyScrollRect && trophyScrollRect.gameObject.activeSelf)
        {


            if (Mathf.Abs(rightStickVertical) > 0.1f)
            {
                float scrollAmount = -rightStickVertical * scrollSpeedGobal * scrollSpeedAddTrophyPage * Time.deltaTime;
                float newPosition = trophyScrollRect.verticalNormalizedPosition + scrollAmount;

                trophyScrollRect.verticalNormalizedPosition = Mathf.Clamp01(newPosition);

            }
        }

        if (traitCardListRect && traitCardListRect.gameObject.activeSelf)
        {
            if (Mathf.Abs(rightStickVertical) > 0.1f)
            {
                float scrollAmount = -rightStickVertical * scrollSpeedGobal * scrollSpeedAddTraitCardPage * Time.deltaTime;
                float newPosition = traitCardListRect.verticalNormalizedPosition + scrollAmount;

                traitCardListRect.verticalNormalizedPosition = Mathf.Clamp01(newPosition);

            }
        }

        UpdateAchievementMenuInput();
        UpdateSettingMenuInput();
        UpdateShopMenuInput();

        if(currentTitleScenePage == TitleScenePage.CharacterSelectPage)
        {
            UpdateCharaSelectMenuInput();
        }
    }

    public void ResetScrollToTop()
    {
        if (trophyScrollRect)
        {
            trophyScrollRect.verticalNormalizedPosition = 1f;
        }

        if (traitCardListRect)
        {
            traitCardListRect.verticalNormalizedPosition = 1f;
        }
    }

    public void ShowTutorPetPage()
    {

    }

    public void ChangeTitleScenePage(int pageId)
    {
        currentTitleScenePage = (TitleScenePage)pageId;

        EventManager.EmitEvent("OnChangeTitleScenePage", currentTitleScenePage);
        switch (currentTitleScenePage)
        {
            case TitleScenePage.StartPage:
                break;
            case TitleScenePage.CharacterSelectPage:
                break;
            case TitleScenePage.PetSelectPage:
                if (!AchievementManager.Instance.progressData.isTutorialPetFinished)
                {
                    AchievementManager.Instance.progressData.isTutorialPetFinished = true;
                    AchievementManager.Instance.SaveProgress();
                    ShowTutorPetPage();
                }
                break;
            case TitleScenePage.MapSelectPage:
                break;
            case TitleScenePage.ShopPage:
                break;
            case TitleScenePage.SettingPage:
                break;
            case TitleScenePage.AchievementPage:
                break;
            case TitleScenePage.EndGamePage:
                break;
            case TitleScenePage.CharaUpgradePage:
                break;
            default:
                break;
        }

    }

    public void UpdateAchievementMenuInput()
    {
        if (currentTitleScenePage != TitleScenePage.AchievementPage) return;

        if (Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            //EventSystem.current.SetSelectedGameObject(null);
             EventSystem.current.SetSelectedGameObject(TitleUIManager.Instance.SetSelectedToDefaultUI());

            AchievementMenuManager.Instance.AchNextPage();
        }

        else if (Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            //EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(TitleUIManager.Instance.SetSelectedToDefaultUI());

            AchievementMenuManager.Instance.AchNextPage();
        }

        //if (Gamepad.current.buttonSouth.wasPressedThisFrame)
        //{
        //    achExitButton.onClick.Invoke();
        //}


    }

    public void UpdateSettingMenuInput()
    {

        if (currentTitleScenePage != TitleScenePage.SettingPage) return;

        if (Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            settingPageController.ToNextPage();
        }

        else if (Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            settingPageController.ToPreviousPage();
        }

        //if (Gamepad.current.buttonSouth.wasPressedThisFrame)
        //{
        //    settingExitButton.onClick.Invoke();
        //}

        if (Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            settingSaveButton.onClick.Invoke();
        }

    }

    public void UpdateShopMenuInput()
    {
        if (currentTitleScenePage != TitleScenePage.ShopPage) return;

        //if (Gamepad.current.buttonSouth.wasPressedThisFrame)
        //{
        //    ShopExitButton.onClick.Invoke();
        //}

        if (Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            ShopBuyButton.onClick.Invoke();
        }

        if (Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            ShopRefundButton.onClick.Invoke();
        }

    }

    public void UpdateCharaSelectMenuInput()
    {
        if(Gamepad.current.leftTrigger.wasPressedThisFrame &&
            charaChangeL_Button != null)
        {
            charaChangeL_Button.onClick.Invoke();
        }

        if (Gamepad.current.rightTrigger.wasPressedThisFrame &&
            charaChangeR_Button != null)
        {
            charaChangeR_Button.onClick.Invoke();
        }
    }
}

