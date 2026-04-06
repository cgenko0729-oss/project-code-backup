using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine


public class DitherObjectControll : MonoBehaviour
{
    [Header("判定を行うレイヤー")]
    [SerializeField] LayerMask rayHitLayer;
    [Header("ディザ抜きの時のアルファ値")]
    [SerializeField] private float ditherAlpha = 0.25f;

    private GameObject player;     // プレイヤー
    private GameObject mainCamera;          // メインカメラ
    private GameObject targetMapObject;     // ターゲットオブジェクト
    public float defaultAlpha = 1.0f;       // デフォルトのアルファ値
    
    void Start()
    {
        // メインカメラを探す
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        // プレイヤーを探す
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // プレイヤーかカメラがなければ早期リターン
        if(player == null || mainCamera == null) { return; }

        Vector3 playerPos = player.transform.position;
        Vector3 cameraPos = this.transform.position;
        Vector3 rayDir = (playerPos - cameraPos).normalized;
        float rayDist = (playerPos - cameraPos).magnitude;

        if (Physics.Raycast(this.transform.position,rayDir, 
            out RaycastHit hit,rayDist, rayHitLayer))
        {
            targetMapObject = hit.collider.gameObject;

            // Alpha値を変更する
            ChangeAlpha(ditherAlpha);
        }
        else
        {
            if(targetMapObject  == null) { return; }

            // Alpha値をデフォルト値に戻す
            ChangeAlpha(defaultAlpha);

            // ターゲットオブジェクトをNULLに戻す
            targetMapObject = null;
        }
    }

    private void ChangeAlpha(float alpha)
    {
        Renderer[] renderers =
                targetMapObject.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            Material[] materials = renderer.materials;
            foreach (var material in materials)
            {
                material.SetFloat("_Alpha", alpha);
            }
        }
    }
}

