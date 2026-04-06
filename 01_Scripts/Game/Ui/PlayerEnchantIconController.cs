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
using UnityEngine.EventSystems;

public class PlayerEnchantIconController: MonoBehaviour, 
    IPointerEnterHandler, IPointerExitHandler,ISelectHandler,IDeselectHandler
{
    [Header("アイコンへの表示情報")]
    public Image iconImage;
    public Mask iconMask;
    public string enchantNameStr;
    public string enchantDescStr;

    [Header("選択中の表示先")]
    public TextMeshProUGUI enchantNameText;
    public TextMeshProUGUI enchantDescText;

    [Header("選択中の拡縮アニメーション")]
    [Tooltip("アニメーションを行うオブジェクト")]
    public GameObject changeScaleObj;
    public Vector3 changeScale;
    public float changeDuration = 0.5f;
    public Vector3 defaultScale;
    private Transform changeTrans;
    private Tween animeTween;

    private void OnDisable()
    {
        animeTween?.Kill();
        if (changeTrans != null)
        {
            changeTrans.localScale = defaultScale;
        }
    }

    private void Start()
    {
        if(iconImage == null || iconMask == null)
        {
#if Editor
            Debug.Log("iconImageかiconMaskが設定されていません");
#endif
        }

        defaultScale = transform.localScale;
        changeTrans = changeScaleObj.transform;
    }

    private void SetEnchantData(string nameStr, string descStr, Vector3 scale)
    {
        if (changeTrans != null)
        { 
            animeTween?.Kill();
            animeTween = changeTrans.DOScale(scale, changeDuration).SetUpdate(0, true);
        }

        enchantNameText.text = nameStr;
        enchantDescText.text = descStr;
    }

    // ポインターが乗った場合は、テキストを変更
    public void OnPointerEnter(PointerEventData eventData)
    {
        SetEnchantData(enchantNameStr, enchantDescStr,changeScale);
    }

    // ポインターが外れた場合は、テキストを初期化
    public void OnPointerExit(PointerEventData eventData)
    {
        SetEnchantData("----", "----", defaultScale);
    }

    // 選択中になった場合、テキストを変更
    public void OnSelect(BaseEventData eventData)
    {
        SetEnchantData(enchantNameStr, enchantDescStr, changeScale);
    }

    // 選択中から外れた場合、テキストを初期化
    public void OnDeselect(BaseEventData eventData)
    {
        SetEnchantData("----", "----", defaultScale);
    }
}

