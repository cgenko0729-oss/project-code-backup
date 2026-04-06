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
using System.Linq;

public class TitlePetData : MonoBehaviour
{
    [SerializeField] public PetType petType;
    private List<Renderer> renderers = new();
    [SerializeField]public List<Material[]> originalMaterials = new();
    public Material silhouetteMaterial;
    private bool isSilhouette = false;

    private void Start()
    {
        StartCoroutine(MaterialInit());
    }

    private void OnDisable()
    {
        OffSilhouette();
    }

    [ContextMenu("ON Silhouette")]
    public void OnSilhouette()
    {
        if(silhouetteMaterial == null || isSilhouette == true) { return; }

        foreach (var renderer in renderers)
        {
            var materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = silhouetteMaterial;
            }
            renderer.materials = materials;
        }

        isSilhouette = true;
    }

    [ContextMenu("OFF Silhouette")]
    public void OffSilhouette()
    {
        if (silhouetteMaterial == null || isSilhouette == false) { return; }

        int num = 0;
        foreach (var renderer in renderers)
        {
            renderer.materials = originalMaterials[num];
            num++;
        }

        isSilhouette = false;
    }

    [ContextMenu("Change Silhouette")]
    public void ChangeSilhouette(bool enableShilhouette)
    {
        if(enableShilhouette == true)
        {
            OnSilhouette();
        }
        else
        {
            OffSilhouette();
        }
    }

    System.Collections.IEnumerator MaterialInit()
    {
        foreach (var targetRenderer in this.gameObject.GetComponentsInChildren<Renderer>())
        {
            renderers.Add(targetRenderer);
            originalMaterials.Add(targetRenderer.materials);
        }

        yield return 1.0f;

        this.gameObject.SetActive(false);
    }
}

