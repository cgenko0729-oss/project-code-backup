using UnityEngine;
using UnityEngine.UI;    
using DG.Tweening;
using DG.Tweening.Core;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.InputSystem;

public class TitleButtonController : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, ISubmitHandler, IPointerClickHandler
{
    public enum EffectType
    {
        None,
        OutLIne,
        InnerOutlIne,
    }

    [Header("ボタンの動きの情報")]
    [Tooltip("デフォルトの拡縮率はGameObjectのScaleを使用する")]
    [SerializeField] private bool useScaleGameObject = false;
    [SerializeField] private Vector3 defaultScale = Vector3.one;
    [SerializeField] private float SelectedScale = 1.25f;
    [SerializeField] private float changeTime = 0.1f;
    
    [Header("押されたときのSE情報")]
    [SerializeField] private AudioClip submitSound;
    [Tooltip("このサウンド自体の音量")]
    [SerializeField] private float soundVolume = 1.0f;
    [SerializeField] private bool enablePlayClickSe = true;

    [Header("ボタンのインタラクティブによる情報")]
    [Tooltip("trueの時の透明度")]
    [SerializeField] private float interaciveAlpha = 1f;
    [Tooltip("falseの時の透明度")]
    [SerializeField] private float noninteractiveAlpha = 0.3f;

    [Header("Submit後の次に選択された状態になるUIオブジェクト")]
    [SerializeField] private GameObject nextSelectedUiObj;
    [Header("Interactableがfalseになった時選択中を戻すUIオブジェクト")]
    [SerializeField] private GameObject previousSelectedUiObj;
    [Header("次の選択されたUIオブジェクトをNULLにするかどうかのフラグ")]
    [SerializeField] private bool nextSelectedIsNull = false;

    [Header("選択中のエフェクトの情報")]
    public EffectType effectType = EffectType.None;
    public UIFXController uiFx;

    private Tween scaleTween;

    public bool hasUiOnSelectSound = true;

    private void Start()
    {
        if (useScaleGameObject == true)
        {
            defaultScale = transform.localScale;
        }

        if (uiFx == null)
        {
            uiFx = GetComponent<UIFXController>();
        }
    }

    private void OnDisable()
    {
        scaleTween?.Kill();
        transform.localScale = defaultScale;

        if (uiFx != null)
        {
            SetOutlineData(false);
        }
    }

    private void Update()
    {
        var btnComp = GetComponent<Button>();
        var canvasGroup = GetComponent<CanvasGroup>();
        if(btnComp != null)
        {
            if (canvasGroup != null)
            {
                if (btnComp.interactable == true)
                {
                    canvasGroup.alpha = interaciveAlpha;
                }
                else
                {
                    canvasGroup.alpha = noninteractiveAlpha;
                }
            }

            // ボタンコンポーネントのInteractableがfalseになった時に、
            // EventSystemで何も選ばれていなければ、指定されているUIオブジェクトをセットする
            var uiObj = EventSystem.current.currentSelectedGameObject;
            if(btnComp.interactable == false && uiObj == null
                && previousSelectedUiObj != null)
            {
                EventSystem.current.SetSelectedGameObject(previousSelectedUiObj);
            }
        }
    }
    
    // 拡縮率のアニメーションを行う
    private void AnimateScale(Vector3 scale, float changeTime,bool enablePlayClickSe = true)
    {   
        // 一度Tweenの削除を行ってから、拡縮処理を行う
        scaleTween?.Kill();
        scaleTween = transform.DOScale(scale, changeTime).SetEase(Ease.OutBack);

        if (enablePlayClickSe == true)
        {
            SoundEffect.Instance.Play(SoundList.UiHover);
        }
    }

    // マウスが上に乗ったとき
    public void OnPointerEnter(PointerEventData eventData)
    {
        var inputdevice = InputDeviceManager.Instance?.GetLastUsedDevice();
        if(inputdevice == null || inputdevice is Gamepad) { return; }

        AnimateScale(defaultScale * SelectedScale, changeTime);
        if (uiFx != null)
        {
            SetOutlineData(true);
        }
    }

    // マウスが上から離れたとき
    public void OnPointerExit(PointerEventData eventData)
    {
        var inputdevice = InputDeviceManager.Instance?.GetLastUsedDevice();
        if (inputdevice == null || inputdevice is Gamepad) { return; }

        AnimateScale(defaultScale, changeTime,false);
        if (uiFx != null)
        {
            SetOutlineData(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (InputDeviceManager.Instance?.GetLastUsedDevice() is Gamepad) { return; }

        if (nextSelectedUiObj != null)
        {
            EventSystem.current.SetSelectedGameObject(nextSelectedUiObj);
        }
    }

    // (キーボードやゲームパッドで)選択されたとき
    public void OnSelect(BaseEventData eventData)
    {
        AnimateScale(defaultScale * SelectedScale, changeTime);

        if(uiFx != null)
        {
            SetOutlineData(true);
        }

        if(hasUiOnSelectSound == true)
        {
            SoundEffect.Instance.Play(SoundList.UiHover);
        }
    }

    // (キーボードやゲームパッドで)選択が外れたとき
    public void OnDeselect(BaseEventData eventData)
    {
        AnimateScale(defaultScale, changeTime, false);
        if (uiFx != null)
        {
            SetOutlineData(false);
        }
    }

    // Submitを行った際にメニューなどが切り替わる場合は、
    // 次に選択されるUIオブジェクトをセットする
    public void OnSubmit(BaseEventData eventData)
    {
        // Submit時のSEを鳴らす
        if (enablePlayClickSe == true)
        {
            SoundEffect.Instance.Play(SoundList.UiClick);
        }

        //if (submitSound != null)
        //{
        //    SoundEffect.Instance.PlayOneSound(submitSound, soundVolume);
        //}

        if(nextSelectedIsNull == true)
        {
            EventSystem.current.SetSelectedGameObject(null);
            return;
        }

        // 次に選択されるUIオブジェクトをセットする
        if (nextSelectedUiObj != null)
        {
            EventSystem.current.SetSelectedGameObject(nextSelectedUiObj);
        }
    }

    private void SetOutlineData(bool enable)
    {
        if (effectType == EffectType.None) { return; }

        switch (effectType)
        {
            case EffectType.OutLIne:
                uiFx.ToggleOutline(enable);
                break;
            case EffectType.InnerOutlIne:
                uiFx.SetInnerOutline(enable);
                break;
        }
    }
}

//[CustomEditor(typeof(TitleButtonController))]
//public class TestEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        return;

//        var title = (TitleButtonController)target;
//        title.effectType = (TitleButtonController.EffectType)EditorGUILayout.EnumPopup(title.effectType);
//        title = EditorGUILayout.

//        switch(title.effectType)
//        {
//            case TitleButtonController.EffectType.None:
//                break;
//            case TitleButtonController.EffectType.OutLIne:
//                break;
//            case TitleButtonController.EffectType.InnerOutlIne:
//                break;
//        }
//    }
//}