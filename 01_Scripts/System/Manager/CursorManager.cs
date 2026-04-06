using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class CursorManager : Singleton<CursorManager>
{

    public Texture2D cursorTexNormal;
    public Texture2D cursorTexMoveRotateMode;
    public Texture2D cursorBoost;
    public Texture2D cursorExchange;
    public Vector2 hotspot = Vector2.zero;

    [Header("Cursor Sprites")]
    public Sprite cursorSpriteNormal;
    public Sprite cursorSpriteMove;
    public Sprite cursorSpriteLoot;

    public Image cursorBoostImage;
    public Image cursorExchangeImage;

    void Start()
    {
        Cursor.SetCursor(cursorTexNormal, hotspot, CursorMode.Auto);
    }

    public void OnEnable()
    {
        EventManager.StartListening("ChangeFacingModeCursor", SetCursorNormal);
        EventManager.StartListening("ChangeFacingModeMove", SetCursorB);

    }

    public void OnDisable()
    {
        EventManager.StopListening("ChangeFacingModeCursor", SetCursorNormal);
        EventManager.StopListening("ChangeFacingModeMove", SetCursorB);
    }

    void Update()
    {

        if(cursorBoostImage)cursorBoostImage.transform.position = Input.mousePosition;
        if(cursorExchangeImage)cursorExchangeImage.transform.position = Input.mousePosition;
    }

    public void SetCursorNormal()
    {
        Cursor.visible = true;
        cursorExchangeImage.gameObject.SetActive(false);
        cursorBoostImage.gameObject.SetActive(false);
        Cursor.SetCursor(cursorTexNormal, hotspot, CursorMode.Auto);
    }

    public void SetCursorOff()
    {
        Cursor.visible = false;
        cursorExchangeImage.gameObject.SetActive(false);
        cursorBoostImage.gameObject.SetActive(false);
        Cursor.SetCursor(cursorTexNormal, hotspot, CursorMode.Auto);
    }

    public void SetCursorB()
    {
        Cursor.SetCursor(cursorTexMoveRotateMode, hotspot, CursorMode.Auto);
    }

    public void SetCursorBoost()
    {
        Cursor.visible = false;
        if (cursorBoostImage.gameObject.activeSelf)
        {
            SetCursorNormal();
            return;
        }
        cursorExchangeImage.gameObject.SetActive(false);
        cursorBoostImage.gameObject.SetActive(false);
        cursorBoostImage.gameObject.SetActive(true);
        //Cursor.SetCursor(cursorBoost, hotspot, CursorMode.Auto);

    }

    public void SetCursorExchange()
    {
        Cursor.visible = false;
        if (cursorExchangeImage.gameObject.activeSelf)
        {
            SetCursorNormal();
            return;
        }
        cursorBoostImage.gameObject.SetActive(false);
        cursorExchangeImage.gameObject.SetActive(true);
        //Cursor.SetCursor(cursorExchange, hotspot, CursorMode.Auto);

       

    }

    


}

