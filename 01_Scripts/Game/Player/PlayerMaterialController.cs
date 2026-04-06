using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine;
using System.Collections; //StateMachine


public class PlayerMaterialController : MonoBehaviour
{
    public Material additiveMaterial;       // 加算マテリアル
    private Dictionary<Renderer, Material[]> originalMaterials = new();

    public float flashDuration = 0.1f;      // 1回の点滅時間
    public int flashCount = 3;             // 点滅回数

    public void Flash()
    {
        float invincibleTime = this.GetComponent<PlayerState>().InvincibleTime;
        float flashTime = (flashDuration * 2 * flashCount);
        flashCount = (int)(invincibleTime / flashTime);

        StopAllCoroutines();
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        for(int i=0;i<flashCount;i++)
        {
            ChangeMaterials();

            yield return new WaitForSeconds(flashDuration);

            RestoreMaterials();

            yield return new WaitForSeconds(flashDuration);
        }
        
    }

    void ChangeMaterials()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer.gameObject.CompareTag("Effect")) { continue; }
            if (renderer.gameObject.CompareTag("PlayerClone")) continue;

            // 元のマテリアル保存
            if (!originalMaterials.ContainsKey(renderer))
                originalMaterials[renderer] = renderer.materials;

            // flash用マテリアル配列生成
            Material[] flashMats = new Material[renderer.materials.Length];
            for (int i = 0; i < flashMats.Length; i++)
                flashMats[i] = additiveMaterial;

            renderer.materials = flashMats;
        }
    }

    void RestoreMaterials()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key == null) continue;
            kvp.Key.materials = kvp.Value;
        }
    }
}

