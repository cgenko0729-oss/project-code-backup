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

public class CharaUpgradeToggleController : MonoBehaviour
{
    public Vector2 defaultDeltaSize;
    public float isSelectedScale = 1f;
    public float defaultScale = 0.7f;
    public float animationTime = 0.5f;

    private LayoutElement layoutElement;
    private Toggle toggle;
    private Tween layoutElementTween;
    private Tween transformTween;

    private void OnDisable()
    {
        layoutElementTween?.Kill();
        transformTween?.Kill();

        // LayoutElementとtransformを戻す
        //layoutElement.preferredWidth = defaultDeltaSize.x;
        //layoutElement.preferredHeight = defaultDeltaSize.y;
        //transform.localScale = Vector3.one * defaultScale;
    }

    void Start()
    {
        toggle = GetComponent<Toggle>();
        layoutElement = GetComponent<LayoutElement>();

        if (toggle == null || layoutElement == null)
        {
#if UNITY_EDITOR
            Debug.Log("ToggleもしくはLayoutElemntコンポーネントが取得できませんでした");
#endif
        }
        else
        {
            layoutElement.preferredWidth = defaultDeltaSize.x;
            layoutElement.preferredHeight = defaultDeltaSize.y;
            transform.localScale = Vector3.one * defaultScale;
        }
    }

    void Update()
    {
        
    }

    public void AnimationScale()
    {
        if(toggle == null || layoutElement == null) { return; }

        float endValue = defaultScale;
        if(toggle.isOn == true)
        {
            endValue = isSelectedScale;
        }

        // LayoutElementのアニメーション
        layoutElementTween?.Kill();
        Vector2 preferredSize = defaultDeltaSize * endValue;
        layoutElementTween =
            layoutElement.DOPreferredSize(preferredSize, animationTime);
        
        // Transformのアニメーション
        transformTween?.Kill();
        transformTween =
            transform.DOScale(endValue, animationTime);
    }
}

