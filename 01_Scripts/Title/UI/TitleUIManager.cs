using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TitleUIManager : Singleton<TitleUIManager>
{
    [Header("�e���j���[��UI�O���[�v")]
    [SerializeField] private GameObject titleGroup;
    [SerializeField] private GameObject charaSelectGroup;
    [SerializeField] private GameObject stageSelectGroup;
    [SerializeField] private GameObject petSelectGroup;
    [SerializeField] private GameObject selectMenu;
    [SerializeField] private GameObject languageListGroup;
    [SerializeField] private GameObject AchivementGroup;

    [Header("�y�[�W�̃f�t�H���g�őI�������UI�I�u�W�F�N�g")]
    public GameObject titleDefaultUI;
    public GameObject charaSelectDefaultUI;
    public GameObject petSelectDefaultUI;
    public GameObject mapSelectDefaultUI;
    public GameObject shopDefaultUI;
    public GameObject settingDefaultUI;
    public GameObject achievementDefaultUI;
    public GameObject endGameDefaultUI;
    public GameObject charaUpgradeDefaultUI;

    [Header("���j���[�̎�ޕύX�ɕK�v�ȏ��")]
    public GameObject nowUiGroup;
    public float changeTime = 0;
    public bool isChanging = false;    // ���݁AUI�̕ύX�����ǂ���

    [Header("�V�[���̍ŏ��ɑI�΂��UI�I�u�W�F�N�g")]
    public GameObject firstSelectUiObj;

    [Header("�V�[���ŏ��̃t�F�[�h�C�����")]
    public Image fadeImage;
    public float fadeDuration;
    public Ease fadeEase;
    private Tween fadeTween;

    private CanvasGroup menuCanvasGroup;

    private void OnDisable()
    {
        fadeTween?.Kill();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // �Z�b�g����Ă��Ȃ����̂�����΃��O���c��
        if(titleGroup == null ||
            charaSelectGroup == null ||
            stageSelectGroup == null ||
            petSelectGroup == null)
        {
            Debug.LogWarning("�ݒ肳��Ă��Ȃ����ڂ�����܂��B");
        }

        // �ŏ��̃t�F�[�h�C�����s��
        if(fadeImage != null)
        {
            fadeImage.color = Color.black;
            fadeTween?.Kill();
            fadeTween = fadeImage.DOColor(Color.clear, fadeDuration).SetEase(fadeEase);
            
        }

        // UI�O���[�v�̏��������s��
        titleGroup.SetActive(true);
        selectMenu.SetActive(true);
        charaSelectGroup.SetActive(true);
        petSelectGroup.SetActive(true);
        stageSelectGroup.SetActive(true);
        menuCanvasGroup = selectMenu?.GetComponent<CanvasGroup>();
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0;
        }
        var titleCanvasGroup = titleGroup?.GetComponent<CanvasGroup>();
        if (titleCanvasGroup != null)
        {
            titleCanvasGroup.interactable = false;
        }

        Invoke("CloseAllTitleMenu", fadeDuration / 3);

        nowUiGroup = charaSelectGroup;
    }

    private void CloseAllTitleMenu()
    {
        // �S�Ẵ��j���[��Disable����
        charaSelectGroup.SetActive(false);
        petSelectGroup.SetActive(false);
        stageSelectGroup.SetActive(false);
        selectMenu.SetActive(false);

        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 1;
        }
        var titleCanvasGroup = titleGroup?.GetComponent<CanvasGroup>();
        if (titleCanvasGroup != null)
        {
            titleCanvasGroup.interactable = true;
            //Debug.Log("TitleGroup��Interactable��Ture");
        }

        if (firstSelectUiObj != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectUiObj);
        }
        else
        {
            //Debug.Log("�V�[���ŏ��ɑI�������UI�I�u�W�F�N�g���ݒ肳��Ă��܂���");
        }

        AchivementGroup.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // 入力がGamePadなら選択中UIのNULLを調べる
        if (Gamepad.current != null && InputDeviceManager.Instance.GetLastUsedDevice() == Gamepad.current)
        {
            // �I�𒆂̃I�u�W�F�N�g��Null�ɂȂ����ꍇ�A�f�t�H���gUI���Z�b�g����
            if (EventSystem.current?.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(
                    TitleUIManager.Instance.SetSelectedToDefaultUI());
            }
        }
    }

    System.Collections.IEnumerator ChangeGroup(GameObject changeGroup,int _changeDir)
    {
        if(nowUiGroup == null || changeGroup == null) { yield return null; }
        
        if(nowUiGroup.gameObject.name != changeGroup.gameObject.name)
        {
            isChanging = true;

            var nowUiGroupTrans = nowUiGroup.GetComponent<RectTransform>();
            var changeUiGroupTrans = changeGroup.GetComponent<RectTransform>();

            nowUiGroupTrans.localPosition = new Vector2(0, 0);
            changeUiGroupTrans.localPosition = new Vector2(1000 * _changeDir, 0);

            nowUiGroupTrans.DOLocalMoveX(-1000 * _changeDir, changeTime);
            changeUiGroupTrans.DOLocalMoveX(0, changeTime);
        
            yield return new WaitForSecondsRealtime(changeTime);

            nowUiGroup.SetActive(false);
            nowUiGroup = changeGroup;
        }

        isChanging = false;
    }

    public void ToCharaSelect()
    {
        if(isChanging == true) { return; }
        
        titleGroup.SetActive(false);
        languageListGroup.SetActive(false);
        selectMenu.SetActive(true);
        charaSelectGroup.SetActive(true);
        StartCoroutine(ChangeGroup(charaSelectGroup, 1));
    }

    public void ToPetSelect()
    {
        if (isChanging == true) { return; }

        petSelectGroup.SetActive(true);
        StartCoroutine(ChangeGroup(petSelectGroup, 1));
    }

    public void ToStageSelect()
    {
        if (isChanging == true) { return; }

        stageSelectGroup.SetActive(true);
        StartCoroutine(ChangeGroup(stageSelectGroup, 1));
    }

    public void BackTitle()
    {
        if (isChanging == true) { return; }

        selectMenu.gameObject.SetActive(false);
        titleGroup.SetActive(true);
        languageListGroup.SetActive(true);
    }

    public void BackCharaSelect()
    {
        if (isChanging == true) { return; }

        charaSelectGroup.SetActive(true);
        StartCoroutine(ChangeGroup(charaSelectGroup, -1));
    }

    public void BackPetSelect()
    {
        if (isChanging == true) { return; }

        petSelectGroup.SetActive(true);
        StartCoroutine(ChangeGroup(petSelectGroup, -1));
    }

    public void OpenAchivement()
    {
        // �^�C�g���ƌ���̕\�����Ȃ�������
        titleGroup.SetActive(false);
        languageListGroup.SetActive(false);
    }

    public void CloseAchivement()
    {
        // �^�C�g���ƌ���̕\�����s������
        titleGroup.SetActive(true);
        languageListGroup.SetActive(true);
    }

    // �I��UI��NULL�ɂȂ����Ƃ��ɁA���̃y�[�W�̃f�t�H���gUI�I�u�W�F�N�g���Z�b�g����
    public GameObject SetSelectedToDefaultUI()
    {
        GameObject setObject = null;
        switch (GamePadManager.Instance.currentTitleScenePage)
        {
            case TitleScenePage.StartPage:
                setObject = titleDefaultUI;
                break;
            case TitleScenePage.CharacterSelectPage:
                setObject = charaSelectDefaultUI;
                break;
            case TitleScenePage.PetSelectPage:
                setObject = petSelectDefaultUI;
                break;
            case TitleScenePage.MapSelectPage:
                setObject = mapSelectDefaultUI;
                break;
            case TitleScenePage.ShopPage:
                setObject = shopDefaultUI;
                break;
            case TitleScenePage.SettingPage:
                setObject = settingDefaultUI;
                break;
            case TitleScenePage.AchievementPage:
                setObject = achievementDefaultUI;
                break;
            case TitleScenePage.EndGamePage:
                setObject = endGameDefaultUI;
                break;
            case TitleScenePage.CharaUpgradePage:
                setObject = charaUpgradeDefaultUI;
                break;
        }

        return setObject;
    }
}
