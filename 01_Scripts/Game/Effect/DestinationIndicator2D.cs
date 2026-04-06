using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine

public class DestinationIndicator2D : MonoBehaviour
{

    [Header("Scene assets")]
    [SerializeField] private Image  iconPrefab;                

    [Header("Edge behaviour")]
    [Range(0f, .25f)]
    [SerializeField] private float  screenMargin   = .05f;     // 0–1
    [SerializeField] private Vector2 edgeOffsetPx  = Vector2.zero; 
    [SerializeField] private bool   hideWhenOnScreen = true;

    private static RectTransform s_canvas;
    private static Camera        s_cam;
    private RectTransform icon;

    public bool isActivated = true;

    public bool isIconRotating = false;

    public TextMeshProUGUI questText;

    private void Awake()
    {
        if (!s_cam)    s_cam    = Camera.main;
        if (!s_canvas) s_canvas = FindObjectOfType<Canvas>().GetComponent<RectTransform>();

        iconPrefab = GameObject.FindGameObjectWithTag("QuestIndicator")?.GetComponent<Image>();

        icon = Instantiate(iconPrefab, s_canvas).rectTransform;
        icon.gameObject.SetActive(true);

        questText = icon.GetComponentInChildren<TextMeshProUGUI>();
        //if(questText) Debug.Log("Quest Text found: " + questText.text);
    }

    private void OnDestroy()
    {
        if (icon) Destroy(icon.gameObject);
    }

    private void Update()
    {
        if (icon)  icon.gameObject.SetActive(isActivated);
        if (!isActivated) return;

        if (!icon) return;

        Vector3 vp = s_cam.WorldToViewportPoint(transform.position);
    
        if (vp.z < 0f) { vp.x = 1f - vp.x; vp.y = 1f - vp.y; vp.z = 0f; } // flip if behind camera
   
        Vector2 unclamped = new Vector2(vp.x, vp.y); //unclamped copy for rotation / visibility
        Vector2 halfIconVp = new((icon.rect.width  * icon.lossyScale.x) / s_canvas.sizeDelta.x / 2f,  (icon.rect.height * icon.lossyScale.y) / s_canvas.sizeDelta.y / 2f); //icon half-size in viewport units
        Vector2 offsetVp = new(edgeOffsetPx.x / s_canvas.sizeDelta.x, edgeOffsetPx.y / s_canvas.sizeDelta.y); //edgeOffsetPx → viewport units

        float m = screenMargin;
    
        vp.x = Mathf.Clamp(vp.x, m + halfIconVp.x + offsetVp.x, 1f - m - halfIconVp.x - offsetVp.x); //outer edge* + extra offset touches the margin
        vp.y = Mathf.Clamp(vp.y, m + halfIconVp.y + offsetVp.y, 1f - m - halfIconVp.y - offsetVp.y);

        
        Vector2 size = s_canvas.sizeDelta;
        icon.anchoredPosition = new Vector2((vp.x - .5f) * size.x, (vp.y - .5f) * size.y); //viewport → canvas position

        Vector2 dir   = (unclamped - new Vector2(.5f, .5f)).normalized;
        float   angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        if(isIconRotating)icon.rotation = Quaternion.Euler(0f, 0f, angle);

        //hide when on screen
        bool onScreen = unclamped.x >= m && unclamped.x <= 1f - m && unclamped.y >= m && unclamped.y <= 1f - m && vp.z > 0f;
        icon.gameObject.SetActive(!(hideWhenOnScreen && onScreen));
    }
}

