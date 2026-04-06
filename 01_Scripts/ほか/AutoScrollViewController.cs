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
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class AutoScrollViewController : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private ScrollRect scrollRect;
    private RectTransform selectedRectTrans;
    
    public float contentHeight;
    public float viewportHeight;
    public float verticalNormalizedPosY;

    void Update()
    {
        if (InputDeviceManager.Instance?.GetLastUsedDevice() is not Gamepad) { return; }

        if(content == null || scrollRect == null) { return; }
        RectTransform contentRectTrans = content.GetComponent<RectTransform>();
        RectTransform scrollRectTrans = scrollRect.GetComponent<RectTransform>();
        
        // UIを選択中かどうか調べる
        var selectedObj = EventSystem.current.currentSelectedGameObject;
        selectedRectTrans = selectedObj?.GetComponent<RectTransform>();
        if(selectedRectTrans == null) { return; }
        // 選択中UIがContentの子オブジェクトであるかを調べる
        if(selectedRectTrans.parent.IsChildOf(contentRectTrans) == false) { return; }
        
        // ContentとViewportの高さ、スクロールできる高さを計算する
        /*float */contentHeight = contentRectTrans.rect.height;
        /*float */viewportHeight = scrollRectTrans.rect.height;
        float scrollableHeight = contentHeight - viewportHeight;

        // 選択中UIを中央の高さに表示するための座標計算
        if (scrollableHeight <= 0) { return; }
        float itemPos = selectedRectTrans.anchoredPosition.y - (selectedRectTrans.rect.height / 2f);
        float targetPos = itemPos + (viewportHeight / 2f);

        // 0～1までの範囲でどのくらいスクロールするかを計算する
        verticalNormalizedPosY = 1f - Mathf.Clamp01(targetPos / -scrollableHeight);
        scrollRect.verticalNormalizedPosition = verticalNormalizedPosY;
    }
}

